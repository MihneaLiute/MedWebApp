namespace MedWebApp.Models
{
    public class ServicePackage
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }

        public List<Service> IncludedServices { get; set; }
        public HashSet<string> Requirements { get; set; } //TODO: consider refactoring into specic Requirement objects or booleans
        public HashSet<string> Disclaimers { get; set; }

        HashSet<string> GetRequirements() //TODO: consider refactoring so that an individual requirement is not just a string; see requirements in Service class
        {
            HashSet<string> result = new HashSet<string>();
            foreach (Service service in IncludedServices)
            {
                result.Union(service.Requirements);
            }
            return result;
        }

        public ServicePackage(string name, string description, int price, List<Service> includedServices)
        {
            Name = name;
            Description = description;
            Price = price;
            IncludedServices = includedServices;
            Requirements = new HashSet<string>();
            foreach (Service service in IncludedServices)
            {
                Requirements.Union(service.Requirements);
            }
            Disclaimers = new HashSet<string>();
            foreach (Service service in IncludedServices)
            {
                Disclaimers.Union(service.Disclaimers);
            }
        }
    }
}
