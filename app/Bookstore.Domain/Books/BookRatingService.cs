using Bookstore.Domain.Customers;

namespace Bookstore.Domain.Books
{
    public record RateBookDto(int BookId, string CustomerSub, int Rating);

    public interface IBookRatingService
    {
        /// <summary>Adds the customer's rating for a book, or updates it if they already rated it.</summary>
        Task RateAsync(RateBookDto dto);

        /// <summary>Returns the customer's rating for the book, or null if they have not rated it.</summary>
        Task<int?> GetCustomerRatingAsync(int bookId, string customerSub);
    }

    public class BookRatingService : IBookRatingService
    {
        private readonly IBookRatingRepository ratingRepository;
        private readonly IBookRepository bookRepository;
        private readonly ICustomerRepository customerRepository;

        public BookRatingService(IBookRatingRepository ratingRepository, IBookRepository bookRepository, ICustomerRepository customerRepository)
        {
            this.ratingRepository = ratingRepository;
            this.bookRepository = bookRepository;
            this.customerRepository = customerRepository;
        }

        public async Task<int?> GetCustomerRatingAsync(int bookId, string customerSub)
        {
            return (await ratingRepository.GetAsync(bookId, customerSub))?.Rating;
        }

        public async Task RateAsync(RateBookDto dto)
        {
            if (dto.Rating < BookRating.MinRating || dto.Rating > BookRating.MaxRating)
            {
                throw new ArgumentOutOfRangeException(nameof(dto), $"Rating must be between {BookRating.MinRating} and {BookRating.MaxRating}.");
            }

            var book = await bookRepository.GetAsync(dto.BookId);

            var customer = await customerRepository.GetAsync(dto.CustomerSub);
            if (customer == null)
            {
                customer = new Customer(dto.CustomerSub);
                await customerRepository.AddAsync(customer);
                await customerRepository.SaveChangesAsync();
            }

            var existing = await ratingRepository.GetAsync(dto.BookId, dto.CustomerSub);
            if (existing == null)
            {
                await ratingRepository.AddAsync(new BookRating(dto.BookId, customer, dto.Rating));
            }
            else
            {
                existing.Rating = dto.Rating;
                existing.UpdatedOn = DateTime.UtcNow;
            }

            await ratingRepository.SaveChangesAsync();

            // Recalculate the aggregate from the stored ratings so it stays correct
            // even if concurrent ratings were submitted.
            var (count, average) = await ratingRepository.GetSummaryAsync(dto.BookId);
            book.RatingCount = count;
            book.AverageRating = Math.Round(average, 2);

            await ratingRepository.SaveChangesAsync();
        }
    }
}
