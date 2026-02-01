using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GauGhar.Data;
using GauGhar.Models;
using System.Security.Claims;

namespace GauGhar.Controllers
{
    [Authorize]
    public class CowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CowController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cow
        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cows = _context.Cows
                .Where(c => c.UserId == userId)
                .Include(c => c.MilkRecords)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                cows = cows.Where(c => c.TagNumber.Contains(searchString) ||
                                       c.Name.Contains(searchString) ||
                                       c.Breed.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                cows = cows.Where(c => c.Status == statusFilter);
            }

            ViewBag.StatusFilter = statusFilter;
            ViewBag.SearchString = searchString;

            return View(await cows.OrderBy(c => c.TagNumber).ToListAsync());
        }

        // GET: Cow/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cow = await _context.Cows
                .Include(c => c.MilkRecords.OrderByDescending(m => m.Date).Take(30))
                .Include(c => c.Vaccinations.OrderByDescending(v => v.VaccinationDate))
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cow == null)
            {
                return NotFound();
            }

            // Calculate average milk production for last 7 days
            var lastWeekMilk = cow.MilkRecords?
                .Where(m => m.Date >= DateTime.Today.AddDays(-7))
                .Select(m => m.MorningQuantity + m.EveningQuantity) // Calculate manually
                .DefaultIfEmpty(0)
                .Average();

            ViewBag.AvgMilkLastWeek = lastWeekMilk ?? 0;

            return View(cow);
        }

        // GET: Cow/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cow/Create - SIMPLIFIED VERSION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TagNumber,Name,Breed,DateOfBirth,PurchaseDate,PurchasePrice,Color,HealthStatus,Status,IsPregnant,PregnancyDate,Remarks")] Cow cow)
        {
            // Check authentication
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Please login first.";
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User ID not found. Please login again.");
                return View(cow);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    cow.UserId = userId;

                    // REMOVED: cow.CreatedAt = DateTime.Now; // This causes the error

                    // Calculate expected delivery date if pregnant
                    if (cow.IsPregnant && cow.PregnancyDate.HasValue)
                    {
                        cow.ExpectedDeliveryDate = cow.PregnancyDate.Value.AddMonths(9).AddDays(7);
                    }

                    _context.Add(cow);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Cow '{cow.Name}' added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    // Handle duplicate tag number
                    if (ex.InnerException?.Message.Contains("TagNumber") == true)
                    {
                        ModelState.AddModelError("TagNumber", "Tag number already exists. Please use a different tag number.");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error saving cow to database. Please try again.";
                    }
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                }
            }

            return View(cow);
        }

        // GET: Cow/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cow = await _context.Cows.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cow == null)
            {
                return NotFound();
            }
            return View(cow);
        }

        // POST: Cow/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TagNumber,Name,Breed,DateOfBirth,PurchaseDate,PurchasePrice,Color,HealthStatus,Status,IsPregnant,PregnancyDate,Remarks,UserId")] Cow cow)
        {
            if (id != cow.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (cow.UserId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Calculate expected delivery date if pregnant
                    if (cow.IsPregnant && cow.PregnancyDate.HasValue)
                    {
                        cow.ExpectedDeliveryDate = cow.PregnancyDate.Value.AddMonths(9).AddDays(7);
                    }
                    else
                    {
                        cow.ExpectedDeliveryDate = null;
                    }

                    _context.Update(cow);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cow updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CowExists(cow.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Error updating cow. Please try again.";
                }
            }
            return View(cow);
        }

        // GET: Cow/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cow = await _context.Cows
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (cow == null)
            {
                return NotFound();
            }

            return View(cow);
        }

        // POST: Cow/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cow = await _context.Cows.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cow != null)
            {
                try
                {
                    _context.Cows.Remove(cow);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cow deleted successfully!";
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Error deleting cow. Please try again.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CowExists(int id)
        {
            return _context.Cows.Any(e => e.Id == id);
        }

        // GET: Cow/Statistics
        public async Task<IActionResult> Statistics()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var stats = new
            {
                TotalCows = await _context.Cows.CountAsync(c => c.UserId == userId),
                ActiveCows = await _context.Cows.CountAsync(c => c.UserId == userId && c.Status == "Active"),
                PregnantCows = await _context.Cows.CountAsync(c => c.UserId == userId && c.IsPregnant),
                SickCows = await _context.Cows.CountAsync(c => c.UserId == userId && c.HealthStatus == "Sick")
            };

            return Json(stats);
        }
    }
}