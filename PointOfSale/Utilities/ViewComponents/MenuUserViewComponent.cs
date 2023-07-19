﻿using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using System.Security.Claims;

namespace PointOfSale.Utilities.ViewComponents
{
    public class MenuUserViewComponent : ViewComponent
    {
        private readonly IUserService _userService;

        public MenuUserViewComponent(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            ClaimsPrincipal claimuser = HttpContext.User;


            string userName = string.Empty;
            string photoUser = string.Empty;
            string emailUser = string.Empty;
            string rolUser = string.Empty;

            if (claimuser.Identity.IsAuthenticated)
            {
                userName = claimuser.Claims
                    .Where(c => c.Type == ClaimTypes.Name)
                    .Select(c => c.Value).SingleOrDefault();

                int IdUser = Convert.ToInt32( claimuser.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value).SingleOrDefault());

                User user_found = await _userService.GetByIdWithRol(IdUser);

                var s = claimuser.Claims
		.Where(c => c.Type == ClaimTypes.Role)
		.Select(c => c.Value).SingleOrDefault();

				rolUser = user_found.IdRolNavigation.Description;

                //if (user_found.Photo != null)
                //    photoUser = Convert.ToBase64String(user_found.Photo);

                emailUser = ((ClaimsIdentity)claimuser.Identity).FindFirst("Email").Value;
            }

            ViewData["userName"] = userName;
            ViewData["photoUser"] = photoUser;
            ViewData["emailUser"] = emailUser;
            ViewData["rolUser"] = rolUser;

            return View();
        }
    }
}
