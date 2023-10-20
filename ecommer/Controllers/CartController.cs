﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // Añadido para UserManager
using ecommer.Repositories;
using System;
using Microsoft.AspNetCore.Http;

namespace ecommer.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepo;
        private readonly UserManager<IdentityUser> _userManager; // Inyectar UserManager
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartController(ICartRepository cartRepo, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _cartRepo = cartRepo;
            _userManager = userManager; // Asignar UserManager
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> AddItem(int bookId, int qty = 1, int redirect = 0)
        {
            var cartCount = await _cartRepo.AddItem(bookId, qty);
            if (redirect == 0)
                return Ok(cartCount);
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> RemoveItem(int bookId)
        {
            var cartCount = await _cartRepo.RemoveItem(bookId);
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _cartRepo.GetUserCart();
            return View(cart);
        }

        public async Task<IActionResult> GetTotalItemInCart()
        {
            var userId = GetUserId();
            int cartItem = await _cartRepo.GetCartItemCount(userId);
            return Ok(cartItem);
        }

        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            bool isCheckedOut = await _cartRepo.DoCheckout();
            if (!isCheckedOut)
                throw new Exception("Algo salió mal en el servidor");
            return RedirectToAction("Index", "Checkout"); // Redirigir a "Index" en el controlador "Checkout"
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }

    }


}

