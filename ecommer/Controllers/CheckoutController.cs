using Microsoft.AspNetCore.Mvc;
using ecommer.Repositories;
using ecommer.Models;
using ecommer.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Stripe;

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

                if (cart == null || cart.CartDetails == null || cart.CartDetails.Count == 0)
                {
                    // Redirige al usuario a la página de "Carrito Vacío" si el carrito está vacío.
                    return RedirectToAction("EmptyCart");
                }

                // Obtén la clave de publicación de Stripe desde tu configuración
                var stripePublishableKey = "pk_test_51NhKybH28o0wLQaAgW5ShASKKcBMFiwKdY20TlnCTQuVG3Qgd7V0Ck39NSYeqIZ8AVMVjNTRPzptqCLsEMyamqJN00sTdRjrqQ"; // Reemplaza con tu clave de Stripe

                var checkoutModel = new CheckoutViewModel
                {
                    Cart = cart,
                    CartDetails = cart.CartDetails.ToList(),
                    StripePublishableKey = stripePublishableKey
                };

                return View(checkoutModel);
            }
            catch (Exception ex)
            {
                // Registra el error o maneja las excepciones de manera adecuada.
                // Puedes redirigir al usuario a la página de "Error" en caso de un error crítico.
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, vuelve a la página de "Checkout" con los errores.
                return View("Index", model);
            }

            try
            {
                // Aquí deberías utilizar la biblioteca de Stripe para procesar el pago.
                StripeConfiguration.ApiKey = "sk_test_51NhKybH28o0wLQaAqJB9q4hYbpACwXC7jw93b8LtEV6jjBBJC0Sgac9N84orqSxXYPcRWCKvDFSGhuPLypJZVQPr00HmNbjpsX"; // Reemplaza con tu clave secreta de Stripe

                var options = new ChargeCreateOptions
                {
                    Amount = (long)(model.CartDetails.Sum(item => item.Quantity * item.Book.Price) * 100), // Monto en centavos
                    Currency = "usd", // Moneda (cambia según tu configuración)
                    Description = "Compra en tu tienda en línea",
                    Source = model.StripeToken, // El token de la tarjeta de crédito
                };

                var service = new ChargeService();
                var charge = service.Create(options);

                // Después de procesar el pago con éxito, puedes guardar la información del pedido en la base de datos, si es necesario.

                // Redirige al usuario a la página de "Confirmación" después del checkout exitoso.
                return RedirectToAction("Confirmation");
            }
            catch (Exception ex)
            {
                // Registra el error o maneja las excepciones de manera adecuada.
                // Puedes redirigir al usuario a la página de "Error" en caso de un error crítico.
                return RedirectToAction("Error");
            }
        }

        // Resto de las acciones del controlador (Confirmation, EmptyCart, Error) aquí.
    }
}
