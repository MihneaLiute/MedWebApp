using System.ComponentModel.DataAnnotations.Schema;

namespace MedWebApp.Models
{
    public class ProviderService
    {
        public int Id { get; set; }
        [ForeignKey("Provider")]
        public int ProviderId { get; set; }
        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public Provider Provider { get; set; } = null!;
        public Service Service { get; set; } = null!;

        public ProviderService()
        {
            
        }
    }
}
