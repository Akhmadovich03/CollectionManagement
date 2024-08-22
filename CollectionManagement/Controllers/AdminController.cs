using CollectionManagement.Data;
using CollectionManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CollectionManagement.Controllers;

public class AdminController : Controller
{
    private readonly CollectionDbContext _context;

    public AdminController(CollectionDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int userId, string? search)
    {
        var query = _context.Users.AsQueryable();
        
        if (search is not null)
        {
            query = query.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));
        }

        var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);
        var users = await query.ToListAsync();

        ViewBag.Search = search;

        return View(model: (user, users));
    }

    [HttpPost]
    public async Task<IActionResult> Block(string userIds, int userId)
    {
        var ids = JsonConvert.DeserializeObject<int[]>(userIds);

        if (ids is null)
        {
            return RedirectToAction("Index", "Admin", new { userId });
        }

        bool isBlocked = false;

        foreach (var id in ids)
        {
            var tempUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (tempUser is null)
            {
                continue;
            }

            if (tempUser.Id == userId)
            {
                isBlocked = true;
            }

            tempUser.IsBlocked = true;
        }
        
        await _context.SaveChangesAsync();

        if (!isBlocked)
        {
            return RedirectToAction("Index","Admin", new { userId });
        }

        return RedirectToAction("Index", "Auth", new { message = "You are a blocked user. You are not allowed to use the system" });
    }

    [HttpPost]
    public async Task<IActionResult> Unlock(string userIds, int userId)
    {
        var ids = JsonConvert.DeserializeObject<int[]>(userIds);

        if (ids is null)
        {
            return RedirectToAction("Index", "Admin", new { userId });
        }

        foreach (var id in ids)
        {
            var tempUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (tempUser != null)
            {
                tempUser.IsBlocked = false;
            }
        }
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Admin", new { userId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string userIds, int userId)
    {
        var ids = JsonConvert.DeserializeObject<int[]>(userIds);

        if (ids is null)
        {
            return RedirectToAction("Index", "Admin", new { userId });
        }

        bool isDeleted = false;

        foreach (var id in ids)
        {
            var tempUser = await _context.Users
                .Include(u => u.Likes)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (tempUser is null)
            {
                continue;
            }

            if (tempUser.Id == userId)
            {
                isDeleted = true;
            }

            _context.Likes.RemoveRange(tempUser.Likes);
            _context.Users.Remove(tempUser);
        }

        await _context.SaveChangesAsync();

        if (!isDeleted)
        {
            return RedirectToAction("Index", "Admin", new { userId });
        }

        return RedirectToAction("Index", "Auth");
    }

    [HttpPost]
    public async Task<IActionResult> SetAsAdmin(string userIds, int userId)
    {
        var ids = JsonConvert.DeserializeObject<int[]>(userIds);

        if (ids is null)
        {
            return RedirectToAction("Index", "Admin", new { userId });
        }

        foreach (var id in ids)
        {
            var tempUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (tempUser != null)
            {
                tempUser.IsAdmin = true;
            }
        }
        
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Admin", new { userId });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromAdmin(string userIds, int userId)
    {
        var ids = JsonConvert.DeserializeObject<int[]>(userIds);

        if (ids is null)
        {
            return RedirectToAction("Index", "Admin", new { userId });
        }

        bool isAdmin = true;

        foreach (var id in ids)
        {
            var tempUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (tempUser is null)
            {
                continue;
            }

            if (tempUser.Id == userId)
            {
                isAdmin = false;
            }

            tempUser.IsAdmin = false;
        }

        await _context.SaveChangesAsync();

        if (isAdmin)
        {
            return RedirectToAction("Index", "Admin", new { userId });
        }

        return RedirectToAction("Index", "Home", new { userId });
    }
}