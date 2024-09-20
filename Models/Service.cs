namespace MedWebApp.Models
{
    public class Service
    {
        public string Name  { get; set; }
        public string Description { get; set; }
        public int DurationHours { get; set; }
        public int Price {get; set; }
        public string Requirements { get; set; } //TODO: consider refactoring into specific booleans
        public string Disclaimers { get; set; }
    }
}
