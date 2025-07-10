using Microsoft.AspNetCore.Mvc;
using SistemaGestaoEscola.Web.Models.Components;
using SistemaGestaoEscola.Web.Helpers.Interfaces;

namespace SistemaGestaoEscola.Web.Models.Components
{
    public class UserSummaryViewComponent : ViewComponent
    {
        private readonly IUserHelper _userHelper;

        public UserSummaryViewComponent(IUserHelper userHelper)
        {
            _userHelper = userHelper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            var role = (await _userHelper.GetRolesAsync(user)).FirstOrDefault();

            var model = new UserSummaryViewModel
            {
                Role = role,
                ProfilePicturePath = string.IsNullOrEmpty(user.ProfilePicturePath)
                    ? "/images/defaultProfilePicture/default.jpg"
                    : user.ProfilePicturePath
            };

            return View(model);
        }
    }
}
