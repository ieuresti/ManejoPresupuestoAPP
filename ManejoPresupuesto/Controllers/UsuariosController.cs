using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ManejoPresupuesto.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> userManager;
        private readonly SignInManager<Usuario> signInManager;

        public UsuariosController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }
            var usuario = new Usuario() { Email = modelo.Email };
            var resultado = await userManager.CreateAsync(usuario, password: modelo.Password);
            if (resultado.Succeeded)
            {
                await signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(modelo);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] // recomendado para habilitar protección CSRF en formularios POST
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }
            // Intento de autenticación con SignInManager:
            // PasswordSignInAsync busca el usuario (por userName/email según tu store),
            // valida la contraseña y, si es correcta, crea la cookie de autenticación.
            // Parámetros:
            //   - modelo.Email: valor usado como userName (en este proyecto se usa el email)
            //   - modelo.Password: contraseña enviada por el usuario
            //   - modelo.Recuerdame: isPersistent -> cookie persistente entre sesiones
            //   - lockoutOnFailure: si es true, incrementa contador de fallos y puede bloquear la cuenta
            var resultado = await signInManager.PasswordSignInAsync(
                modelo.Email, modelo.Password, modelo.Recuerdame, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                // Credenciales correctas: redirigir a la página principal de transacciones
                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nombre de usuario o password incorrecto");
                return View(modelo);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // recomendado: proteger contra CSRF
        public async Task<IActionResult> Logout()
        {
            // Invalidar la cookie de autenticación del esquema de Identity.
            // Esto cierra la sesión del usuario en la aplicación.
            // Alternativa: await signInManager.SignOutAsync(); (equivalente y recomendado cuando se usa Identity)
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            // Redirigir a una página pública tras cerrar sesión.
            return RedirectToAction("Index", "Transacciones");
        }
    }
}
