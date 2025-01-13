using PointOfSale.Model.Jobs;
using PointOfSale.Business.Embeddings;
using PointOfSale.Business.Contracts;
using System.Text.Json;
using PointOfSale.Model.Embeddings;
using PointOfSale.Data.DBContext;
using PointOfSale.Model;
using Microsoft.EntityFrameworkCore;

namespace PointOfSale.Jobs.Executors
{
    public class GenerateEmbeddingsJobExecutor : IJobExecutor
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IUnitOfWork _unitOfWork;

        public GenerateEmbeddingsJobExecutor(IEmbeddingService embeddingService, IUnitOfWork unitOfWork)
        {
            _embeddingService = embeddingService;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(DeferredJob job)
        {
            Console.WriteLine("Iniciando generación de embeddings...");

            var textsToEmbed = new[] { "Producto A", "Producto B", "Descripción C" }; // Simulación de datos

            var listEmbedding = new List<Embedding>();

            var embJob = JsonSerializer.Deserialize<EmbeddingsJobExecutor>(job.TaskData);
            //     return JsonSerializer.Serialize(obj);



            // Sale
            var sales = await GetSalesByTiendaByCreationDate(embJob.IdTienda, embJob.CreationDate);

            foreach (var sale in sales)
            {
                var text = $"Venta #{sale.SaleNumber}, Usuario que la registró: {sale.RegistrationUser}, Cliente: {sale.ClientName ?? string.Empty}, Tipo Factura: {sale.TypeDocumentSaleNavigation.TipoFactura.ToString()}, " +
                           $"Total: ${sale.Total}, Fecha: {sale.RegistrationDate:dd/MM/yyyy HH:mm}, " +
                           $"Tienda: {sale.Tienda.Nombre}, Forma de pago: {sale.TypeDocumentSaleNavigation?.Description}, " +
                           $"Productos: {string.Join(", ", sale.DetailSales.Select(ds => $"{ds.DescriptionProduct} (Cantidad: {ds.Quantity}, Precio: ${ds.Price})"))}";


                var embedding = await _embeddingService.GenerateEmbedding(text);

                var embNew = _embeddingService.CreateEmbedding($"Sale_Id_{sale.IdSale}","", embedding);
                listEmbedding.Add(embNew);

                Console.WriteLine($"Embedding generado y almacenado para '{text}'.");
            }

            await _embeddingService.SaveRangeEmbeddingAsync(listEmbedding);

            Console.WriteLine("Generación de embeddings completada.");
        }

        private async Task<List<Sale>> GetSalesByTiendaByCreationDate(int idTienda, DateTime creationDate)
        {
            var saleRepository = _unitOfWork.Repository<Sale>();

            var query = saleRepository.QueryUnitOfWork(_ => _.IdTienda == idTienda && _.RegistrationDate >= creationDate)
                                      .Include(dv => dv.DetailSales)
                                      .Include(dv => dv.Tienda)
                                      .Include(_ => _.TypeDocumentSaleNavigation);

            return await query.ToListAsync();
        }
    }

    public class EmbeddingsJobExecutor
    {
        public int IdTienda { get; set; }
        public DateTime CreationDate { get; set; }
    }
}