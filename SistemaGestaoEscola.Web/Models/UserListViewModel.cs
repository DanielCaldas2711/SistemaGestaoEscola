namespace SistemaGestaoEscola.Web.Models
{
    public class UserListViewModel
    {
        public IEnumerable<UserRoleViewModel> Users { get; set; } = new List<UserRoleViewModel>();

        public string? SearchTerm { get; set; }

        public string? SelectedRole { get; set; }

        public IEnumerable<string> Roles { get; set; } = new List<string>();

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }
    }
}
