using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class TiendaService : ITiendaService
    {
        private readonly IGenericRepository<Tienda> _repository;
        public TiendaService(IGenericRepository<Tienda> repository)
        {
            _repository = repository;
        }

        public async Task<List<Tienda>> List()
        {
            try
            {
            IQueryable<Tienda> query = await _repository.Query();
            return query.OrderBy(_ => _.Nombre).ToList();
            }
            catch (Exception e)
            {

                throw;
            }

        }

        public async Task<Tienda> Add(Tienda entity)
        {
            try
            {
                var list = await List();

                var idLastTienda = list.Max(_ => _.IdTienda);
                entity.IdTienda = ++idLastTienda;

                Tienda Tienda_created = await _repository.Add(entity);
                if (Tienda_created.IdTienda == 0)
                    throw new TaskCanceledException("Tienda no se pudo crear.");

                return Tienda_created;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Tienda> Edit(Tienda entity)
        {
            try
            {
                Tienda Tienda_found = await _repository.Get(c => c.IdTienda == entity.IdTienda);

                Tienda_found.Nombre = entity.Nombre;
                Tienda_found.Direccion = entity.Direccion;
                Tienda_found.Telefono = entity.Telefono;
                Tienda_found.Email = entity.Email;
                Tienda_found.Logo = entity.Logo;
                Tienda_found.NombreImpresora = entity.NombreImpresora;
                Tienda_found.IdListaPrecio = entity.IdListaPrecio;
                Tienda_found.Cuit = entity.Cuit;

                Tienda_found.ModificationDate = TimeHelper.GetArgentinaTime();
                Tienda_found.ModificationUser = entity.ModificationUser;

                bool response = await _repository.Edit(Tienda_found);

                if (!response)
                    throw new TaskCanceledException("Tienda no se pudo cambiar.");

                return Tienda_found;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Delete(int idTienda)
        {
            try
            {
                Tienda Tienda_found = await _repository.Get(c => c.IdTienda == idTienda);

                if (Tienda_found == null)
                    throw new TaskCanceledException("The Tienda no existe");


                bool response = await _repository.Delete(Tienda_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<Tienda> Get(int tiendaId)
        {
            try
            {
                Tienda Tienda_found = await _repository.Get(c => c.IdTienda == tiendaId);

                if (Tienda_found == null)
                    throw new TaskCanceledException("Tienda no se pudo encontrar.");

                return Tienda_found;
            }
            catch
            {
                throw;
            }
        }

        public X509Certificate2 GetCertificateAfipInformation()
        {
            string certificatePath = @"C:\Users\sebastian.viscusso\Desktop\Seba\Certificados AFIP generados\certificado.pfx";
            string certificatePassword = "password"; // Replace with your certificate's password

            // Load the certificate
            X509Certificate2 certificate = new X509Certificate2(certificatePath, certificatePassword);

            // Display certificate information
            Console.WriteLine("Subject: " + certificate.Subject); // SERIALNUMBER=CUIT 23365081999, CN=Test
            Console.WriteLine("Valid From: " + certificate.NotBefore); // fecha inicio
            Console.WriteLine("Valid To: " + certificate.NotAfter); // fecha caducidad

            return certificate;
        }

    }
    
}
