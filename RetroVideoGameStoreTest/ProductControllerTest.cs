using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetroVideoGameStore.Controllers;
using RetroVideoGameStore.Data;
using RetroVideoGameStore.Models;

namespace RetroVideoGameStoreTest
{
    [TestClass]
    public sealed class ProductsControllerTest
    {
        // Class-level variables used for all unit tests
        // Mock in-memory dB
        private ApplicationDbContext _context;
        // Mock list of products
        List<Product> products = new List<Product>();
        // Controller object used for all unit tests
        ProductsController controller;

        // Arrange: code that runs automatically before every unit test
        [TestInitialize]
        public void TestInitialize()
        {
            // Create new in-memory dB
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Create mock data and add to in-memory dB
            var category = new Category { Id = 300, Name = "Some Category" };
            _context.Categories.Add(category);

            products.Add(new Product { Id = 46, Name = "Product Forty-Six", Price = 46.46, CategoryId = 300, Category = category });
            products.Add(new Product { Id = 87, Name = "Product Eighty-Seven", Price = 87.87, CategoryId = 300, Category = category });
            products.Add(new Product { Id = 51, Name = "Product Fifty-One", Price = 51.51, CategoryId = 300, Category = category });
            foreach (var p in products)
            {
                _context.Add(p);
            }
            _context.SaveChanges();

            // Now create the controller and pass it the dbcontext
            controller = new ProductsController(_context);
        }

         [TestMethod]
        public void IndexViewIsNotNull()
        {
            // Arrange: set up what we need to execute the method we want to test (variables, etc.)
            //var controller = new ProductsController();
            // We don't need the above as it is donw in the TestInitialize() method!

            // Act: execute the method we want to test and obtain a result
            var result = controller.Index();

            // Assert: check that the result we got is what we expected
            Assert.IsNotNull(result);

        }

        [TestMethod]
        public void IndexLoadsIndexView()
        {
            // Act (note that we need to add the .Result; because of the async Task<IActionResult> in the ProductsController Index() method)
            var result = (ViewResult)controller.Index().Result;

            // Assert
            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod]
        public void IndexLoadsProducts()
        {
            // Act - get the view result, then get the model of that view
            var result = (ViewResult)controller.Index().Result;
            var data = (List<Product>)result.Model;
            // Assert - check Product list we initialized is the same as the data from the Index() method
            CollectionAssert.AreEqual(products.OrderBy(p => p.Name).ToList(), data);
        }
    }
}