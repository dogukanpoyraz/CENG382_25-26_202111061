using System;

namespace LabProject.Models
{
    /* Create a class named User with the following properties:
     * Username (string)
     * Password (string)
     * Role (string)
     * IsActive (bool)
     * CreatedAt (DateTime)
     */
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
