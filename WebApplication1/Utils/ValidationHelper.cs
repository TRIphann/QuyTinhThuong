using System.Text.RegularExpressions;

namespace QLDuLichRBAC_Upgrade.Utils
{
    public static class ValidationHelper
    {
        public static (bool IsValid, string ErrorMessage) ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username is required");

            if (username.Length < 3 || username.Length > 20)
                return (false, "Username must be between 3-20 characters");

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
                return (false, "Username can only contain letters, numbers and underscore");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required");

            if (password.Length < 3)
                return (false, "Password must be at least 3 characters");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required");

            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(email, emailPattern))
                return (false, "Invalid email format");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return (true, string.Empty);

            if (!Regex.IsMatch(phone, @"^0[0-9]{9,10}$"))
                return (false, "Phone number must have 10-11 digits and start with 0");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (false, "Full name is required");

            if (fullName.Length < 2 || fullName.Length > 50)
                return (false, "Full name must be between 2-50 characters");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateRegistration(
            string username, string password, string fullName, string email, string phone)
        {
            var usernameValidation = ValidateUsername(username);
            if (!usernameValidation.IsValid)
                return usernameValidation;

            var passwordValidation = ValidatePassword(password);
            if (!passwordValidation.IsValid)
                return passwordValidation;

            var fullNameValidation = ValidateFullName(fullName);
            if (!fullNameValidation.IsValid)
                return fullNameValidation;

            var emailValidation = ValidateEmail(email);
            if (!emailValidation.IsValid)
                return emailValidation;

            var phoneValidation = ValidatePhone(phone);
            if (!phoneValidation.IsValid)
                return phoneValidation;

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateLogin(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username is required");

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required");

            return (true, string.Empty);
        }
    }
}
