using CollectionManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollectionManagement.Models;
using static CollectionManagement.Others.SHA3_256;

namespace CollectionManagement.Controllers;

public class AuthController(CollectionDbContext context) : Controller
{
    private readonly CollectionDbContext _context = context;

    public IActionResult Index(string? message = null)
    {
        return View(model: message);
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        string passwordHash = await ComputeSha3_256Hash(password);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash);

        if (user is null)
        {
            return RedirectToAction("Index", new { message = "Email or password is incorrect. Please try again!" });
        }

        user.LastUpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Home", new {userId = user.Id});
    }

    public IActionResult ChangingPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangingPassword(string email, string newPassword, string confirmPassword)
    {
        if (!ModelState.IsValid)
        {
            return View(model: "Please, fill out all inputs");
        }

        string newPasswordHash = await ComputeSha3_256Hash(newPassword);
        string confirmPasswordHash = await ComputeSha3_256Hash(confirmPassword);

        if (newPasswordHash != confirmPasswordHash)
        {
            return View(model: "Passwords do not match.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (user is null)
        {
            return View(model: "User not found.");
        }

        user.LastUpdatedAt = DateTime.Now;
        user.PasswordHash = newPasswordHash;

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Auth", new { message = "Password changed successfully!" });
    }

    public IActionResult SignUp()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(string userName, string email, string password)
    {
        if (!ModelState.IsValid)
        {
            return View(model: "Please, fill out all inputs");
        }

        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            return View(model: "User with this email already exists!");
        }

        string passwordHash = await ComputeSha3_256Hash(password);

        User user = new()
        {
            UserName = userName,
            Email = email,
            PasswordHash = passwordHash,
            IsAdmin = email == "akhmadovich03@gmail.com",
            CreatedAt = DateTime.Now,
            LastUpdatedAt = DateTime.Now,
        };

        await _context.AddAsync(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", new { message = $"{user.UserName} successfully signed up!" });
    }

    public IActionResult Back()
    {
        return RedirectToAction("Index");
    }
}