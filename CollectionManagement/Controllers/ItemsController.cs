using CollectionManagement.Data;
using CollectionManagement.Models;
using CollectionManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CollectionManagement.Controllers;

public class ItemsController : Controller
{
    private readonly CollectionDbContext _context;
    private readonly IHubContext<CommentHub> _hubContext;

    public ItemsController(CollectionDbContext context, IHubContext<CommentHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<IActionResult> Index(int collectionId, int userId, string? search)
    {
        var user = new User();

        if (userId != 0)
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        var query = _context.Items.AsQueryable();

        if (search is not null)
        {
            query = query.Where(i => i.Name.Contains(search) || (i.CustomFieldsData != null && i.CustomFieldsData.Contains(search)));
        }

        var items = await query
            .Where(i => i.CollectionId == collectionId)
            .Include(i => i.Likes)
            .Include(i => i.Collection)
            .ThenInclude(c => c.User)
            .Select(i => new ItemDTO
            {
                Id = i.Id,
                Name = i.Name,
                Collection = i.Collection,
                CreatedAt = i.CreatedAt,
                LikesCount = i.Likes.Count(),
                IsUserLiked = _context.Likes.Any(l => l.ItemId == i.Id && l.LikedUserId == user!.Id)
            })
            .ToListAsync();

        var collection = await _context.Collections.FirstOrDefaultAsync(c => c.Id == collectionId);

        var viewModel = new ItemsPageViewModel()
        {
            Collection = collection!,
            Items = items,
            User = user
        };

        ViewBag.Search = search;

        return View(model: viewModel);
    }

    public async Task<IActionResult> Create(int collectionId, string name, string customFieldsData, int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        var item = new Item()
        {
            CollectionId = collectionId,
            Name = name,
            CustomFieldsData = customFieldsData,
            CreatedAt = DateTime.Now
        };

        await _context.Items.AddAsync(item);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", new { collectionId, userId = user!.Id });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleLike(int itemId, int userId, bool isLiked)
    {
        if (!isLiked)
        {
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.ItemId == itemId && l.LikedUserId == userId);

            if (like is null)
            {
                return Json(new { sucess = false });
            }

            _context.Remove(like);
        }
        else
        {
            var like = new Like()
            {
                ItemId = itemId,
                LikedUserId = userId
            };

            await _context.AddAsync(like);
        }

        await _context.SaveChangesAsync();

        var likesCount = await _context.Likes.CountAsync(l => l.ItemId == itemId);

        return Json(new { success = true, likesCount });
    }

    public async Task<IActionResult> Details(int itemId, int userId, string? search)
    {
        var user = new User();

        if (userId != 0)
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        var query = _context.Comments.Where(c => c.ItemId == itemId).Include(c => c.User).AsQueryable();

        if (search is not null)
        {
            query = query.Where(c => c.Content.Contains(search));
        }

        var comments = await query.ToListAsync();

        var item = await _context.Items
            .Include(i => i.Collection)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        string? temp = item!.CustomFieldsData;

        ViewBag.Search = search;

        return View(model: (user, item, comments));
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(int userId, int itemId, string content)
    {
        var comment = new Comment()
        {
            ItemId = itemId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        await _context.AddAsync(comment);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("ReceiveCommentUpdate");

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetComments(int itemId)
    {
        var comments = await _context.Comments
            .Where(c => c.ItemId == itemId)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return PartialView("_CommentsPartial", comments);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int itemId, int collectionId, int userId)
    {
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);

        if (item is null)
        {
            return Json(new { success = false, message = "Item not found." });
        }

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();

        var user = new User();

        if (userId != 0)
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        var redirectUrl = Url.Action("Index", "Items", new { collectionId, userId = user!.Id });

        return Json(new { success = true, redirectUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int itemId, int userId, string itemName, Dictionary<string, string> customFields)
    {
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);

        if (item is null)
        {
            return NotFound();
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        item.Name = itemName;
        
        customFields.Remove("itemName");
        customFields.Remove("itemId");
        customFields.Remove("userId");

        string customfieldsData = JsonConvert.SerializeObject(customFields);
        item.CustomFieldsData = customfieldsData;

        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { itemId, userId = user!.Id });
    }
}