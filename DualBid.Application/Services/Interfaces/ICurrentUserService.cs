using DualBid.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.Services.Interfaces
{
    public interface ICurrentUserService
    {
        int? GetCurrentUserId();
        Task<UserDTO?> GetCurrentUserAsync();
        Task SetCurrentUserAsync(int userId);
        void ClearCurrentUser();
        bool HasCurrentUser();
    }
}
