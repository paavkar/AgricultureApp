using System.ComponentModel.DataAnnotations;

namespace AgricultureApp.Application.DTOs
{
    public class LoginDto
    {
        [Required]
        public string EmailOrUsername { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }
}
