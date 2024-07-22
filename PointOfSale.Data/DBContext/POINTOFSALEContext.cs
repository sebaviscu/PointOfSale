using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PointOfSale.Model;

namespace PointOfSale.Data.DBContext
{
    public partial class POINTOFSALEContext : DbContext
    {
        public POINTOFSALEContext()
        {
        }

        public POINTOFSALEContext(DbContextOptions<POINTOFSALEContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<CorrelativeNumber> CorrelativeNumbers { get; set; } = null!;
        public virtual DbSet<DetailSale> DetailSales { get; set; } = null!;
        public virtual DbSet<Menu> Menus { get; set; } = null!;
        public virtual DbSet<Tienda> Tienda { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<Rol> Rols { get; set; } = null!;
        public virtual DbSet<RolMenu> RolMenus { get; set; } = null!;
        public virtual DbSet<Sale> Sales { get; set; } = null!;
        public virtual DbSet<TypeDocumentSale> TypeDocumentSales { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Turno> Turno { get; set; } = null!;
        public virtual DbSet<Cliente> Clientes { get; set; } = null!;
        public virtual DbSet<ClienteMovimiento> ClienteMovimientos { get; set; } = null!;
        public virtual DbSet<Proveedor> Proveedor { get; set; } = null!;
        public virtual DbSet<ProveedorMovimiento> ProveedorMovimiento { get; set; } = null!;
        public virtual DbSet<Promocion> Promocion { get; set; } = null!;
        public virtual DbSet<Gastos> Gastos { get; set; } = null!;
        public virtual DbSet<TipoDeGasto> TipoDeGasto { get; set; } = null!;
        public virtual DbSet<VentaWeb> VentaWeb { get; set; } = null!;
        public virtual DbSet<AuditoriaModificaciones> AuditoriaModificaciones { get; set; } = null!;
        public virtual DbSet<Notifications> Notificaciones { get; set; } = null!;
        public virtual DbSet<ListaPrecio> ListaPrecios { get; set; } = null!;
        public virtual DbSet<Pedido> Pedido { get; set; } = null!;
        public virtual DbSet<PedidoProducto> PedidoProducto { get; set; } = null!;
        public virtual DbSet<Ajustes> Ajustes { get; set; } = null!;
        public virtual DbSet<Stock> Stocks { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Stock>(entity =>
            {
                entity.HasKey(e => e.IdStock);

                entity.ToTable("Stock");

                entity.HasOne(d => d.Tienda)
                    .WithMany(p => p.Stocks)
                    .HasForeignKey(d => d.IdTienda);

                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.Stocks)
                    .HasForeignKey(d => d.IdProducto);

            });
            modelBuilder.Entity<Ajustes>(entity =>
            {
                entity.HasKey(e => e.IdAjuste);

                entity.ToTable("Ajustes");

            });

            modelBuilder.Entity<PedidoProducto>(entity =>
            {
                entity.HasKey(e => e.IdPedidoProducto);

                entity.ToTable("PedidoProducto");

                entity.HasOne(d => d.Pedido)
                    .WithMany(p => p.Productos)
                    .HasForeignKey(d => d.IdPedido);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.PedidoProductos)
                    .HasForeignKey(d => d.IdProducto);
            });

            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(e => e.IdPedido);

                entity.ToTable("Pedidos");

                entity.HasOne(d => d.Proveedor)
                    .WithMany(p => p.Pedidos)
                    .HasForeignKey(d => d.IdProveedor);

                entity.HasOne(d => d.Tienda)
                    .WithMany(p => p.Pedidos)
                    .HasForeignKey(d => d.IdTienda);

                entity.HasOne(d => d.ProveedorMovimiento)
                    .WithOne(p => p.Pedido)
                    .HasForeignKey<Pedido>(d => d.IdProveedorMovimiento);

            });

            modelBuilder.Entity<Vencimiento>(entity =>
            {
                entity.HasKey(e => e.IdVencimiento);

                entity.ToTable("Vencimientos");

                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.Vencimientos)
                    .HasForeignKey(d => d.IdProducto);

                entity.HasOne(d => d.Tienda)
                    .WithMany(p => p.Vencimientos)
                    .HasForeignKey(d => d.IdTienda);
            });

            modelBuilder.Entity<ListaPrecio>(entity =>
            {
                entity.HasKey(e => e.IdListaPrecios);

                entity.ToTable("ListaPrecios");

                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.ListaPrecios)
                    .HasForeignKey(d => d.IdProducto);
            });

            modelBuilder.Entity<Notifications>(entity =>
            {
                entity.HasKey(e => e.IdNotifications);

                entity.ToTable("Notifications");
            });

            modelBuilder.Entity<AuditoriaModificaciones>(entity =>
            {
                entity.HasKey(e => e.IdAuditoriaModificaciones);

                entity.ToTable("AuditoriaModificaciones");
            });

            modelBuilder.Entity<VentaWeb>(entity =>
            {
                entity.HasKey(e => e.IdVentaWeb);

                entity.ToTable("VentaWeb");

                entity.Property(e => e.IdVentaWeb).HasColumnName("idVentaWeb");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Total)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("total");

                entity.HasOne(d => d.FormaDePago)
                    .WithMany(p => p.VentasWeb)
                    .HasForeignKey(d => d.IdFormaDePago);
            });

            modelBuilder.Entity<TipoDeGasto>(entity =>
            {
                entity.HasKey(e => e.IdTipoGastos);

                entity.ToTable("TipoGastos");

            });

            modelBuilder.Entity<Gastos>(entity =>
            {
                entity.HasKey(e => e.IdGastos);

                entity.ToTable("Gastos");

                entity.HasOne(d => d.TipoDeGasto)
                    .WithMany(p => p.Gastos)
                    .HasForeignKey(d => d.IdTipoGasto);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Gastos)
                    .HasForeignKey(d => d.IdUsuario);

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Promocion>(entity =>
            {
                entity.HasKey(e => e.IdPromocion);

                entity.ToTable("Promocion");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<ProveedorMovimiento>(entity =>
            {
                entity.HasKey(e => e.IdProveedorMovimiento);

                entity.ToTable("ProveedorMovimiento");

                entity.HasOne(d => d.Proveedor)
                    .WithMany(p => p.ProveedorMovimiento)
                    .HasForeignKey(d => d.IdProveedor);

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.HasKey(e => e.IdProveedor);

                entity.ToTable("Proveedor");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<ClienteMovimiento>(entity =>
            {
                entity.HasKey(e => e.IdClienteMovimiento);

                entity.ToTable("ClienteMovimiento");

                entity.HasOne(d => d.Cliente)
                    .WithMany(p => p.ClienteMovimientos)
                    .HasForeignKey(d => d.IdCliente);

                //entity.HasOne(d => d.Sale)
                //    .WithOne(p => p.ClienteMovimiento)
                //    .HasForeignKey<Sale>(c => c.IdSale);

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.IdCliente);

                entity.ToTable("Cliente");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Turno>(entity =>
            {
                entity.HasKey(e => e.IdTurno)
                    .HasName("PK__Turno__AA068B01D5580F03");

                entity.ToTable("Turno");

                entity.Property(e => e.IdTurno)
                    .HasColumnName("idTurno");

                entity.Property(e => e.IdTurno).HasColumnName("idTurno");

                entity.HasOne(d => d.Tienda)
                    .WithMany(p => p.Turnos)
                    .HasForeignKey(d => d.IdTienda)
                    .HasConstraintName("FK__Turno__idTurno__4F7CD00D");

            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.IdCategory)
                    .HasName("PK__Category__79D361B6930E16FF");

                entity.ToTable("Category");

                entity.Property(e => e.IdCategory).HasColumnName("idCategory");

                entity.Property(e => e.Description)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("description");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<CorrelativeNumber>(entity =>
            {
                entity.HasKey(e => e.IdCorrelativeNumber)
                    .HasName("PK__Correlat__D71CDFB02EFC51E4");

                entity.ToTable("CorrelativeNumber");

                entity.Property(e => e.IdCorrelativeNumber).HasColumnName("idCorrelativeNumber");

                entity.Property(e => e.DateUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("dateUpdate");

                entity.Property(e => e.LastNumber).HasColumnName("lastNumber");

                entity.Property(e => e.Management)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("management");

                entity.Property(e => e.QuantityDigits).HasColumnName("quantityDigits");
            });

            modelBuilder.Entity<DetailSale>(entity =>
            {
                entity.HasKey(e => e.IdDetailSale)
                    .HasName("PK__DetailSa__D072342E21B249E9");

                entity.ToTable("DetailSale");

                entity.Property(e => e.IdDetailSale).HasColumnName("idDetailSale");

                entity.Property(e => e.BrandProduct)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("brandProduct");

                entity.Property(e => e.CategoryProducty)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("categoryProducty");

                entity.Property(e => e.DescriptionProduct)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("descriptionProduct");

                entity.Property(e => e.IdProduct).HasColumnName("idProduct");

                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.DetalleVentas)
                    .HasForeignKey(d => d.IdProduct);

                entity.Property(e => e.IdSale).HasColumnName("idSale");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.Total)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("total");

                entity.HasOne(d => d.IdSaleNavigation)
                    .WithMany(p => p.DetailSales)
                    .HasForeignKey(d => d.IdSale)
                    .HasConstraintName("FK__DetailSal__idSal__300424B4");

                entity.HasOne(d => d.VentaWeb)
                    .WithMany(p => p.DetailSales)
                    .HasForeignKey(d => d.IdVentaWeb);
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.HasKey(e => e.IdMenu)
                    .HasName("PK__Menu__C26AF48328C80B96");

                entity.ToTable("Menu");

                entity.Property(e => e.IdMenu).HasColumnName("idMenu");

                entity.Property(e => e.Controller)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("controller");

                entity.Property(e => e.Description)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("description");

                entity.Property(e => e.Icon)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("icon");

                entity.Property(e => e.IdMenuParent).HasColumnName("idMenuParent");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.PageAction)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("pageAction");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.IdMenuParentNavigation)
                    .WithMany(p => p.InverseIdMenuParentNavigation)
                    .HasForeignKey(d => d.IdMenuParent)
                    .HasConstraintName("FK__Menu__idMenuPare__108B795B");
            });

            modelBuilder.Entity<Tienda>(entity =>
            {
                entity.HasKey(e => e.IdTienda)
                    .HasName("PK__Tienda__CF09B22C0A792CB4");

                entity.ToTable("Tienda");

                entity.Property(e => e.IdTienda)
                    .HasColumnName("idTienda");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("nombre");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.IdProduct)
                    .HasName("PK__Product__5EEC79D18F8E118B");

                entity.ToTable("Product");

                entity.Property(e => e.IdProduct).HasColumnName("idProduct");

                entity.Property(e => e.BarCode)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("barCode");

                entity.Property(e => e.Brand)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("brand");

                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("description");

                entity.Property(e => e.IdCategory).HasColumnName("idCategory");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.Photo).HasColumnName("photo");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModificationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("modificationDate");

				entity.Property(e => e.ModificationUser)
	                .HasColumnName("modificationUser");

				entity.HasOne(d => d.IdCategoryNavigation)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.IdCategory)
                    .HasConstraintName("FK__Product__idCateg__22AA2996");


                entity.HasOne(d => d.Proveedor)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.IdProveedor);
            });

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.HasKey(e => e.IdRol)
                    .HasName("PK__Rol__3C872F76804F2E15");

                entity.ToTable("Rol");

                entity.Property(e => e.IdRol).HasColumnName("idRol");

                entity.Property(e => e.Description)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("description");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<RolMenu>(entity =>
            {
                entity.HasKey(e => e.IdRolMenu)
                    .HasName("PK__RolMenu__CD2045D86DACA6AF");

                entity.ToTable("RolMenu");

                entity.Property(e => e.IdRolMenu).HasColumnName("idRolMenu");

                entity.Property(e => e.IdMenu).HasColumnName("idMenu");

                entity.Property(e => e.IdRol).HasColumnName("idRol");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.IdMenuNavigation)
                    .WithMany(p => p.RolMenus)
                    .HasForeignKey(d => d.IdMenu)
                    .HasConstraintName("FK__RolMenu__idMenu__182C9B23");

                entity.HasOne(d => d.IdRolNavigation)
                    .WithMany(p => p.RolMenus)
                    .HasForeignKey(d => d.IdRol)
                    .HasConstraintName("FK__RolMenu__idRol__173876EA");
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.IdSale)
                    .HasName("PK__Sale__C4AEB198091B7829");

                entity.ToTable("Sale");

                entity.Property(e => e.IdSale).HasColumnName("idSale");

                entity.Property(e => e.ClientName)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("clientName");

                entity.Property(e => e.CustomerDocument)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("customerDocument");

                entity.Property(e => e.IdTypeDocumentSale).HasColumnName("idTypeDocumentSale");

                entity.Property(e => e.IdUsers).HasColumnName("idUsers");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SaleNumber)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .HasColumnName("saleNumber");

                entity.Property(e => e.Total)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("total");


                entity.HasOne(d => d.TypeDocumentSaleNavigation)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.IdTypeDocumentSale)
                    .HasConstraintName("FK__Sale__idTypeDocu__2B3F6F97");

                entity.HasOne(d => d.IdUsersNavigation)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.IdUsers)
                    .HasConstraintName("FK__Sale__idUsers__2C3393D0");

                entity.HasOne(d => d.Turno)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.IdTurno)
                    .HasConstraintName("FK__Sale__idTurno__5CD6CB2B");

                entity.HasOne(d => d.ClienteMovimiento)
                    .WithOne(p => p.Sale)
                    .HasForeignKey<Sale>(c => c.IdClienteMovimiento);
            });

            modelBuilder.Entity<TypeDocumentSale>(entity =>
            {
                entity.HasKey(e => e.IdTypeDocumentSale)
                    .HasName("PK__TypeDocu__18211B893F81F3B8");

                entity.ToTable("TypeDocumentSale");

                entity.Property(e => e.IdTypeDocumentSale).HasColumnName("idTypeDocumentSale");

                entity.Property(e => e.Description)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("description");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.IdUsers)
                    .HasName("PK__Users__981CF2B10C1B1086");

                entity.Property(e => e.IdUsers).HasColumnName("idUsers");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.IdRol).HasColumnName("idRol");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("phone");

                entity.Property(e => e.Photo).HasColumnName("photo");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.IdRolNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.IdRol)
                    .HasConstraintName("FK__Users__idRol__1BFD2C07");

				entity.Property(e => e.IdTienda).HasColumnName("idTienda");

				entity.HasOne(d => d.Tienda)
                    .WithMany(p => p.Usuarios)
                    .HasForeignKey(d => d.IdTienda)
                    .HasConstraintName("FK__Users__idTienda__5812160E");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
