using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using PointOfSale.Model.Auditoria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly IGenericRepository<AuditoriaModificaciones> _repository;

        public AuditoriaService(IGenericRepository<AuditoriaModificaciones> repository)
        {
            _repository = repository;
        }

        public void SaveAuditoria(Category antes, Category despues)
        {
            var a = new AuditoriaModificaciones();
            a.IdEntidad = antes.IdCategory;
            a.Entidad = antes.GetType().Name;
            a.Descripcion = antes.Description.ToUpper();
            a.EntidadAntes = JsonSerializer.Serialize(new ACategoria(antes));
            a.EntidadDespues = JsonSerializer.Serialize(new ACategoria(despues));
            a.ModificationDate = DateTime.Now;
            a.ModificationUser = despues.ModificationUser.ToUpper();

            _repository.Add(a);
        }

        public void SaveAuditoria(User antes, User Despues)
        {
            var a = new AuditoriaModificaciones();
            a.IdEntidad = antes.IdUsers;
            a.Entidad = antes.GetType().Name;
            a.Descripcion = antes.Name.ToUpper();
            a.EntidadAntes = JsonSerializer.Serialize(new AUser(antes));
            a.EntidadAntes = JsonSerializer.Serialize(new AUser(Despues));
            a.ModificationDate = DateTime.Now;
            a.ModificationUser = Despues.ModificationUser.ToUpper();

            _repository.Add(a);
        }


        public void SaveAuditoria(VentaWeb antes, VentaWeb despues)
        {
            var a = new AuditoriaModificaciones();
            a.IdEntidad = antes.IdVentaWeb;
            a.Entidad = antes.GetType().Name;
            a.Descripcion = antes.Nombre.ToUpper();
            a.EntidadAntes = JsonSerializer.Serialize(new AVentaWeb(antes));
            a.EntidadDespues = JsonSerializer.Serialize(new AVentaWeb(despues));
            a.ModificationDate = DateTime.Now;
            a.ModificationUser = despues.ModificationUser.ToUpper();

            _repository.Add(a);
        }
        public void SaveAuditoria(TypeDocumentSale antes, TypeDocumentSale despues)
        {
            var a = new AuditoriaModificaciones();
            a.IdEntidad = antes.IdTypeDocumentSale;
            a.Entidad = antes.GetType().Name;
            a.Descripcion = antes.Description.ToUpper();
            a.EntidadAntes = JsonSerializer.Serialize(new ATipoDocumentoVenta(antes));
            a.EntidadDespues = JsonSerializer.Serialize(new ATipoDocumentoVenta(despues));
            a.ModificationDate = DateTime.Now;
            a.ModificationUser = string.Empty; /*despues.ModificationUser.ToUpper();*/

            _repository.Add(a);
        }
    }
}
