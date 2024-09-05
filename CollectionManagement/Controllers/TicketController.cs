using CollectionManagement.Data;
using CollectionManagement.Models;
using CollectionManagement.Models.ViewModels;
using CollectionManagement.Others;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using static CollectionManagement.Models.Constants;

namespace CollectionManagement.Controllers;

public class TicketController : Controller
{
    private readonly CollectionDbContext _context;

    public TicketController(CollectionDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Tickets(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var usersCollections = await _context.Collections
            .Where(c => c.UserId == userId)
            .Select(c => c.Name)
            .ToListAsync();

        ViewBag.Collections = usersCollections;
        ViewBag.UserEmail = user!.Email;

        var accountId = await GetUserAccountIdAsync(user.Email);

        var tickets = await GetTicketsAsync(accountId!);

        var model = new TicketsViewModel()
        {
            User = user,
            Tickets = tickets
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] TicketCreationModel model)
    {
        bool isUserCreated = false;
        var accountId = await GetUserAccountIdAsync(model.Reported);

        if (accountId is null)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Reported);

            accountId = await CreateJiraUserAsync(model.Reported, user!.UserName);

            isUserCreated = true;
        }

        var jiraIssue = new
        {
            fields = new
            {
                project = new { key = "CM" },
                summary = model.Summary,
                issuetype = new { name = "Task" },
                priority = new { name = model.Priority },
                customfield_10062 = model.Collection,
                customfield_10090 = model.Link,
                customfield_10092 = new { accountId }
            }
        };

        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{JIRA_EMAIL}:{JIRA_API_TOKEN}")));

        var content = new StringContent(JsonConvert.SerializeObject(jiraIssue), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"https://{JIRA_DOMAIN}/rest/api/2/issue", content);

        var temp = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var issueResponse = JsonConvert.DeserializeObject<JiraIssueResponse>(await response.Content.ReadAsStringAsync());

            int statusId = model.Status switch
            {
                "backlog" => 11,
                "selectedForDevelopment" => 21,
                "inProgress" => 31,
                _ => 41
            };

            var transitionContent = new StringContent(
                "{\"transition\": {\"id\": \"" + statusId + "\"}}",
                Encoding.UTF8,
                "application/json");

            var transitionResponse = await client.PostAsync($"https://{JIRA_DOMAIN}/rest/api/2/issue/{issueResponse?.Key}/transitions", transitionContent);

            StringBuilder resultMessage = new();

            if (isUserCreated)
            {
                resultMessage.Append("Created Jira account for you. Jira sends you invitation message to your innbox via e-mail");
            }

            resultMessage.Append($"\n<a href=\"https://{JIRA_DOMAIN}/browse/{issueResponse?.Key}\">You can see your ticket here</a> or in your profile");

            return Json(new { success = true, message = resultMessage.ToString() });
        }
        else
        {
            return Json(new { sucess = false });
        }
    }

    public async Task<string?> GetUserAccountIdAsync(string email)
    {
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{JIRA_EMAIL}:{JIRA_API_TOKEN}")));

        var response = await client.GetAsync($"https://{JIRA_DOMAIN}/rest/api/3/user/search?query={email}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        dynamic? users = JsonConvert.DeserializeObject(content);

        if (users!.Count == 0)
        {
            return null;
        }

        var accountId = users?[0].accountId;

        return accountId;
    }

    public async Task<string?> CreateJiraUserAsync(string email, string displayName)
    {
        var userCreationPayload = new
        {
            emailAddress = email,
            displayName,
            name = email.Split('@')[0],
            notification = "true",
            products = new[] { "jira-software" }
        };

        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{JIRA_EMAIL}:{JIRA_API_TOKEN}")));

        var content = new StringContent(JsonConvert.SerializeObject(userCreationPayload), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"https://{JIRA_DOMAIN}/rest/api/2/user", content);

        if (!response.IsSuccessStatusCode)
        {
            var tempContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(tempContent);

            return null;
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        dynamic? createdUser = JsonConvert.DeserializeObject(responseContent);

        return createdUser!.accountId;
    }

    public async Task<List<Ticket>> GetTicketsAsync(string accountId)
    {
        var jqlQuery = Uri.EscapeDataString($"\"reported[user picker (single user)]\" = \"{accountId}\" ORDER BY created DESC");
        var url = $"https://{JIRA_DOMAIN}/rest/api/2/search?jql={jqlQuery}";

        var authHeader = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{JIRA_EMAIL}:{JIRA_API_TOKEN}"));

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", authHeader);

        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        var jsonResponse = JObject.Parse(content);
        var tickets = jsonResponse["issues"]?
            .Select(issue => new Ticket
            {
                Key = issue["key"].ToString(),
                Summary = issue["fields"]!["summary"].ToString(),
                Priority = issue["fields"]!["priority"]!["name"].ToString(),
                Collection = issue["fields"]!["customfield_10062"]?.ToString() ?? "N/A",
                Link = issue["fields"]!["customfield_10090"]?.ToString() ?? "N/A",
                Reported = issue["fields"]!["customfield_10092"]?.ToString() ?? "N/A",
                Status = issue["fields"]!["status"]!["name"]?.ToString() ?? "N/A",
                TicketURL = $"https://{JIRA_DOMAIN}/browse/{issue["key"].ToString()}"
            }).ToList();

        return tickets!;
    }
}