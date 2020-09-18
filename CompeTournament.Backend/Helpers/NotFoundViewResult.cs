namespace CompeTournament.Backend.Helpers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Net;

    public class NotFoundViewResult : ViewResult
    {
        public NotFoundViewResult(string viewName)
        {
            ViewName = viewName;
            StatusCode = (int)HttpStatusCode.NotFound;
        }
    }
}
