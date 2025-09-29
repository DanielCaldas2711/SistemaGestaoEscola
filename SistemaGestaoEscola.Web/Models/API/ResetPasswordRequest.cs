namespace SistemaGestaoEscola.Web.Models.API
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = "";
        public string Code { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}
