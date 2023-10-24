
using Microsoft.AspNetCore.Mvc;
using ecommer.Models;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Details(int id)
    {
        // Buscar el libro por ID en la base de datos
        var book = _context.Books.Find(id);

        if (book == null)
        {
            return NotFound(); // Manejo de error si el libro no se encuentra
        }

        return View(book);
    }
}
