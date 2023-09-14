using System.ComponentModel.DataAnnotations;

namespace ClientTDDApi.DTOs.Client
{
    public abstract class ClientInputDTO : IValidatableObject
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
        public string? Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
        [RegularExpression("^\\w+([\\.-]?\\w+)*@\\w+([\\.-]?\\w+)*(\\.\\w{2,3})+$")]
        public string? Email { get; set; }

        public abstract IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
    }
}
