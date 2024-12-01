using JetBrains.Annotations;
using MedWebApp.Controllers;
using MedWebApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace MedWebAppTest.Controllers;

[TestSubject(typeof(AppointmentsController))]
public class AppointmentsControllerTest
{
    private readonly AppointmentsController _appointmentsController;
    private readonly Mock<ApplicationDbContext> _contextMock = new Mock<ApplicationDbContext>(
    new DbContextOptions<ApplicationDbContext>()
    ) { CallBase = true };
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock = new Mock<UserManager<IdentityUser>>(
        Mock.Of<IUserStore<IdentityUser>>(),
        null, null, null, null, null, null, null, null
    ) { CallBase = true };
    private readonly Mock<ILogger<AppointmentsController>> _loggerMock = new Mock<ILogger<AppointmentsController>>();
    private readonly Mock<IWebHostEnvironment> _webEnvMock = new Mock<IWebHostEnvironment>();
    
    public AppointmentsControllerTest()
    {
        _appointmentsController = new AppointmentsController(_contextMock.Object, _userManagerMock.Object, _loggerMock.Object, _webEnvMock.Object);
    }
    
    [Fact]
    public async Task GetProvidersForService_ShouldReturnProviders_WhenServiceExists()
    {
        // Arrange
        int serviceId = 1;
        var providers = new[]
        {
            new { Id = 1, DisplayName = "Provider A" },
            new { Id = 2, DisplayName = "Provider D" }
        }.ToList();
        _contextMock.Setup(
            x => x.Provider
                .Include(p => p.AvailableServices)
                .Where(p => p.AvailableServices.Any(s => s.Id == serviceId))
                .Select(p => new { p.Id, p.DisplayName })
                //.ToListAsync(CancellationToken.None)
        //).ReturnsAsync(providers);
        ).Returns(providers.AsQueryable());
        // Act
        var result = await _appointmentsController.GetProvidersForService(serviceId);
        // Assert
        result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(providers);
    }
}