using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Final.Models;

namespace Final.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context; 

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Hiển thị danh sách sản phẩm
        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        //Hiển thị form thêm sản phẩm  
        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Product/Create.cshtml"); // Trỏ đúng đến View
        }

        //Xử lý thêm sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo mật chống CSRF
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        //Hiển thị form chỉnh sửa sản phẩm
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // 🟢 Xử lý chỉnh sửa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo mật chống CSRF
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Update(product);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // 🟢 Hiển thị trang xác nhận xóa
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id); 
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // 🟢 Xử lý xóa sản phẩm
        [HttpPost, ActionName("Delete")] // Đặt tên để khớp với View
        [ValidateAntiForgeryToken] 
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        //Hiển thị chi tiết sản phẩm
        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
    }
}