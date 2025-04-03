using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabProject.Models;
using System.Collections.Generic;
using System.Linq;

namespace LabProject.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public ClassInformationModel ClassInfo { get; set; }

        public static List<ClassInformationModel> ClassList { get; set; } = new();
        public List<ClassInformationTable> FilteredList { get; set; } = new();

        public int CurrentPage { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        



        public void OnGet(string? keyword, int currentPage = 1)
        {
            CurrentPage = currentPage;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                FilteredList = new List<ClassInformationTable>();
                return;
            }

            var filtered = ClassList
                .Where(c =>
                    c.ClassName.Contains(keyword, System.StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(keyword, System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            TotalPages = (int)Math.Ceiling(filtered.Count / (double)PageSize);

            FilteredList = filtered
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new ClassInformationTable
                {
                    Id = c.Id,
                    ClassName = c.ClassName,
                    StudentCount = c.StudentCount,
                    Description = c.Description
                }).ToList();
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
        public IActionResult OnPostEdit(int id)
        {
            var item = ClassList.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                ClassInfo = new ClassInformationModel
                {
                    Id = item.Id,
                    ClassName = item.ClassName,
                    StudentCount = item.StudentCount,
                    Description = item.Description
                };
            }
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

        public IActionResult OnPostGenerateFakeData()
        {
            int currentMaxId = ClassList.Any() ? ClassList.Max(c => c.Id) : 0;
            Random rand = new();

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



        // Created by me - Returns the list for display in the Razor page
        public List<ClassInformationModel> DisplayList => ClassList;
    }
}
