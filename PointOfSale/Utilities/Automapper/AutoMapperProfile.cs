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
                    opt => opt.MapFrom(source => source.IdTypeDocumentSaleNavigation.Description)
                )
                .ForMember(destiny =>
                    destiny.Users,
                    opt => opt.MapFrom(source => source.IdUsersNavigation.Name)
                )
                .ForMember(destiny =>
                    destiny.Total,
                    opt => opt.MapFrom(source => Convert.ToString(source.Total.Value, new CultureInfo("es-PE")))
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
                    opt => opt.MapFrom(source => source.IdSaleNavigation.IdTypeDocumentSaleNavigation.Description)
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

            CreateMap<Tienda, VMTienda>();

            CreateMap<VMTienda, Tienda>();


            CreateMap<Turno, VMTurno>()
                .ForMember(user => user.Fecha, opt => opt.MapFrom(userEdit => userEdit.FechaInicio.ToShortDateString()))
                .ForMember(user => user.HoraInicio, opt => opt.MapFrom(userEdit => userEdit.FechaInicio.ToShortTimeString()))
                .ForMember(user => user.HoraFin, opt => opt.MapFrom(userEdit => userEdit.FechaFin.HasValue ? userEdit.FechaFin.Value.ToShortTimeString() : string.Empty))
                .ForMember(user => user.Total, opt => opt.MapFrom(userEdit => userEdit.Sales.Any() ? "$" + userEdit.Sales.Sum(_ => _.Total).ToString() : string.Empty));

            CreateMap<VMTurno, Turno>();


            CreateMap<Cliente, VMCliente>();

            CreateMap<VMCliente, Cliente>();


            CreateMap<ClienteMovimiento, VMClienteMovimiento>();

            CreateMap<VMClienteMovimiento, ClienteMovimiento>();


            CreateMap<Proveedor, VMProveedor>();

            CreateMap<VMProveedor, Proveedor>();


            CreateMap<Promocion, VMPromocion>()
                .ForMember(user => user.IdCategory, opt => opt.MapFrom(userEdit => userEdit.IdCategory.Split(",", System.StringSplitOptions.None)))
                .ForMember(user => user.Dias, opt => opt.MapFrom(userEdit => userEdit.Dias.Split(",", System.StringSplitOptions.None)));


            CreateMap<VMPromocion, Promocion>()
                .ForMember(userEdit => userEdit.IdCategory, opt => opt.MapFrom(user => string.Join(", ", user.IdCategory)))
                .ForMember(userEdit => userEdit.Dias, opt => opt.MapFrom(user => string.Join(", ", user.Dias)));


            CreateMap<ProveedorMovimiento, VMProveedorMovimiento>();

            CreateMap<VMProveedorMovimiento, ProveedorMovimiento>();


            CreateMap<Gastos, VMGastos>()
                .ForMember(user => user.Comentario, opt => opt.MapFrom(userEdit => userEdit.Comentario != string.Empty ? userEdit.Comentario : userEdit.IdUsuario != null ? userEdit.User.Name : string.Empty))
                .ForMember(user => user.TipoGastoString, opt => opt.MapFrom(userEdit => userEdit.TipoDeGasto != null && userEdit.TipoDeGasto.GastoParticular != 0 ? userEdit.TipoDeGasto.Descripcion : string.Empty))
                .ForMember(user => user.GastoParticular, opt => opt.MapFrom(userEdit => userEdit.TipoDeGasto != null ? userEdit.TipoDeGasto.GastoParticular.ToString() : string.Empty))
                .ForMember(user => user.ImporteString, opt => opt.MapFrom(userEdit => "$" + userEdit.Importe))
                .ForMember(user => user.FechaString, opt => opt.MapFrom(userEdit => userEdit.RegistrationDate.ToString("dd/MM/yyyy h:mm tt")));


            CreateMap<VMGastos, Gastos>();

            CreateMap<TipoDeGasto, VMTipoDeGasto>();

            CreateMap<VMTipoDeGasto, TipoDeGasto>();
        }
    }
}
