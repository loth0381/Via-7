using Microsoft.AspNetCore.Mvc;
using ecommer.Repositories;
using ecommer.Models;
using ecommer.ViewModels; // Asegúrate de que el espacio de nombres 'ViewModels' esté referenciado.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ecommer.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICartRepository _cartRepo;

        public CheckoutController(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var cart = await _cartRepo.GetUserCart();

                if (cart == null)
                {
                    return RedirectToAction("EmptyCart");
                }

                var checkoutModel = new CheckoutViewModel
                {
                    Cart = cart,
                    CartDetails = cart.CartDetails?.ToList() ?? new List<CartDetail>()
                };

                return View(checkoutModel);
            }
            catch (Exception ex)
            {
                // Manejar excepciones de manera adecuada, como registrar el error o redirigir a una página de error.
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessCheckout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // Aquí puedes agregar la lógica para guardar el pedido y la información del usuario en la base de datos.
                // Por ejemplo, puedes usar el CartRepository para guardar los detalles del pedido en la base de datos.

                // Redirigir a una página de confirmación después del checkout
                return RedirectToAction("Confirmation");
            }
            catch (Exception ex)
            {
                // Manejar excepciones de manera adecuada, como registrar el error o redirigir a una página de error.
                return RedirectToAction("Error");
            }
        }

        public IActionResult Confirmation()
        {
            return View();
        }

        public IActionResult EmptyCart()
        {
            // Página de carrito vacío (puedes personalizarla según tus necesidades)
            return View();
        }

        public IActionResult Error()
        {
            // Página de error (puedes personalizarla según tus necesidades)
            return View();
        }
    }
}
