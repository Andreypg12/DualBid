using DualBid.Application.DTOs;
using DualBid.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.Services.Interfaces
{
     public interface IServiceUser
    {
        Task<ICollection<UserDTO>> ListAsync();
        Task<UserDTO?> FindByIdAsync(int id);
        Task UpdateAsync(int id, UserDTO dto);
        Task<UserDTO> LoginAsync(string id, string password);
        Task<bool> RegisterAsync(string name, string lastNames, string email, string password, int roleId);
        Task<bool> EmailExistsAsync(string email);

        Task<UserProfileEditDTO> GetUserProfileAsync(int userId);
        Task<bool> UpdateUserProfileAsync(int userId, UserProfileEditDTO dto);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ValidateCurrentPasswordAsync(int userId, string password);
        Task<bool> EmailExistsForOtherUserAsync(int userId, string email);
    }
}
