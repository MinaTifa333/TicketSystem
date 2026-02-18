namespace TicketSystem.Components
{
    using Microsoft.AspNetCore.Identity;

    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError InvalidUserName(string userName)
            => new IdentityError
            {
                Code = nameof(InvalidUserName),
                Description = $"❌ اسم المستخدم '{userName}' غير صالح. يجب أن يكون باللغة الإنجليزية فقط وبدون مسافات."
            };

        public override IdentityError DuplicateUserName(string userName)
            => new IdentityError
            {
                Code = nameof(DuplicateUserName),
                Description = $"❌ اسم المستخدم '{userName}' مستخدم بالفعل."
            };

        public override IdentityError DuplicateEmail(string email)
            => new IdentityError
            {
                Code = nameof(DuplicateEmail),
                Description = $"❌ البريد '{email}' مسجّل من قبل."
            };

        public override IdentityError PasswordTooShort(int length)
            => new IdentityError
            {
                Code = nameof(PasswordTooShort),
                Description = $"❌ كلمة المرور قصيرة. الحد الأدنى: {length}."
            };

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => new IdentityError
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = "❌ كلمة المرور يجب أن تحتوي على رمز خاص."
            };

        public override IdentityError PasswordRequiresDigit()
            => new IdentityError
            {
                Code = nameof(PasswordRequiresDigit),
                Description = "❌ كلمة المرور يجب أن تحتوي على رقم."
            };

        public override IdentityError PasswordRequiresLower()
            => new IdentityError
            {
                Code = nameof(PasswordRequiresLower),
                Description = "❌ كلمة المرور يجب أن تحتوي على حرف صغير."
            };

        public override IdentityError PasswordRequiresUpper()
            => new IdentityError
            {
                Code = nameof(PasswordRequiresUpper),
                Description = "❌ كلمة المرور يجب أن تحتوي على حرف كبير."
            };
    }

}
