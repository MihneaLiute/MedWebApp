namespace MedWebApp.Models
{
    public class ServicePackage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }

        public List<Service> IncludedServices { get; set; }
        public List<string>? Requirements { get; set; } //TODO: consider refactoring into specic Requirement objects or booleans
        public List<string>? Disclaimers { get; set; }

        public List<string> GetRequirements() //TODO: consider refactoring so that an individual requirement is not just a string; see requirements in Service class
        {
            List<string> result = new List<string>();
            foreach (var service in IncludedServices)
            {
                result = result.Union(service.Requirements).ToList();
            }
             return result;
        }

        public List<string> GetDisclaimers()
        {
            List<string> result = new List<string>();
            foreach (var service in IncludedServices)
            {
                result = result.Union(service.Disclaimers).ToList();
            }
            return result;
        }

        public ServicePackage()
        {
            
        }

        public ServicePackage(int id)
        {
            Id = id;
        }

        public ServicePackage(int id, string name, string description, int price, List<Service> includedServices)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            IncludedServices = includedServices;
            Requirements = this.GetRequirements();
            Disclaimers = this.GetDisclaimers();
        }
    }
}
