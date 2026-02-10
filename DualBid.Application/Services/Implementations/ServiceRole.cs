using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Models;
using DualBid.Infraestructure.Repository.Interfaces;

namespace DualBid.Application.Services.Implementations
{
    public class ServiceRole : IServiceRole
    {
        // Repositorio encargado de acceder a los datos de Roles
        // (consultas a base de datos, listas, búsquedas, etc.)
        private readonly IRepositoryRole _repositoryRole;

        // AutoMapper se utiliza para convertir entidades del dominio
        // en DTOs y evitar exponer directamente las entidades
        private readonly IMapper _mapper;

        // Constructor del Servicio, Recibe las dependencias mediante Inyección de Dependencias
        public ServiceRole(IRepositoryRole repositoryRole, IMapper mapper)
        {
            _repositoryRole = repositoryRole;
            _mapper = mapper;
        }


        // Obtiene un rol específico según su identificador
        // Retorna un RoleDTO si existe, o null si no se encuentra
        public Task<RoleDTO?> FindByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        // Obtiene la lista completa de roles desde la base de datos
        // Convierte las entidades Role a RoleDTO antes de devolver el resultado
        public async Task<ICollection<RoleDTO>> ListAsync()
        {
            // Llama al repositorio para obtener los roles (entidades)
            var list = await _repositoryRole.ListAsync();

            // Mapea la lista de entidades a una lista de DTOs
            return _mapper.Map<ICollection<RoleDTO>>(list);
        }
    }
}
