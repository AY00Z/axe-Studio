namespace backend.Domain.Entities.Auth
{
    public class AuthCode
    {
        public string Code { get; set; }
        public string Token { get; set; }
        public string RedirectUrl { get; set; }
        public DateTime Expiry { get; set; }
    }
}
