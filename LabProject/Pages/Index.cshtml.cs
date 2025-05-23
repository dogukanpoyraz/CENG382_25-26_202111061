using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabProject.Models;
using System.Collections.Generic;
using System.Linq;
using LabProject.Helpers; // Assuming this is where the Utils class is located
using System.Text;
using LabProject.Data;
using Microsoft.EntityFrameworkCore;

namespace LabProject.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public ClassInformationModel ClassInfo { get; set; }
        public List<string> SelectedColumns { get; set; }
        public string SortColumn { get; set; }
        public bool SortAscending { get; set; }

        // Created by me - Used to store and manage filtered class data along with pagination properties
        // Supports displaying a specific subset of results based on current page and keyword filtering
        public List<ClassInformationTable> FilteredList { get; set; } = new();

        public List<ClassInformationModel> PagedDisplayList { get; set; } = new();

        public int CurrentPage { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
        {
            _context = context;
        }


        // AI Prompt: "Write an OnGet handler in Razor Pages that filters a list of class data by keyword 
        // (searching both class name and description), paginates the results, 
        // and prepares a simplified list of results for displaying in a table. 
        // The method should take an optional search keyword and a current page number as parameters."
        public async Task OnGetAsync(string? keyword, int currentPage = 1, string sortColumn = "Id", bool sortAscending = true)
        {
            TrySeedFakeData();

            var cookieUsername = Request.Cookies["username"];
            var cookieToken = Request.Cookies["token"];
            var cookieSessionId = Request.Cookies["session_id"];

            var sessionUsername = HttpContext.Session.GetString("username");
            var sessionToken = HttpContext.Session.GetString("token");
            var sessionId = HttpContext.Session.GetString("session_id");

            if (string.IsNullOrEmpty(cookieUsername) || string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(cookieSessionId) ||
                string.IsNullOrEmpty(sessionUsername) || string.IsNullOrEmpty(sessionToken) || string.IsNullOrEmpty(sessionId) ||
                cookieUsername != sessionUsername || cookieToken != sessionToken || cookieSessionId != sessionId)
            {
                TempData["AuthError"] = "You must be logged in to view this page.";
                Response.Redirect("/Login");
                return;
            }

            CurrentPage = currentPage;
            SortColumn = sortColumn;
            SortAscending = sortAscending;

            var query = _context.Classes
            .Where(c => c.IsActive)
            .AsQueryable();


            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c =>
                    c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            query = (sortColumn, sortAscending) switch
            {
                ("Id", true) => query.OrderBy(c => c.Id),
                ("Id", false) => query.OrderByDescending(c => c.Id),
                ("StudentCount", true) => query.OrderBy(c => c.PersonCount),
                ("StudentCount", false) => query.OrderByDescending(c => c.PersonCount),
                ("ClassName", true) => query.OrderBy(c => ExtractNumber(c.Name)),
                ("ClassName", false) => query.OrderByDescending(c => ExtractNumber(c.Name)),
                _ => query.OrderBy(c => c.Id)
            };

            var totalItemCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItemCount / (double)PageSize);

            var pageData = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                FilteredList = pageData
                    .Select(c => new ClassInformationTable
                    {
                        Id = c.Id,
                        ClassName = c.Name,
                        StudentCount = c.PersonCount,
                        Description = c.Description
                    }).ToList();
                PagedDisplayList = new List<ClassInformationModel>();
            }
            else
            {
                PagedDisplayList = pageData.Select(c => new ClassInformationModel
                {
                    Id = c.Id,
                    ClassName = c.Name,
                    StudentCount = c.PersonCount,
                    Description = c.Description
                }).ToList();

                FilteredList = new List<ClassInformationTable>();
            }
        }

        // AI Prompt: "Write a handler method that adds a new ClassInformationModel to a static list with validation"
        public IActionResult OnPostAdd()
        {
            if (!ModelState.IsValid)
                return Page();

            var newClass = new Class
            {
                Name = ClassInfo.ClassName,
                PersonCount = ClassInfo.StudentCount,
                Description = ClassInfo.Description,
                IsActive = true
            };

            _context.Classes.Add(newClass);
            _context.SaveChanges();

            return RedirectToPage();
        }


        // Created by me - Removes an item by ID and updates remaining IDs
        public IActionResult OnPostDelete(int id)
        {
            var item = _context.Classes.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                item.IsActive = false;
                _context.SaveChanges();
            }

            return RedirectToPage();
        }



        // AI Prompt: "Pre-fill the form with data for the selected class item for editing"
        public async Task<IActionResult> OnPostEditAsync(int id, string? keyword, int currentPage = 1)
        {
            var query = _context.Classes
            .Where(c => c.IsActive)
            .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c =>
                    c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            var filteredList = await query.ToListAsync();
            int itemIndex = filteredList.FindIndex(c => c.Id == id);

            if (itemIndex != -1)
            {
                currentPage = (itemIndex / PageSize) + 1;

                var item = filteredList[itemIndex];
                ClassInfo = new ClassInformationModel
                {
                    Id = item.Id,
                    ClassName = item.Name,
                    StudentCount = item.PersonCount,
                    Description = item.Description
                };
            }

            await OnGetAsync(keyword, currentPage);

            return Page();
        }



        // AI Prompt: "Update the edited class information and refresh the static list"
        public IActionResult OnPostUpdate()
        {
            if (!ModelState.IsValid)
                return Page();

            var item = _context.Classes.FirstOrDefault(x => x.Id == ClassInfo.Id);
            if (item != null)
            {
                item.Name = ClassInfo.ClassName;
                item.PersonCount = ClassInfo.StudentCount;
                item.Description = ClassInfo.Description;

                _context.SaveChanges();
            }

            return RedirectToPage();
        }


        // Created by me - Cancels the edit process and reloads the page
        public IActionResult OnPostCancelEdit()
        {
            ClassInfo = null;
            return RedirectToPage();
        }

        // AI Prompt: "Generate a Razor Page handler that creates 100 fake class entries with random data and adds them to a static list."
        public IActionResult OnPostGenerateFakeData()
        {
            if (_context.Classes.Any())
            {
                return RedirectToPage();
            }

            var rand = new Random();

            for (int i = 1; i <= 100; i++)
            {
                _context.Classes.Add(new Class
                {
                    Name = $"Class {i}",
                    PersonCount = rand.Next(10, 100),
                    Description = $"This is a description for Class {i}.",
                    IsActive = true
                });
            }

            _context.SaveChanges();

            return RedirectToPage();
        }


        // Created by me - Extracts trailing number from class name like "Class 42"
        private int ExtractNumber(string className)
        {
            if (string.IsNullOrEmpty(className))
                return 0;

            var match = System.Text.RegularExpressions.Regex.Match(className, @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }

        /*AI Prompt: "Create a Razor Page handler that exports the filtered or full list of class data to JSON format.
            The method should take an export type parameter to determine whether to export the filtered list or the full list.
            The exported JSON file should be downloadable with a timestamp in the filename."
            The method should also handle the case where no data is available for export gracefully."
            The export type can be "filtered" or "full". */

        public async Task<IActionResult> OnPostExportJsonAsync(string exportType, List<string> selectedColumns, List<int> selectedIds)
        {
            List<ClassInformationModel> dataToExport;

            if (exportType == "all")
            {
                var allEntities = await _context.Classes
                .Where(c => c.IsActive) 
                .ToListAsync();


                dataToExport = allEntities.Select(c => new ClassInformationModel
                {
                    Id = c.Id,
                    ClassName = c.Name,
                    StudentCount = c.PersonCount,
                    Description = c.Description
                }).ToList();

                selectedColumns = null;
            }
            else
            {
                var filteredQuery = _context.Classes.AsQueryable();

                if (selectedIds != null && selectedIds.Any())
                {
                    filteredQuery = filteredQuery.Where(c => selectedIds.Contains(c.Id));
                }

                var filteredEntities = await filteredQuery.ToListAsync();

                dataToExport = filteredEntities.Select(c => new ClassInformationModel
                {
                    Id = c.Id,
                    ClassName = c.Name,
                    StudentCount = c.PersonCount,
                    Description = c.Description
                }).ToList();
            }

            var json = exportType == "all"
                ? Utils.Instance.ExportToJson(dataToExport)
                : Utils.Instance.ExportToJson(dataToExport, selectedColumns);

            var bytes = Encoding.UTF8.GetBytes(json);
            var fileName = $"export_{exportType}_{DateTime.Now:yyyyMMdd_HHmmss}.json";

            return File(bytes, "application/json", fileName);
        }

        private void TrySeedFakeData()
        {
            if (_context.Classes.Any())
                return;

            var rand = new Random();
            for (int i = 1; i <= 100; i++)
            {
                _context.Classes.Add(new Class
                {
                    Name = $"Class {i}",
                    PersonCount = rand.Next(10, 100),
                    Description = $"This is a description for Class {i}.",
                    IsActive = true
                });
            }
            _context.SaveChanges();
        }
    }   
}
