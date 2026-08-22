using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace DataverseConnection.Internal
{
    /// <summary>
    /// Builds the interactive (MSAL-based) credentials with healthy caching defaults so a user logs
    /// in as rarely as possible. Tokens are persisted to the operating system credential store where
    /// available and the account is remembered via a serialized <see cref="AuthenticationRecord"/>,
    /// allowing later runs to acquire tokens silently.
    /// </summary>
    /// <remarks>
    /// Persistent encryption relies on the OS keychain (DPAPI on Windows, Keychain on macOS,
    /// libsecret on Linux). Headless Linux environments commonly have no keychain, so Linux alone
    /// permits Azure Identity's unencrypted file fallback. The cache must therefore be treated as a
    /// secret and made available only to the user or container that owns it.
    /// </remarks>
    internal static class PersistentCredentialCache
    {
        private const string CacheName = "DataverseConnection";

        private static string RecordDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".dataverseconnection");

        /// <summary>
        /// Creates an <see cref="InteractiveBrowserCredential"/> with persistent token caching.
        /// </summary>
        public static TokenCredential CreateInteractiveBrowser(string dataverseUrl)
        {
            var key = CreatePersistenceKey("interactive-browser", dataverseUrl);
            var record = TryLoadRecord(key);
            var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
            {
                TokenCachePersistenceOptions = CreateTokenCachePersistenceOptions(key),
                AuthenticationRecord = record,
            });

            return new PersistentAuthCredential(
                credential,
                record is not null,
                (ctx, ct) => credential.AuthenticateAsync(ctx, ct),
                key,
                () => new InteractiveBrowserCredential());
        }

        /// <summary>
        /// Creates a <see cref="DeviceCodeCredential"/> with persistent token caching.
        /// </summary>
        public static TokenCredential CreateDeviceCode(string dataverseUrl)
        {
            var key = CreatePersistenceKey("device-code", dataverseUrl);
            var record = TryLoadRecord(key);
            var credential = new DeviceCodeCredential(new DeviceCodeCredentialOptions
            {
                TokenCachePersistenceOptions = CreateTokenCachePersistenceOptions(key),
                AuthenticationRecord = record,
            });

            return new PersistentAuthCredential(
                credential,
                record is not null,
                (ctx, ct) => credential.AuthenticateAsync(ctx, ct),
                key,
                () => new DeviceCodeCredential());
        }

        internal static string CreatePersistenceKey(string credentialType, string dataverseUrl)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(credentialType);
            ArgumentException.ThrowIfNullOrWhiteSpace(dataverseUrl);

            var uri = new Uri(dataverseUrl, UriKind.Absolute);
            var environment = uri.GetLeftPart(UriPartial.Authority).ToLowerInvariant();
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(environment))).ToLowerInvariant();
            return $"{credentialType}-{hash}";
        }

        internal static string CreateCacheName(string key) => $"{CacheName}-{key}";

        internal static TokenCachePersistenceOptions CreateTokenCachePersistenceOptions(
            string key,
            bool? isLinux = null) => new()
            {
                Name = CreateCacheName(key),

                // Containers and other headless Linux hosts generally do not provide libsecret. Azure
                // Identity still prefers libsecret when it is available; this only permits its
                // file-based fallback so browser and device-code sessions survive process restarts.
                UnsafeAllowUnencryptedStorage = isLinux ?? OperatingSystem.IsLinux(),
            };

        internal static AuthenticationRecord? TryLoadRecord(string key)
        {
            try
            {
                var path = RecordPath(key);
                if (!File.Exists(path))
                    return null;

                using var stream = File.OpenRead(path);
                return AuthenticationRecord.Deserialize(stream);
            }
            catch
            {
                // A corrupt or unreadable record simply means we re-authenticate.
                return null;
            }
        }

        internal static void TrySaveRecord(string key, AuthenticationRecord record)
        {
            try
            {
                Directory.CreateDirectory(RecordDirectory);
                using var stream = File.Create(RecordPath(key));
                record.Serialize(stream);
            }
            catch
            {
                // Best effort: failing to persist the record only means a prompt next time.
            }
        }

        private static string RecordPath(string key) =>
            Path.Combine(RecordDirectory, $"auth-record-{key}.json");
    }

    /// <summary>
    /// Wraps an interactive credential so the <see cref="AuthenticationRecord"/> is persisted after the
    /// first successful sign-in, enabling silent token acquisition on subsequent runs. If the encrypted
    /// token cache is unavailable, it falls back to a non-persistent credential.
    /// </summary>
    internal sealed class PersistentAuthCredential : TokenCredential
    {
        private readonly TokenCredential _inner;
        private readonly Func<TokenRequestContext, CancellationToken, Task<AuthenticationRecord>> _authenticate;
        private readonly string _key;
        private readonly Func<TokenCredential> _createFallback;
        private readonly SemaphoreSlim _gate = new(1, 1);

        private volatile bool _recordEnsured;
        private TokenCredential? _fallback;

        internal string PersistenceKey => _key;

        public PersistentAuthCredential(
            TokenCredential inner,
            bool hasRecord,
            Func<TokenRequestContext, CancellationToken, Task<AuthenticationRecord>> authenticate,
            string key,
            Func<TokenCredential> createFallback)
        {
            _inner = inner;
            _recordEnsured = hasRecord;
            _authenticate = authenticate;
            _key = key;
            _createFallback = createFallback;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) =>
            GetTokenAsync(requestContext, cancellationToken).AsTask().GetAwaiter().GetResult();

        public override async ValueTask<AccessToken> GetTokenAsync(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            if (_fallback is not null)
                return await _fallback.GetTokenAsync(requestContext, cancellationToken).ConfigureAwait(false);

            try
            {
                await EnsureRecordAsync(requestContext, cancellationToken).ConfigureAwait(false);
                return await _inner.GetTokenAsync(requestContext, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (IsPersistenceFailure(ex))
            {
                _fallback = _createFallback();
                return await _fallback.GetTokenAsync(requestContext, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task EnsureRecordAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            if (_recordEnsured)
                return;

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_recordEnsured)
                    return;

                var record = await _authenticate(requestContext, cancellationToken).ConfigureAwait(false);
                PersistentCredentialCache.TrySaveRecord(_key, record);
                _recordEnsured = true;
            }
            finally
            {
                _gate.Release();
            }
        }

        private static bool IsPersistenceFailure(Exception exception)
        {
            for (Exception? ex = exception; ex is not null; ex = ex.InnerException)
            {
                var name = ex.GetType().Name;
                if (name.Contains("Persist", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("persistence", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("libsecret", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
