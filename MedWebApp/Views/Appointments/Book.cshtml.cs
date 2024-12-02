﻿using MedWebApp.Models;

namespace MedWebApp.Views.Appointments
{
    public class AppointmentBookingViewModel
    {
        public int? SelectedServiceId { get; set; }
        public int? SelectedProviderId { get; set; }
        public DateTime? SelectedDateTime { get; set; }

        // For dropdown population
        public List<Service> AvailableServices { get; set; } = new();
        public List<Provider> AvailableProviders { get; set; } = new();
        public List<DateTime> AvailableTimeSlots { get; set; } = new();
    }
}