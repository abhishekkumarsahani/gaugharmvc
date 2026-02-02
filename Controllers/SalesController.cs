using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GauGhar.Data;
using GauGhar.Models;
using System.Security.Claims;

namespace GauGhar.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Sales
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string? buyerType)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.MilkSales
                .Where(s => s.UserId == userId)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(s => s.SaleDate >= fromDate.Value);
                ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            }

            if (toDate.HasValue)
            {
                query = query.Where(s => s.SaleDate <= toDate.Value);
                ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            }

            if (!string.IsNullOrEmpty(buyerType))
            {
                query = query.Where(s => s.BuyerType == buyerType);
                ViewBag.BuyerType = buyerType;
            }

            var sales = await query.OrderByDescending(s => s.SaleDate).ToListAsync();

            // Calculate totals
            ViewBag.TotalQuantity = sales.Sum(s => s.Quantity);
            ViewBag.TotalAmount = sales.Sum(s => s.Quantity * s.RatePerLiter);
            ViewBag.AverageRate = sales.Any() ? sales.Average(s => s.RatePerLiter) : 0;

            return View(sales);
        }

        // GET: Sales/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BuyerName,BuyerType,Quantity,RatePerLiter,SaleDate,Remarks")] MilkSale milkSale)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                milkSale.UserId = userId;
                _context.Add(milkSale);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sale recorded successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(milkSale);
        }

        // GET: Sales/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var milkSale = await _context.MilkSales
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (milkSale == null)
            {
                return NotFound();
            }
            return View(milkSale);
        }

        // POST: Sales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BuyerName,BuyerType,Quantity,RatePerLiter,SaleDate,Remarks,UserId")] MilkSale milkSale)
        {
            if (id != milkSale.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (milkSale.UserId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(milkSale);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Sale updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MilkSaleExists(milkSale.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(milkSale);
        }

        // GET: Sales/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var milkSale = await _context.MilkSales
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (milkSale == null)
            {
                return NotFound();
            }

            return View(milkSale);
        }

        // POST: Sales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var milkSale = await _context.MilkSales.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (milkSale != null)
            {
                _context.MilkSales.Remove(milkSale);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sale deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MilkSaleExists(int id)
        {
            return _context.MilkSales.Any(e => e.Id == id);
        }

        // GET: Sales/Summary
        public async Task<IActionResult> Summary(int? year, int? month)
        {
            var currentYear = year ?? DateTime.Today.Year;
            var currentMonth = month ?? DateTime.Today.Month;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var startDate = new DateTime(currentYear, currentMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var monthlySales = await _context.MilkSales
                .Where(s => s.UserId == userId &&
                           s.SaleDate >= startDate &&
                           s.SaleDate <= endDate)
                .GroupBy(s => s.SaleDate)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalSales = g.Sum(s => s.Quantity * s.RatePerLiter),
                    TotalQuantity = g.Sum(s => s.Quantity)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            ViewBag.Year = currentYear;
            ViewBag.Month = currentMonth;
            ViewBag.MonthName = startDate.ToString("MMMM");
            ViewBag.TotalSales = monthlySales.Sum(x => x.TotalSales);
            ViewBag.TotalQuantity = monthlySales.Sum(x => x.TotalQuantity);

            return View(monthlySales);
        }
    }
}