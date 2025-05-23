using Microsoft.EntityFrameworkCore;
using LabProject.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

/* AI Prompt: Create a C# ASP.NET Core web application that uses Razor Pages. 
    The application should also include a session management system to store user preferences. */

/* AI Prompt: Add Entity Framework Core to the ASP.NET Core web application. 
    Use SQL Server as the database provider and configure the connection string in appsettings.json. 
    Create a DbContext class named SchoolDbContext with a DbSet property for a Class entity. 
    The Class entity should have properties for Id, Name, PersonCount, Description, and IsActive. */

builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolDbConnection")));

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
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
