using Final.Models;
using Microsoft.AspNetCore.Mvc;
using Final.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Final.Services.Vnpay;

namespace Final.Controllers
{
    [Authorize(Roles = "User")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, ILogger<CartController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IActionResult Index()
        {
            var cartItems = GetCartItems();

            var cart = new Cart();
            foreach (var item in cartItems)
            {
                cart.AddItem(item.Product, item.Quantity);
            }

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cartItems = GetCartItems();
            var existingItem = cartItems.FirstOrDefault(p => p.Product.Id == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cartItems.Add(new CartItem
                {
                    Product = product,
                    ProductId = product.Id,
                    Quantity = 1,
                    Price = product.Price
                });
            }

            SaveCart(cartItems);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult IncreaseQuantity(int productId)
        {
            UpdateCartItemQuantity(productId, 1);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DecreaseQuantity(int productId)
        {
            UpdateCartItemQuantity(productId, -1);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            var cartItems = GetCartItems();
            var itemToRemove = cartItems.FirstOrDefault(p => p.Product.Id == productId);

            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                SaveCart(cartItems);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var cartItems = GetCartItems();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Index");
            }

            ModelState.Clear();

            var orderModel = new OrderModel
            {
                CartItems = cartItems,
                City = "",
                Province = "",
            };

            var totalPrice = cartItems.Sum(item => item.Price * item.Quantity);
            ViewData["TotalPrice"] = totalPrice;

            return View(orderModel);
        }

        private void SaveCart(List<CartItem> cartItems)
        {
            HttpContext.Session.SetJson("Cart", cartItems);
            _logger.LogInformation("Cart saved to session: {CartItems}", JsonConvert.SerializeObject(cartItems));
        }

        private List<CartItem> GetCartItems()
        {
            return HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        }

        private void UpdateCartItemQuantity(int productId, int quantityChange)
        {
            var cartItems = GetCartItems();
            var item = cartItems.FirstOrDefault(p => p.Product.Id == productId);

            if (item != null)
            {
                item.Quantity += quantityChange;
                if (item.Quantity <= 0)
                {
                    cartItems.Remove(item);
                }
                SaveCart(cartItems);
            }
        }        
    }
}