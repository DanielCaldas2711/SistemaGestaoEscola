namespace SistemaGestaoEscola.Web.Models.API
{
    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? NewPassword { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
