﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using eBookSystem.Models;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace eBookSystem.Controllers
{
    public class BookController : Controller
    {
        private readonly Database _database;

        public BookController(Database database)
        {
            _database = database;
        }

        // GET: Admin/Book (to display the form)
        [HttpGet]
        public IActionResult Book()
        {
            var model = new Book(); // Initialize a new Book model
            return View("~/Views/Admin/Book.cshtml", model);
        }

        // POST: Admin/Book (to handle form submission)
        [HttpPost]
        public IActionResult Book(Book book, IFormFile coverImage)
        {
            try
            {
                if (coverImage != null && coverImage.Length > 0)
                {
                    book.CoverImage = GetByteArrayFromImage(coverImage);
                }
                else
                {
                    book.CoverImage = GetDefaultCoverImage();
                }

                // Remove the validation error for CoverImage if it's set correctly
                if (book.CoverImage != null && book.CoverImage.Length > 0)
                {
                    ModelState.Remove("CoverImage");
                }

                if (ModelState.IsValid)
                {
                    string res = _database.InsertBook(book);
                    TempData["msg"] = res == "OK" ? "Book added successfully!" : $"Error: {res}";

                    // Redirect to the GET action to display the form again
                    return RedirectToAction(nameof(Book));
                }
                else
                {
                    // Log the validation errors and check if CoverImage is correctly set
                    TempData["msg"] = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)) + " | CoverImage length: " + (book.CoverImage?.Length ?? 0);
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = ex.Message;
                throw;
            }

            // Return the same view with the model containing validation errors
            return View("~/Views/Admin/Book.cshtml", book);
        }

        // GET: Admin/ManageBook (to display the form and list of books)
        [HttpGet]
        public IActionResult ManageBook()
        {
            var model = new BookViewModel
            {
                Book = new Book(),
                Books = _database.GetAllBooks()
            };
            return View("~/Views/Admin/ManageBook.cshtml", model);
        }

        // GET: Admin/UpdateBook/{id} (to display the edit form)
        [HttpGet]
        public IActionResult UpdateBook(int id)
        {
            var book = _database.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/UpdateBook.cshtml", book);
        }

        // POST: Admin/UpdateBook (to handle edit form submission)
        [HttpPost]
        [HttpPost]
        public IActionResult UpdateBook(Book book, IFormFile coverImage)
        {
            try
            {
                // Fetch the existing book to get the current CoverImage
                var existingBook = _database.GetBookById(book.Id);
                if (existingBook == null)
                {
                    return NotFound();
                }

                // If a new cover image is provided, update the CoverImage field
                if (coverImage != null && coverImage.Length > 0)
                {
                    book.CoverImage = GetByteArrayFromImage(coverImage);
                }
                else
                {
                    // Use the existing CoverImage
                    book.CoverImage = existingBook.CoverImage;
                }
                ModelState.Remove("CoverImage");
                if (ModelState.IsValid)
                {
                    string res = _database.UpdateBook(book);
                    TempData["msg"] = res == "OK" ? "Book updated successfully!" : $"Error: {res}";

                    return RedirectToAction(nameof(ManageBook));
                }
                else
                {
                    TempData["msg"] = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = ex.Message;
                throw;
            }

            return View("~/Views/Admin/UpdateBook.cshtml", book);
        }


        // GET: Admin/DeleteBook/{id} (to delete a book)
        [HttpGet]
        public IActionResult DeleteBook(int id)
        {
            try
            {
                string res = _database.DeleteBook(id);
                TempData["msg"] = res == "OK" ? "Book deleted successfully!" : $"Error: {res}";
            }
            catch (Exception ex)
            {
                TempData["msg"] = ex.Message;
                throw;
            }

            return RedirectToAction(nameof(ManageBook));
        }

        private byte[] GetDefaultCoverImage()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "defaultCover.png");
            if (!System.IO.File.Exists(filePath))
            {
                throw new InvalidOperationException("Default cover image not found at " + filePath);
            }
            return System.IO.File.ReadAllBytes(filePath);
        }

        private byte[] GetByteArrayFromImage(IFormFile file)
        {
            using (var target = new MemoryStream())
            {
                file.CopyTo(target);
                return target.ToArray();
            }
        }
        public IActionResult Details(int id)
        {
            var book = _database.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }

            // Get reviews for this book
            book.Reviews = _database.GetBookReviews(id);

            return View(book);
        }

        // Review controllers

        [Authorize]
        [HttpPost]
        public IActionResult AddReview(Review review)
        {
            try
            {
                // Get the current user's ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                review.UserName = User.Identity.Name;

                review.UserId = userId;

                string res = _database.InsertReview(review);
                TempData["msg"] = res == "OK" ? "Review added successfully!" : $"Error: {res}";
            }
            catch (Exception ex)
            {
                TempData["msg"] = ex.Message;
                throw;
            }

            return RedirectToAction(nameof(Details), new { id = review.BookId });
        }


    }
}
