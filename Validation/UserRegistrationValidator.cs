using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;

namespace study4_be.Validation
{
    public class UserRegistrationValidator
    {
        private readonly UserRepository _userRepository;
        public UserRegistrationValidator(UserRepository repository) { _userRepository = repository; }
        public bool Validate(User user, out string errorMessage)
        {
            if (string.IsNullOrEmpty(user.UserEmail) || !IsValidEmail(user.UserEmail))
            {
                errorMessage = "A valid email is required.";
                return false;
            }
            if (_userRepository.CheckEmailExists(user.UserEmail))
            {
                errorMessage = "Email already exists.";
                return false;
            }
            if (string.IsNullOrEmpty(user.UserPassword))
            {
                errorMessage = "Password is required.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var mail = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
