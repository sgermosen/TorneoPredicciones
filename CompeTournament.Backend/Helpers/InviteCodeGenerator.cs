namespace CompeTournament.Backend.Helpers
{
    using System.Security.Cryptography;

    public static class InviteCodeGenerator
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public static string Generate(int length = 8)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var chars = new char[length];
            for (var i = 0; i < length; i++)
            {
                chars[i] = Alphabet[bytes[i] % Alphabet.Length];
            }

            return new string(chars);
        }
    }
}
