using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using JetBrains.Annotations;
using MedWebApp.Controllers;
using MedWebApp.Data;
using MedWebApp.Models;
using MedWebApp.Views.Appointments;

namespace MedWebApp.Tests;

[TestSubject(typeof(AppointmentsController))]
public class AppointmentsControllerTest : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly AppointmentsController _appointmentsController;
    
    public AppointmentsControllerTest()
    {
        // Create and open a SQLite in-memory connection
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Setup DbContext options for SQLite in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Create the context with SQLite
        _context = new ApplicationDbContext(options);
        
        // Ensure database is created
        _context.Database.EnsureCreated();
        
        // Create mocks for user role store and user manager
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        var userRoleStoreMock = userStoreMock.As<IUserRoleStore<IdentityUser>>();

        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            userRoleStoreMock.Object,
            null, null, null, null, null, null, null, null
        ) { CallBase = true };

        var loggerMock = new Mock<ILogger<AppointmentsController>>();
        var webEnvMock = new Mock<IWebHostEnvironment>();

        // Create controller with real context and mocked dependencies
        _appointmentsController = new AppointmentsController(
            _context, 
            _userManagerMock.Object, 
            loggerMock.Object, 
            webEnvMock.Object
        );
    }

    private void SeedStandardTestData()
    {
        // Create and add a test service
        var service1 = new Service
        {
            Id = 1, 
            Name = "Test Service with Providers",
            Description = "Placeholder Description",
            DurationHours = 1,
            Price = 100
        };
        _context.Service.Add(service1);
        var service2 = new Service
        {
            Id = 2, 
            Name = "Test Service with one Provider",
            Description = "Placeholder Description",
            DurationHours = 2,
            Price = 200
        };
        _context.Service.Add(service2);
        var service3 = new Service
        {
            Id = 3, 
            Name = "Test Service without Providers",
            Description = "Placeholder Description",
            DurationHours = 3,
            Price = 300
        };
        _context.Service.Add(service2);
        // Create and add test IdentityUsers for customer and provider profiles
        var customerA = new IdentityUser("Customer A");
        var customerB = new IdentityUser("Customer B");
        var providerA = new IdentityUser("Provider A");
        var providerD = new IdentityUser("Provider D");
        var admin = new IdentityUser("Admin");
        _context.Users.Add(customerA);
        _context.Users.Add(customerB);
        _context.Users.Add(providerA);
        _context.Users.Add(providerD);
        _context.Users.Add(admin);
        // Mock inclusion in admin role for the users
        _userManagerMock.Setup(m => m.IsInRoleAsync(admin, "admin"))
            .ReturnsAsync(true);
        _userManagerMock.Setup(m => m.IsInRoleAsync(customerA, "admin"))
            .ReturnsAsync(false);
        _userManagerMock.Setup(m => m.IsInRoleAsync(customerB, "admin"))
            .ReturnsAsync(false);
        _userManagerMock.Setup(m => m.IsInRoleAsync(providerA, "admin"))
            .ReturnsAsync(false);
        _userManagerMock.Setup(m => m.IsInRoleAsync(providerD, "admin"))
            .ReturnsAsync(false);
        // Create test providers
        var providerProfileA = new Provider 
        { 
            Id = 1, 
            UserId = providerA.Id,
            DisplayName = "Provider A",
            Type = "placeholder",
            AvailableServices = new List<Service> { service1, service2 }
        };
        var providerProfileD = new Provider 
        { 
            Id = 2, 
            UserId = providerD.Id,
            DisplayName = "Provider D",
            Type = "placeholder",
            AvailableServices = new List<Service> { service1 }
        };
        // Add test providers
        _context.Provider.Add(providerProfileA);
        _context.Provider.Add(providerProfileD);
        
        //Create and add test appointments
        var appointment1 = new Appointment
        {
            Id = 1,
            Customer = customerA,
            CustomerId = customerA.Id,
            Provider = providerProfileA,
            ProviderId = providerProfileA.Id,
            BookedService = service1,
            ServiceId = service1.Id,
            DateTime = DateTime.Today.AddDays(10).AddHours(8) // 10 days from now at 8am
        };
        _context.Appointment.Add(appointment1);

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetProvidersForService_ShouldReturnProviders_WhenServiceExistsAndHasProviders()
    {
        // Arrange
        SeedStandardTestData();
        int serviceId = 1;

        // Act
        var result = await _appointmentsController.GetProvidersForService(serviceId);

        // Assert
        result.Should().BeOfType<JsonResult>();
        var jsonResult = result as JsonResult;
        
        var expectedProviders = new[]
        {
            new { Id = 1, DisplayName = "Provider A" },
            new { Id = 2, DisplayName = "Provider D" }
        };
        
        jsonResult.Value.Should().BeEquivalentTo(expectedProviders);
    }
    
    [Fact]
    public async Task GetProvidersForService_ShouldReturnEmptyList_WhenServiceDoesNotExist()
    {
        // Arrange
        SeedStandardTestData();
        int serviceId = 5;

        // Act
        var result = await _appointmentsController.GetProvidersForService(serviceId);

        // Assert
        result.Should().BeOfType<JsonResult>();
        var jsonResult = result as JsonResult;

        var expectedProviders = new List<object>();
        
        jsonResult.Value.Should().BeEquivalentTo(expectedProviders);
    }
    
    [Fact]
    public async Task GetProvidersForService_ShouldReturnEmptyList_WhenServiceExistsButHasNoProviders()
    {
        // Arrange
        SeedStandardTestData();
        int serviceId = 3;

        // Act
        var result = await _appointmentsController.GetProvidersForService(serviceId);

        // Assert
        result.Should().BeOfType<JsonResult>();
        var jsonResult = result as JsonResult;

        var expectedProviders = new List<object>();
        
        jsonResult.Value.Should().BeEquivalentTo(expectedProviders);
    }

    [Fact]
    public async Task GetAvailableTimeSlots_ShouldReturnTimeslots_WhenProviderIsAvailable()
    {
        // Arrange
        SeedStandardTestData();
        int serviceId = 1;
        int providerId = 1;
        DateTime date = DateTime.Today;
        
        // Act
        var result = await _appointmentsController.GetAvailableTimeSlots(providerId, serviceId, date);
        // Assert
        result.Should().BeOfType<JsonResult>();
        var jsonResult = result as JsonResult;
        var expectedTimeSlots = new List<DateTime>();
        var startTime = date.Date.AddHours(5);
        var endTime = date.Date.AddHours(24);
        for (var time = startTime; time < endTime; time = time.AddMinutes(30))
        {
            expectedTimeSlots.Add(time);
        }
        jsonResult.Value.Should().BeEquivalentTo(expectedTimeSlots);
    }
    
    [Fact]
    public async Task GetAvailableTimeSlots_ShouldReturnRestrictedTimeslots_WhenProviderHasAppointmentThatDay()
    {
        // Arrange
        SeedStandardTestData();
        int serviceId = 2;
        int providerId = 1;
        DateTime date = DateTime.Today.AddDays(10);
        
        // Act
        var result = await _appointmentsController.GetAvailableTimeSlots(providerId, serviceId, date);
        // Prepare Expected Result
        var expectedTimeSlots = new List<DateTime>();
        var startTime = date.Date.AddHours(5);
        var endTime = date.Date.AddHours(24);
        for (var time = startTime; time < endTime; time = time.AddMinutes(30))
        {
            expectedTimeSlots.Add(time);
        }
        // When getting available timeslots for a 2 hour appointment on a day with a different 1 hour appointment at 8am
        // The timeslots 1 hour after the existing booking should be unavailable
        expectedTimeSlots.Remove(date.Date.AddHours(8));
        expectedTimeSlots.Remove(date.Date.AddHours(8).AddMinutes(30));
        // And the timeslots less than 2 hours before the existing booking should be unavailable
        expectedTimeSlots.Remove(date.Date.AddHours(7).AddMinutes(30));
        expectedTimeSlots.Remove(date.Date.AddHours(7));
        expectedTimeSlots.Remove(date.Date.AddHours(6).AddMinutes(30));
        // Assert
        result.Should().BeOfType<JsonResult>();
        var jsonResult = result as JsonResult;
        jsonResult.Value.Should().BeEquivalentTo(expectedTimeSlots);
    }

    [Fact]
    public async Task Book_ShouldReturnNotFound_WhenNoUserLoggedIn()
    {
        // Arrange
        SeedStandardTestData();
        _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((IdentityUser)null);
        // Prepare the appointment data
        var appointmentDataVM = new AppointmentBookingViewModel
        {
            SelectedServiceId = 2,
            SelectedProviderId = 1,
            SelectedDateTime = DateTime.Today.AddDays(11).AddHours(6) // free timeslot
        };
        // Act
        var result = await _appointmentsController.Book(appointmentDataVM);
        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _context.Appointment.Count().Should().Be(1);
    }
    
    [Fact]
    public async Task Book_ShouldReturnBadRequest_WhenInvalidData()
    {
        // Arrange
        SeedStandardTestData();
        // Make sure the right customer is trying to book
        IdentityUser customerA = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Customer A");
        _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(customerA);
        // Prepare the appointment data
        var appointmentDataVM = new AppointmentBookingViewModel
        {
            SelectedServiceId = 2,
            //SelectedProviderId = 1, // No provider selected in vm
            SelectedDateTime = DateTime.Today.AddDays(11).AddHours(6) // free timeslot
        };
        // Act
        var result = await _appointmentsController.Book(appointmentDataVM);
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _context.Appointment.Count().Should().Be(1);
    }
    
    [Fact]
    public async Task Book_ShouldReturnBadRequest_WhenBusyTimeSlot()
    {
        // Arrange
        SeedStandardTestData();
        // Make sure the right customer is trying to book
        IdentityUser customerA = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Customer A");
        _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(customerA);
        // Prepare the appointment data
        var appointmentDataVM = new AppointmentBookingViewModel
        {
            SelectedServiceId = 2,
            SelectedProviderId = 1,
            SelectedDateTime = DateTime.Today.AddDays(10).AddHours(8) // 10 days from now at 8am, when provider A is already booked
        };
        // Act
        var result = await _appointmentsController.Book(appointmentDataVM);
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _context.Appointment.Count().Should().Be(1);
    }
    
    [Fact]
    public async Task Book_ShouldCreateAppointment_WhenValidData()
    {
        // Arrange
        SeedStandardTestData();
        // Make sure the right customer is trying to book
        IdentityUser customerA = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Customer A");
        _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(customerA);
        // Prepare the appointment data
        var appointmentDataVM = new AppointmentBookingViewModel
        {
            SelectedServiceId = 2,
            SelectedProviderId = 1,
            SelectedDateTime = DateTime.Today.AddDays(10).AddHours(9)
        };
        // Act
        var result = await _appointmentsController.Book(appointmentDataVM);
        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.ActionName.Should().Be("Confirmation");
        redirectToActionResult.RouteValues.Should().ContainKey("id");
        _context.Appointment.Count().Should().Be(2);
        var savedAppointment = await _context.Appointment
            .Include(a => a.Customer)
            .Include(a => a.BookedService)
            .FirstOrDefaultAsync(a => a.DateTime == appointmentDataVM.SelectedDateTime);
        savedAppointment.Should().NotBeNull();
        savedAppointment.Customer.Should().BeEquivalentTo(customerA);
        savedAppointment.BookedService.Id.Should().Be(appointmentDataVM.SelectedServiceId);
        savedAppointment.DateTime.Should().Be(appointmentDataVM.SelectedDateTime);
    }

    [Fact]
    public async Task BookForOther_ShouldReturnForbidden_WhenNotLoggedIn()
    {
        // Arrange
        SeedStandardTestData();
        // BookForOther attempted by a non-logged in user for a customer
        IdentityUser customerB = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Customer B");
        _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((IdentityUser) null);
        // Prepare the appointment data
        var appointmentDataVM = new AppointmentBookingViewModel
        {
            SelectedCustomerId = customerB.Id,
            SelectedServiceId = 2,
            SelectedProviderId = 1,
            SelectedDateTime = DateTime.Today.AddDays(10).AddHours(9)
        };
        // Act
        var result = await _appointmentsController.BookForOther(appointmentDataVM);
        // Assert
        result.Should().BeOfType<ForbidResult>();
        _context.Appointment.Count().Should().Be(1);
    }
    
    [Fact]
    public async Task BookForOther_ShouldReturnForbidden_WhenNotLoggedInAsAdmin()
    {
        // Arrange
        SeedStandardTestData();
        // BookForOther attempted by a customer for another customer
        IdentityUser customerA = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Customer A");
        IdentityUser customerB = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Customer B");
        _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(customerA);
        // Prepare the appointment data
        var appointmentDataVM = new AppointmentBookingViewModel
        {
            SelectedCustomerId = customerB.Id,
            SelectedServiceId = 2,
            SelectedProviderId = 1,
            SelectedDateTime = DateTime.Today.AddDays(10).AddHours(9)
        };
        // Act
        var result = await _appointmentsController.BookForOther(appointmentDataVM);
        // Assert
        result.Should().BeOfType<ForbidResult>();
        _context.Appointment.Count().Should().Be(1);
    }

    [Fact]
    public async Task BookForOther_ShouldReturnNotFound_WhenNoUserSelected()
    {
        // Arrange
        SeedStandardTestData();
        // BookForOther attempted by an admin for a non-existent user
        IdentityUser admin = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Admin");
        _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(admin);
        // Prepare the appointment data
        var appointmentDataVM = new AppointmentBookingViewModel
        {
            SelectedCustomerId = "nouserid42", // Non-existent user
            SelectedServiceId = 2,
            SelectedProviderId = 1,
            SelectedDateTime = DateTime.Today.AddDays(10).AddHours(9)
        };
        // Act
        var result = await _appointmentsController.BookForOther(appointmentDataVM);
        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _context.Appointment.Count().Should().Be(1);
    }

    [Fact]
    public async Task BookForOther_ShouldReturnBadRequest_WhenInvalidData()
    {
        // Arrange
        SeedStandardTestData();
        // BookForOther attempted by an admin for another customer
        IdentityUser customerA = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Customer A");
        IdentityUser admin = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Admin");
        _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(admin);
        // Prepare the appointment data
        var appointmentDataVM = new AppointmentBookingViewModel
        {
            SelectedCustomerId = customerA.Id,
            SelectedServiceId = 2, 
            // SelectedProviderId = 1, // No provider selected in vm
            SelectedDateTime = DateTime.Today.AddDays(10).AddHours(9)
        };
        // Act
        var result = await _appointmentsController.BookForOther(appointmentDataVM);
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _context.Appointment.Count().Should().Be(1);
    }

    [Fact]
    public async Task BookForOther_ShouldReturnBadRequest_WhenBusyTimeSlot()
    {
        // Arrange
        SeedStandardTestData();
        // BookForOther attempted by an admin for another customer
        IdentityUser customerA = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Customer A");
        IdentityUser admin = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Admin");
        _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(admin);
        // Prepare the appointment data
        // Prepare the appointment data
        var appointmentDataVM = new AppointmentBookingViewModel
        {
            SelectedCustomerId = customerA.Id,
            SelectedServiceId = 2,
            SelectedProviderId = 1,
            SelectedDateTime = DateTime.Today.AddDays(10).AddHours(8) // 10 days from now at 8am, when provider A is already booked
        };
        // Act
        var result = await _appointmentsController.BookForOther(appointmentDataVM);
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _context.Appointment.Count().Should().Be(1);
    }
    
    
    [Fact]
    public async Task BookForOther_ShouldCreateAppointment_WhenValidData()
    {
        // Arrange
        SeedStandardTestData();
        // BookForOther attempted by an admin for a customer
        IdentityUser customerA = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Customer A");
        IdentityUser admin = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Admin");
        _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(admin);
        // Prepare the appointment data
        var appointmentDataVM = new AppointmentBookingViewModel
        {
            SelectedCustomerId = customerA.Id,
            SelectedServiceId = 2,
            SelectedProviderId = 1,
            SelectedDateTime = DateTime.Today.AddDays(10).AddHours(9)
        };
        // Act
        var result = await _appointmentsController.BookForOther(appointmentDataVM);
        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.ActionName.Should().Be("Confirmation");
        redirectToActionResult.RouteValues.Should().ContainKey("id");
        _context.Appointment.Count().Should().Be(2);
        var savedAppointment = await _context.Appointment
            .Include(a => a.Customer)
            .Include(a => a.BookedService)
            .Where(a => a.CustomerId == appointmentDataVM.SelectedCustomerId && a.ProviderId == appointmentDataVM.SelectedProviderId)
            .FirstOrDefaultAsync(a => a.DateTime == appointmentDataVM.SelectedDateTime);
        savedAppointment.Should().NotBeNull();
        savedAppointment.Customer.Should().BeEquivalentTo(customerA);
        savedAppointment.BookedService.Id.Should().Be(appointmentDataVM.SelectedServiceId);
        savedAppointment.DateTime.Should().Be(appointmentDataVM.SelectedDateTime);
    }
    
    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}