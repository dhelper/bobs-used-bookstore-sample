using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Bookstore.Web.Helpers;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Web.ViewModel.Search;

namespace Bookstore.Web.Controllers
{
    [AllowAnonymous]
    public class SearchController : Controller
    {
        private readonly IBookService inventoryService;
        private readonly IShoppingCartService shoppingCartService;
        private readonly IBookRatingService bookRatingService;

        public SearchController(IBookService inventoryService, IShoppingCartService shoppingCartService, IBookRatingService bookRatingService)
        {
            this.inventoryService = inventoryService;
            this.shoppingCartService = shoppingCartService;
            this.bookRatingService = bookRatingService;
        }

        public async Task<IActionResult> Index(string searchString, string sortBy = "Name", int pageIndex = 1, int pageSize = 10)
        {
            var books = await inventoryService.GetBooksAsync(searchString, sortBy, pageIndex, pageSize);

            return View(new SearchIndexViewModel(books));
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await inventoryService.GetBookAsync(id);

            var viewModel = new SearchDetailsViewModel(book);

            var sub = User.GetSub();
            if (User.Identity?.IsAuthenticated == true && sub != null)
            {
                viewModel.UserRating = await bookRatingService.GetCustomerRatingAsync(id, sub);
            }

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(int bookId, int rating)
        {
            if (rating < BookRating.MinRating || rating > BookRating.MaxRating)
            {
                this.SetNotification($"Rating must be between {BookRating.MinRating} and {BookRating.MaxRating}");
                return RedirectToAction("Details", new { id = bookId });
            }

            await bookRatingService.RateAsync(new RateBookDto(bookId, User.GetSub()!, rating));

            this.SetNotification("Your rating has been saved");

            return RedirectToAction("Details", new { id = bookId });
        }

        public async Task<IActionResult> AddItemToShoppingCart(int bookId)
        {
            var dto = new AddToShoppingCartDto(HttpContext.GetShoppingCartCorrelationId(), bookId, 1);

            await shoppingCartService.AddToShoppingCartAsync(dto);

            this.SetNotification("Item added to shopping cart");

            return RedirectToAction("Index", "Search");
        }

        public async Task<IActionResult> AddItemToWishlist(int bookId)
        {
            var dto = new AddToWishlistDto(HttpContext.GetShoppingCartCorrelationId(), bookId);

            await shoppingCartService.AddToWishlistAsync(dto);

            this.SetNotification("Item added to wishlist");

            return RedirectToAction("Index", "Search");
        }
    }
}
