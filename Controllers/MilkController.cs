using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GauGhar.Data;
using GauGhar.Models;
using System.Security.Claims;

namespace GauGhar.Controllers
{
    [Authorize]
    public class MilkController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MilkController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Milk
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, int? cowId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.MilkRecords
                .Include(m => m.Cow)
                .Where(m => m.UserId == userId)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(m => m.Date >= fromDate.Value);
                ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            }

            if (toDate.HasValue)
            {
                query = query.Where(m => m.Date <= toDate.Value);
                ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            }

            if (cowId.HasValue)
            {
                query = query.Where(m => m.CowId == cowId.Value);
                ViewBag.CowId = cowId.Value;
            }

            var milkRecords = await query.OrderByDescending(m => m.Date).ToListAsync();

            ViewBag.Cows = await _context.Cows
                .Where(c => c.UserId == userId && c.Status == "Active")
                .OrderBy(c => c.TagNumber)
                .ToListAsync();

            // Calculate totals - manually sum morning and evening
            ViewBag.TotalMorning = milkRecords.Sum(m => m.MorningQuantity);
            ViewBag.TotalEvening = milkRecords.Sum(m => m.EveningQuantity);
            ViewBag.GrandTotal = milkRecords.Sum(m => m.MorningQuantity + m.EveningQuantity);

            return View(milkRecords);
        }

        // GET: Milk/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Cows = await _context.Cows
                .Where(c => c.UserId == userId && c.Status == "Active")
                .OrderBy(c => c.TagNumber)
                .ToListAsync();

            return View();
        }

        // POST: Milk/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CowId,Date,MorningQuantity,EveningQuantity,Remarks")] MilkRecord milkRecord)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check for duplicate entry for same cow on same day
            var exists = await _context.MilkRecords
                .AnyAsync(m => m.CowId == milkRecord.CowId &&
                              m.Date == milkRecord.Date.Date &&
                              m.UserId == userId);

            if (exists)
            {
                ModelState.AddModelError(string.Empty, "Milk record already exists for this cow on selected date.");
            }

            if (ModelState.IsValid)
            {
                milkRecord.UserId = userId;
                _context.Add(milkRecord);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Milk record added successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Cows = await _context.Cows
                .Where(c => c.UserId == userId && c.Status == "Active")
                .OrderBy(c => c.TagNumber)
                .ToListAsync();

            return View(milkRecord);
        }

        // GET: Milk/DailySummary
        public async Task<IActionResult> DailySummary(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var summary = await _context.MilkRecords
                .Include(m => m.Cow)
                .Where(m => m.UserId == userId && m.Date == selectedDate)
                .GroupBy(m => m.Cow)
                .Select(g => new DailyMilkSummary
                {
                    Cow = g.Key,
                    MorningQuantity = g.Sum(m => m.MorningQuantity),
                    EveningQuantity = g.Sum(m => m.EveningQuantity),
                    TotalQuantity = g.Sum(m => m.MorningQuantity + m.EveningQuantity)
                })
                .OrderBy(s => s.Cow.TagNumber)
                .ToListAsync();

            ViewBag.SelectedDate = selectedDate;
            ViewBag.TotalMorning = summary.Sum(s => s.MorningQuantity);
            ViewBag.TotalEvening = summary.Sum(s => s.EveningQuantity);
            ViewBag.GrandTotal = summary.Sum(s => s.TotalQuantity);

            return View(summary);
        }

        // GET: Milk/MonthlyReport
        public async Task<IActionResult> MonthlyReport(int? year, int? month)
        {
            var currentYear = year ?? DateTime.Today.Year;
            var currentMonth = month ?? DateTime.Today.Month;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var startDate = new DateTime(currentYear, currentMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var monthlyData = await _context.MilkRecords
                .Where(m => m.UserId == userId &&
                           m.Date >= startDate &&
                           m.Date <= endDate)
                .GroupBy(m => m.Date)
                .Select(g => new MonthlyData
                {
                    Date = g.Key,
                    TotalMilk = g.Sum(m => m.MorningQuantity + m.EveningQuantity),
                    RecordCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            ViewBag.Year = currentYear;
            ViewBag.Month = currentMonth;
            ViewBag.MonthName = startDate.ToString("MMMM");
            ViewBag.TotalMilk = monthlyData.Sum(x => x.TotalMilk);
            ViewBag.AverageDaily = monthlyData.Any() ? monthlyData.Average(x => x.TotalMilk) : 0;

            return View(monthlyData);
        }

        // Helper classes
        public class DailyMilkSummary
        {
            public Cow? Cow { get; set; }
            public decimal MorningQuantity { get; set; }
            public decimal EveningQuantity { get; set; }
            public decimal TotalQuantity { get; set; }
        }

        public class MonthlyData
        {
            public DateTime Date { get; set; }
            public decimal TotalMilk { get; set; }
            public int RecordCount { get; set; }
        }
    }
}