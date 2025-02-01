using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using PointOfSale.Model.Auditoria;

namespace PointOfSale.Business.Services;

public class BackupService : ServiceBase<BackupProducto>, IBackupService
{
    private readonly IGenericRepository<Product> _repositoryProduct;

    public BackupService(
        IGenericRepository<BackupProducto> repository,
        IGenericRepository<Product> repositoryProduct)
        : base(repository)
    {
        _repositoryProduct = repositoryProduct;
    }

    public async Task SaveBackup(string modificationUser, DateTime registrationDate, string? correlativeNumberMasivo, Product product)
    {

        var pbOldQuery = await _repository.Query();
        var pbOld = pbOldQuery.FirstOrDefault(_ => _.IdProduct == product.IdProduct);

        if (pbOld != null)
        {
            await _repository.Delete(pbOld);
        }

        var pb = new BackupProducto();
        pb.RegistrationUser = modificationUser;
        pb.RegistrationDate = registrationDate;

        if (correlativeNumberMasivo != null)
            pb.CorrelativeNumberMasivo = correlativeNumberMasivo;

        pb.IdProduct = product.IdProduct;
        pb.IdCategory = product.IdCategory;
        pb.Category = product.IdCategoryNavigation?.Description;
        pb.Description = product.Description;
        pb.Price = product.Price ?? 0;
        pb.CostPrice = product.CostPrice;
        pb.PriceWeb = product.PriceWeb;
        pb.PorcentajeProfit = product.PorcentajeProfit;
        pb.IsActive = product.IsActive;
        pb.Proveedor = product.Proveedor?.Nombre;
        pb.IdProveedor = product.IdProveedor;
        pb.TipoVenta = product.TipoVenta;
        pb.Comentario = product.Comentario;
        pb.Iva = product.Iva;
        pb.FormatoWeb = product.FormatoWeb;
        pb.PrecioFormatoWeb = product.PrecioFormatoWeb;
        pb.Destacado = product.Destacado;
        pb.ProductoWeb = product.ProductoWeb;
        pb.ModificarPrecio = product.ModificarPrecio;
        pb.PrecioAlMomento = product.PrecioAlMomento;
        pb.ExcluirPromociones = product.ExcluirPromociones;
        pb.SKU = product.SKU;

        if (product.ListaPrecios.Count == 3)
        {
            pb.Precio1 = product.ListaPrecios[0].Precio;
            pb.PorcentajeProfit1 = product.ListaPrecios[0].PorcentajeProfit;
            pb.Precio2 = product.ListaPrecios[1].Precio;
            pb.PorcentajeProfit2 = product.ListaPrecios[1].PorcentajeProfit;
            pb.Precio3 = product.ListaPrecios[2].Precio;
            pb.PorcentajeProfit3 = product.ListaPrecios[2].PorcentajeProfit;
        }
        await Add(pb);
    }


    public async Task RestoreBackup(string modificationUser, int idBackupProduct, string? correlativeNumber)
    {
        if (!string.IsNullOrEmpty(correlativeNumber))
        {
            var backups = await _repository.Query(_ => _.CorrelativeNumberMasivo == correlativeNumber);
            var productIds = backups.Select(dv => dv.IdProduct).Distinct().ToList();

            var productsQuery = await _repositoryProduct.Query();

            var products = await productsQuery.Include(p => p.ListaPrecios).Where(p => productIds.Contains(p.IdProduct))
                                       .ToListAsync();

            foreach (var b in backups)
            {
                var prod = products.First(_ => _.IdProduct == b.IdProduct);

                await RestoreProduct(modificationUser, prod, b);
            }
        }
        else
        {
            var backup = await GetById(idBackupProduct);
            var prodQuery = await _repositoryProduct.Query(p => p.IdProduct == backup.IdProduct);

            var product = await prodQuery.Include(p => p.ListaPrecios).FirstOrDefaultAsync();

            await RestoreProduct(modificationUser, product, backup);
        }

    }

    private async Task RestoreProduct(string modificationUser, Product product, BackupProducto backup)
    {
        await SaveBackup(modificationUser, TimeHelper.GetArgentinaTime(), null, product);

        product.Description = backup.Description;
        product.IdCategory = backup?.IdCategory;
        product.Price = backup.Price;
        product.CostPrice = backup.CostPrice;
        product.PriceWeb = backup.PriceWeb;
        product.PorcentajeProfit = backup.PorcentajeProfit;
        product.IsActive = backup.IsActive;
        product.IdProveedor = backup.IdProveedor;
        product.TipoVenta = backup.TipoVenta;
        product.Comentario = backup.Comentario;
        product.Iva = backup.Iva;
        product.FormatoWeb = backup.FormatoWeb;
        product.PrecioFormatoWeb = backup.PrecioFormatoWeb;
        product.Destacado = backup.Destacado;
        product.ProductoWeb = backup.ProductoWeb;
        product.ModificarPrecio = backup.ModificarPrecio;
        product.PrecioAlMomento = backup.PrecioAlMomento;
        product.ExcluirPromociones = backup.ExcluirPromociones;
        product.SKU = backup.SKU;

        if (product.ListaPrecios != null && product.ListaPrecios.Count == 3)
        {
            product.ListaPrecios[0].Precio = backup.Precio1;
            if (backup.PorcentajeProfit1.HasValue)
                product.ListaPrecios[0].PorcentajeProfit = backup.PorcentajeProfit1.Value;

            product.ListaPrecios[1].Precio = backup.Precio2;
            if (backup.PorcentajeProfit2.HasValue)
                product.ListaPrecios[1].PorcentajeProfit = backup.PorcentajeProfit2.Value;

            product.ListaPrecios[2].Precio = backup.Precio3;
            if (backup.PorcentajeProfit3.HasValue)
                product.ListaPrecios[2].PorcentajeProfit = backup.PorcentajeProfit3.Value;
        }

        await _repositoryProduct.Edit(product);
    }

}

