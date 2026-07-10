namespace CompeTournament.Mobile.Core.Services
{
    using System.Net;

    public class ApiException : Exception
    {
        public ApiException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
