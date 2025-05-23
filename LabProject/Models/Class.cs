using System.ComponentModel.DataAnnotations;

/* AI Promtt: Create a C# class named Class with the following properties:
 - Id (int, primary key)
 - Name (string, required)
 - PersonCount (int, required)
 - Description (string)
 - IsActive (bool, required)
 The class should also include data annotations for validation */
namespace LabProject.Models
{
    public class Class
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int PersonCount { get; set; }

        public string Description { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
