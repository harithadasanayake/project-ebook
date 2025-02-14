﻿using eBookSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBookSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly Database _database;

        public AdminController(Database database)
        {
            _database = database;
        }

        // Action to display the admin login form
        public IActionResult AdminLogin()
        {
            return View();
        }

        public IActionResult AdminDashboard()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("AdminLogin");
            }

            var totalSales = _database.GetTotalSales();
            var totalSalesCount = _database.GetTotalSalesCount();
            var totalUsers = _database.GetTotalUsers();
            var mostPopularBooks = _database.GetMostPopularBooks(4); // Change to get top 4 books
            var recentOrders = _database.GetRecentOrders(5);

            var model = new AdminDashboardViewModel
            {
                TotalSales = totalSales,
                TotalSalesCount = totalSalesCount,
                TotalUsers = totalUsers,
                MostPopularBooks = mostPopularBooks ?? new List<Book>(),
                RecentOrders = recentOrders ?? new List<Order>()
            };

            return View(model);
        }

        public IActionResult Book()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("AdminLogin");
            }
            return View();
        }

        public IActionResult ManageBook()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("AdminLogin");
            }

            var model = new BookViewModel
            {
                Book = new Book(),
                Books = _database.GetAllBooks()
            };
            return View(model);
        }

        public IActionResult UpdateBook(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("AdminLogin");
            }

            var book = _database.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // Action to handle the admin login request
        [HttpPost]
        [ValidateAntiForgeryToken] // Add anti-forgery token to prevent CSRF attacks
        public IActionResult AdminLogin(string username, string password)
        {
            // Perform authentication logic here
            if (username == "admin" && password == "123")
            {
                HttpContext.Session.SetString("IsAdminLoggedIn", "true");
                return RedirectToAction("AdminDashboard");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("IsAdminLoggedIn") == "true";
        }
    }
}