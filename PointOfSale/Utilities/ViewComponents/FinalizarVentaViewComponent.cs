using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Models;
using System.Security.Claims;

namespace PointOfSale.Utilities.ViewComponents
{
    public class FinalizarVentaViewComponent : ViewComponent
    {
        private readonly IMenuService _menuService;
        private readonly IMapper _mapper;
        public FinalizarVentaViewComponent(IMenuService menuService, IMapper mapper)
        {
            _menuService = menuService;
            _mapper = mapper;
        }
        public async Task<IViewComponentResult> InvokeAsync(VMSale model)
        {

            ClaimsPrincipal claimuser = HttpContext.User;
            List<VMMenu> listaMenus;
            listaMenus = new List<VMMenu>() { };




            return View(listaMenus);
        }
    }
}
