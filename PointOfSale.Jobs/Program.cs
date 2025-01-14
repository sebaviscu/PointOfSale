using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Data.Repository;
using PointOfSale.Jobs.Services;
using PointOfSale.Jobs.Factories;
using PointOfSale.Data.DBContext;
using PointOfSale.Business.Embeddings;
using PointOfSale.Jobs.Executors;

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
            .AddSingleton<IConfiguration>(configuration) // Registrar IConfiguration
            .AddDbContext<POINTOFSALEContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SQL_Publich")))

            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>))

            .AddScoped<DeferredJobRepository>()
            .AddScoped<IJobExecutorFactory, JobExecutorFactory>()
            .AddScoped<IEmbeddingService, EmbeddingService>()
            .AddScoped<GenerateEmbeddingsJobExecutor>()

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
