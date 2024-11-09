using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedWebApp.Data;
using MedWebApp.Models;

namespace MedWebApp.Controllers
{
    public class ServicePackagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicePackagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ServicePackages
        public async Task<IActionResult> Index()
        {
            return View(await _context.ServicePackage.ToListAsync());
        }

        // GET: ServicePackages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicePackage = await _context.ServicePackage
                .FirstOrDefaultAsync(m => m.Id == id);
            if (servicePackage == null)
            {
                return NotFound();
            }

            return View(servicePackage);
        }

        // GET: ServicePackages/Create
        public async Task<IActionResult> Create()
        {
            // Get all services for the checkbox list
            ViewBag.AllServices = await _context.Service.ToListAsync();
            return View();
        }

        // POST: ServicePackages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price")] ServicePackage servicePackage, int[] selectedServices)
        {
            if (selectedServices != null && selectedServices.Any())
            {
                // Load all selected services in one go
                var servicesToAdd = await _context.Service
                    .Where(s => selectedServices.Contains(s.Id))
                    .ToListAsync();

                servicePackage.IncludedServices = new List<Service>();
                foreach (var service in servicesToAdd)
                {
                    servicePackage.IncludedServices?.Add(service);
                }
                servicePackage.Disclaimers = servicePackage.GetDisclaimers();
                servicePackage.Requirements = servicePackage.GetRequirements();
            }
            //if (ModelState.IsValid)
            //{
                _context.Add(servicePackage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            //}
            //ViewBag.AllServices = await _context.Service.ToListAsync();
            //return View(servicePackage);
        }

        // GET: ServicePackages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicePackage = await _context.ServicePackage.FindAsync(id);
            if (servicePackage == null)
            {
                return NotFound();
            }

            // Get all services for the checkbox list
            ViewBag.AllServices = await _context.Service.ToListAsync();

            return View(servicePackage);
        }

        // POST: ServicePackages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price")] ServicePackage servicePackage, int[] selectedServices)
        {
            if (id != servicePackage.Id)
            {
                return NotFound();
            }
            if (selectedServices != null && selectedServices.Any())
            {
                // Load all selected services in one go
                var servicesToAdd = await _context.Service
                    .Where(s => selectedServices.Contains(s.Id))
                    .ToListAsync();
                servicePackage.IncludedServices = new List<Service>();
                foreach (var service in servicesToAdd)
                {
                    servicePackage.IncludedServices?.Add(service);
                }
                servicePackage.Disclaimers = servicePackage.GetDisclaimers();
                servicePackage.Requirements = servicePackage.GetRequirements();
            }
            //if (ModelState.IsValid)
            //{
                try
                {
                    _context.Update(servicePackage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServicePackageExists(servicePackage.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            //}
            //ViewBag.AllServices = await _context.Service.ToListAsync();
            //return View(servicePackage);
        }

        // GET: ServicePackages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicePackage = await _context.ServicePackage
                .FirstOrDefaultAsync(m => m.Id == id);
            if (servicePackage == null)
            {
                return NotFound();
            }

            return View(servicePackage);
        }

        // POST: ServicePackages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var servicePackage = await _context.ServicePackage.FindAsync(id);
            if (servicePackage != null)
            {
                _context.ServicePackage.Remove(servicePackage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServicePackageExists(int id)
        {
            return _context.ServicePackage.Any(e => e.Id == id);
        }
    }
}
