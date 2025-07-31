using Microsoft.AspNetCore.Mvc;
using SistemaGestaoEscola.Web.Models.Components;
using SistemaGestaoEscola.Web.Helpers.Interfaces;

namespace SistemaGestaoEscola.Web.Helpers.Components
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

            var role = await _userHelper.GetRolesAsync(user);

            var model = new UserSummaryViewModel
            {
                Name = $"{user.FirstName.ToUpper()} - {role.FirstOrDefault().ToUpper()}",

                ProfilePicturePath = string.IsNullOrEmpty(user.ProfilePicturePath)
                    ? "/images/defaultProfilePicture/default.jpg"
                    : user.ProfilePicturePath
            };

            return View(model);
        }
    }
}
