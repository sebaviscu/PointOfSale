using AFIP.Facturacion.DependencyInjection;
using AFIP.Facturacion.Services;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Embeddings;
using PointOfSale.Business.Plugins;
using PointOfSale.Business.SemanticKernel;
using PointOfSale.Business.Services;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.DBContext;
using PointOfSale.Data.Repository;
using PointOfSale.Utilities.Automapper;
using System.Globalization;
using System.Text.Json.Serialization;

public class Program
{
    public static void Main(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        try
        {

            var builder = WebApplication.CreateBuilder(args);

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

            var timeExpire = TimeHelper.GetArgentinaTime().AddHours(2);
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(option =>
                {
                    option.LoginPath = "/Access/Login"; // Ruta de inicio de sesión
                    option.ExpireTimeSpan = TimeSpan.FromHours(3); // Tiempo de inactividad permitido
                    option.SlidingExpiration = true; // Renueva la cookie en cada solicitud activa

                    // Configuración de persistencia de la cookie
                    option.Cookie.MaxAge = TimeSpan.FromDays(1); // Persistencia de la cookie por 1 día
                    option.Cookie.HttpOnly = true; // Mejora de seguridad para evitar acceso desde JavaScript
                    option.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Solo se envía en conexiones HTTPS
                });

            builder.Services.AddDbContext<POINTOFSALEContext>(options =>
            {
                //options.UseSqlServer(builder.Configuration.GetConnectionString("SQL"), sqlServerOptionsAction: sqlOptions =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SQL_Publich"), sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5, // Número máximo de reintentos
                        maxRetryDelay: TimeSpan.FromSeconds(10), // Tiempo máximo de espera entre reintentos
                        errorNumbersToAdd: null // Especifica números de error adicionales para los que se realizará un reintento
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
            builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
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
            builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
            builder.Services.AddScoped<ISemanticKernelService, SemanticKernelService>();


            builder.Services.AddScoped<SalesPlugin>();


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

            //builder.Host.UseSerilog();

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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Access}/{action=Login}/{id?}");

            app.Run();
        }
        catch (Exception ex)
        {
        }
    }
}
