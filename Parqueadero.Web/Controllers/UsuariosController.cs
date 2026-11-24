using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Data;
using Parqueadero.Data.Identity;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuariosController(
    UserManager<AplicacionUser> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await CrearModeloAsync(cancellationToken));
    }

    private async Task<UsuariosViewModel> CrearModeloAsync(CancellationToken cancellationToken)
    {
        var users = await userManager.Users.AsNoTracking().OrderBy(user => user.Email).ToListAsync(cancellationToken);
        var userRoles = new Dictionary<string, string>();
        foreach (var user in users)
        {
            userRoles[user.Id] = string.Join(", ", await userManager.GetRolesAsync(user));
        }

        return new UsuariosViewModel
        {
            Usuarios = users,
            Roles = DatabaseSeeder.Roles,
            RolesPorUsuario = userRoles
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(UsuariosViewModel model, CancellationToken cancellationToken)
    {
        if (!DatabaseSeeder.Roles.Contains(model.Rol) || !await roleManager.RoleExistsAsync(model.Rol))
        {
            ModelState.AddModelError(nameof(model.Rol), "Selecciona un rol válido.");
        }

        if (!ModelState.IsValid)
        {
            var datos = await CrearModeloAsync(cancellationToken);
            model.Usuarios = datos.Usuarios;
            model.Roles = datos.Roles;
            model.RolesPorUsuario = datos.RolesPorUsuario;
            model.Password = "";
            return View("Index", model);
        }

        var user = new AplicacionUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = true,
            NombreCompleto = model.NombreCompleto.Trim()
        };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Index));
        }

        result = await userManager.AddToRoleAsync(user, model.Rol);
        if (!result.Succeeded)
        {
            await userManager.DeleteAsync(user);
            TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Index));
        }

        TempData["Mensaje"] = "Usuario creado.";
        return RedirectToAction(nameof(Index));
    }
}
