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

        public void OnGet()
        {
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

        // Created by me - Returns the list for display in the Razor page
        public List<ClassInformationModel> DisplayList => ClassList;
    }
}
