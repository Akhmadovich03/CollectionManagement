using CollectionManagement.Models.Enums;
using CollectionManagement.Models;
using CollectionManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollectionManagement.Data;
using Newtonsoft.Json;

namespace CollectionManagement.Controllers;

public class CollectionsController : Controller
{
    private readonly CollectionDbContext _context;
    private readonly BlobService _blobService;

    public CollectionsController(CollectionDbContext context, BlobService blobService)
    {
        _context = context;
        _blobService = blobService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
    string name,
    string? description,
    string category,
    int userId,
    string? customFieldsJson,
    IFormFile? imageFile)
    {
        var user = new User();

        if (userId != 0)
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index", new { userId = user!.Id });
        }

        string? imageURL = "https://cloud-atg.moph.go.th/quality/sites/default/files/default_images/default.png";

        if (imageFile != null && imageFile.Length > 0)
        {
            using var stream = imageFile.OpenReadStream();
            imageURL = await _blobService.UploadImageAsync(stream, imageFile.FileName);
        }

        var collection = new Collection
        {
            Name = name,
            Description = description,
            Category = Enum.Parse<Category>(category),
            UserId = userId,
            ImageURL = imageURL,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            CustomFields = customFieldsJson
        };

        await _context.Collections.AddAsync(collection);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Profile", new { userId = user!.Id });
    }

    private async Task<(User, Collection)> TakingDatasForView(int collectionId, int userId)
    {
        var user = new User();

        if (userId != 0)
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        var collection = await _context.Collections
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == collectionId);

        return (user!, collection!);
    }

    public async Task<IActionResult> Delete(int collectionId, int userId)
    {
        var model = await TakingDatasForView(collectionId, userId);

        return View(model: model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete((int collectionId, int userId) model)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == model.collectionId);

        _context.Remove(collection!);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Profile", new { model.userId });
    }

    public async Task<IActionResult> Edit(int collectionId, int userId)
    {
        var model = await TakingDatasForView(collectionId, userId);

        ViewBag.Categories = Enum.GetNames<Category>();

        return View(model: model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        int collectionId, 
        int userId,
        string name, 
        string category, 
        Dictionary<string, string> customFields, 
        string description,
        IFormFile? imageFile)
    {
        var collection = await _context.Collections.FirstOrDefaultAsync(c => c.Id == collectionId);

        string? imageURL = collection!.ImageURL;
        
        if (imageFile != null && imageFile.Length > 0)
        {
            using var stream = imageFile.OpenReadStream();
            imageURL = await _blobService.UploadImageAsync(stream, imageFile.FileName);
        }

        Category newCategory = Enum.Parse<Category>(category);
        
        customFields.Remove("collectionId");
        customFields.Remove("name");
        customFields.Remove("collectionId");
        customFields.Remove("description");
        
        string newCustomFields = JsonConvert.SerializeObject(customFields);

        collection!.Name = name;
        collection.Category = newCategory;
        collection.Description = description;
        collection.CustomFields = newCustomFields;
        collection.ImageURL = imageURL;

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Profile", new { userId });
    }
}