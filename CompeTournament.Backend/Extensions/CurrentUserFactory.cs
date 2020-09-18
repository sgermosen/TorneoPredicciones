namespace CompeTournament.Backend.Extensions
{
    using Microsoft.AspNetCore.Http;
    using System.Linq;
    using System.Security.Claims;

    public interface ICurrentUserFactory
    {
        CurrentUser Get { get; }
    }

    public class CurrentUserFactory : ICurrentUserFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserFactory(
            IHttpContextAccessor httpContextAccessor
        )
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public CurrentUser Get
        {
            get {
                var result = new CurrentUser();

                if (_httpContextAccessor.HttpContext == null || !_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                {
                    return result;
                }

                //var userClaims = new UserClaims();
                var claims = _httpContextAccessor.HttpContext.User.Claims.ToList();
                //foreach (var claim in claims)
                //{
                //    switch (claim.Type)
                //    {
                //        case "nameidentifier":
                //            result.UserId = claim.Value;
                //            break;
                //        //case UserClaimsKey.Name:
                //        //    userClaims.UserName = claim.Value;
                //        //    break;
                //        //case UserClaimsKey.Role:
                //        //    userClaims.UserRoles.Add(claim.Value);
                //        //    break;
                //    }
                //}
                // var claims = _httpContextAccessor.HttpContext.User.Claims;

                //if (claims.Any(x => x.Type.Equals("nameidentifier")))
                {
                    result.UserId = claims.First().Value;
                }

                if (claims.Any(x => x.Type.Equals(ClaimTypes.Name)))
                {
                    result.Name = claims.Where(x => x.Type.Equals(ClaimTypes.Name)).First().Value;
                }

                if (claims.Any(x => x.Type.Equals(ClaimTypes.Email)))
                {
                    result.Email = claims.Where(x => x.Type.Equals(ClaimTypes.Email)).First().Value;
                }
                 
                //if (claims.Any(x => x.Type.Equals("ShopId")))
                //{
                //    result.ShopId = long.Parse(claims.Where(x => x.Type.Equals("ShopId")).First().Value);
                //}

                //if (claims.Any(x => x.Type.Equals(ClaimTypes.Surname)))
                //{
                //    result.Lastname = claims.Where(x => x.Type.Equals(ClaimTypes.Surname)).First().Value;
                //}

                //if (claims.Any(x => x.Type.Equals(ClaimTypes.NameIdentifier)))
                //{
                //    result.SeoUrl = claims.Where(x => x.Type.Equals(ClaimTypes.NameIdentifier)).First().Value;
                //}

                //if (claims.Any(x => x.Type.Equals("access_token")))
                //{
                //    result.Token = claims.Where(x => x.Type.Equals("access_token")).First().Value;
                //}

                //if (claims.Any(x => x.Type.Equals("ImageProfile")))
                //{
                //    result.Image = claims.Where(x => x.Type.Equals("ImageProfile")).First().Value;
                //}

                //if (claims.Any(x => x.Type.Equals(ClaimTypes.Role)))
                //{
                //    result.Role = claims.Where(x => x.Type.Equals(ClaimTypes.Role)).First().Value;
                //}

                return result;
            }
        }
    }

    public class CurrentUser
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string SeoUrl { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Image { get; set; }
        public string Token { get; set; }
        public string Role { get; set; } 
        // public string Password { get; set; }
    }
}
