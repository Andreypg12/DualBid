using AutoMapper;
using DualBid.Application.Config;
using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Models;
using DualBid.Infraestructure.Repository.Implementations;
using DualBid.Infraestructure.Repository.Interfaces;
using Libreria.Application.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.Services.Implementations
{
    public class ServiceUser : IServiceUser
    {
        private readonly IRepositoryUser _repository;
        private readonly IMapper _mapper;
        private readonly IOptions<AppConfig> _options;

        public ServiceUser(IRepositoryUser repository, IMapper mapper, IOptions<AppConfig> options)
        {
            _repository = repository;
            _mapper = mapper;
            _options = options;
        }

        public async Task<UserDTO?> FindByIdAsync(int id)
        {
            var @object = await _repository.FindByIdAsync(id);
            var objectMapped = _mapper.Map<UserDTO>(@object);
            return objectMapped;
        }

        public async Task<ICollection<UserDTO>> ListAsync()
        {
            var list = await _repository.ListAsync();
            return _mapper.Map<ICollection<UserDTO>>(list);
        }

        public async Task UpdateAsync(int id, UserDTO dto)
        {

            var entity = await _repository.FindByIdAsync(id);

            _mapper.Map(dto, entity);

            await _repository.UpdateAsync(entity);
        }

        public async Task<UserDTO> LoginAsync(string id, string password)
        {
            UserDTO usuarioDTO = null!;

            // Llave secreta
            string secret = _options.Value.Crypto.Secret;
            // Password encriptado
            string passwordEncrypted = Cryptography.Encrypt(password, secret);

            var @object = await _repository.LoginAsync(id, passwordEncrypted);

            if (@object != null)
            {
                usuarioDTO = _mapper.Map<UserDTO>(@object);
            }

            return usuarioDTO;
        }

        public async Task<bool> RegisterAsync(string name, string lastNames, string email, string password, int roleId)
        {
            try
            {
                // Verificar si el email ya existe
                if (await EmailExistsAsync(email))
                {
                    return false;
                }

                string secret = _options.Value.Crypto.Secret;
                string passwordEncrypted = Cryptography.Encrypt(password, secret);

                var user = new User
                {
                    Name = name,
                    LastNames = lastNames,
                    Email = email,
                    Password = passwordEncrypted,
                    RoleId = roleId,
                    StateId = 1 // Estado activo por defecto
                };

                await _repository.RegisterAsync(user);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _repository.EmailExistsAsync(email);
        }

        public async Task<UserProfileEditDTO> GetUserProfileAsync(int userId)
        {
            var user = await _repository.FindByIdAsync(userId);

            if (user == null)
                return null!;

            return new UserProfileEditDTO
            {
                Id = user.Id,
                Name = user.Name,
                LastNames = user.LastNames,
                Email = user.Email,
                RoleDescription = user.Role?.Description ?? "User",
                RoleId = user.Role?.Id ?? 3,
                StateDescription = user.State?.Description ?? "Active"
            };
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, UserProfileEditDTO dto)
        {
            var user = await _repository.FindByIdAsync(userId);
            if (user == null)
                return false;

            user.Name = dto.Name;
            user.LastNames = dto.LastNames;
            user.Email = dto.Email;

            await _repository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _repository.FindByIdAsync(userId);
            if (user == null)
                return false;

            string secret = _options.Value.Crypto.Secret;

            // Encriptar la contraseña que el usuario ingresó como "actual"
            string currentPasswordEncrypted = Cryptography.Encrypt(currentPassword, secret);

            // Comparar con la contraseña almacenada (ya encriptada)
            if (user.Password != currentPasswordEncrypted)
                return false;

            // Encriptar nueva contraseña
            string newPasswordEncrypted = Cryptography.Encrypt(newPassword, secret);
            user.Password = newPasswordEncrypted;

            await _repository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> ValidateCurrentPasswordAsync(int userId, string password)
        {

            string secret = _options.Value.Crypto.Secret;
            string passwordEncrypted = Cryptography.Encrypt(password.Trim(), secret);

            return await _repository.ValidateCurrentPasswordAsync(userId, passwordEncrypted);
        }

        public async Task<bool> EmailExistsForOtherUserAsync(int userId, string email)
        {
            var users = await _repository.ListAsync();
            return users.Any(u => u.Email == email && u.Id != userId);
        }
    }
}
