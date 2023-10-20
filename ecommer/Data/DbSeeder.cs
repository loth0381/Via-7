using ecommer.Constants;
using Microsoft.AspNetCore.Identity;

public class DbSeeder
{
    public static async Task SeedDefaultData(IServiceProvider serviceProvider)
    {
        try
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Crear roles si no existen
            await EnsureRoleExists(roleManager, Roles.Admin);
            await EnsureRoleExists(roleManager, Roles.User);

            // Crear el usuario administrador si no existe
            await EnsureUserExists(userManager, "Henri@gmail.com", "Henri@123", Roles.Admin);
        }
        catch (Exception ex)
        {
            // Registra o notifica la excepción en lugar de imprimir un mensaje.
            Console.WriteLine($"Error en la semilla de la base de datos: {ex.Message}");
        }
    }

    private static async Task EnsureRoleExists(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var role = new IdentityRole(roleName);
            await roleManager.CreateAsync(role);
        }
    }

    private static async Task EnsureUserExists(UserManager<IdentityUser> userManager, string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
            else
            {
                // Maneja el error de creación del usuario de manera adecuada.
                Console.WriteLine($"Error al crear el usuario: {string.Join(", ", result.Errors)}");
            }
        }
    }
}
