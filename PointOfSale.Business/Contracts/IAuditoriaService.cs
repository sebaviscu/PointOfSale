using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IAuditoriaService
    {
        void SaveAuditoria(Category antes, Category Despues);

        void SaveAuditoria(User antes, User Despues);
        void SaveAuditoria(string origen, string text);
    }
}
