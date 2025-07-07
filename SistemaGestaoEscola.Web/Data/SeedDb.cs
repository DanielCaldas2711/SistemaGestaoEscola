using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Helpers.Interfaces;

namespace SistemaGestaoEscola.Web.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public SeedDb(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync(); //Creates or Updates DB including all migrations

            await EnsureRolesAsync(); //Checks if roles are created and creates them if necessary

            await EnsureUserAsync(new User
            {
                FirstName = "Daniel",
                LastName = "Silva",
                Email = "admin@escola.com",
                UserName = "admin@escola.com",
                PhoneNumber = "910489426",
                LockoutEnabled = false
            });      //Creating Admin user if it is not created

        }

        private async Task EnsureRolesAsync()
        {
            var roles = new[] { "Admin", "Funcionario", "Aluno", "Anonimo" };

            foreach (var role in roles)
            {
                await _userHelper.CheckRoleAsync(role);
            }
        }

        private async Task EnsureUserAsync(User user)
        {
            var UserExist = await _userHelper.GetUserByEmailAsync(user.Email);
            if (UserExist == null)
            {
                var result = await _userHelper.AddUserAsync(user, "Admin123!");

                if (result.Succeeded)
                {
                    await _userHelper.AddUserToRoleAsync(user, "Admin");

                    var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

                    await _userHelper.ConfirmEmailAsync(user, token);
                }
            }
        }
    }
}
