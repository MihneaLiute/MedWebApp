using Microsoft.AspNetCore.Identity;

namespace MedWebApp.Models
{
    public class Provider
    {
        public int Id { get; set; }

        public IdentityUser User { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public List<ProviderService> ProviderServices { get; set; } = [];
        public List<Service> AvailableServices { get; set; } = [];

        public Provider()
        {
            
        }
    }
}
