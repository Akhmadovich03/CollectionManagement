using CollectionManagement.Data;
using CollectionManagement.Models.Enums;
using Microsoft.AspNetCore.Localization;
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

    [HttpPost]
    public async Task<IActionResult> EditLanguage(string language, int userId)
    {
        if (Enum.TryParse<Language>(language, out var languageType))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                return Json(new { success = false });
            }

            user.PageLanguage = languageType;

            await _context.SaveChangesAsync();

            var culture = user.PageLanguage == Language.Uzbek ? "uz" : "en";
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture))
            );

            return Json(new { success = true });
        }

        return Json(new { success = false, message = "Invalid language preference." });
    }
}