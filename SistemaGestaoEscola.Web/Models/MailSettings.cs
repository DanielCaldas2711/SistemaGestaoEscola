namespace SistemaGestaoEscola.Web.Models
{
    public class MailSettings
    {
        public string NameFrom { get; set; } = string.Empty;

        public string From { get; set; } = string.Empty;

        public string Smtp { get; set; } = string.Empty;

        public int Port { get; set; }

        public string Password { get; set; } = string.Empty;
    }
}
