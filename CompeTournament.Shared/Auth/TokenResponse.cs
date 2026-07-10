namespace CompeTournament.Shared.Auth
{
    using System;

    public class TokenResponse
    {
        public string Token { get; set; }

        public DateTime Expiration { get; set; }

        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }

        public UserDto User { get; set; }
    }
}
