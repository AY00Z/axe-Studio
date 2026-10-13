using backend.Domain.Entities.Auth;

namespace backend.ApplicationCore.Auth.Responses
{
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string? JwtToken { get; set; }
        public string? ErrorType { get; set; }
        public int ResponseStatus { get; set; }
        public User User { get; set; } = new User();
    }
}

