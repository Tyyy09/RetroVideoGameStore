using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetroVideoGameStore.Data;
using RetroVideoGameStore.Models;

namespace RetroVideoGameStore.Controllers
{
    public class ShopController : Controller
    {
        // DB connection
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories.OrderBy(c => c.Name).ToList();
            return View(categories);
        }

        // Shop/Browse/3
        public IActionResult Browse(int id)
        {
            var products = _context.Products
                .Where(p => p.CategoryId == id)
                .OrderBy(p => p.Name)
                .ToList();
            return View(products);
        }

        // Shop/AddToCart
        [HttpPost]
        public IActionResult AddToCart(int ProductId, int Quantity)
        {
            var price = _context.Products.Find(ProductId).Price;
            var customerId = GetCustomerId();

            var cartItem = _context.Carts
                .FirstOrDefault(c => c.CustomerId == customerId && c.ProductId == ProductId);

            if (cartItem != null)
            {
                cartItem.Quantity += Quantity;
                _context.Update(cartItem);
            }
            else
            {
                _context.Carts.Add(new Cart
                {
                    ProductId = ProductId,
                    Quantity = Quantity,
                    Price = price,
                    CustomerId = customerId,
                    DateCreated = DateTime.Now
                });
            }

            _context.SaveChanges();

            // Update cart badge count in navbar
            var itemCount = (from c in _context.Carts
                             where c.CustomerId == customerId
                             select c.Quantity).Sum();
            HttpContext.Session.SetInt32("ItemCount", itemCount);

            return RedirectToAction("Cart");
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cartItem = _context.Carts.Find(id);
            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                _context.SaveChanges();
            }

            // Update cart badge count in navbar
            var customerId = GetCustomerId();
            var itemCount = (from c in _context.Carts
                             where c.CustomerId == customerId
                             select c.Quantity).Sum();
            HttpContext.Session.SetInt32("ItemCount", itemCount);

            return RedirectToAction("Cart");
        }

        private string GetCustomerId()
        {
            if (HttpContext.Session.GetString("CustomerId") == null)
            {
                HttpContext.Session.SetString("CustomerId", Guid.NewGuid().ToString());
            }
            return HttpContext.Session.GetString("CustomerId");
        }

        // GET: /Shop/Checkout

        [Authorize]

        public IActionResult Checkout()

        {

            return View();

        }
        // Shop/Cart
        public IActionResult Cart()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            var cartItems = _context.Carts
                .Include(c => c.Product)
                .Where(c => c.CustomerId == customerId)
                .ToList();
            return View(cartItems);
        }

        // POST: /Shop/Checkout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout([Bind("FirstName,LastName,Address,City,Province,PostalCode,Phone")] Order order)
        {
            // Auto-fill the 3 properties we removed from the form
            order.OrderDate = DateTime.Now;
            order.CustomerId = User.Identity.Name;
            order.OrderTotal = (from c in _context.Carts
                                where c.CustomerId == HttpContext.Session.GetString("CustomerId")
                                select c.Quantity * c.Price).Sum();

            // We can't store the whole object in session (ASP.NET Core sessions don't store objects)
            // Return a view (or redirect) so all paths return a value
            return RedirectToAction("Payment");
        }


    }

}