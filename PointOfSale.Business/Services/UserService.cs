using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class UserService : IUserService
    {
        public DateTime DateTimeNowArg = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
        private readonly IGenericRepository<User> _repository;
        public UserService(IGenericRepository<User> repository)
        {
            _repository = repository;
        }

        public async Task<List<User>> List()
        {
            IQueryable<User> query = await _repository.Query();
            return query.Include(r => r.IdRolNavigation).Include(t => t.Tienda).OrderBy(_ => _.Name).ToList();
        }
        public async Task<User> Add(User entity)
        {
            User user_exists = await _repository.Get(u => u.Email == entity.Email);

            if (user_exists != null)
                throw new TaskCanceledException("The email already exists");

            try
            {

                User user_created = await _repository.Add(entity);

                if (user_created.IdUsers == 0)
                    throw new TaskCanceledException("Error al crear user");

                IQueryable<User> query = await _repository.Query(u => u.IdUsers == user_created.IdUsers);
                user_created = query.Include(r => r.IdRolNavigation).First();

                return user_created;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<User> Edit(User entity)
        {

            User user_exists = await _repository.Get(u => u.Email == entity.Email && u.IdUsers != entity.IdUsers);

            if (user_exists != null)
                throw new TaskCanceledException("The email already exists");

            try
            {
                IQueryable<User> queryUser = await _repository.Query(u => u.IdUsers == entity.IdUsers);

                User user_edit = queryUser.First();

                user_edit.Name = entity.Name;
                user_edit.Email = entity.Email;
                user_edit.Phone = entity.Phone;
                user_edit.IdRol = entity.IdRol;
                user_edit.IsActive = entity.IsActive;
                user_edit.Password = entity.Password;
                user_edit.ModificationUser = entity.ModificationUser;
                user_edit.ModificationDate = DateTimeNowArg;
                user_edit.IdTienda = entity.IdTienda == -1 ? null : entity.IdTienda;


                if (entity.Photo != null && entity.Photo.Length > 0)
                {
                    user_edit.Photo = entity.Photo;
                }

                bool response = await _repository.Edit(user_edit);
                if (!response)
                    throw new TaskCanceledException("No se pudo modificar user");

                User user_edited = queryUser.Include(r => r.IdRolNavigation).First();

                return user_edited;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> Delete(int idUser)
        {
            try
            {
                User user_found = await _repository.Get(u => u.IdUsers == idUser);

                if (user_found == null)
                    throw new TaskCanceledException("User no existe");

                var usersList = await List();

                if (usersList.Count == 1)
                    throw new TaskCanceledException("Debe existir al menos un usuario en el sistema");


                if (user_found.IsAdmin && usersList.Count(_ => _.IsAdmin) == 1)
                    throw new TaskCanceledException("Debe existir al menos un usuario Administrador");

                bool response = await _repository.Delete(user_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }



        public async Task<User> GetByCredentials(string email, string password)
        {
            var query = await _repository.Query();
            var user_found = query.SingleOrDefault(u => u.Email.Equals(email) && u.Password.Equals(password));

            return user_found;
        }

        public async Task<User> GetById(int IdUser)
        {

            User user_found = await _repository.Get(u => u.IdUsers == IdUser);

            return user_found;
        }

        public async Task<User> GetByIdWithRol(int IdUser)
        {
            IQueryable<User> query = await _repository.Query(u => u.IdUsers == IdUser);
            var user = query.Include(r => r.IdRolNavigation).First();
            return user;
        }

        public async Task<List<User>> GetAllUsers()
        {
            IQueryable<User> query = await _repository.Query(u => u.IsActive == true);
            return query.OrderBy(_ => _.Name).ToList();
        }
    }
}
