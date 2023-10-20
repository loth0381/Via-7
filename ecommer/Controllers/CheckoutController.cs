using Microsoft.AspNetCore.Mvc;
using ecommer.Repositories;
using ecommer.Models;
using ecommer.ViewModels;
using System;
using System.Threading.Tasks;

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
        var cart = await _cartRepo.GetUserCart();
        var checkoutModel = new CheckoutViewModel
        {
            Cart = cart
        };

        return View(checkoutModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ProcessCheckout(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        // Aquí puedes agregar la lógica para guardar el pedido y la información del usuario en la base de datos.
        // Por ejemplo, puedes usar el CartRepository para guardar los detalles del pedido en la base de datos.

        // Redirigir a una página de confirmación o a la página de inicio después del checkout.
        return RedirectToAction("Confirmation");
    }

    public IActionResult Confirmation()
    {
        return View();
    }
}
