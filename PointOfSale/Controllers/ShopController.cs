using Microsoft.AspNetCore.Mvc;

namespace PointOfSale.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Shop()
        {
            return View();
        }
    }
}
