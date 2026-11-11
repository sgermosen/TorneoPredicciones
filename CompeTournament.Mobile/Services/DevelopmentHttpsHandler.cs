#if DEBUG
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace CompeTournament.Mobile.Services
{
    public static class DevelopmentHttpsHandler
    {
        public static HttpMessageHandler Create()
        {
#if ANDROID
            return new Xamarin.Android.Net.AndroidMessageHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => IsTrusted(certificate, errors)
            };
#else
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => IsTrusted(certificate, errors)
            };
#endif
        }

        public static bool ValidateCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors)
        {
            return IsTrusted(certificate, errors);
        }

        private static bool IsTrusted(X509Certificate? certificate, SslPolicyErrors errors)
        {
            return errors == SslPolicyErrors.None || certificate?.Issuer == "CN=localhost";
        }
    }
}
#endif
