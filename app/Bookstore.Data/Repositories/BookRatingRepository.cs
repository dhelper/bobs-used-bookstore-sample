using Bookstore.Domain.Books;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class BookRatingRepository : IBookRatingRepository
    {
        private readonly ApplicationDbContext dbContext;

        public BookRatingRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        async Task<BookRating?> IBookRatingRepository.GetAsync(int bookId, string customerSub)
        {
            return await dbContext.BookRating
                .SingleOrDefaultAsync(x => x.BookId == bookId && x.Customer.Sub == customerSub);
        }

        async Task IBookRatingRepository.AddAsync(BookRating rating)
        {
            await dbContext.BookRating.AddAsync(rating);
        }

        async Task<(int Count, double Average)> IBookRatingRepository.GetSummaryAsync(int bookId)
        {
            var summary = await dbContext.BookRating
                .Where(x => x.BookId == bookId)
                .GroupBy(x => x.BookId)
                .Select(g => new { Count = g.Count(), Average = g.Average(x => (double)x.Rating) })
                .SingleOrDefaultAsync();

            return summary == null ? (0, 0d) : (summary.Count, summary.Average);
        }

        async Task IBookRatingRepository.SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
