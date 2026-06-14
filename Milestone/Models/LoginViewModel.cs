using System.ComponentModel.DataAnnotations;

namespace Milestone.Models {
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}