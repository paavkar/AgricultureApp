using System.ComponentModel.DataAnnotations;

namespace AgricultureApp.Application.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string UserName { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        [Required]
        public string Name { get; set; } = "";
    }
}
