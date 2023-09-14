using ClientTDDApi.Entities;
using ClientTDDApi.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ClientTDDApi.DTOs.Client
{
    public class ClientInsertDTO : ClientInputDTO
    {
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            IEnumerable<ValidationResult> result = new List<ValidationResult>();
            IClientRepository clientRepository = validationContext.GetRequiredService<IClientRepository>();
            Entities.Client? client = clientRepository.FindByEmail(Email);
            if(client != null)
            {
                ValidationResult validationResult = new ValidationResult("Email is already been used", new[] { nameof(Email) });
                yield return validationResult;
            }
        }
    }
}
