using Microsoft.EntityFrameworkCore;

namespace MedWebApp.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name  { get; set; }
        public string Description { get; set; }
        public int DurationHours { get; set; }
        public int Price {get; set; }
        public List<string>? Requirements { get; set; } //TODO: consider refactoring into specic Requirement objects or booleans
        public List<string>? Disclaimers { get; set; }


        public Service()
        {
            
        }
        public Service(int id)
        {
            Id = id;
        }

        public Service(int id, string name, string description, int durationHours, int price)
        {
            Id = id;
            Name = name;
            Description = description;
            DurationHours = durationHours;
            Price = price;
        }

        public Service(int id, string name, string description, int durationHours, int price, List<string> requirements, List<string> disclaimers)
        {
            Id = id;
            Name = name;
            Description = description;
            DurationHours = durationHours;
            Price = price;
            Requirements = requirements;
            Disclaimers = disclaimers;
        }
    }
}
