using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using Humanizer;
using Libreria.Web.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using X.PagedList.Extensions;

namespace Libreria.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UserController : Controller
    {
        private readonly IServiceUser _serviceUser;
        private readonly IServiceUserStatus _serviceUserStatus;

        public UserController(IServiceUser serviceAutor, IServiceUserStatus serviceUserStatus)
        {
            _serviceUser = serviceAutor;
            _serviceUserStatus = serviceUserStatus;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string filter = "all", int? page = 1)
        {
            var collection = await _serviceUser.ListAsync();

            // Aplicar filtro según la opción seleccionada
            IEnumerable<UserDTO> filtered;

            if (filter == "blocked")
            {
                // Solo bloqueados (ID 3)
                filtered = collection.Where(u => u.State.Id == 3);
            }
            else // "all" - muestra activos (1) e inactivos (2) juntos
            {
                filtered = collection.Where(u => u.State.Id == 1 || u.State.Id == 2);
            }

            int pageNumber = page ?? 1;
            int pageSize = 5;

            // Pasar el filtro actual a la vista usando ViewBag
            ViewBag.CurrentFilter = filter;

            return View(filtered.ToPagedList(pageNumber, pageSize));
        }

        public async Task<ActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                       "User not found",
                       $"There is no user without an ID",
                       SweetAlertMessageType.error
                   );
                    return RedirectToAction("Index");
                }
                var @object = await _serviceUser.FindByIdAsync(id.Value);

                if (@object == null)
                {
                    throw new Exception("User does not exist");

                }

                ViewBag.States = await _serviceUserStatus.ListAsync();

                return View(@object);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, UserDTO dto)
        {
            try
            {
                // Verificar que el ID coincida
                if (id != dto.Id)
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Error",
                        "El ID del usuario no coincide",
                        SweetAlertMessageType.error
                    );
                    return RedirectToAction("Index");
                }

                // Validar el modelo
                if (!ModelState.IsValid)
                {
                    var errores = string.Join("<br>",
                        ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    );

                    ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                        "Validation errors",
                        $"The form has errors: {errores}",
                        SweetAlertMessageType.warning
                    );

                    // Recargar los estados para el combo
                    ViewBag.States = await _serviceUserStatus.ListAsync();

                    // Recargar el usuario completo para la vista
                    var usuarioActual = await _serviceUser.FindByIdAsync(id);
                    if (usuarioActual != null)
                    {
                        dto.RegistrationDate = usuarioActual.RegistrationDate;
                        dto.Role = usuarioActual.Role;
                        dto.State = usuarioActual.State;
                    }

                    return View("Details", dto);
                }

                // Obtener el usuario existente para preservar datos que no se envían
                var existingUser = await _serviceUser.FindByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Preservar la contraseña si no se envió una nueva
                if (string.IsNullOrWhiteSpace(dto.Password))
                {
                    dto.Password = existingUser.Password;
                }

                // Preservar la fecha de registro original
                dto.RegistrationDate = existingUser.RegistrationDate;

                // Actualizar el usuario
                await _serviceUser.UpdateAsync(id, dto);

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "User updated",
                    $"The user {dto.CompleteName} was updated succesfully.",
                    SweetAlertMessageType.success
                );

                return RedirectToAction("Details", new { id = dto.Id });
            }
            catch (Exception ex)
            {
                ViewBag.Notificacion = SweetAlertHelper.CrearNotificacion(
                    "Error",
                    $"unknow error: {ex.Message}",
                    SweetAlertMessageType.error
                );

                ViewBag.States = await _serviceUserStatus.ListAsync();
                return View("Details", dto);
            }
        }

        public async Task<IActionResult> MyProfile(bool edit = false)
        {
            var userId = GetCurrentUserId();

            if (userId == 0)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Access Denied",
                    "You must be logged in to view your profile",
                    SweetAlertMessageType.error
                );
                return RedirectToAction("Login", "Account");
            }

            var profile = await _serviceUser.GetUserProfileAsync(userId);

            if (profile == null)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Profile Not Found",
                    "Unable to load your profile information",
                    SweetAlertMessageType.error
                );
                return RedirectToAction("Index", "Home");
            }

            ViewBag.IsEditing = edit;
            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserProfileEditDTO dto)
        {
            var userId = GetCurrentUserId();

            if (userId == 0)
            {
                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Access Denied",
                    "You must be logged in to update your profile",
                    SweetAlertMessageType.error
                );
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                var profile = await _serviceUser.GetUserProfileAsync(userId);
                dto.RoleDescription = profile?.RoleDescription ?? "";
                dto.RoleId = profile?.RoleId ?? 3;
                dto.StateDescription = profile?.StateDescription ?? "";

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Validation Error",
                    "Please correct the errors in the form",
                    SweetAlertMessageType.warning
                );
                return View("MyProfile", dto);
            }

            try
            {
                // Verificar si el email ya existe para otro usuario
                var emailExists = await _serviceUser.EmailExistsForOtherUserAsync(userId, dto.Email);

                if (emailExists)
                {
                    var profile = await _serviceUser.GetUserProfileAsync(userId);
                    dto.RoleDescription = profile?.RoleDescription ?? "";
                    dto.RoleId = profile?.RoleId ?? 3;
                    dto.StateDescription = profile?.StateDescription ?? "";

                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Email already in use",
                        $"The email '{dto.Email}' is already registered to another account",
                        SweetAlertMessageType.error
                    );
                    return View("MyProfile", dto);
                }

                // Actualizar datos básicos
                await _serviceUser.UpdateUserProfileAsync(userId, dto);

                // Cambiar contraseña si se proporcionó
                if (!string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    var isValidCurrent = await _serviceUser.ValidateCurrentPasswordAsync(userId, dto.CurrentPassword ?? "");

                    if (!isValidCurrent)
                    {
                        var profile = await _serviceUser.GetUserProfileAsync(userId);
                        dto.RoleDescription = profile?.RoleDescription ?? "";
                        dto.RoleId = profile?.RoleId ?? 3;
                        dto.StateDescription = profile?.StateDescription ?? "";

                        TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                            "Incorrect Password",
                            "The current password you entered is incorrect",
                            SweetAlertMessageType.error
                        );
                        return View("MyProfile", dto);
                    }

                    await _serviceUser.ChangePasswordAsync(userId, dto.CurrentPassword!, dto.NewPassword);

                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Profile & Password Updated",
                        "Your profile information and password have been updated successfully",
                        SweetAlertMessageType.success
                    );
                }
                else
                {
                    TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                        "Profile Updated",
                        "Your profile information has been updated successfully",
                        SweetAlertMessageType.success
                    );
                }

                return RedirectToAction("MyProfile");
            }
            catch (Exception ex)
            {
                var profile = await _serviceUser.GetUserProfileAsync(userId);
                dto.RoleDescription = profile?.RoleDescription ?? "";
                dto.RoleId = profile?.RoleId ?? 3;
                dto.StateDescription = profile?.StateDescription ?? "";

                TempData["Notificacion"] = SweetAlertHelper.CrearNotificacion(
                    "Update Failed",
                    $"An error occurred: {ex.Message}",
                    SweetAlertMessageType.error
                );
                return View("MyProfile", dto);
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return userIdClaim != null ? int.Parse(userIdClaim) : 0;
        }
    }
}

