using PointOfSale.Model;
using PointOfSale.Model.Auditoria;

namespace PointOfSale.Business.Contracts
{
    public interface IBackupService : IServiceBase<BackupProducto>
    {
        Task SaveBackup(string modificationUser, DateTime RegistrationDate, string? correlativeNumberMasivo, Product product);

        Task RestoreBackup(string modificationUser, int idBackupProduct, string? correlativeNumber);
    }
}
