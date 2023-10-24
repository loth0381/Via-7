using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ecommer.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartRepository(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager)
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

                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCarts.Add(cart);
                    _db.SaveChanges();
                }

                // Cart detail section
                var cartItem = _db.CartDetails
                                  .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);

                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book = _db.Books.Find(bookId);

                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = book.Price
                    };

                    _db.CartDetails.Add(cartItem);
                }

                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Manejar la excepción de manera adecuada, como registrarla o notificarla
            }

            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<int> RemoveItem(int bookId)
        {
            //using var transaction = _db.Database.BeginTransaction();
            string userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                throw new Exception("El usuario no ha iniciado sesión");

            try
            {
                var cart = await GetCart(userId);

                if (cart is null)
                    throw new Exception("Carrito no válido");

                // Cart detail section
                var cartItem = _db.CartDetails
                                  .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);

                if (cartItem is null)
                    throw new Exception("No hay elementos en el carrito");
                else if (cartItem.Quantity == 1)
                    _db.CartDetails.Remove(cartItem);
                else
                    cartItem.Quantity = cartItem.Quantity - 1;

                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                // Manejar la excepción de manera adecuada, como registrarla o notificarla
            }

            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }


        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();

            if (userId == null)
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
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }

        public async Task<int> GetCartItemCount(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new Exception("Usuario no válido");

            var cartItemCount = await _db.CartDetails
                .Where(cd => cd.ShoppingCart.UserId == userId)
                .CountAsync();

            return cartItemCount;
        }

        public async Task<bool> DoCheckout()
        {
            using var transaction = _db.Database.BeginTransaction();

            try
            {
                // Lógica para mover datos de cartDetail a order y order detail, luego eliminar cartDetail
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    throw new Exception("El usuario no ha iniciado sesión");

                var cart = await GetCart(userId);

                if (cart is null)
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
                _db.SaveChanges();

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

                _db.SaveChanges();

                // Eliminar los detalles del carrito
                _db.CartDetails.RemoveRange(cartDetail);
                _db.SaveChanges();

                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                // Manejar la excepción de manera adecuada, como registrarla o notificarla
                return false;
            }
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}