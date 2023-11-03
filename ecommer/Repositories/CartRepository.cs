using System;
using System.Linq;
using System.Threading.Tasks;
using ecommer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ecommer.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartRepository(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> AddItem(int bookId, int qty)
        {
            string userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                throw new Exception("El usuario no ha iniciado sesión");

            using var transaction = _db.Database.BeginTransaction();

            try
            {
                var cart = await GetCart(userId);

                if (cart == null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCarts.Add(cart);
                    await _db.SaveChangesAsync();
                }

                var cartItem = _db.CartDetails
                    .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);

                if (cartItem != null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book = await _db.Books.FindAsync(bookId);

                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = book.Price
                    };

                    _db.CartDetails.Add(cartItem);
                }

                await _db.SaveChangesAsync();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Registra o notifica la excepción de manera adecuada
                transaction.Rollback();
                throw;
            }

            return await GetCartItemCount(userId);
        }

        public async Task<int> RemoveItem(int bookId)
        {
            string userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                throw new Exception("El usuario no ha iniciado sesión");

            try
            {
                var cart = await GetCart(userId);

                if (cart == null)
                    throw new Exception("Carrito no válido");

                var cartItem = _db.CartDetails
                    .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);

                if (cartItem == null)
                    throw new Exception("No hay elementos en el carrito");
                else if (cartItem.Quantity == 1)
                    _db.CartDetails.Remove(cartItem);
                else
                    cartItem.Quantity--;

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Registra o notifica la excepción de manera adecuada
                throw;
            }

            return await GetCartItemCount(userId);
        }

        public async Task<ShoppingCart> GetUserCart()
        {
            string userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                throw new Exception("Usuario no válido");

            var shoppingCart = await _db.ShoppingCarts
                .Include(a => a.CartDetails)
                .ThenInclude(a => a.Book)
                .ThenInclude(a => a.Genre)
                .Where(a => a.UserId == userId)
                .FirstOrDefaultAsync();

            return shoppingCart;
        }

        public async Task<ShoppingCart> GetCart(string userId)
        {
            return await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<int> GetCartItemCount(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new Exception("Usuario no válido");

            return await _db.CartDetails
                .Where(cd => cd.ShoppingCart.UserId == userId)
                .CountAsync();
        }

        public async Task<bool> DoCheckout()
        {
            using var transaction = _db.Database.BeginTransaction();

            try
            {
                string userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    throw new Exception("El usuario no ha iniciado sesión");

                var cart = await GetCart(userId);

                if (cart == null)
                    throw new Exception("Carrito no válido");

                var cartDetail = _db.CartDetails
                    .Where(a => a.ShoppingCartId == cart.Id)
                    .ToList();

                if (cartDetail.Count == 0)
                    throw new Exception("El carrito está vacío");

                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    OrderStatusId = 1 // Pendiente
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                foreach (var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };

                    _db.OrderDetails.Add(orderDetail);
                }

                await _db.SaveChangesAsync();

                _db.CartDetails.RemoveRange(cartDetail);
                await _db.SaveChangesAsync();

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                // Registra o notifica la excepción de manera adecuada
                transaction.Rollback();
                return false;
            }
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            return _userManager.GetUserId(principal);
        }
    }
}
