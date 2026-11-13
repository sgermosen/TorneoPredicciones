namespace CompeTournament.Backend.Helpers
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public class FcmPushSender : IPushSender
    {
        private const string Scope = "https://www.googleapis.com/auth/firebase.messaging";
        private const string DefaultTokenUri = "https://oauth2.googleapis.com/token";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FcmPushSender> _logger;
        private readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1, 1);

        private string _cachedAccessToken;
        private DateTime _accessTokenExpiresAt;

        public FcmPushSender(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<FcmPushSender> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(IEnumerable<string> tokens, string message)
        {
            var recipients = (tokens ?? Enumerable.Empty<string>())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();

            if (recipients.Count == 0)
            {
                return;
            }

            var account = LoadServiceAccount();
            if (account == null)
            {
                _logger.LogDebug("FCM sin configurar (Fcm:ServiceAccountPath); no se envia push.");
                return;
            }

            string accessToken;
            try
            {
                accessToken = await GetAccessTokenAsync(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo obtener el token de acceso de FCM.");
                return;
            }

            var client = _httpClientFactory.CreateClient();
            var endpoint = $"https://fcm.googleapis.com/v1/projects/{account.ProjectId}/messages:send";
            var title = _configuration["Fcm:NotificationTitle"] ?? "Copa Amistosa";

            foreach (var token in recipients)
            {
                var payload = new
                {
                    message = new
                    {
                        token,
                        notification = new { title, body = message }
                    }
                };

                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                    {
                        Content = JsonContent.Create(payload)
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    using var response = await client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("FCM respondio {Status} al enviar push: {Body}", (int)response.StatusCode, body);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando push a un dispositivo.");
                }
            }
        }

        private async Task<string> GetAccessTokenAsync(ServiceAccount account)
        {
            if (_cachedAccessToken != null && DateTime.UtcNow < _accessTokenExpiresAt)
            {
                return _cachedAccessToken;
            }

            await _tokenLock.WaitAsync();
            try
            {
                if (_cachedAccessToken != null && DateTime.UtcNow < _accessTokenExpiresAt)
                {
                    return _cachedAccessToken;
                }

                var assertion = BuildAssertion(account);

                var client = _httpClientFactory.CreateClient();
                using var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                    new KeyValuePair<string, string>("assertion", assertion)
                });

                using var response = await client.PostAsync(account.TokenUri, content);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                var accessToken = doc.RootElement.GetProperty("access_token").GetString();
                var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;

                _cachedAccessToken = accessToken;
                _accessTokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 60);
                return accessToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private static string BuildAssertion(ServiceAccount account)
        {
            var now = DateTime.UtcNow;

            using var rsa = RSA.Create();
            rsa.ImportFromPem(account.PrivateKey);

            var credentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                issuer: account.ClientEmail,
                audience: account.TokenUri,
                claims: new[]
                {
                    new Claim("scope", Scope),
                    new Claim(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now).ToString(), ClaimValueTypes.Integer64)
                },
                notBefore: now,
                expires: now.AddMinutes(55),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ServiceAccount LoadServiceAccount()
        {
            var path = _configuration["Fcm:ServiceAccountPath"];
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            try
            {
                using var stream = File.OpenRead(path);
                using var doc = JsonDocument.Parse(stream);
                var root = doc.RootElement;

                var account = new ServiceAccount
                {
                    ClientEmail = root.TryGetProperty("client_email", out var clientEmail) ? clientEmail.GetString() : null,
                    PrivateKey = root.TryGetProperty("private_key", out var privateKey) ? privateKey.GetString() : null,
                    TokenUri = root.TryGetProperty("token_uri", out var tokenUri) ? tokenUri.GetString() : DefaultTokenUri,
                    ProjectId = _configuration["Fcm:ProjectId"]
                        ?? (root.TryGetProperty("project_id", out var projectId) ? projectId.GetString() : null)
                };

                if (string.IsNullOrWhiteSpace(account.ClientEmail)
                    || string.IsNullOrWhiteSpace(account.PrivateKey)
                    || string.IsNullOrWhiteSpace(account.ProjectId))
                {
                    _logger.LogWarning("La cuenta de servicio de FCM esta incompleta.");
                    return null;
                }

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo leer la cuenta de servicio de FCM.");
                return null;
            }
        }

        private sealed class ServiceAccount
        {
            public string ClientEmail { get; set; }
            public string PrivateKey { get; set; }
            public string TokenUri { get; set; }
            public string ProjectId { get; set; }
        }
    }
}
