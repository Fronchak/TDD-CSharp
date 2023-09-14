using ClientTDDApi.Interfaces;
using ClientTDDApi.Entities;
using System.ComponentModel.DataAnnotations;

namespace ClientTDDApi.DTOs.Auth
{
    public class UserRegisterDTO : IValidatableObject
    {
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression("^\\w+([\\.-]?\\w+)*@\\w+([\\.-]?\\w+)*(\\.\\w{2,3})+$")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(4, ErrorMessage = "Password must have at least 4 characteres")]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords must match")]
        public string? ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            IUserRepository userRepository = validationContext.GetRequiredService<IUserRepository>();
            if (Email == null) yield break;
            Entities.User? user = userRepository.FindByEmail(Email);
            if (user != null)
            {
                ValidationResult validationResult = new ValidationResult("Email is already been used", new[] { nameof(Email) });
                yield return validationResult;
            }
        }
    }
}
