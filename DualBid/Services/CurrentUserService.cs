using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private const string SessionKey = "CurrentUserId";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IserviceUser _serviceUser;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            IserviceUser serviceUser)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceUser = serviceUser;
        }

        public int? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.Session.GetInt32(SessionKey);
        }

        public async Task<UserDTO?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return null;

            return await _serviceUser.FindByIdAsync(userId.Value);
        }

        public async Task SetCurrentUserAsync(int userId)
        {
            var user = await _serviceUser.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("El usuario no existe.");

            _httpContextAccessor.HttpContext?.Session.SetInt32(SessionKey, userId);
        }

        public void ClearCurrentUser()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
        }

        public bool HasCurrentUser()
        {
            return GetCurrentUserId().HasValue;
        }
    }
}
