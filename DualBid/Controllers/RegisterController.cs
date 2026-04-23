// Controllers/RegisterController.cs
using DualBid.Application.Services.Interfaces;
using DualBid.ViewModels.Register;
using Libreria.Web.Util;
using Microsoft.AspNetCore.Mvc;

namespace DualBid.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IServiceUser _serviceUser;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(IServiceUser serviceUser, ILogger<RegisterController> logger)
        {
            _serviceUser = serviceUser;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(ViewModelRegister viewModelRegister)
        {
            if (!ModelState.IsValid)
            {
                string errores = string.Join("<br>", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Unspecified validation error"
                        : e.ErrorMessage));

                _logger.LogWarning("Registration validation error. Details: {Errors}", errores);
                return View("Index", viewModelRegister);
            }

            // Verificar si el email ya existe
            if (await _serviceUser.EmailExistsAsync(viewModelRegister.Email))
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Registration Error",
                    "The email address is already registered. Please use a different email.",
                    SweetAlertMessageType.warning
                );

                _logger.LogWarning("Registration attempt with existing email: {Email}", viewModelRegister.Email);
                return View("Index", viewModelRegister);
            }

            // Registrar el usuario
            var result = await _serviceUser.RegisterAsync(
                viewModelRegister.Name,
                viewModelRegister.LastNames,
                viewModelRegister.Email,
                viewModelRegister.Password,
                viewModelRegister.RoleId
            );

            if (result)
            {
                _logger.LogInformation("Successful registration for user: {Email} with role: {RoleId}",
                    viewModelRegister.Email, viewModelRegister.RoleId);

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Registration Successful!",
                    $"Welcome {viewModelRegister.Name}! Your account has been created successfully. You can now log in.",
                    SweetAlertMessageType.success
                );

                return RedirectToAction("Index", "Login");
            }
            else
            {
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "Registration Error",
                    "An error occurred while creating your account. Please try again later.",
                    SweetAlertMessageType.error
                );

                _logger.LogError("Registration failed for user: {Email}", viewModelRegister.Email);
                return View("Index", viewModelRegister);
            }
        }
    }
}