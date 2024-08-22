using CollectionManagement.Data;
using CollectionManagement.Models;
using CollectionManagement.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollectionManagement.Controllers;

public class UserPreferenceController : Controller
{
    private readonly CollectionDbContext _context;

    public UserPreferenceController(CollectionDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> EditTheme(string theme, int userId)
    {
        if (Enum.TryParse<Theme>(theme, out var themeType))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                return Json(new { success = false });
            }

            user.PageTheme = themeType;
            
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        return Json(new { success = false, message = "Invalid theme preference." });
    }
}