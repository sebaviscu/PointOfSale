using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _repository;
        private readonly IGenericRepository<Horario> _repositoryHorario;

        public UserService(IGenericRepository<User> repository, IGenericRepository<Horario> repositoryHorario)
        {
            _repository = repository;
            _repositoryHorario = repositoryHorario;
        }

        public async Task<List<User>> List()
        {
            IQueryable<User> query = await _repository.Query(_ => !_.IsSuperAdmin);
            return query.Include(r => r.IdRolNavigation).Include(r => r.Horarios).Include(t => t.Tienda).OrderBy(_ => _.Name).ToList();
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

                User user_edit = queryUser.Include(_ => _.Horarios).First();

                user_edit.Name = entity.Name;
                user_edit.Email = entity.Email;
                user_edit.Phone = entity.Phone;
                user_edit.IdRol = entity.IdRol;
                user_edit.IsActive = entity.IsActive;
                user_edit.Password = entity.Password;
                user_edit.ModificationUser = entity.ModificationUser;
                user_edit.ModificationDate = TimeHelper.GetArgentinaTime();
                user_edit.IdTienda = entity.IdTienda == -1 ? null : entity.IdTienda;
                user_edit.SinHorario = entity.SinHorario;

                SetHorarios(user_edit.Horarios.ToList(), entity.Horarios.ToList());

                bool response = await _repository.Edit(user_edit);
                if (!response)
                    throw new TaskCanceledException("No se pudo modificar user");

                User user_edited = queryUser.Include(r => r.IdRolNavigation).Include(r => r.Horarios).First();

                return user_edited;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void SetHorarios(List<Horario> actual, List<Horario> nuevos)
        {
            foreach (var h in actual)
            {
                var cambio = nuevos.First(_ => _.DiaSemana == h.DiaSemana);
                h.HoraSalida = cambio.HoraSalida;
                h.HoraEntrada = cambio.HoraEntrada;
                h.ModificationDate = cambio.ModificationDate;
                h.ModificationUser = cambio.ModificationUser;
            }
        }

        public async Task<bool> Delete(int idUser)
        {
            try
            {
                User user_found = await _repository.Get(u => u.IdUsers == idUser);

                if (user_found == null)
                    throw new TaskCanceledException("User no existe");

                var usersList = await GetAllUsers();

                if (usersList.Count == 1)
                    throw new TaskCanceledException("Debe existir al menos un usuario en el sistema");


                if (user_found.IsAdmin && usersList.Count(_ => _.IsAdmin && !_.IsSuperAdmin) == 1)
                    throw new TaskCanceledException("Debe existir al menos un usuario Administrador");

                bool response = await _repository.Delete(user_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<User?> GetByCredentials2(string email, string password)
        {
            var query = await _repository.Query(_ => !_.IsSuperAdmin);
            var user_found = query.SingleOrDefault(u => u.Email.ToUpper().Equals(email.ToUpper()));

            if (user_found != null)
            {
                var passwordDescrypt = EncryptionHelper.DecryptString(user_found.Password);
                if (passwordDescrypt == password)
                {
                    return user_found;
                }
            }

            return null;
        }

        public async Task<(User? Usuario, string? Mensaje)> GetByCredentials(string email, string password)
        {
            var query = await _repository.Query();
            var user_found = query.Include(_ => _.Horarios).SingleOrDefault(u => u.IsActive.Value && u.Email.ToUpper().Equals(email.ToUpper()));

            if (user_found != null)
            {

                var passwordDescrypt = EncryptionHelper.DecryptString(user_found.Password);
                if (passwordDescrypt == password)
                {

                    if (user_found.IsSuperAdmin)
                    {
                        return (user_found, null);
                    }

                    // Si el usuario tiene "SinHorario" en true, retornamos el usuario sin validar horarios
                    if (user_found.SinHorario == true)
                    {
                        return (user_found, null);
                    }

                    // Validamos el horario si "SinHorario" es false
                    var now = TimeHelper.GetArgentinaTime();
                    var diaSemanaActual = (DiasSemana)((int)now.DayOfWeek == 0 ? 7 : (int)now.DayOfWeek); // Convertir de DateTime.DayOfWeek a DiasSemana (donde 0 = Domingo)

                    var horarioValido = user_found.Horarios?.Any(h =>
                        h.DiaSemana == diaSemanaActual &&
                        TimeSpan.Parse(h.HoraEntrada) <= now.TimeOfDay &&
                        TimeSpan.Parse(h.HoraSalida) >= now.TimeOfDay);

                    // Si encontramos un horario válido, retornamos el usuario
                    if (horarioValido == true)
                    {
                        return (user_found, null);
                    }
                    else
                    {
                        // Retornamos null indicando que el usuario no está en su horario permitido
                        return (null, "Usuario fuera de su horario permitido");
                    }
                }
            }

            // Si el usuario no fue encontrado o la contraseña es incorrecta
            return (null, "Usuario no encontrado o contraseña incorrecta");
        }



        public async Task<bool> CheckFirstLogin(string email, string password)
        {
            var query = await _repository.Query();
            var user_found = query.SingleOrDefault(u => u.Email.ToUpper().Equals(email.ToUpper()));

            if (user_found != null && user_found.Email == "admin" && user_found.Name == "admin" && string.IsNullOrEmpty(password))
            {
                var cantUsers = await _repository.Query();

                if (cantUsers.ToList().Count == 1)
                    return true;
            }

            return false;
        }

        public async Task<User> GetById(int IdUser)
        {
            var query = await _repository.Query();
            return query.Include(_ => _.Horarios).SingleOrDefault(u => u.IdUsers == IdUser);
        }

        public async Task<User> GetByIdWithRol(int IdUser)
        {
            IQueryable<User> query = await _repository.Query(u => u.IdUsers == IdUser);
            var user = query.Include(r => r.IdRolNavigation).First();
            return user;
        }

        public async Task<List<User>> GetAllUsers()
        {
            IQueryable<User> query = await _repository.Query(u => u.IsActive.Value && !u.IsSuperAdmin);
            return query.OrderBy(_ => _.Name).ToList();
        }
    }
}
