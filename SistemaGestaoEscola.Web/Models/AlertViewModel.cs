using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Models
{
    public class AlertViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }
        public string ReceiverId { get; set; }
        public List<SelectListItem>? Admins { get; set; }
    }
}
