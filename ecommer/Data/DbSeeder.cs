using ecommer.Constants;
using Microsoft.AspNetCore.Identity;

public class DbSeeder
{
    public static async Task SeedDefaultData(IServiceProvider service)
    {
        var userMgr = service.GetService<UserManager<IdentityUser>>();
        var roleMgr = service.GetService<RoleManager<IdentityRole>>();

        // Crear roles si no existen
        if (!await roleMgr.RoleExistsAsync(Roles.Admin.ToString()))
        {
            await roleMgr.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
        }

        if (!await roleMgr.RoleExistsAsync(Roles.User.ToString()))
        {
            await roleMgr.CreateAsync(new IdentityRole(Roles.User.ToString()));
        }

        // Crear el usuario administrador si no existe
        var admin = new IdentityUser
        {
            UserName = "Henri@gmail.com",
            Email = "Henri@gmail.com",
            EmailConfirmed = true
        };

        var userInDb = await userMgr.FindByEmailAsync(admin.Email);
        if (userInDb is null)
        {
            await userMgr.CreateAsync(admin, "Henri@123");
            await userMgr.AddToRoleAsync(admin, Roles.Admin.ToString());
        }
    }
}
