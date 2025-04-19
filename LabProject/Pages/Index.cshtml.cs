using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabProject.Models;
using System.Collections.Generic;
using System.Linq;
using LabProject.Helpers; // Assuming this is where the Utils class is located
using System.Text;

namespace LabProject.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public ClassInformationModel ClassInfo { get; set; }
        public List<string> SelectedColumns { get; set; }
        public string SortColumn { get; set; }
        public bool SortAscending { get; set; }

        public static List<ClassInformationModel> ClassList { get; set; } = new();
        
        // Created by me - Used to store and manage filtered class data along with pagination properties
        // Supports displaying a specific subset of results based on current page and keyword filtering
        public List<ClassInformationTable> FilteredList { get; set; } = new();

        public List<ClassInformationModel> PagedDisplayList { get; set; } = new();

        public int CurrentPage { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // AI Prompt: "Write an OnGet handler in Razor Pages that filters a list of class data by keyword 
        // (searching both class name and description), paginates the results, 
        // and prepares a simplified list of results for displaying in a table. 
        // The method should take an optional search keyword and a current page number as parameters."
        public void OnGet(string? keyword, int currentPage = 1, string sortColumn = "Id", bool sortAscending = true)
        {

            if (ClassList.Count == 0)
            {
                OnPostGenerateFakeData();
            }

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

            var query = ClassList.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c =>
                    c.ClassName.Contains(keyword, System.StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(keyword, System.StringComparison.OrdinalIgnoreCase));
            }

            query = (sortColumn, sortAscending) switch
            {
                ("Id", true) => query.OrderBy(c => c.Id),
                ("Id", false) => query.OrderByDescending(c => c.Id),
                ("StudentCount", true) => query.OrderBy(c => c.StudentCount),
                ("StudentCount", false) => query.OrderByDescending(c => c.StudentCount),
                ("ClassName", true) => query.OrderBy(c => ExtractNumber(c.ClassName)),
                ("ClassName", false) => query.OrderByDescending(c => ExtractNumber(c.ClassName)),
                _ => query.OrderBy(c => c.Id)
            };

            TotalPages = (int)System.Math.Ceiling(query.Count() / (double)PageSize);

            var pageData = query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                FilteredList = pageData
                    .Select(c => new ClassInformationTable
                    {
                        Id = c.Id,
                        ClassName = c.ClassName,
                        StudentCount = c.StudentCount,
                        Description = c.Description
                    }).ToList();
                PagedDisplayList = new List<ClassInformationModel>();
            }
            else
            {
                PagedDisplayList = pageData;
                FilteredList = new List<ClassInformationTable>();
            }
        }



        // AI Prompt: "Write a handler method that adds a new ClassInformationModel to a static list with validation"
        public IActionResult OnPostAdd()
        {
            if (!ModelState.IsValid)
                return Page();

            ClassList.Add(new ClassInformationModel
            {
                ClassName = ClassInfo.ClassName,
                StudentCount = ClassInfo.StudentCount,
                Description = ClassInfo.Description
            });
            ReindexIds();

            return RedirectToPage();
        }

        // Created by me - Removes an item by ID and updates remaining IDs
        public IActionResult OnPostDelete(int id)
        {
            var item = ClassList.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                ClassList.Remove(item);
                ReindexIds();
            }
            return RedirectToPage();
        }

        // AI Prompt: "Pre-fill the form with data for the selected class item for editing"
        public IActionResult OnPostEdit(int id, string? keyword, int currentPage = 1)
        {
            List<ClassInformationModel> filteredList;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                filteredList = ClassList
                    .Where(c =>
                        c.ClassName.Contains(keyword, System.StringComparison.OrdinalIgnoreCase) ||
                        c.Description.Contains(keyword, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else
            {
                filteredList = ClassList;
            }

            int itemIndex = filteredList.FindIndex(c => c.Id == id);

            if (itemIndex != -1)
            {
                currentPage = (itemIndex / PageSize) + 1;

                var item = filteredList[itemIndex];
                ClassInfo = new ClassInformationModel
                {
                    Id = item.Id,
                    ClassName = item.ClassName,
                    StudentCount = item.StudentCount,
                    Description = item.Description
                };
            }

            OnGet(keyword, currentPage);

            return Page();
        }


        // AI Prompt: "Update the edited class information and refresh the static list"
        public IActionResult OnPostUpdate()
        {
            if (!ModelState.IsValid)
                return Page();

            var item = ClassList.FirstOrDefault(x => x.Id == ClassInfo.Id);
            if (item != null)
            {
                item.ClassName = ClassInfo.ClassName;
                item.StudentCount = ClassInfo.StudentCount;
                item.Description = ClassInfo.Description;
            }
            return RedirectToPage();
        }

        // Created by me - Cancels the edit process and reloads the page
        public IActionResult OnPostCancelEdit()
        {
            ClassInfo = null;
            return RedirectToPage();
        }
        
        // AI Prompt: "Recalculate and assign new IDs sequentially starting from 1"
        private void ReindexIds()
        {
            for (int i = 0; i < ClassList.Count; i++)
            {
                ClassList[i].Id = i + 1;
            }
        }

        // AI Prompt: "Generate a Razor Page handler that creates 100 fake class entries with random data and adds them to a static list."
        public IActionResult OnPostGenerateFakeData()
        {
            int currentMaxId = ClassList.Any() ? ClassList.Max(c => c.Id) : 0;
            System.Random rand = new();

            for (int i = 1; i <= 100; i++)
            {
                ClassList.Add(new ClassInformationModel
                {
                    Id = currentMaxId + i,
                    ClassName = $"Class {currentMaxId + i}",
                    StudentCount = rand.Next(10, 100),
                    Description = $"This is a description for Class {currentMaxId + i}."
                });
            }

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

        public IActionResult OnPostExportJson(string exportType, List<string> selectedColumns, List<int> selectedIds)
        {
            List<ClassInformationModel> dataToExport;

            if (exportType == "all")
            {
                dataToExport = ClassList;
                selectedColumns = null;
            }

            else
            {
                var filtered = FilteredList.Any()
                    ? FilteredList.Select(c => new ClassInformationModel
                    {
                        Id = c.Id,
                        ClassName = c.ClassName,
                        StudentCount = c.StudentCount,
                        Description = c.Description
                    }).ToList()
                    : ClassList;

                dataToExport = selectedIds != null && selectedIds.Any()
                    ? filtered.Where(x => selectedIds.Contains(x.Id)).ToList()
                    : filtered;
            }

            var json = exportType == "all"
                ? Utils.Instance.ExportToJson(dataToExport)
                : Utils.Instance.ExportToJson(dataToExport, selectedColumns);

            var bytes = Encoding.UTF8.GetBytes(json);
            var fileName = $"export_{exportType}_{DateTime.Now:yyyyMMdd_HHmmss}.json";

            return File(bytes, "application/json", fileName);
        }


        // Created by me - Returns the list for display in the Razor page
        public List<ClassInformationModel> DisplayList => ClassList;
    }
}
