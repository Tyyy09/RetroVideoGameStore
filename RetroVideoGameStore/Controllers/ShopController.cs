using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RetroVideoGameStore.Data;
using RetroVideoGameStore.Models;
using Stripe;
using Stripe.Checkout;
using System.Configuration;

namespace RetroVideoGameStore.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IConfiguration _configuration;

        public ShopController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories.OrderBy(c => c.Name).ToList();
            return View(categories);
        }

        public IActionResult Browse(int id)
        {
            var products = _context.Products.Where(p => p.CategoryId == id).OrderBy(p => p.Name).ToList();
            return View(products);
        }

        [HttpPost]
        public IActionResult AddToCart(int ProductId, int Quantity)
        {
            var price = _context.Products.Find(ProductId).Price;
            var customerId = GetCustomerId();

            var cartItem = _context.Carts.SingleOrDefault(c => c.ProductId == ProductId && c.CustomerId == customerId);

            if (cartItem != null)
            {
                cartItem.Quantity += Quantity;
                _context.Update(cartItem);
                _context.SaveChanges();
            }
            else
            {
                var cart = new Cart
                {
                    ProductId = ProductId,
                    Quantity = Quantity,
                    Price = price,
                    CustomerId = customerId,
                    DateCreated = DateTime.Now
                };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            return RedirectToAction("Cart");
        }

        private string GetCustomerId()
        {
            if (HttpContext.Session.GetString("CustomerId") == null)
            {
                var customerId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CustomerId", customerId);
            }
            return HttpContext.Session.GetString("CustomerId");
        }

        public IActionResult Cart()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            var cartItems = _context.Carts.Include(c => c.Product).Where(c => c.CustomerId == customerId).ToList();

            var itemCount = (from c in _context.Carts
                             where c.CustomerId == customerId
                             select c.Quantity).Sum();
            HttpContext.Session.SetInt32("ItemCount", itemCount);

            return View(cartItems);
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cartItem = _context.Carts.Find(id);

            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                _context.SaveChanges();
            }
            return RedirectToAction("Cart");
        }

        [Authorize]
        public IActionResult Checkout()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout([Bind("FirstName,LastName,Address,City,Province,PostalCode,Phone")] Order order)
        {
            order.OrderDate = DateTime.Now;
            order.CustomerId = User.Identity.Name;
            order.OrderTotal = (from c in _context.Carts
                                where c.CustomerId == HttpContext.Session.GetString("CustomerId")
                                select c.Quantity * c.Price).Sum();

            HttpContext.Session.SetObject("Order", order);
            return RedirectToAction("Payment");
        }

        [Authorize]
        public IActionResult Payment()
        {
            var order = HttpContext.Session.GetObject<Order>("Order");
            ViewBag.Total = order.OrderTotal;
            ViewBag.PublishableKey = _configuration.GetSection("Stripe")["PublishableKey"];
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult ProcessPayment()
        {
            var order = HttpContext.Session.GetObject<Order>("Order");
            var orderTotal = order.OrderTotal;

            StripeConfiguration.ApiKey = _configuration.GetSection("Stripe")["SecretKey"];

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long?)(orderTotal * 100),
                            Currency = "cad",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Retro Video Game Store Purchase"
                            }
                        }
                    }
                },
                Mode = "payment",
                SuccessUrl = "https://" + Request.Host + "/Shop/SaveOrder",
                CancelUrl = "https://" + Request.Host + "/Shop/Cart"
            };

            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        // GET: /Shop/SaveOrder

        [Authorize]

        public IActionResult SaveOrder()

        {
            // Grab the current order from the session variable
            var order = HttpContext.Session.GetObject<Order>("Order");
            // Create a new order in the dB - this generates and copies the new Id to this order object
            _context.Orders.Add(order);
            _context.SaveChanges();



            // Copy each item from the user's cart to a new OrderDetail record
            var cartItems = _context.Carts.Where(c => c.CustomerId == HttpContext.Session.GetString("CustomerId"));
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }

            // Save new line items to the dB
            _context.SaveChanges();

            // Empty the cart
            foreach (var item in cartItems)

            {
                _context.Carts.Remove(item);
            }
            _context.SaveChanges();
            // Load the Details page for the new order
            return RedirectToAction("Details", "Orders", new { @id = order.OrderId });

        }
        // GET: Orders/Details/5 
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var order = await _context.Orders.Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }
            // Ensure current user owns the order being requested
            if (User.Identity.Name != order.CustomerId)

            {
                return Unauthorized();
            }
            return View(order);
        }
    }
}