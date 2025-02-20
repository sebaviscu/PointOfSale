using AFIP.Facturacion.DependencyInjection;
using AFIP.Facturacion.Services;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Externos.PrintServices;
using PointOfSale.Business.Services;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.DBContext;
using PointOfSale.Data.Repository;
using PointOfSale.Utilities.Automapper;
using System.Globalization;
using System.Text.Json.Serialization;
using Serilog;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Antiforgery;

public class Program
{
    public static void Main(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Delevopment";


        var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

        // Crear y verificar carpeta de logs
        string logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        if (!Directory.Exists(logsDirectory))
        {
            Directory.CreateDirectory(logsDirectory);
        }
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            var logDirectory = Path.Combine(builder.Environment.ContentRootPath, "logs");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory); // Crear la carpeta si no existe
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .Enrich.FromLogContext()
                .WriteTo.File(Path.Combine(logDirectory, "logfile.txt"),
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
                .CreateLogger();


            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "keys")))
                .SetApplicationName("PuntoDeVenta");

            builder.Host.UseSerilog();

            builder.Services.AddHttpClient();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers().AddJsonOptions(x =>
                            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            builder.Services.AddCors(_ =>
            {
                _.AddPolicy("NuevaPolitica", app =>
                {
                    app.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(option =>
                {
                    option.LoginPath = "/Access/Login"; // Ruta de inicio de sesión
                    option.SlidingExpiration = true; // Renueva la cookie en cada solicitud activa
                    option.ExpireTimeSpan = TimeSpan.FromMinutes(120);
                    option.Cookie.MaxAge = option.ExpireTimeSpan;
                    option.Cookie.HttpOnly = true; // Mejora de seguridad para evitar acceso desde JavaScript
                    option.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Solo se envía en conexiones HTTPS
                });

            builder.Services.AddDbContext<POINTOFSALEContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("SQL"), sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    );
                });
            });


            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<ISaleRepository, SaleRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRolService, RolService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ITypeDocumentSaleService, TypeDocumentSaleService>();
            builder.Services.AddScoped<ISaleService, SaleService>();
            builder.Services.AddScoped<IDashBoardService, DashBoardService>();
            builder.Services.AddScoped<IMenuService, MenuService>();
            builder.Services.AddScoped<ITiendaService, TiendaService>();
            builder.Services.AddScoped<ITurnoService, TurnoService>();
            builder.Services.AddScoped<IClienteService, ClienteService>();
            builder.Services.AddScoped<IProveedorService, ProveedorService>();
            builder.Services.AddScoped<IPromocionService, PromocionService>();
            builder.Services.AddScoped<IGastosService, GastosService>();
            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddScoped<IShopService, ShopService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IImportarExcelService, ImportarExcelService>();
            builder.Services.AddScoped<IPedidoService, PedidoService>();
            builder.Services.AddScoped<IIvaService, IvaService>();
            builder.Services.AddScoped<IAjusteService, AjusteService>();
            builder.Services.AddScoped<IAfipService, AfipService>();
            builder.Services.AddScoped<IFileStorageService, FileStorageService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IMovimientoCajaService, MovimientoCajaService>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<IFormatosVentaService, FormatosVentaService>();
            builder.Services.AddScoped<ILovService, LovService>();
            builder.Services.AddScoped<IPagoEmpresaService, PagoEmpresaService>();
            builder.Services.AddScoped<IBackupService, BackupService>();
            builder.Services.AddScoped<ICorrelativeNumberService, CorrelativeNumberService>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IPrintService, PrintService>();

            var cultureInfo = new CultureInfo("es-ES");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            builder.Services.AddAFIPConfiguration(x =>
            {
                x.IsProdEnvironment = false;
            });

            builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

            builder.Services.AddControllersWithViews().AddNewtonsoftJson(x =>
            {
                x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            builder.Services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "X-CSRF-TOKEN";
                options.HeaderName = "X-CSRF-TOKEN";
            });

            var rutaInicioWeb = builder.Configuration["RutaInicioWeb"];
            bool usarShopIndex = !string.IsNullOrEmpty(rutaInicioWeb) && bool.TryParse(rutaInicioWeb, out bool parsedValue) && parsedValue;

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors("NuevaPolitica");
            app.UseAuthentication();

            app.UseAuthorization();

            app.MapGet("/", async context =>
            {
                context.Response.Redirect(usarShopIndex ? "/Shop/Index" : "/Access/Login");
                await Task.CompletedTask;
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Access}/{action=Login}/{id?}");


            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Ocurrió un error en el servidor.");
                });
            });


            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Error crítico en la aplicación");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
