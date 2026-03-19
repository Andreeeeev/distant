using distant.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace distant.Controllers
{
    public class JournalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JournalController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var results = await _context.Results
                .Include(r => r.Test)
                    .ThenInclude(t => t.Course)
                .Where(r => r.StudentId == userIdString)
                .OrderByDescending(r => r.AttemptDate)
                .ToListAsync();

            // Высчитываем средний балл по каждому курсу
            // Группируем результаты по названию курса и считаем Average
            var averageGrades = results
                .GroupBy(r => r.Test.Course.Name)
                .ToDictionary(
                    g => g.Key,
                    g => Math.Round(g.Average(r => r.Score), 2) // Округляем до 2 знаков (например, 4.33)
                );

            // Передаем словарь со средними баллами на страницу через ViewBag
            ViewBag.AverageGrades = averageGrades;

            return View(results);
        }
    }
}