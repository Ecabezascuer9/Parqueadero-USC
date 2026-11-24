using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Interfaces;
using Parqueadero.Data;
using Parqueadero.Data.Identity;
using Parqueadero.Data.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = SharedDatabase.GetConnectionString();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services
    .AddIdentity<AplicacionUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
builder.Services.AddScoped<IParqueaderoService, ParqueaderoService>();
builder.Services.AddScoped(typeof(Parqueadero.Core.Interfaces.IRepositorio<>), typeof(Parqueadero.Data.Repositories.Repositorio<>));
builder.Services.AddControllersWithViews();

var app = builder.Build();
await DatabaseSeeder.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
