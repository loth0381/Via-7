using Microsoft.EntityFrameworkCore;

namespace ecommer.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _db;

        public HomeRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Genre>> Genres()
        {
            return await _db.Genres.ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooks(string sTerm = "", int genreId = 0)
        {
            sTerm = sTerm.ToLower();
            var query = from book in _db.Books
                        join genre in _db.Genres on book.GenreId equals genre.Id
                        where string.IsNullOrWhiteSpace(sTerm) || book.BookName.ToLower().StartsWith(sTerm)
                        select new Book
                        {
                            Id = book.Id,
                            Image = book.Image,
                            AuthorName = book.AuthorName,
                            BookName = book.BookName,
                            GenreId = book.GenreId,
                            Price = book.Price,
                            GenreName = genre.GenreName
                        };

            if (genreId > 0)
            {
                query = query.Where(book => book.GenreId == genreId);
            }

            var books = await query.ToListAsync();
            return books;
        }
    }
}
