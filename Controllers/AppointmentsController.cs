using MedWebApp.Data;
using MedWebApp.Models;
using MedWebApp.Models.ViewModels;
using MedWebApp.Views.Appointments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedWebApp.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AppointmentsController> _logger;
        private readonly IWebHostEnvironment _env;

        public AppointmentsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<AppointmentsController> logger,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _env = env;
        }

        // GET: Initial booking page
        public async Task<IActionResult> Book()
        {
            var vm = new AppointmentBookingViewModel
            {
                AvailableServices = await _context.Service.ToListAsync()
            };
            return View(vm);
        }

        // GET: Get available providers for a service
        [HttpGet]
        public async Task<IActionResult> GetProvidersForService(int serviceId)
        {
            var providers = await _context.Provider
                .Include(p => p.AvailableServices)
                .Where(p => p.AvailableServices.Any(s => s.Id == serviceId))
                .Select(p => new { p.Id, p.DisplayName })
                .ToListAsync();

            return Json(providers);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(int providerId, int serviceId, DateTime date)
        {
            try
            {
                // First verify all entities exist
                var provider = await _context.Provider
                    .FirstOrDefaultAsync(p => p.Id == providerId);
                if (provider == null)
                {
                    return BadRequest($"Provider with ID {providerId} not found");
                }

                var service = await _context.Service
                    .FirstOrDefaultAsync(s => s.Id == serviceId);
                if (service == null)
                {
                    return BadRequest($"Service with ID {serviceId} not found");
                }

                // Get existing appointments with explicit includes
                var existingAppointments = await _context.Appointment
                    .Include(a => a.Provider)
                    .Include(a => a.BookedService)
                    .Where(a => a.Provider.Id == providerId &&
                               a.DateTime.Date == date.Date)
                    .Select(a => new { a.DateTime, a.BookedService.DurationHours })
                    .ToListAsync();

                // Generate available time slots
                var availableSlots = new List<DateTime>();
                var startTime = date.Date.AddHours(5); // 5 AM
                var endTime = date.Date.AddHours(24);  // 12 AM or midnight

                while (startTime < endTime)
                {
                    bool isAvailable = true;

                    foreach (var appointment in existingAppointments)
                    {
                        if (startTime < appointment.DateTime.AddHours(appointment.DurationHours) &&
                            startTime.AddHours(service.DurationHours) > appointment.DateTime)
                        {
                            isAvailable = false;
                            break;
                        }
                    }

                    if (isAvailable)
                    {
                        availableSlots.Add(startTime);
                    }

                    startTime = startTime.AddMinutes(30);
                }

                return Json(availableSlots);
            }
            catch (Exception ex)
            {
                // Log the full exception
                _logger.LogError(ex, $"Error getting available time slots. ProviderId: {providerId}, ServiceId: {serviceId}, Date: {date}");

                // Return a more detailed error message in development
                if (_env.IsDevelopment())
                {
                    return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
                }

                return StatusCode(500, new { error = "An error occurred while getting available time slots." });
            }
        }

        // POST: Create the appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(AppointmentBookingViewModel vm)
        {
            if (!vm.SelectedServiceId.HasValue ||
                !vm.SelectedProviderId.HasValue ||
                !vm.SelectedDateTime.HasValue)
            {
                return BadRequest("Missing required fields");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound("User not found");

            var appointment = new Appointment
            {
                Customer = currentUser,
                Provider = await _context.Provider.FindAsync(vm.SelectedProviderId.Value),
                BookedService = await _context.Service.FindAsync(vm.SelectedServiceId.Value),
                DateTime = vm.SelectedDateTime.Value
            };

            _context.Appointment.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Confirmation), new { id = appointment.Id });
        }

        public async Task<IActionResult> Confirmation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointment
                .Include(a => a.Customer)
                .Include(a => a.Provider)
                .ThenInclude(p => p.User)
                .Include(a => a.BookedService)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.BookedService == null)
            {
                Service bookedService = await _context.Service
                    .Include(s => s.Description)
                    .FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);
                if (bookedService == null)
                {
                    return BadRequest("bookedService is null for appointmentId" + id);
                }
                appointment.BookedService = bookedService;
            }

            if (appointment.Provider == null)
            {
                Provider provider = await _context.Provider.FindAsync(appointment.ProviderId);
                if (provider == null)
                {
                    return BadRequest("provider is null for appointmentId" + id);
                }
                appointment.Provider = provider;
                appointment.Provider.User = await _userManager.FindByIdAsync(provider.UserId);
            }

            // Verify the current user is the appointment owner
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Id != appointment.Customer.Id)
            {
                return Forbid();
            }

            return View(appointment);
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            List<Appointment> appointments = await _context.Appointment
                .Include(a => a.BookedService)
                .Include(a => a.Provider)
                .ThenInclude(p => p.User)
                .ToListAsync();
            return View(appointments);
        }


        // GET: ShowCustomerAppointments
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> ShowCustomerAppointments()
        {
            List<Appointment> customerAppointments = await _context.Appointment
                .Include(a => a.BookedService)
                .Include(a => a.Provider)
                .ThenInclude(p => p.User)
                .Where(x => x.Customer.NormalizedUserName == HttpContext.User.Identity.Name)
                .ToListAsync();
            return View("Index", customerAppointments);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Appointment? appointment = await _context.Appointment
                .Include(a => a.BookedService)
                .Include(a => a.Provider)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var appointment = await _context.Appointment.FindAsync(id.Value);
            if (appointment == null)
            {
                return NotFound();
            }

            List<Service> allServices = await _context.Service.ToListAsync();
            AppointmentEditViewModel vm = new AppointmentEditViewModel 
            {
                AppointmentId = appointment.Id,
                AvailableServices = allServices,
                SelectedServiceId = appointment.ServiceId,
                SelectedProviderId = appointment.ProviderId,
                SelectedDateTime = appointment.DateTime
            };
            return View(vm);
        }

        // POST: Appointments/Edit/5
        // POST: Edit the appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppointmentEditViewModel vm)
        {
            if (!vm.AppointmentId.HasValue ||
                !vm.SelectedServiceId.HasValue ||
                !vm.SelectedProviderId.HasValue ||
                !vm.SelectedDateTime.HasValue)
            {
                return BadRequest("Missing required fields");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound("User not found");

            Provider updatedProvider = await _context.Provider.FindAsync(vm.SelectedProviderId.Value);
            if (updatedProvider == null) return NotFound("provider with id" + vm.SelectedProviderId.Value + " not found");
            Service updatedService = await _context.Service.FindAsync(vm.SelectedServiceId.Value);
            if (updatedService == null) return NotFound("Service with id" + vm.SelectedServiceId.Value + " not found");
            
            // var updatedAppointment = new Appointment
            // {
            //     Customer = currentUser,
            //     Provider = updatedProvider,
            //     BookedService = updatedService,
            //     DateTime = vm.SelectedDateTime.Value
            // };
            
            var appointmentToUpdate = await _context.Appointment.FindAsync(vm.AppointmentId.Value);
            
            if (appointmentToUpdate == null)
            {
                return BadRequest("No appointment found with id=" + vm.AppointmentId);
            }
            // Check if the currently logged user is the owner of the appointment being edited
            if (appointmentToUpdate.CustomerId != currentUser.Id || appointmentToUpdate.Customer != currentUser)
            {
                return BadRequest("Current user does not match appointment's customer.");
            }
            
            appointmentToUpdate.ServiceId = updatedService.Id;
            appointmentToUpdate.BookedService = updatedService;
            appointmentToUpdate.ProviderId = updatedProvider.Id;
            appointmentToUpdate.Provider = updatedProvider;
            appointmentToUpdate.DateTime = vm.SelectedDateTime.Value;
            
            //_context.Appointment.Entry(appointmentToUpdate).CurrentValues.SetValues(updatedAppointment);
            _context.Update(appointmentToUpdate);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Confirmation), new { id = appointmentToUpdate.Id });
        }


        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointment.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointment.Remove(appointment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointment.Any(e => e.Id == id);
        }
    }
}
