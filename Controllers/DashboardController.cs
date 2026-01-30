using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GauGhar.Data;
using GauGhar; // Add this using
using System.Security.Claims;
using GauGhar.Models;

namespace GauGhar.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var today = DateTime.Today;

            // Get dashboard statistics
            var dashboardData = new DashboardViewModel
            {
                TotalCows = await _context.Cows.CountAsync(c => c.UserId == userId),
                ActiveCows = await _context.Cows.CountAsync(c => c.UserId == userId && c.Status == "Active"),
                PregnantCows = await _context.Cows.CountAsync(c => c.UserId == userId && c.IsPregnant),
                TodayMilk = await _context.MilkRecords
                    .Where(m => m.UserId == userId && m.Date == today)
                    .SumAsync(m => m.TotalQuantity),
                TodaySales = await _context.MilkSales
                    .Where(s => s.UserId == userId && s.SaleDate == today)
                    .SumAsync(s => s.Quantity * s.RatePerLiter),
                TodayExpenses = await _context.Expenses
                    .Where(e => e.UserId == userId && e.ExpenseDate == today)
                    .SumAsync(e => e.Amount)
            };

            // Get monthly data for charts
            var startDate = new DateTime(today.Year, today.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var milkData = await _context.MilkRecords
                .Where(m => m.UserId == userId && m.Date >= startDate && m.Date <= endDate)
                .GroupBy(m => m.Date.Day)
                .Select(g => new
                {
                    Day = g.Key,
                    TotalMilk = g.Sum(m => m.TotalQuantity)
                })
                .OrderBy(x => x.Day)
                .ToListAsync();

            var salesData = await _context.MilkSales
                .Where(s => s.UserId == userId && s.SaleDate >= startDate && s.SaleDate <= endDate)
                .GroupBy(s => s.SaleDate.Day)
                .Select(g => new
                {
                    Day = g.Key,
                    TotalSales = g.Sum(s => s.Quantity * s.RatePerLiter)
                })
                .OrderBy(x => x.Day)
                .ToListAsync();

            var expenseData = await _context.Expenses
                .Where(e => e.UserId == userId && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                .GroupBy(e => e.ExpenseDate.Day)
                .Select(g => new
                {
                    Day = g.Key,
                    TotalExpenses = g.Sum(e => e.Amount)
                })
                .OrderBy(x => x.Day)
                .ToListAsync();

            ViewBag.MilkChartData = milkData;
            ViewBag.SalesChartData = salesData;
            ViewBag.ExpenseChartData = expenseData;
            ViewBag.CurrentMonth = today.ToString("MMMM yyyy");

            return View(dashboardData);
        }

        // GET: Dashboard/GetMonthlyProfitLoss
        public async Task<IActionResult> GetMonthlyProfitLoss(int year, int month)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var totalSales = await _context.MilkSales
                .Where(s => s.UserId == userId && s.SaleDate >= startDate && s.SaleDate <= endDate)
                .SumAsync(s => s.Quantity * s.RatePerLiter);

            var totalExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                .SumAsync(e => e.Amount);

            var profitLoss = totalSales - totalExpenses;

            return Json(new
            {
                TotalSales = totalSales,
                TotalExpenses = totalExpenses,
                ProfitLoss = profitLoss,
                IsProfit = profitLoss >= 0
            });
        }

        // GET: Dashboard/GetYearlyReport
        public async Task<IActionResult> GetYearlyReport(int year)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var monthlyData = new List<object>();

            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var sales = await _context.MilkSales
                    .Where(s => s.UserId == userId && s.SaleDate >= startDate && s.SaleDate <= endDate)
                    .SumAsync(s => s.Quantity * s.RatePerLiter);

                var expenses = await _context.Expenses
                    .Where(e => e.UserId == userId && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                    .SumAsync(e => e.Amount);

                monthlyData.Add(new
                {
                    Month = startDate.ToString("MMM"),
                    Sales = sales,
                    Expenses = expenses,
                    Profit = sales - expenses
                });
            }

            return Json(monthlyData);
        }
    }
}