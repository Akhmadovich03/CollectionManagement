using CollectionManagement.Data;
using CollectionManagement.Models;
using CollectionManagement.Models.Enums;
using CollectionManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static CollectionManagement.Others.SHA3_256;

namespace CollectionManagement.Controllers;

public class ProfileController : Controller
{
    private readonly CollectionDbContext _context;

    public ProfileController(CollectionDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int userId, string? search)
    {
        var user = new User();

        if (userId != 0)
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        var query = _context.Collections.AsQueryable();

        if (search is not null)
        {
            query = query.Where(c => c.Name.Contains(search)
                                || (c.Description != null && c.Description.Contains(search))
                                || (c.CustomFields != null && c.CustomFields.Contains(search)));
        }

        var collections = await query
            .Where(c => c.UserId == user!.Id || user.IsAdmin)
            .Include(c => c.Items)
            .ToListAsync();

        var users = await _context.Users.ToListAsync();

        ViewBag.Categories = Enum.GetNames<Category>();
        ViewBag.Search = search;

        return View(model:  (user, collections, users));
    }

    public async Task<IActionResult> Edit(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        return View(model: user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int userId, string userName, string password, string confirmPassword, string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return Json(new { success = false, errorMessage = "User not found" });
        }

        if (user.Email != email)
        {
            var sameEmailedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (sameEmailedUser != null)
            {
                return Json(new { success = false, errorMessage = "User with this email already exists" });
            }
        }

        if (!string.IsNullOrEmpty(password) || !string.IsNullOrEmpty(confirmPassword))
        {
            if (password != confirmPassword)
            {
                return Json(new { success = false, errorPasswordMessage = "Passwords do not match" });
            }

            user.PasswordHash = await ComputeSha3_256Hash(password);
        }

        user.UserName = userName;
        user.Email = email;

        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }
}