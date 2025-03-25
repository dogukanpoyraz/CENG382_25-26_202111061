using System.ComponentModel.DataAnnotations;

namespace LabProject.Models
{
    public class ClassInformationModel
    {
        private static int _idCounter = 1;

        public ClassInformationModel()
        {
            Id = _idCounter++;
        }

        public int Id { get; set; }

        [Required]
        public string ClassName { get; set; }

        [Required]
        [Range(1, 1000)]
        public int StudentCount { get; set; }

        public string Description { get; set; }
    }
}
