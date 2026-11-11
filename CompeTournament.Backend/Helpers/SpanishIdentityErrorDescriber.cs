namespace CompeTournament.Backend.Helpers
{
    using Microsoft.AspNetCore.Identity;

    public class SpanishIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DuplicateEmail(string email) =>
            new IdentityError { Code = nameof(DuplicateEmail), Description = $"El correo {email} ya está registrado." };

        public override IdentityError DuplicateUserName(string userName) =>
            new IdentityError { Code = nameof(DuplicateUserName), Description = $"El usuario {userName} ya está registrado." };

        public override IdentityError InvalidEmail(string email) =>
            new IdentityError { Code = nameof(InvalidEmail), Description = $"El correo {email} no es válido." };

        public override IdentityError InvalidUserName(string userName) =>
            new IdentityError { Code = nameof(InvalidUserName), Description = $"El usuario {userName} no es válido." };

        public override IdentityError PasswordMismatch() =>
            new IdentityError { Code = nameof(PasswordMismatch), Description = "La contraseña no es correcta." };

        public override IdentityError PasswordTooShort(int length) =>
            new IdentityError { Code = nameof(PasswordTooShort), Description = $"La contraseña debe tener al menos {length} caracteres." };

        public override IdentityError PasswordRequiresDigit() =>
            new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "La contraseña debe tener al menos un número." };

        public override IdentityError PasswordRequiresLower() =>
            new IdentityError { Code = nameof(PasswordRequiresLower), Description = "La contraseña debe tener al menos una minúscula." };

        public override IdentityError PasswordRequiresUpper() =>
            new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "La contraseña debe tener al menos una mayúscula." };

        public override IdentityError PasswordRequiresNonAlphanumeric() =>
            new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "La contraseña debe tener al menos un símbolo." };

        public override IdentityError InvalidToken() =>
            new IdentityError { Code = nameof(InvalidToken), Description = "El enlace no es válido o ya venció." };

        public override IdentityError DefaultError() =>
            new IdentityError { Code = nameof(DefaultError), Description = "Ocurrió un error inesperado." };
    }
}
