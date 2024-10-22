namespace MedWebApp.Models
{
    public class Service
    {
        public string Name  { get; set; }
        public string Description { get; set; }
        public int DurationHours { get; set; }
        public int Price {get; set; }
        public HashSet<string> Requirements { get; set; } //TODO: consider refactoring into specic Requirement objects or booleans
        public HashSet<string> Disclaimers { get; set; }

        public Service(string name, string description, int duration, int price, HashSet<string> requirements, HashSet<string> disclaimers)
        {
            Name = name;
            Description = description;
            DurationHours = duration;   
            Price = price;
            Requirements = requirements;
            Disclaimers = disclaimers;
        }
    }
}
