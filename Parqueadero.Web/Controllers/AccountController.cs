using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Parqueadero.Data.Identity;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

public class AccountController(
    SignInManager<AplicacionUser> signInManager,
    UserManager<AplicacionUser> userManager) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.Recordarme,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is not null && await userManager.IsInRoleAsync(user, "Estudiante"))
            {
                return RedirectToAction("Estudiantes", "Parqueaderos");
            }
            if (user is not null && await userManager.IsInRoleAsync(user, "Docente"))
            {
                return RedirectToAction("Docentes", "Parqueaderos");
            }
            if (user is not null && await userManager.IsInRoleAsync(user, "Egresado"))
            {
                return RedirectToAction("Egresados", "Parqueaderos");
            }
            if (user is not null && await userManager.IsInRoleAsync(user, "Administrador"))
            {
                return RedirectToAction("Index", "Reportes");
            }

            if (user is not null && await userManager.IsInRoleAsync(user, "Empleado"))
            {
                return RedirectToAction("Index", "Acceso");
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult RegistrarEgresado() => View(new RegistroEgresadoViewModel());

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarEgresado(RegistroEgresadoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var correoUniversidad = model.CorreoUniversidad.Trim();
        if (!correoUniversidad.EndsWith("@usc.edu.co", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.CorreoUniversidad), "Debe usar un correo institucional @usc.edu.co.");
            return View(model);
        }

        var existente = await userManager.FindByEmailAsync(correoUniversidad);
        if (existente is not null)
        {
            ModelState.AddModelError(nameof(model.CorreoUniversidad), "Ya existe una cuenta con ese correo.");
            return View(model);
        }

        var user = new AplicacionUser
        {
            UserName = correoUniversidad,
            Email = correoUniversidad,
            EmailConfirmed = true,
            NombreCompleto = model.NombreCompleto.Trim(),
            CorreoPersonal = model.CorreoPersonal.Trim(),
            CorreoUniversidad = correoUniversidad
        };
        var resultado = await userManager.CreateAsync(user, model.Password);
        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await userManager.AddToRoleAsync(user, "Egresado");
        await signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Egresados", "Parqueaderos");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
