using Azure.Storage.Blobs;
using CollectionManagement.Data;
using CollectionManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace CollectionManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSignalR().AddAzureSignalR(builder.Configuration.GetConnectionString("AzureSignalRConnectionString"));
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<CollectionDbContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetConnectionString("AppConnectionString")));
            builder.Services.AddSingleton<BlobService>();

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBPh8sVXJyS0d+X1RPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9nSXZTfkRnXH1ddHxTR2I=;Mgo+DSMBMAY9C3t2U1hhQlJBfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTX5bdk1iXXpcc3BdQGFe");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<CommentHub>("/commentHub");
            app.Run();
        }
    }
}
