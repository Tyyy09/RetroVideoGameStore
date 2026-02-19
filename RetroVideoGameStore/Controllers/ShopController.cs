using Microsoft.AspNetCore.Mvc;
using RetroVideoGameStore.Data;

namespace RetroVideoGameStore.Controllers
{
    public class ShopController : Controller
    {
        //db connectio
        private readonly ApplicationDbContext _context;
        //constructor
        //connect to Db whenever thus controller is called
        public ShopController(ApplicationDbContext context)
        {
            _context = context;
           
        }
        public IActionResult Index()
        {
            //Get list of categories 
            var categories = _context.Categories.OrderBy(c => c.Name).ToList();
            return View(categories);
        }
    }
}
