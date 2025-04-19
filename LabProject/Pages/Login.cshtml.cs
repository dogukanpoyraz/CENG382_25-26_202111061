using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabProject.Models;
using System.Text.Json;

namespace LabProject.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnPost()
        {
            var usersFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "users.json");
            var json = System.IO.File.ReadAllText(usersFile);
            var users = JsonSerializer.Deserialize<List<User>>(json);

            var user = users.FirstOrDefault(u => u.Username == Username && u.Password == Password && u.IsActive);

            if (user != null)
            {
                var token = Guid.NewGuid().ToString();

                HttpContext.Session.SetString("username", user.Username);
                HttpContext.Session.SetString("token", token);
                HttpContext.Session.SetString("session_id", HttpContext.Session.Id);

                var options = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };

                Response.Cookies.Append("username", user.Username, options);
                Response.Cookies.Append("token", token, options);
                Response.Cookies.Append("session_id", HttpContext.Session.Id, options);

                return RedirectToPage("/Index");
            }

            ErrorMessage = "Username or password is incorrect or account is inactive.";
            return Page();
        }
    }
}
