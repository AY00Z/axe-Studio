namespace backend.ApplicationCore.Auth.Requests
{
    public class LogoutRequest
    {
        public string Token { get; set; }
        public bool   Logout { get; set; }
    }
}
