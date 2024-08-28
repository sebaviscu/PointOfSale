using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Models;
using System.Security.Claims;

namespace PointOfSale.Utilities.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly IMenuService _menuService;
        private readonly IMapper _mapper;
        private readonly ITurnoService _turnoService;

        public MenuViewComponent(IMenuService menuService, IMapper mapper, ITurnoService turnoService)
        {
            _menuService = menuService;
            _mapper = mapper;
            _turnoService = turnoService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            var menuView = new MenuView();

            if (claimuser.Identity.IsAuthenticated)
            {
                string idUser = claimuser.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value).SingleOrDefault();

                menuView.Menus = _mapper.Map<List<VMMenu>>(await _menuService.GetMenus(int.Parse(idUser)));
            }
            else
            {
                menuView.Menus = new List<VMMenu>() { };
            }

            var idTienda = ((ClaimsIdentity)claimuser.Identity).FindFirst("Tienda").Value;
            var turno = await _turnoService.GetTurnoActualConVentas(Convert.ToInt32(idTienda));

            menuView.TurnoAbierto = turno != null;

            return View(menuView);
        }
    }
}
