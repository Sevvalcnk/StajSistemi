using Microsoft.AspNetCore.Identity;

namespace StajSistemi.Helpers // Eğer klasörün adı Identity ise burayı .Identity yapabilirsin
{
    public class TurkishIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError InvalidUserName(string userName) => new IdentityError { Code = nameof(InvalidUserName), Description = $"'{userName}' kullanıcı adı geçersizdir." };

        public override IdentityError DuplicateEmail(string email) => new IdentityError { Code = nameof(DuplicateEmail), Description = $"'{email}' adresi zaten başka bir kullanıcı tarafından alınmış." };

        public override IdentityError DuplicateUserName(string userName) => new IdentityError { Code = nameof(DuplicateUserName), Description = $"'{userName}' kullanıcı adı zaten kullanımda." };

        public override IdentityError PasswordTooShort(int length) => new IdentityError { Code = nameof(PasswordTooShort), Description = $"Şifre en az {length} karakter olmalıdır." };

        public override IdentityError PasswordRequiresLower() => new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Şifre en az bir küçük harf ('a'-'z') içermelidir." };

        public override IdentityError PasswordRequiresUpper() => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Şifre en az bir büyük harf ('A'-'Z') içermelidir." };

        public override IdentityError PasswordRequiresDigit() => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Şifre en az bir rakam ('0'-'9') içermelidir." };

        public override IdentityError PasswordRequiresNonAlphanumeric() => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Şifre en az bir özel karakter (.,*-! vb.) içermelidir." };
    }
}