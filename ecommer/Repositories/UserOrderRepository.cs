using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ecommer.Repositories
{
    public class UserOrderRepository : IUserOrderRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public UserOrderRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Order>> UserOrders()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("El usuario no ha iniciado sesión.");

                var orders = await _db.Orders
                    .Include(x => x.OrderStatus)
                    .Include(x => x.OrderDetail)
                    .ThenInclude(x => x.Book)
                    .ThenInclude(x => x.Genre)
                    .Where(a => a.UserId == userId)
                    .ToListAsync();

                return orders;
            }
            catch (Exception ex)
            {
                // Manejar la excepción de manera adecuada, como registrarla o notificarla
                throw new Exception("Error al obtener los pedidos del usuario.", ex);
            }
        }

        private string GetUserId()
        {
            try
            {
                var principal = _httpContextAccessor.HttpContext.User;
                string userId = _userManager.GetUserId(principal);
                return userId;
            }
            catch (Exception ex)
            {
                // Manejar la excepción de manera adecuada, como registrarla o notificarla
                throw new Exception("Error al obtener el identificador del usuario.", ex);
            }
        }
    }
}
