namespace Bookstore.Domain.Books
{
    public interface IBookRatingRepository
    {
        internal protected Task<BookRating?> GetAsync(int bookId, string customerSub);

        internal protected Task AddAsync(BookRating rating);

        internal protected Task<(int Count, double Average)> GetSummaryAsync(int bookId);

        Task SaveChangesAsync();
    }
}
