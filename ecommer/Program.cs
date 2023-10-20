using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using ecommer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ecommer.Repositories;
using ecommer;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de la cadena de conexi�n
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Agregar servicios al contenedor de inyecci�n de dependencias
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// Registra las interfaces de los repositorios
builder.Services.AddTransient<IHomeRepository, HomeRepository>();
builder.Services.AddTransient<ICartRepository, CartRepository>();
builder.Services.AddTransient<IUserOrderRepository, UserOrderRepository>();

// Configuraci�n de Stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddControllersWithViews();

// Agregar configuraci�n de sesiones si las usas para mantener el estado del carrito
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Agregar soporte para sesiones si las usas
app.UseSession();

app.UseRouting();

app.UseAuthentication();  // Agrega autenticaci�n
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Ajusta la ruta predeterminada seg�n tus necesidades
app.MapRazorPages();

// Sembrar la base de datos
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate(); // Aplicar migraciones autom�ticamente
        await DbSeeder.SeedDefaultData(services);
    }
    catch (Exception ex)
    {
        // Maneja la excepci�n de manera adecuada (registra, notifica, etc.).
        Console.WriteLine($"Error al sembrar la base de datos: {ex.Message}");
    }
}

app.Run();
