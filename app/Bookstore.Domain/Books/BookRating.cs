using Bookstore.Domain.Customers;

namespace Bookstore.Domain.Books
{
    public class BookRating : Entity
    {
        public const int MinRating = 1;
        public const int MaxRating = 5;

        // Empty constructor required by EF Core
        public BookRating() { }

        public BookRating(int bookId, Customer customer, int rating)
        {
            BookId = bookId;
            Customer = customer;
            Rating = rating;
        }

        public Book Book { get; set; } = null!;
        public int BookId { get; set; }

        public Customer Customer { get; set; } = null!;
        public int CustomerId { get; set; }

        public int Rating { get; set; }
    }
}
