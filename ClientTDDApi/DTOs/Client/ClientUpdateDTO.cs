using ClientTDDApi.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ClientTDDApi.DTOs.Client
{
    public class ClientUpdateDTO : ClientInputDTO
    {
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            IEnumerable<ValidationResult> result = new List<ValidationResult>();
            IClientRepository clientRepository = validationContext.GetRequiredService<IClientRepository>();
            IHttpContextAccessor acessor = validationContext.GetRequiredService<IHttpContextAccessor>();
            HttpContext context = acessor.HttpContext;
            RouteData routeData = context.GetRouteData();
            RouteValueDictionary routeValues = routeData.Values;
            object? idObj = routeValues.GetValueOrDefault("id");
            int? id = null;
            try
            {
                id = int.Parse(idObj?.ToString() ?? "");
            }
            catch (Exception) { }

            Entities.Client? client = clientRepository.FindByEmail(Email);
            if (client != null && id != null && !client.Id.Equals(id))
            {
                ValidationResult validationResult = new ValidationResult("Email is already been used", new[] { nameof(Email) });
                yield return validationResult;
            }
        }
    }
}
