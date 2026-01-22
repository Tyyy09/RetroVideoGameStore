using Microsoft.AspNetCore.Mvc;
using RetroVideoGameStore.Models;
namespace RetroVideoGameStore.Controllers
{
    //CategoriesController class extend Controller class
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            //Use category model to create fake list of 10 categories
            //create empty list of categories
            var categories = new List<Category>();
            for ( int i = 1; i<= 10; i++)
            {
                categories.Add(new Category
                {
                    Id = i,
                    Name = "Category " + i.ToString()
                });

            }
            return View(categories);
        }
        public IActionResult Browse(string categoryName)
        {
            // wrap the category name passed in with the URL
            ViewBag.categoriesName = categoryName;

            return View();
        }
        public IActionResult AddCategory()
        {
            //display a form for the user to add new category

            return View();
        }
    }
}
