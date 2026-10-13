namespace backend.ApplicationCore.Auth.Requests
{
    public class Credential
    {
        public string Username { get; set; }
        public string Password { get; set; } 
        public string Token { get; set; }
        public DateTime LastUpdated { get; set; }
    }

}
