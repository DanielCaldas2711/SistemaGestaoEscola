using System.ComponentModel.DataAnnotations;
using SistemaGestaoEscola.Web.Data.Entities.Interfaces;

namespace SistemaGestaoEscola.Web.Data.Entities
{
    public class Alert : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string FromUserId { get; set; }
        public User FromUser { get; set; }

        [Required]
        public string ToUserId { get; set; }
        public User ToUser { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
