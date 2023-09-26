using PointOfSale.Models;
using System.Globalization;
using PointOfSale.Model;
using AutoMapper;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Utilities.Automapper
{
    public class AutoMapperProfile : Profile
    {
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
                destiny.Price,
                opt => opt.MapFrom(source => Convert.ToString(source.Price.Value, new CultureInfo("es-PE")))
            ).ForMember(destiny =>
                destiny.PhotoBase64,
                opt => opt.MapFrom(source => Convert.ToBase64String(source.Photo))
            );

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
                opt => opt.MapFrom(source => Convert.ToDecimal(source.Price, new CultureInfo("es-PE")))
            );
            #endregion

            #region TypeDocumentSale
            CreateMap<TypeDocumentSale, VMTypeDocumentSale>().ReverseMap();
            #endregion

            #region Sale
            CreateMap<Sale, VMSale>()
                .ForMember(destiny =>
                    destiny.TypeDocumentSale,
                    opt => opt.MapFrom(source => source.TypeDocumentSaleNavigation.Description)
                )
                .ForMember(destiny =>
                    destiny.Users,
                    opt => opt.MapFrom(source => source.IdUsersNavigation.Name)
                )
                .ForMember(destiny =>
                    destiny.Total,
                    opt => opt.MapFrom(source => "$" + Convert.ToString(source.Total.Value, new CultureInfo("es-PE")))
                ).ForMember(destiny =>
                    destiny.RegistrationDate,
                    opt => opt.MapFrom(source => source.RegistrationDate.Value.ToString("dd/MM/yyyy h:mm tt"))
                );

            CreateMap<VMSale, Sale>()
                .ForMember(destiny =>
                    destiny.Total,
                    opt => opt.MapFrom(source => Convert.ToDecimal(source.Total, new CultureInfo("es-PE")))
                );
            #endregion

            #region DetailSale
            CreateMap<DetailSale, VMDetailSale>()
                .ForMember(destiny =>
                    destiny.Price,
                    opt => opt.MapFrom(source => Convert.ToString(source.Price.Value, new CultureInfo("es-PE")))
                )
                .ForMember(destiny =>
                    destiny.Total,
                    opt => opt.MapFrom(source => Convert.ToString(source.Total.Value, new CultureInfo("es-PE")))
                );

            CreateMap<VMDetailSale, DetailSale>()
                .ForMember(destiny =>
                    destiny.Price,
                    opt => opt.MapFrom(source => Convert.ToDecimal(source.Price, new CultureInfo("es-PE")))
                )
                .ForMember(destiny =>
                    destiny.Total,
                    opt => opt.MapFrom(source => Convert.ToDecimal(source.Total, new CultureInfo("es-PE")))
                );

            CreateMap<DetailSale, VMSalesReport>()
                .ForMember(destiny =>
                    destiny.RegistrationDate,
                    opt => opt.MapFrom(source => source.IdSaleNavigation.RegistrationDate.Value.ToString("dd/MM/yyyy h:mm tt"))
                )
                .ForMember(destiny =>
                    destiny.SaleNumber,
                    opt => opt.MapFrom(source => source.IdSaleNavigation.SaleNumber)
                )
                .ForMember(destiny =>
                    destiny.DocumentType,
                    opt => opt.MapFrom(source => source.IdSaleNavigation.TypeDocumentSaleNavigation.Description)
                )
                .ForMember(destiny =>
                    destiny.DocumentClient,
                    opt => opt.MapFrom(source => source.IdSaleNavigation.CustomerDocument)
                )
                .ForMember(destiny =>
                    destiny.ClientName,
                    opt => opt.MapFrom(source => source.IdSaleNavigation.ClientName)
                )
                .ForMember(destiny =>
                    destiny.TotalSale,
                    opt => opt.MapFrom(source => Convert.ToString(source.IdSaleNavigation.Total.Value, new CultureInfo("es-PE")))
                )
                .ForMember(destiny =>
                    destiny.Product,
                    opt => opt.MapFrom(source => source.DescriptionProduct)
                )
                .ForMember(destiny =>
                    destiny.Price,
                    opt => opt.MapFrom(source => Convert.ToString(source.Price.Value, new CultureInfo("es-PE")))
                )
                .ForMember(destiny =>
                    destiny.Total,
                    opt => opt.MapFrom(source => Convert.ToString(source.Total.Value, new CultureInfo("es-PE")))
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
                .ForMember(user => user.Total, opt => opt.MapFrom(userEdit => userEdit.Sales.Any() ? "$" + userEdit.Sales.Where(s => s.IdClienteMovimiento == null).Sum(_ => _.Total).ToString() : string.Empty));

            CreateMap<VMTurno, Turno>();


            CreateMap<Cliente, VMCliente>()
                    .ForMember(user => user.Total, opt => opt.MapFrom(userEdit => userEdit.ClienteMovimientos != null ? "$" + (userEdit.ClienteMovimientos.Where(_ => _.TipoMovimiento == TipoMovimientoCliente.Ingreso).Sum(_ => _.Total) - userEdit.ClienteMovimientos.Where(_ => _.TipoMovimiento == TipoMovimientoCliente.Egreso).Sum(_ => _.Total)).ToString() : string.Empty));

            CreateMap<VMCliente, Cliente>();


            CreateMap<ClienteMovimiento, VMClienteMovimiento>()
                .ForMember(user => user.TotalString, opt => opt.MapFrom(userEdit => "$" + userEdit.Total.ToString()))
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
                .ForMember(user => user.ImporteString, opt => opt.MapFrom(userEdit => "$" + userEdit.Importe.ToString()))
                .ForMember(user => user.ImporteSinIvaString, opt => opt.MapFrom(userEdit => "$" + userEdit.ImporteSinIva.ToString()))
                .ForMember(user => user.IvaImporteString, opt => opt.MapFrom(userEdit => "$" + userEdit.IvaImporte.ToString()))
                .ForMember(user => user.RegistrationDateString, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.ToString("dd/MM/yyyy HH:mm")));

            CreateMap<VMProveedorMovimiento, ProveedorMovimiento>();


            CreateMap<Gastos, VMGastos>()
                .ForMember(user => user.Comentario, opt => opt.MapFrom(userEdit => userEdit.Comentario != string.Empty ? userEdit.Comentario : userEdit.IdUsuario != null ? userEdit.User.Name : string.Empty))
                .ForMember(user => user.TipoGastoString, opt => opt.MapFrom(userEdit => userEdit.TipoDeGasto != null && userEdit.TipoDeGasto.GastoParticular != 0 ? userEdit.TipoDeGasto.Descripcion : string.Empty))
                .ForMember(user => user.GastoParticular, opt => opt.MapFrom(userEdit => userEdit.TipoDeGasto != null ? userEdit.TipoDeGasto.GastoParticular.ToString() : string.Empty))
                .ForMember(user => user.ImporteString, opt => opt.MapFrom(userEdit => "$" + userEdit.Importe))
                .ForMember(user => user.FechaString, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.ToString("dd/MM/yyyy HH:mm")));

            CreateMap<Gastos, VMGastosTablaDinamica>()
                .ForMember(user => user.Importe_Sin_Iva, opt => opt.MapFrom(userEdit => userEdit.ImporteSinIva != null ? userEdit.ImporteSinIva : 0))
                .ForMember(user => user.Iva_Importe, opt => opt.MapFrom(userEdit => userEdit.IvaImporte != null ? userEdit.IvaImporte : 0))
                .ForMember(user => user.Iva, opt => opt.MapFrom(userEdit => userEdit.Iva != null ? userEdit.Iva : 0))
                .ForMember(user => user.Comentario, opt => opt.MapFrom(userEdit => userEdit.Comentario != string.Empty ? userEdit.Comentario : userEdit.IdUsuario != null ? userEdit.User.Name : string.Empty))
                .ForMember(user => user.Gasto, opt => opt.MapFrom(userEdit => userEdit.TipoDeGasto != null ? userEdit.TipoDeGasto.GastoParticular.ToString() : string.Empty))
                .ForMember(user => user.Fecha, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.ToString("dd/MM/yyyy HH:mm")))
                .ForMember(user => user.Tipo_Gasto, opt => opt.MapFrom(userEdit => userEdit.TipoDeGasto.GastoParticular == TipoDeGastoEnum.Sueldos ? (userEdit.User != null ? userEdit.User.Name : string.Empty) : userEdit.TipoDeGasto.Descripcion.ToString()))
                .ForMember(user => user.Tipo_Factura, opt => opt.MapFrom(userEdit => string.IsNullOrEmpty(userEdit.TipoFactura) ? "-" : ((Model.Enum.TipoFactura)Convert.ToInt32(userEdit.TipoFactura)).ToString()))
                .ForMember(user => user.Nro_Factura, opt => opt.MapFrom(userEdit => userEdit.NroFactura == string.Empty ? "-" : userEdit.NroFactura));

            CreateMap<VMGastos, Gastos>();

            CreateMap<TipoDeGasto, VMTipoDeGasto>();
            CreateMap<VMTipoDeGasto, TipoDeGasto>();

            CreateMap<VMVentaWeb, VentaWeb>();
            CreateMap<VentaWeb, VMVentaWeb>()
                .ForMember(user => user.Fecha, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.Value.ToString("dd/MM/yyyy HH:mm")))
                .ForMember(user => user.TotalString, opt => opt.MapFrom(userEdit => "$" + userEdit.Total))
                .ForMember(user => user.FormaDePago, opt => opt.MapFrom(userEdit => userEdit.FormaDePago != null ? userEdit.FormaDePago.Description : string.Empty));

            ;
        }
    }
}
