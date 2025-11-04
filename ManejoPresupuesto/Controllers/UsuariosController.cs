using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ManejoPresupuesto.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> userManager;
        private readonly SignInManager<Usuario> signInManager;
        private readonly IServicioEmail servicioEmail;

        public UsuariosController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            IServicioEmail servicioEmail)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.servicioEmail = servicioEmail;
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult OlvideMiPassword(string mensaje = "")
        {
            ViewBag.Mensaje = mensaje;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] // Recomendable para proteger contra CSRF
        public async Task<IActionResult> OlvideMiPassword(OlvideMiPasswordViewModel modelo)
        {
            // Mensaje genérico que se mostrará al usuario independientemente de si el email existe.
            // Evita que un atacante pueda saber si una cuenta está registrada (user enumeration).
            var mensaje = "Proceso concluido. Si el email dado se corresponde con uno de nuestros usuarios, en su bandeja de entrada podrá encontrar las instrucciones para recuperar su contraseña.";
            ViewBag.Mensaje = mensaje;
            // Limpiamos el ModelState para que no se muestren errores de validación y solo se muestre el mensaje.
            ModelState.Clear();
            // Buscar el usuario por correo electrónico.
            var usuario = await userManager.FindByEmailAsync(modelo.Email);
            // Si el usuario no existe devolvemos la vista con el mensaje genérico (no revelar existencia).
            if (usuario is null)
            {
                return View();
            }
            // Generar token seguro para restablecer contraseña.
            // Este token será validado más tarde por Identity en la acción de recuperación.
            var codigo = await userManager.GeneratePasswordResetTokenAsync(usuario);
            // Codificar el token en Base64 URL-safe para incluirlo de forma segura en la URL.
            var codigoBase64 = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(codigo));
            // Construir el enlace absoluto que el usuario recibirá por email.
            // "Request.Scheme" asegura que el protocolo sea el actual (http/https).
            var enlace = Url.Action("RecuperarPassword", "Usuarios", new { codigo = codigoBase64 }, protocol: Request.Scheme);
            // Enviar el email con el enlace de recuperación.
            await servicioEmail.EnviarEmailCambioPassword(modelo.Email, enlace);
            // Devolver la vista (el usuario verá el mensaje genérico).
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RecuperarPassword(string codigo = null)
        {
            if (codigo is null)
            {
                var mensaje = "Codigo no encontrado";
                return RedirectToAction("OlvideMiPassword", new { mensaje });
            }

            var modelo = new RecuperarPasswordViewModel();
            modelo.CodigoReseteo = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(codigo));
            return View(modelo);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RecuperarPassword(RecuperarPasswordViewModel modelo)
        {
            var usuario = await userManager.FindByEmailAsync(modelo.Email);
            if (usuario == null)
            {
                return RedirectToAction("PasswordCambiado");
            }

            var resultados = await userManager.ResetPasswordAsync(usuario, modelo.CodigoReseteo, modelo.Password);
            return RedirectToAction("PasswordCambiado");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult PasswordCambiado() {
            return View();
        }
    }
}
