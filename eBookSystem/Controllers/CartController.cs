using eBookSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;

public class CartController : Controller
{
    private readonly Database _database;

    public CartController(Database database)
    {
        _database = database;
    }

    public IActionResult Index()
    {
        var userId = User.Identity.Name; // Assuming the user's ID is the username
        var cart = _database.GetCartByUserId(userId) ?? new Cart { UserId = userId, CartItems = new List<CartItem>() };
        return View(cart);
    }

    [HttpPost]
    public IActionResult AddToCart(int bookId, int quantity)
    {
        var userId = User.Identity.Name;
        var cart = _database.GetCartByUserId(userId);
        if (cart == null)
        {
            _database.CreateCart(userId);
            cart = _database.GetCartByUserId(userId);
        }

        _database.AddToCart(cart.Id, bookId, quantity);
        return RedirectToAction("Index");
    }
    [HttpPost]
    public IActionResult UpdateQuantity(int cartItemId, int quantity)
    {
        if (quantity < 1)
        {
            _database.RemoveFromCart(cartItemId);
        }
        else
        {
            _database.UpdateCartItemQuantity(cartItemId, quantity);
        }
        return RedirectToAction("Index");
    }
    [HttpPost]
    public IActionResult RemoveFromCart(int cartItemId)
    {
        _database.RemoveFromCart(cartItemId);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ClearCart()
    {
        var userId = User.Identity.Name;
        var cart = _database.GetCartByUserId(userId);
        if (cart != null)
        {
            _database.ClearCart(cart.Id);
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Checkout()
    {
        var userId = User.Identity.Name;
        var cart = _database.GetCartByUserId(userId);
        if (cart == null || !cart.CartItems.Any())
        {
            return BadRequest("Your cart is empty.");
        }

        var totalAmount = cart.CartItems.Sum(item => item.Book.Price * item.Quantity);
        return RedirectToAction("Checkout", "Order", new { totalAmount });
    }
}
