using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabProject.Pages
{
    public class LogoutModel : PageModel
    {
        /* AI Prompt: The page should clear the user's session and remove any authentication cookies. 
            After logging out, the user should be redirected to the login page.*/
        public IActionResult OnGet()
        {
            HttpContext.Session.Clear();

            Response.Cookies.Delete("username");
            Response.Cookies.Delete("token");
            Response.Cookies.Delete("session_id");

            return RedirectToPage("/Login");
        }
    }
}
