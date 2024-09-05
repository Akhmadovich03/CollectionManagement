using CollectionManagement.Data;
using CollectionManagement.Models;
using CollectionManagement.Models.Enums;
using CollectionManagement.Models.ViewModels;
using CollectionManagement.Others;
using CollectionManagement.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using static CollectionManagement.Models.Constants;

namespace CollectionManagement.Controllers;

public class HomeController : Controller
{
    private readonly CollectionDbContext _context;
    private readonly BlobService _blobService;

    public HomeController(CollectionDbContext context, BlobService blobService)
    {
        _context = context;
        _blobService = blobService;
    }

    public async Task<IActionResult> Index(int userId, string? search)
    {
        var user = new User();

        if (userId != 0)
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        var collectionQuery = _context.Collections.AsQueryable();
        var itemQuery = _context.Items.AsQueryable();

        if (search is not null)
        {
            collectionQuery = collectionQuery.Where(c => c.Name.Contains(search)
                                || (c.Description != null && c.Description.Contains(search))
                                || (c.CustomFields != null && c.CustomFields.Contains(search)));

            itemQuery = itemQuery.Where(i => i.Name.Contains(search) || (i.CustomFieldsData != null && i.CustomFieldsData.Contains(search)));
        }

        var collections = await collectionQuery
            .Include(c => c.Items)
            .OrderByDescending(c => c.Items.Count())
            .Take(5)
            .ToListAsync();

        var items = await itemQuery
            .Include(i => i.Likes)
            .Include(i => i.Collection)
            .Select(i => new ItemDTO
            {
                Id = i.Id,
                Name = i.Name,
                CreatedAt = i.CreatedAt,
                LikesCount = i.Likes.Count(),
                Collection = i.Collection,
                IsUserLiked = _context.Likes.Any(l => l.ItemId == i.Id && l.LikedUserId == user!.Id)
            })
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var usersCollections = await _context.Collections
            .Where(c => c.UserId == userId)
            .Select(c => c.Name)
            .ToListAsync();

        ViewBag.Collections = usersCollections;
        ViewBag.UserEmail = user!.Email;
        ViewBag.Search = search;

        MainPageViewModel viewModel = new()
        {
            SignedInUser = user,
            Collections = collections,
            Items = items
        };

        return View(model: viewModel);
    }
}