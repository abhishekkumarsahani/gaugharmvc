using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GauGhar.Data;
using GauGhar.Models;
using System.Security.Claims;

namespace GauGhar.Controllers
{
    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Expense
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, int? categoryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.Expenses
                .Include(e => e.ExpenseCategory)
                .Where(e => e.UserId == userId)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate >= fromDate.Value);
                ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            }

            if (toDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate <= toDate.Value);
                ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            }

            if (categoryId.HasValue)
            {
                query = query.Where(e => e.ExpenseCategoryId == categoryId.Value);
                ViewBag.CategoryId = categoryId.Value;
            }

            var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();

            // Calculate totals
            ViewBag.TotalAmount = expenses.Sum(e => e.Amount);
            ViewBag.AverageExpense = expenses.Any() ? expenses.Average(e => e.Amount) : 0;

            // Get categories for dropdown
            ViewBag.Categories = await _context.ExpenseCategories.ToListAsync();

            return View(expenses);
        }

        // GET: Expense/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.ExpenseCategories.ToListAsync();
            return View();
        }

        // POST: Expense/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ExpenseCategoryId,Description,Amount,ExpenseDate,Remarks")] Expense expense)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                expense.UserId = userId;
                _context.Add(expense);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Expense added successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.ExpenseCategories.ToListAsync();
            return View(expense);
        }

        // GET: Expense/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var expense = await _context.Expenses
                .Include(e => e.ExpenseCategory)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.ExpenseCategories.ToListAsync();
            return View(expense);
        }

        // POST: Expense/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ExpenseCategoryId,Description,Amount,ExpenseDate,Remarks,UserId")] Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (expense.UserId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Expense updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
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

            ViewBag.Categories = await _context.ExpenseCategories.ToListAsync();
            return View(expense);
        }

        // GET: Expense/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var expense = await _context.Expenses
                .Include(e => e.ExpenseCategory)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expense/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Expense deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }

        // GET: Expense/Summary
        public async Task<IActionResult> Summary(int? year, int? month)
        {
            var currentYear = year ?? DateTime.Today.Year;
            var currentMonth = month ?? DateTime.Today.Month;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var startDate = new DateTime(currentYear, currentMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Monthly summary by category
            var monthlySummary = await _context.Expenses
                .Include(e => e.ExpenseCategory)
                .Where(e => e.UserId == userId &&
                           e.ExpenseDate >= startDate &&
                           e.ExpenseDate <= endDate)
                .GroupBy(e => e.ExpenseCategory)
                .Select(g => new CategorySummary
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToListAsync();

            // Daily expenses
            var dailyExpenses = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.ExpenseDate >= startDate &&
                           e.ExpenseDate <= endDate)
                .GroupBy(e => e.ExpenseDate)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            ViewBag.Year = currentYear;
            ViewBag.Month = currentMonth;
            ViewBag.MonthName = startDate.ToString("MMMM");
            ViewBag.TotalExpenses = monthlySummary.Sum(x => x.TotalAmount);
            ViewBag.DailyExpenses = dailyExpenses;

            return View(monthlySummary);
        }

        // Helper class for summary
        public class CategorySummary
        {
            public ExpenseCategory? Category { get; set; }
            public decimal TotalAmount { get; set; }
            public int Count { get; set; }
        }
    }
}