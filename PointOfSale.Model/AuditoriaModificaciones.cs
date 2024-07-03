using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class AuditoriaModificaciones
    {
        public int IdAuditoriaModificaciones { get; set; }
        public string? Entidad { get; set; }
        public int?   IdEntidad { get; set;}
        public string? Descripcion { get; set; }
        public string? EntidadAntes { get; set; }
        public string? EntidadDespues { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}


//create table AuditoriaModificaciones(
//IdAuditoriaModificaciones int primary key identity(1,1),
//Entidad varchar(150) null,
//IdEntidad int null,
//Descripcion varchar(150) null,
//EntidadAntes varchar(max) null,
//EntidadDespues varchar(max) null,
//[modificationDate] [datetime] null,
//[modificationUser] varchar(50) null,
//)
