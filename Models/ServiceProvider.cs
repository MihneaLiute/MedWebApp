using System.Globalization;

namespace MedWebApp.Models
{
    public class ServiceProvider : User
    {
        public string Type { get; set; } 
        public List <Service> AvailableServices { get; set; }
        public Calendar AvailabilitySchedule { get; set; } // TODO: replace Calendar class with something appropriate

        public ServiceProvider(string Email, string FirstName, string LastName, string PhoneNumber, string type, List<Service> services, Calendar availability) : base(Email, FirstName, LastName, PhoneNumber)
        {
            Type = type;
            AvailableServices = services;
            AvailabilitySchedule = availability;
        }
    }
}
