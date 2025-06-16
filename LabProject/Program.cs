using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using LabProject.Data;
using LabProject.Models;

var builder = WebApplication.CreateBuilder(args);


/* AI Prompt: Create a C# ASP.NET Core web application that uses Razor Pages. 
    The application should also include a session management system to store user preferences. */

/* AI Prompt: Add Entity Framework Core to the ASP.NET Core web application. 
    Use SQL Server as the database provider and configure the connection string in appsettings.json. 
    Create a DbContext class named SchoolDbContext with a DbSet property for a Class entity. 
    The Class entity should have properties for Id, Name, PersonCount, Description, and IsActive. */


builder.Services.AddRazorPages();

builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolDbConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<SchoolDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
