using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;
using Veace.api.Models;
using veaco.Models;

namespace Veace.api.Controllers
{
    public class PartsController : Controller
    {
        private readonly AppDbContext _context;

        public PartsController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 VIEW ALL PARTS``
        public async Task<IActionResult> Index()
        {
            var parts = await _context.Parts.Include(p => p.Vendor).ToListAsync();
            return View(parts);
        }

        // 🔹 CREATE (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 🔹 CREATE (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Part part)
        {
            if (ModelState.IsValid)
            {
                _context.Parts.Add(part);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(part);
        }

        // 🔹 EDIT (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return NotFound();
            return View(part);
        }

        // 🔹 EDIT (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(Part part)
        {
            if (ModelState.IsValid)
            {
                _context.Update(part);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(part);
        }

        // 🔹 DELETE (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return NotFound();
            return View(part);
        }

        // 🔹 DELETE (POST)
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}