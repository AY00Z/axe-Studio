namespace backend.ApplicationCore.Auth.Requests
{
    public class App2AuthRequest
    {
        public string Token { get; set; }
        public string RedirectUrl { get; set; }
        public string State { get; set; }
    }
}
