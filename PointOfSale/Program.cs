using AFIP.Facturacion.DependencyInjection;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.DBContext;
using PointOfSale.Data.Repository;
using PointOfSale.Utilities.Automapper;
using System.Text.Json.Serialization;
using PointOfSale.Controllers;

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

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(option =>
                {
                    option.LoginPath = "/Access/Login";
                    option.ExpireTimeSpan = TimeSpan.FromHours(3);
                });

            builder.Services.AddDbContext<POINTOFSALEContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("SQL_Publich"));
            });

            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

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

            string certificatePath = @"C:\Users\sebastian.viscusso\Desktop\Seba\Certificados AFIP generados\certificado.pfx";

            builder.Services.AddAFIPConfiguration(x =>
            {
                x.CertificatePassword = "password";
                x.Cuit = 23365081999;
                x.IsProdEnvironment = false;
                x.Verbose = false;
                x.x509CertificateFilePath = certificatePath;
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
