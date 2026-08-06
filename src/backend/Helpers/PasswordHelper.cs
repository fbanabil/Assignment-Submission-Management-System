namespace Backend.Helpers
{
    public interface IPasswordHelper
    {
        public Task<string> HashPassword(string password);
        public Task<bool> VerifyPassword(string password, string hashedPassword);

    }
    public class PasswordHelper : IPasswordHelper
    {
        private readonly IConfiguration _configuration;

        public PasswordHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<string> HashPassword(string password)
        {
            string pepper = _configuration["Security:PasswordPepper"] ?? string.Empty;
            string passwordWithPepper = password + pepper;
            string hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(passwordWithPepper, 12);
            return Task.FromResult(hashedPassword);
        }


        public Task<bool> VerifyPassword(string password, string hashedPassword)
        {
            string pepper = _configuration["Security:PasswordPepper"] ?? string.Empty;
            string passwordWithPepper = password + pepper;
            bool isValid = BCrypt.Net.BCrypt.EnhancedVerify(passwordWithPepper, hashedPassword);
            return Task.FromResult(isValid);

        }
    }
}
