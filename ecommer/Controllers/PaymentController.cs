using ecommer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;
using ecommer.Models; // Agrega el espacio de nombres necesario

namespace ecommer.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;

        public PaymentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new CheckoutViewModel
            {
                StripePublishableKey = _configuration.GetSection("Stripe")["PublishableKey"] // Configura la clave pública de Stripe
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessPayment(CheckoutViewModel model)
        {
            if (ModelState.IsValid)
            {
                var stripeSecretKey = _configuration.GetSection("Stripe")["SecretKey"];
                StripeConfiguration.ApiKey = stripeSecretKey;

                // Calcula el total del carrito
                double totalAmount = 0;
                if (model.Cart != null && model.Cart.CartDetails != null)
                {
                    totalAmount = model.Cart.CartDetails
                        .Select(item => item.Book.Price * item.Quantity)
                        .Sum();
                }

                var options = new ChargeCreateOptions
                {
                    Amount = (long)(totalAmount * 100),
                    Currency = "usd",
                    Description = "Compra en tu tienda en línea",
                    Source = model.StripeToken // Utiliza el token Stripe proporcionado en el formulario
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                // Agrega la lógica para guardar el pedido y la información del cliente en la base de datos.

                // Redirige a una página de confirmación o a la página de inicio después del pago.
                return RedirectToAction("Confirmation");
            }

            // Si hay errores de validación, muestra la vista de pago nuevamente con los errores.
            return View("Index", model);
        }

        [HttpGet]
        public IActionResult Confirmation()
        {
            // Puedes implementar una acción para mostrar la confirmación del pedido aquí
            return View();
        }
    }
}
