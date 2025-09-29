namespace SistemaGestaoEscola.Web.Models.API
{
    public class PasswordResetCode
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
