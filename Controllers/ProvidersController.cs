using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedWebApp.Data;
using MedWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MedWebApp.Controllers
{
    public class ProvidersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProvidersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Providers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Provider.ToListAsync());
        }

        // GET: Providers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var provider = await _context.Provider
                .FirstOrDefaultAsync(m => m.Id == id);
            if (provider == null)
            {
                return NotFound();
            }

            return View(provider);
        }

        // GET: Providers/Register
        public async Task<IActionResult> Register()
        {
            var AllServices = await _context.Service.AsNoTracking().ToListAsync();
            ViewBag.Services = new MultiSelectList(
                AllServices,
                "Id", // value field
                "Name" // display field
                );
            return View();
        }

        // POST: Providers/Register
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Id,UserId,Type")] Provider provider, int[] selectedServices)
        {
            //if (ModelState.IsValid)
            //{
                provider.UserId = _userManager.GetUserId(User);
                provider.User = await _userManager.GetUserAsync(User);

                try
                {
                    if (selectedServices != null)
                    {
                        foreach (var serviceId in selectedServices)
                        {
                            provider.ProviderServices.Add(new ProviderService
                            {
                                ProviderId = provider.Id,
                                ServiceId = serviceId
                            });
                        }
                        var SelServices = await _context.Service.Where(s => selectedServices.Contains(s.Id)).AsNoTracking().ToListAsync();
                        provider.AvailableServices = SelServices;
                    }
                    _context.Add(provider);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Edit), new { id = provider.Id , provider, selectedServices});
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (ProviderExists(provider.Id))
                    {
                        return BadRequest();
                    }
                    throw;
                }
            //}
            //return View(provider);
            //return RedirectToAction(nameof(Edit), new { id = provider.Id });
        }

        // GET: Providers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Providers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Type")] Provider provider)
        {
            if (ModelState.IsValid)
            {
                _context.Add(provider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(provider);
        }

        public async Task<IActionResult> ManageProvider()
        {
            // Get current logged in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // Redirects to login if user is not authenticated
            }

            // Check if a provider exists for this user
            var provider = await _context.Provider
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (provider != null)
            {
                // Provider exists, redirect to Edit
                return RedirectToAction(nameof(Edit), new { id = provider.Id , provider = provider, selectedSeervices = new int[20] });
            }

            // No provider found, redirect to Create
            return RedirectToAction(nameof(Register));
        }

        // GET: Providers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Load provider and services in a single operation
            var provider = await _context.Provider
                .AsNoTracking()  // Add this to prevent tracking
                .Include(p => p.AvailableServices)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (provider == null)
            {
                return NotFound();
            }

            // Load all services in a separate operation
            var allServices = await _context.Service
                .AsNoTracking()  // Add this to prevent tracking
                .ToListAsync();

            ViewBag.Services = new MultiSelectList(
                allServices,
                "Id",
                "Name",
                provider.AvailableServices.Select(s => s.Id)
            );

            return View(provider);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Provider provider, int[] selectedServices)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Load the provider with its services
                    var providerToUpdate = await _context.Provider
                        .Include(p => p.AvailableServices)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (providerToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Clear existing services
                    providerToUpdate.AvailableServices.Clear();

                    if (selectedServices != null && selectedServices.Any())
                    {
                        // Load all selected services in one go
                        var servicesToAdd = await _context.Service
                            .Where(s => selectedServices.Contains(s.Id))
                            .ToListAsync();

                        foreach (var service in servicesToAdd)
                        {
                            providerToUpdate.AvailableServices.Add(service);
                        }
                    }

                    // Update other properties
                    _context.Entry(providerToUpdate).CurrentValues.SetValues(provider);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Edit), new { id = provider.Id , provider, selectedServices});
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (! ProviderExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            // If we got this far, something failed
            // Reload the services for the dropdown
            ViewBag.Services = new MultiSelectList(
                await _context.Service.AsNoTracking().ToListAsync(),
                "Id",
                "Name",
                selectedServices
            );

            return View(provider);
        }

        // GET: Providers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var provider = await _context.Provider
                .FirstOrDefaultAsync(m => m.Id == id);
            if (provider == null)
            {
                return NotFound();
            }

            return View(provider);
        }

        // POST: Providers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var provider = await _context.Provider.FindAsync(id);
            if (provider != null)
            {
                _context.Provider.Remove(provider);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProviderExists(int id)
        {
            return _context.Provider.Any(e => e.Id == id);
        }
    }
}
