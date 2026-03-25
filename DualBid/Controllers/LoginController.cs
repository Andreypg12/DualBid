using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DualBid.Application.Services.Interfaces;
using DualBid.ViewModels.Login;
using Libreria.Web.Util;

namespace DualBid.Controllers
{
    public class LoginController : Controller
    {
        private readonly IserviceUser _serviceUsuario;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IserviceUser serviceUsuario, ILogger<LoginController> logger)
        {
            _serviceUsuario = serviceUsuario;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn(ViewModelLogin viewModelLogin)
        {
            if (!ModelState.IsValid)
            {
                string errores = string.Join("<br>", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Unspecified validation error"
                        : e.ErrorMessage));

                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "Validation errors",
                    $"The form contains the following errors: {errores}",
                    SweetAlertMessageType.warning
                );

                _logger.LogWarning("Login validation error for user {User}. Details: {Errors}",
                    viewModelLogin.User, errores);

                return View("Index", viewModelLogin);
            }

            var usuarioLog = await _serviceUsuario.LoginAsync(viewModelLogin.User, viewModelLogin.Password);

            if (usuarioLog == null)
            {
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "Access denied",
                    "Invalid username or password.",
                    SweetAlertMessageType.warning
                );

                _logger.LogWarning("Failed login attempt for user {User}", viewModelLogin.User);

                return View("Index", viewModelLogin);
            }

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, $"{usuarioLog.Name} {usuarioLog.LastNames}"),
                new Claim(ClaimTypes.Role, usuarioLog.Role.Description),
                new Claim(ClaimTypes.NameIdentifier, usuarioLog.Id.ToString())
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
                IsPersistent = false
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
            );

            _logger.LogInformation("Successful login for user {User}", viewModelLogin.User);

            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                "Welcome",
                $"Login successful. Hello, {usuarioLog.Name}.",
                SweetAlertMessageType.success
            );

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> LogOff()
        {
            _logger.LogInformation("Successful logout for {User}", User.Identity?.Name);

            await HttpContext.SignOutAsync();

            TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                "Session ended",
                "You have successfully logged out.",
                SweetAlertMessageType.success
            );

            return RedirectToAction("Index", "Login");
        }

        public IActionResult Forbidden()
        {
            return View();
        }
    }
}