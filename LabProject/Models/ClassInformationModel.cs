// AI Prompt: "Create a C# model class for a Razor Pages application to represent class/course data with validation attributes. 
// The class should include properties like Id (auto-incremented), ClassName (required), StudentCount (required, in range), and Description. 
// The class will be used for in-memory data storage in a static list."

using System.ComponentModel.DataAnnotations;

namespace LabProject.Models
{
    public class ClassInformationModel
    {
        public int Id { get; set; }

        [Required]
        public string ClassName { get; set; }

        [Required]
        [Range(1, 1000)]
        public int StudentCount { get; set; }

        public string Description { get; set; }
    }
}
