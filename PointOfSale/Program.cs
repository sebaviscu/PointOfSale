using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Data.DBContext;
using PointOfSale.Data.Repository;
using PointOfSale.Utilities.Automapper;
using PointOfSale.Utilities.Extensions;
using System.Text.Json.Serialization;

public class Program
{
    public static void Main(string[] args)
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
                option.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });


        builder.Services.AddDbContext<POINTOFSALEContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("SQL_Publich"));
            //options.UseSqlServer(builder.Configuration.GetConnectionString("SQL"));
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
        builder.Services.AddScoped<IGastosService, GastosService>();
        builder.Services.AddScoped<ITicketService, TicketService>();
        builder.Services.AddScoped<IShopService, ShopService>();
        builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();

        //builder.Services.AddAFIPConfiguration(x =>
        //{
        //    x.CertificatePassword = "";
        //    x.Cuit = 0;
        //    x.IsProdEnvironment = false;
        //    x.Verbose = true;
        //    x.x509CertificateFilePath = "";
        //});


        //var context = new CustomAssemblyLoadContext();
        //context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "Utilities/LibraryPDF/libwkhtmltox.dll"));
        builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));


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
}
