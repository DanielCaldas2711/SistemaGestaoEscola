using SistemaGestaoEscola.Web.Data.Entities;

namespace SistemaGestaoEscola.Web.DTOs
{
    public class AlertDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ToUserId { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
