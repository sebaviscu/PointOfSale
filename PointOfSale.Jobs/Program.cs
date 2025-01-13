using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Data;
using PointOfSale.Data.Repository;
using PointOfSale.Jobs.Services;
using PointOfSale.Jobs.Factories;
using System;
using System.Threading.Tasks;
using PointOfSale.Data.DBContext;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Business.Embeddings;
using AFIP.Facturacion.Services;
using PointOfSale.Business.Utilities;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Configurar el archivo de configuración (appsettings.json)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Configurar el contenedor de dependencias
        var serviceProvider = new ServiceCollection()
            .AddDbContext<POINTOFSALEContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SQL_Publich")))

            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>))

            .AddScoped<DeferredJobRepository>()
            .AddScoped<IJobExecutorFactory, JobExecutorFactory>()
            .AddScoped<IEmbeddingService, EmbeddingService>()

            .AddScoped<DeferredJobService>()
            .BuildServiceProvider();

        // Obtener el servicio DeferredJobService del contenedor
        var jobService = serviceProvider.GetService<DeferredJobService>();

        if (jobService != null)
        {
            while (true) // Bucle infinito para ejecutar periódicamente los jobs
            {
                Console.WriteLine($"Iniciando procesamiento de jobs a las {DateTime.Now}...");
                await jobService.ProcessJobsAsync();
                Console.WriteLine($"Finalizó el procesamiento de jobs a las {DateTime.Now}.");

                // Esperar 30 minutos antes de la próxima ejecución
                await Task.Delay(TimeSpan.FromMinutes(30));
            }
        }
        else
        {
            Console.WriteLine("No se pudo iniciar el servicio de jobs.");
        }
    }
}
