using PointOfSale.Models;
using System.Globalization;
using PointOfSale.Model;
using AutoMapper;
using static PointOfSale.Model.Enum;
using Newtonsoft.Json.Linq;
using PointOfSale.Model.Output;
using System.Security.Cryptography.X509Certificates;
using PointOfSale.Model.Afip.Factura;

namespace PointOfSale.Utilities.Automapper
{
    public class AutoMapperProfile : Profile
    {
        public DateTime DateTimeNowArg = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));

        public AutoMapperProfile()
        {

            #region Rol
            CreateMap<Rol, VMRol>().ReverseMap();
            #endregion

            #region User
            CreateMap<User, VMUser>()
            .ForMember(destiny =>
                destiny.IsActive,
                opt => opt.MapFrom(source => source.IsActive == true ? 1 : 0)
            )
            .ForMember(destiny =>
                destiny.NameRol,
                opt => opt.MapFrom(source => source.IdRolNavigation.Description)
            ).ForMember(destiny =>
                destiny.PhotoBase64,
                opt => opt.MapFrom(source => Convert.ToBase64String(source.Photo))
            )
            .ForMember(destiny =>
                destiny.Photo,
                opt => opt.Ignore()
            )
            .ForMember(destiny =>
                destiny.IdTienda,
                opt => opt.MapFrom(source => source.IdTienda)
            )
            .ForMember(destiny =>
                destiny.TiendaName,
                opt => opt.MapFrom(source => source.Tienda.Nombre)
                );

            CreateMap<VMUser, User>()
            .ForMember(destiny =>
                destiny.IsActive,
                opt => opt.MapFrom(source => source.IsActive == 1 ? true : false)
            )
            .ForMember(destiny =>
                destiny.IdTienda,
                opt => opt.MapFrom(source => source.IdTienda)
            )
            .ForMember(destiny =>
                destiny.IdRolNavigation,
                opt => opt.Ignore()
            );
            #endregion

            #region Category
            CreateMap<Category, VMCategory>()
            .ForMember(destiny =>
                destiny.IsActive,
                opt => opt.MapFrom(source => source.IsActive == true ? 1 : 0)
            );

            CreateMap<VMCategory, Category>()
            .ForMember(destiny =>
                destiny.IsActive,
                opt => opt.MapFrom(source => source.IsActive == 1 ? true : false)
            );

            #endregion

            #region Product
            CreateMap<Product, VMProduct>()
            .ForMember(destiny =>
                destiny.IsActive,
                opt => opt.MapFrom(source => source.IsActive == true ? 1 : 0)
            )
            .ForMember(destiny =>
                destiny.NameCategory,
                opt => opt.MapFrom(source => source.IdCategoryNavigation.Description)
            )
            .ForMember(destiny =>
                destiny.NameProveedor,
                opt => opt.MapFrom(source => source.Proveedor.Nombre)
            )
            .ForMember(destiny =>
                destiny.PriceString,
                opt => opt.MapFrom(source => "$" + source.Price)
            )
            .ForMember(destiny =>
                destiny.PhotoBase64,
                opt => opt.MapFrom(source => Convert.ToBase64String(source.Photo))
            )
            .ForMember(dest => dest.Photo, opt => opt.Ignore())
            .ForMember(user => user.ModificationDateString, opt => opt.MapFrom(userEdit => userEdit.ModificationDate.HasValue ? userEdit.ModificationDate.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty))
            .ForMember(destiny =>
                destiny.Price,
                opt => opt.MapFrom(source => source.ListaPrecios.Any() && source.ListaPrecios.Count > 0 ? source.ListaPrecios[0].Precio.ToString() : "0"))
            .ForMember(destiny =>
                destiny.PorcentajeProfit,
                opt => opt.MapFrom(source => source.ListaPrecios.Any() && source.ListaPrecios.Count > 0 ? source.ListaPrecios[0].PorcentajeProfit : 0))
            .ForMember(destiny =>
                destiny.Precio2,
                opt => opt.MapFrom(source => source.ListaPrecios.Any() && source.ListaPrecios.Count > 1 ? source.ListaPrecios[1].Precio.ToString() : "0"))
            .ForMember(destiny =>
                destiny.PorcentajeProfit2,
                opt => opt.MapFrom(source => source.ListaPrecios.Any() && source.ListaPrecios.Count > 1 ? source.ListaPrecios[1].PorcentajeProfit : 0))
            .ForMember(destiny =>
                destiny.Precio3,
                opt => opt.MapFrom(source => source.ListaPrecios.Any() && source.ListaPrecios.Count > 1 ? source.ListaPrecios[2].Precio.ToString() : "0"))
            .ForMember(destiny =>
                destiny.PorcentajeProfit3,
                opt => opt.MapFrom(source => source.ListaPrecios.Any() && source.ListaPrecios.Count > 1 ? source.ListaPrecios[2].PorcentajeProfit : 0))
            .ForMember(dest => dest.IdCategoryNavigation, opt => opt.MapFrom(src => src.IdCategoryNavigation))
            .ForMember(dest => dest.Proveedor, opt => opt.MapFrom(src => src.Proveedor))
            .ForMember(dest => dest.ListaPrecios, opt => opt.MapFrom(src => src.ListaPrecios))
            .ForMember(dest => dest.Vencimientos, opt => opt.MapFrom(src => src.Vencimientos))
            .ForMember(dest => dest.PriceWeb, opt => opt.MapFrom(src => src.PriceWeb.ToString()))
            .ForMember(dest => dest.PrecioFormatoWeb, opt => opt.MapFrom(src => src.PrecioFormatoWeb.ToString()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.ProductTags.Select(pt => pt.Tag).ToList()))
            ;

            CreateMap<VMProduct, Product>()
            .ForMember(destiny =>
                destiny.IsActive,
                opt => opt.MapFrom(source => source.IsActive == 1 ? true : false)
            )
            .ForMember(destiny =>
                destiny.IdCategoryNavigation,
                opt => opt.Ignore()
            )
            .ForMember(destiny =>
                destiny.Proveedor,
                opt => opt.Ignore()
            )
            .ForMember(destiono =>
                destiono.Price,
                opt => opt.MapFrom(source => Convert.ToDecimal(source.Price))
            )
            .ForMember(dest => dest.PriceWeb, opt => opt.MapFrom(src => Convert.ToDecimal(src.PriceWeb)))
            .ForMember(dest => dest.PrecioFormatoWeb, opt => opt.MapFrom(src => Convert.ToDecimal(src.PrecioFormatoWeb)))
            .ForMember(dest => dest.ProductTags, opt => opt.Ignore());


            CreateMap<ListaPrecio, VmProductsSelect2>()
            .ForMember(destiny => destiny.IdProduct, opt => opt.MapFrom(source => source.Producto != null ? source.Producto.IdProduct : 0))
            .ForMember(destiny => destiny.Description, opt => opt.MapFrom(source => source.Producto != null ? source.Producto.Description : string.Empty))
            .ForMember(destiny => destiny.IdCategory, opt => opt.MapFrom(source => source.Producto != null ? source.Producto.IdCategory : 0))
            .ForMember(destiny => destiny.PhotoBase64, opt => opt.MapFrom(source => source.Producto != null ? Convert.ToBase64String(ImageHelper.ResizeImage(source.Producto.Photo, 60, 60)) : string.Empty))
            .ForMember(destiny => destiny.TipoVenta, opt => opt.MapFrom(source => source.Producto != null ? source.Producto.TipoVenta : 0))
            .ForMember(destiny => destiny.Price, opt => opt.MapFrom(source => source.Precio))
            .ForMember(destiny => destiny.Iva, opt => opt.MapFrom(source => source.Producto != null ? source.Producto.Iva : 21m))
            .ForMember(destiny => destiny.CategoryProducty, opt => opt.MapFrom(source => source.Producto != null && source.Producto.IdCategoryNavigation != null ? source.Producto.IdCategoryNavigation.Description : string.Empty))
            ;


            CreateMap<ListaPrecio, VMProduct>()
            .ForMember(destiny => destiny.IdProduct, opt => opt.MapFrom(source => source.Producto != null ? source.Producto.IdProduct : 0))
            .ForMember(destiny => destiny.Description, opt => opt.MapFrom(source => source.Producto != null ? source.Producto.Description : string.Empty))
            .ForMember(destiny => destiny.IdCategory, opt => opt.MapFrom(source => source.Producto != null ? source.Producto.IdCategory : 0))
            .ForMember(destiny => destiny.PhotoBase64, opt => opt.MapFrom(source => source.Producto != null ? Convert.ToBase64String(source.Producto.Photo) : string.Empty))
            .ForMember(destiny => destiny.TipoVenta, opt => opt.MapFrom(source => source.Producto != null ? source.Producto.TipoVenta : 0))
            .ForMember(destiny => destiny.Price, opt => opt.MapFrom(source => source.Precio))
            ;

            #endregion
            //System.Enum.GetName(typeof(TipoVenta), source.TipoVenta)
            #region TypeDocumentSale
            CreateMap<TypeDocumentSale, VMTypeDocumentSale>()
                .ForMember(destiny =>
                    destiny.TipoFacturaString,
                    opt => opt.MapFrom(source => System.Enum.GetName(typeof(TipoFactura), source.TipoFactura))
                );
            CreateMap<VMTypeDocumentSale, TypeDocumentSale>();

            #endregion

            #region Sale
            CreateMap<Sale, VMSale>()
                .ForMember(destiny =>
                    destiny.TypeDocumentSale,
                    opt => opt.MapFrom(source => source.TypeDocumentSaleNavigation != null ? source.TypeDocumentSaleNavigation.Description : string.Empty)
                )
                .ForMember(destiny =>
                    destiny.Users,
                    opt => opt.MapFrom(source => source.IdUsersNavigation.Name)
                )
                .ForMember(destiny =>
                    destiny.Total,
                    opt => opt.MapFrom(source => source.Total.Value)
                ).ForMember(destiny =>
                    destiny.TotalDecimal,
                    opt => opt.MapFrom(source => source.Total)
                ).ForMember(destiny =>
                    destiny.RegistrationDate,
                    opt => opt.MapFrom(source => source.RegistrationDate.Value.ToString("dd/MM/yyyy h:mm tt"))
                )
                .ForMember(destiny =>
                    destiny.CantidadProductos,
                    opt => opt.MapFrom(source => source.DetailSales != null ? source.DetailSales.Count : 0))
                .ForMember(destiny =>
                    destiny.DescuentoRecargo,
                    opt => opt.MapFrom(source => source.DescuentoRecargo != null ? source.DescuentoRecargo.ToString() : ""));

            CreateMap<VMSale, Sale>()
                .ForMember(destiny =>
                    destiny.Total,
                    opt => opt.MapFrom(source => source.Total)

                )
                .ForMember(destiny =>
                    destiny.DescuentoRecargo,
                    opt => opt.MapFrom(source => !string.IsNullOrEmpty(source.DescuentoRecargo) ? Convert.ToDecimal(source.DescuentoRecargo) : 0));
            #endregion

            #region DetailSale
            CreateMap<DetailSale, VMDetailSale>()
                .ForMember(destiny =>
                    destiny.Price,
                    opt => opt.MapFrom(source => source.Price.Value)
                )
                .ForMember(destiny =>
                    destiny.Total,
                    opt => opt.MapFrom(source => source.Total.Value)
                );

            CreateMap<VMDetailSale, DetailSale>()
                .ForMember(destiny =>
                    destiny.Price,
                    opt => opt.MapFrom(source => source.Price)
                )
                .ForMember(destiny =>
                    destiny.Total,
                    opt => opt.MapFrom(source => source.Total)
                );

            #endregion

            #region Menu
            CreateMap<Menu, VMMenu>()
                .ForMember(destiny =>
                destiny.SubMenus,
                opt => opt.MapFrom(source => source.InverseIdMenuParentNavigation));
            #endregion Menu

            CreateMap<Tienda, VMTienda>()
                .ForMember(destiny => destiny.PhotoBase64, opt => opt.MapFrom(source => Convert.ToBase64String(source.Logo)));

            CreateMap<VMTienda, Tienda>();


            CreateMap<Turno, VMTurno>()
                .ForMember(user => user.Fecha, opt => opt.MapFrom(userEdit => userEdit.FechaInicio.ToShortDateString()))
                .ForMember(user => user.HoraInicio, opt => opt.MapFrom(userEdit => userEdit.FechaInicio.ToShortTimeString()))
                .ForMember(user => user.HoraFin, opt => opt.MapFrom(userEdit => userEdit.FechaFin.HasValue ? userEdit.FechaFin.Value.ToShortTimeString() : string.Empty))
                .ForMember(user => user.Total, opt => opt.MapFrom(userEdit => userEdit.Sales.Any() ? "$" + userEdit.Sales.Where(s => s.IdClienteMovimiento == null).Sum(_ => _.Total).Value.ToString("F2") : string.Empty));

            CreateMap<VMTurno, Turno>();


            CreateMap<Cliente, VMCliente>()
                    .ForMember(user => user.TotalDecimal, opt => opt.MapFrom(userEdit => userEdit.ClienteMovimientos != null ? userEdit.ClienteMovimientos.Where(_ => _.TipoMovimiento == TipoMovimientoCliente.Pagos).Sum(_ => _.Total) - userEdit.ClienteMovimientos.Where(_ => _.TipoMovimiento == TipoMovimientoCliente.Gastos).Sum(_ => _.Total) : 0))
                    .ForMember(user => user.Total, opt => opt.MapFrom(userEdit => userEdit.ClienteMovimientos != null ? "$" + (userEdit.ClienteMovimientos.Where(_ => _.TipoMovimiento == TipoMovimientoCliente.Pagos).Sum(_ => _.Total) - userEdit.ClienteMovimientos.Where(_ => _.TipoMovimiento == TipoMovimientoCliente.Gastos).Sum(_ => _.Total)).ToString("F2") : string.Empty));

            CreateMap<VMCliente, Cliente>();


            CreateMap<ClienteMovimiento, VMClienteMovimiento>()
                .ForMember(user => user.TotalString, opt => opt.MapFrom(userEdit => "$" + userEdit.Total.ToString("F2")))
                .ForMember(user => user.RegistrationDateString, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.ToString("dd/MM/yyyy HH:mm")));

            CreateMap<VMClienteMovimiento, ClienteMovimiento>();


            CreateMap<ProveedorMovimiento, VMMovimientoProveedoresTablaDinamica>()
                .ForMember(user => user.Importe, opt => opt.MapFrom(userEdit => userEdit.Importe))
                .ForMember(user => user.Iva, opt => opt.MapFrom(userEdit => userEdit.Iva != null ? userEdit.Iva : 0))
                .ForMember(user => user.Importe_Sin_Iva, opt => opt.MapFrom(userEdit => userEdit.ImporteSinIva != null ? userEdit.ImporteSinIva : 0))
                .ForMember(user => user.Iva_Importe, opt => opt.MapFrom(userEdit => userEdit.IvaImporte != null ? userEdit.IvaImporte : 0))
                .ForMember(user => user.Tipo_Factura, opt => opt.MapFrom(userEdit => userEdit.TipoFactura == string.Empty ? "-" : ((Model.Enum.TipoFactura)Convert.ToInt32(userEdit.TipoFactura)).ToString()))
                .ForMember(user => user.Fecha, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate))
                .ForMember(user => user.Nombre_Proveedor, opt => opt.MapFrom(userEdit => userEdit.Proveedor.Nombre))
                .ForMember(user => user.Cuil, opt => opt.MapFrom(userEdit => userEdit.Proveedor.Cuil))
                .ForMember(user => user.Direccion, opt => opt.MapFrom(userEdit => userEdit.Proveedor.Direccion))
                .ForMember(user => user.Telefono, opt => opt.MapFrom(userEdit => userEdit.Proveedor.Telefono))
                .ForMember(user => user.Nro_Factura, opt => opt.MapFrom(userEdit => userEdit.NroFactura == string.Empty ? "-" : userEdit.NroFactura));

            CreateMap<Proveedor, VMProveedor>();

            CreateMap<VMProveedor, Proveedor>();


            CreateMap<Promocion, VMPromocion>()
                .ForMember(user => user.IdCategory, opt => opt.MapFrom(userEdit => userEdit.IdCategory.Split(",", System.StringSplitOptions.None)))
                .ForMember(user => user.Dias, opt => opt.MapFrom(userEdit => userEdit.Dias.Split(",", System.StringSplitOptions.None)));


            CreateMap<VMPromocion, Promocion>()
                .ForMember(userEdit => userEdit.IdCategory, opt => opt.MapFrom(user => string.Join(", ", user.IdCategory)))
                .ForMember(userEdit => userEdit.Dias, opt => opt.MapFrom(user => string.Join(", ", user.Dias)));


            CreateMap<ProveedorMovimiento, VMProveedorMovimiento>()
                .ForMember(user => user.ImporteString, opt => opt.MapFrom(userEdit => "$" + userEdit.Importe.ToString("F2")))
                .ForMember(user => user.ImporteSinIvaString, opt => opt.MapFrom(userEdit => "$" + userEdit.ImporteSinIva.ToString()))
                .ForMember(user => user.IvaImporteString, opt => opt.MapFrom(userEdit => "$" + userEdit.IvaImporte.ToString()))
                .ForMember(user => user.NombreProveedor, opt => opt.MapFrom(userEdit => userEdit.Proveedor != null ? userEdit.Proveedor.Nombre : ""))
                .ForMember(user => user.RegistrationDateString, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.ToString("dd/MM/yyyy HH:mm")));

            CreateMap<VMProveedorMovimiento, ProveedorMovimiento>();


            CreateMap<Gastos, VMGastos>()
                .ForMember(user => user.Comentario, opt => opt.MapFrom(userEdit => userEdit.Comentario != string.Empty ? userEdit.Comentario : userEdit.IdUsuario != null ? userEdit.User.Name : string.Empty))
                .ForMember(user => user.TipoGastoString, opt => opt.MapFrom(userEdit => userEdit.TipoDeGasto != null ? userEdit.TipoDeGasto.Descripcion : string.Empty))
                .ForMember(user => user.GastoParticular, opt => opt.MapFrom(userEdit => userEdit.TipoDeGasto != null ? userEdit.TipoDeGasto.GastoParticular.ToString() : string.Empty))
                .ForMember(user => user.FacturaCompleta, opt => opt.MapFrom(userEdit => userEdit.NroFactura))
                .ForMember(user => user.ImporteString, opt => opt.MapFrom(userEdit => "$" + userEdit.Importe.ToString("F2")))
                .ForMember(user => user.FechaString, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.ToString("dd/MM/yyyy HH:mm")));


            CreateMap<Gastos, VMGastosTablaDinamica>()
                .ForMember(user => user.Importe_Sin_Iva, opt => opt.MapFrom(userEdit => userEdit.ImporteSinIva != null ? userEdit.ImporteSinIva : 0))
                .ForMember(user => user.Iva_Importe, opt => opt.MapFrom(userEdit => userEdit.IvaImporte != null ? userEdit.IvaImporte : 0))
                .ForMember(user => user.Iva, opt => opt.MapFrom(userEdit => userEdit.Iva != null ? userEdit.Iva : 0))
                .ForMember(user => user.Comentario, opt => opt.MapFrom(userEdit => userEdit.Comentario != string.Empty ? userEdit.Comentario : userEdit.IdUsuario != null ? userEdit.User.Name : string.Empty))
                .ForMember(user => user.Gasto, opt => opt.MapFrom(userEdit => userEdit.TipoDeGasto != null ? userEdit.TipoDeGasto.GastoParticular.ToString() : string.Empty))
                .ForMember(user => user.Fecha, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate))
                .ForMember(user => user.Tipo_Gasto, opt => opt.MapFrom(userEdit => userEdit.TipoDeGasto.GastoParticular == TipoDeGastoEnum.Sueldos ? (userEdit.User != null ? userEdit.User.Name : string.Empty) : userEdit.TipoDeGasto.Descripcion.ToString()))
                .ForMember(user => user.Tipo_Factura, opt => opt.MapFrom(userEdit => string.IsNullOrEmpty(userEdit.TipoFactura) ? "-" : ((Model.Enum.TipoFactura)Convert.ToInt32(userEdit.TipoFactura)).ToString()))
                .ForMember(user => user.Nro_Factura, opt => opt.MapFrom(userEdit => userEdit.NroFactura == string.Empty ? "-" : userEdit.NroFactura));

            CreateMap<VMGastos, Gastos>();

            CreateMap<TipoDeGasto, VMTipoDeGasto>();
            CreateMap<VMTipoDeGasto, TipoDeGasto>();

            CreateMap<VMVentaWeb, VentaWeb>();
            CreateMap<VentaWeb, VMVentaWeb>()
                .ForMember(user => user.Fecha, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.Value.ToString("dd/MM/yyyy HH:mm")))
                .ForMember(user => user.TotalString, opt => opt.MapFrom(userEdit => "$" + userEdit.Total.Value.ToString("F2")))
                .ForMember(user => user.FormaDePago, opt => opt.MapFrom(userEdit => userEdit.FormaDePago != null ? userEdit.FormaDePago.Description : string.Empty));


            CreateMap<Notifications, VMNotifications>()
                .ForMember(user => user.RegistrationDateString, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.ToString("dd/MM/yyyy HH:mm")))
                .ForMember(user => user.ModificationDateString, opt => opt.MapFrom(userEdit => userEdit.ModificationDate.HasValue ? userEdit.ModificationDate.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty));
            CreateMap<VMNotifications, Notifications>();

            CreateMap<Vencimiento, VMVencimiento>()
                .ForMember(user => user.Estado, opt => opt.MapFrom(userEdit =>
                userEdit.FechaVencimiento.Date <= DateTimeNowArg.Date ?
                    EstadoVencimiento.Vencido :
                userEdit.FechaVencimiento.Date > DateTime.UtcNow.Date && userEdit.FechaVencimiento.Date < DateTimeNowArg.AddDays(7).Date ?
                    EstadoVencimiento.ProximoVencimiento :
                    EstadoVencimiento.Apto))
                .ForMember(user => user.Producto, opt => opt.MapFrom(userEdit => userEdit.Producto != null ? userEdit.Producto.Description : string.Empty))
                .ForMember(user => user.FechaVencimientoString, opt => opt.MapFrom(userEdit => userEdit.FechaVencimiento.ToString("dd/MM/yyyy")))
                .ForMember(user => user.FechaElaboracionString, opt => opt.MapFrom(userEdit => userEdit.FechaElaboracion.HasValue ? userEdit.FechaElaboracion.Value.ToString("dd/MM/yyyy") : string.Empty));
            CreateMap<VMVencimiento, Vencimiento>();

            CreateMap<VMPedido, Pedido>();
            CreateMap<Pedido, VMPedido>()
                                .ForMember(user => user.RegistrationDateString, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.ToString("dd/MM/yyyy HH:mm")))
                                .ForMember(user => user.FechaCerradoString, opt => opt.MapFrom(userEdit => userEdit.FechaCerrado.HasValue ? userEdit.FechaCerrado.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty))
                                .ForMember(user => user.ImporteEstimadoString, opt => opt.MapFrom(userEdit => !userEdit.ImporteFinal.HasValue || userEdit.ImporteFinal.Value == 0 ? "$" + userEdit.ImporteEstimado.Value.ToString("F2") + " * " : "$" + userEdit.ImporteFinal.Value.ToString("F2")));

            CreateMap<VMPedidoProducto, PedidoProducto>();
            CreateMap<PedidoProducto, VMPedidoProducto>();

            CreateMap<Product, VMPedidoProducto>()
                                .ForMember(user => user.IdProducto, opt => opt.MapFrom(userEdit => userEdit.IdProduct));

            CreateMap<VMPedidoProducto, Product>()
                                .ForMember(user => user.IdProduct, opt => opt.MapFrom(userEdit => userEdit.IdProducto));

            CreateMap<Proveedor, VMPedidosProveedor>();
            CreateMap<VMPedidosProveedor, Proveedor>();

            CreateMap<ProveedorMovimiento, VMIvaRowOutput>()
                                .ForMember(user => user.Fecha, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate))
                                .ForMember(user => user.FechaString, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.ToString("dd/MM/yyyy HH:mm")))
                                .ForMember(user => user.Proveedor, opt => opt.MapFrom(userEdit => userEdit.Proveedor != null ? userEdit.Proveedor.Nombre : string.Empty))
                                .ForMember(user => user.Importe, opt => opt.MapFrom(userEdit => userEdit.Importe))
                                .ForMember(user => user.ImporteIva, opt => opt.MapFrom(userEdit => userEdit.IvaImporte))
                                .ForMember(user => user.ImporteSinIva, opt => opt.MapFrom(userEdit => userEdit.ImporteSinIva))
                                .ForMember(user => user.Factura, opt => opt.MapFrom(userEdit => userEdit.NroFactura))
                                .ForMember(user => user.TipoFactura, opt => opt.MapFrom(userEdit => string.IsNullOrEmpty(userEdit.TipoFactura) ? "-" : ((Model.Enum.TipoFactura)Convert.ToInt32(userEdit.TipoFactura)).ToString()));


            CreateMap<Gastos, VMIvaRowOutput>()
                                .ForMember(user => user.Fecha, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate))
                                .ForMember(user => user.FechaString, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.ToString("dd/MM/yyyy HH:mm")))
                                .ForMember(user => user.Importe, opt => opt.MapFrom(userEdit => userEdit.Importe))
                                .ForMember(user => user.ImporteIva, opt => opt.MapFrom(userEdit => userEdit.IvaImporte))
                                .ForMember(user => user.ImporteSinIva, opt => opt.MapFrom(userEdit => userEdit.ImporteSinIva))
                                .ForMember(user => user.TipoGastos, opt => opt.MapFrom(userEdit => ((Model.Enum.TipoDeGastoEnum)Convert.ToInt32(userEdit.TipoDeGasto.GastoParticular)).ToString()))
                                .ForMember(user => user.Gastos, opt => opt.MapFrom(userEdit => userEdit.TipoDeGasto.Descripcion))
                                .ForMember(user => user.TipoFactura, opt => opt.MapFrom(userEdit => string.IsNullOrEmpty(userEdit.TipoFactura) ? "-" : ((Model.Enum.TipoFactura)Convert.ToInt32(userEdit.TipoFactura)).ToString()));


            CreateMap<Sale, VMIvaRowOutput>()
                                .ForMember(user => user.Fecha, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate))
                                .ForMember(user => user.FechaString, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.Value.ToString("dd/MM/yyyy HH:mm")))
                                .ForMember(user => user.MetodoPago, opt => opt.MapFrom(userEdit => userEdit.TypeDocumentSaleNavigation.Description))
                                .ForMember(user => user.Importe, opt => opt.MapFrom(userEdit => userEdit.Total))
                                .ForMember(user => user.ImporteIva, opt => opt.MapFrom(userEdit => Math.Round(userEdit.Total.Value / Convert.ToDecimal("1.21"), 2)))
                                .ForMember(user => user.ImporteSinIva, opt => opt.MapFrom(userEdit => userEdit.Total - Math.Round(userEdit.Total.Value / Convert.ToDecimal("1.21"), 2)));


            CreateMap<Ajustes, VMAjustes>().ReverseMap();
            CreateMap<AjustesWeb, VMAjustesWeb>().ReverseMap();
            CreateMap<AjustesFacturacion, VMAjustesFacturacion>().ReverseMap();
            CreateMap<Ajustes, VMAjustesSale>().ReverseMap();

            CreateMap<Stock, VMStock>().ReverseMap();
            CreateMap<ListaPrecio, VMListaPrecio>().ReverseMap();

            CreateMap<Stock, VMStockSimplificado>().ReverseMap();
            CreateMap<Proveedor, VMProveedorSimplificado>().ReverseMap();


            CreateMap<Product, VMProductSimplificado>()
            .ForMember(destiny =>
                destiny.Price,
                opt => opt.MapFrom(source => source.ListaPrecios.Any() && source.ListaPrecios.Count > 0 ? source.ListaPrecios[0].Precio.ToString() : "0"))
            .ForMember(destiny =>
                destiny.PorcentajeProfit,
                opt => opt.MapFrom(source => source.ListaPrecios.Any() && source.ListaPrecios.Count > 0 ? source.ListaPrecios[0].PorcentajeProfit : 0))
            .ForMember(destiny =>
                destiny.Precio2,
                opt => opt.MapFrom(source => source.ListaPrecios.Any() && source.ListaPrecios.Count > 1 ? source.ListaPrecios[1].Precio.ToString() : "0"))
            .ForMember(destiny =>
                destiny.PorcentajeProfit2,
                opt => opt.MapFrom(source => source.ListaPrecios.Any() && source.ListaPrecios.Count > 1 ? source.ListaPrecios[1].PorcentajeProfit : 0))
            .ForMember(destiny =>
                destiny.Precio3,
                opt => opt.MapFrom(source => source.ListaPrecios.Any() && source.ListaPrecios.Count > 1 ? source.ListaPrecios[2].Precio.ToString() : "0"))
            .ForMember(destiny =>
                destiny.PorcentajeProfit3,
                opt => opt.MapFrom(source => source.ListaPrecios.Any() && source.ListaPrecios.Count > 1 ? source.ListaPrecios[2].PorcentajeProfit : 0));


            CreateMap<VMProductSimplificado, Product>();

            CreateMap<VMCodigoBarras, CodigoBarras>().ReverseMap();
            CreateMap<VMRazonMovimientoCaja, RazonMovimientoCaja>().ReverseMap();
            CreateMap<VMMovimientoCaja, MovimientoCaja>().ReverseMap();
            CreateMap<VMVentasPorTipoDeVenta, VentasPorTipoDeVenta>().ReverseMap();
            CreateMap<Tag, VMTag>().ReverseMap();
            CreateMap<FormatosVenta, VMFormatosVenta>().ReverseMap();

            CreateMap<FacturaEmitida, VMFacturaEmitida>().ReverseMap();

            CreateMap<FacturaEmitida, VMFacturaEmitida>()
                    .ForMember(user => user.NroFacturaString, opt => opt.MapFrom(userEdit => userEdit.NroFactura.Value != 0 ? userEdit.NroFactura.Value.ToString("D8") : string.Empty))
                    .ForMember(user => user.PuntoVentaString, opt => opt.MapFrom(userEdit => userEdit.PuntoVenta.ToString("D4")));


            CreateMap<Tag, VMCategoriaWeb>()
                    .ForMember(user => user.IdTag, opt => opt.MapFrom(userEdit => userEdit.IdTag))
                    .ForMember(user => user.Descripcion, opt => opt.MapFrom(userEdit => userEdit.Nombre))
                    .ForMember(user => user.IdCategoria, opt => opt.MapFrom(userEdit => -2));

            CreateMap<Category, VMCategoriaWeb>()
                    .ForMember(user => user.IdTag, opt => opt.MapFrom(userEdit => -2))
                    .ForMember(user => user.Descripcion, opt => opt.MapFrom(userEdit => userEdit.Description))
                    .ForMember(user => user.IdCategoria, opt => opt.MapFrom(userEdit => userEdit.IdCategory));
        }
    }
}
