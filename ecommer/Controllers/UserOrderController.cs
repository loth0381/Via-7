using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ecommer.Controllers
{
    [Authorize]
    public class UserOrderController : Controller
    {
        private readonly IUserOrderRepository _userOrderRepo;

        public UserOrderController(IUserOrderRepository userOrderRepo)
        {
            _userOrderRepo = userOrderRepo;
        }

        public async Task<IActionResult> UserOrders()
        {
            try
            {
                var orders = await _userOrderRepo.UserOrders();

                if (orders == null)
                {
                    // Manejar la situación en la que no se pueden recuperar órdenes.
                    return View("NoOrders");
                }

                return View(orders);
            }
            catch (Exception ex)
            {
                // Manejar errores inesperados, por ejemplo, registrar y notificar.
                return View("Error");
            }
        }
    }
}
