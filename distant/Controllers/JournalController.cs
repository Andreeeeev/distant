using distant.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace distant.Controllers
{
    [Authorize]
    public class JournalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JournalController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? courseId, DateTime? dateFrom, DateTime? dateTo)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Формируем "гибкий" базовый запрос (пока без выгрузки из БД)
            var query = _context.Results
                .Include(r => r.Test)
                    .ThenInclude(t => t.Course)
                .Where(r => r.StudentId == userIdString)
                .AsQueryable();

            // 2. Применяем фильтры (если пользователь их ввел)
            if (courseId.HasValue && courseId.Value > 0)
                query = query.Where(r => r.Test.CourseId == courseId.Value);

            if (dateFrom.HasValue)
                query = query.Where(r => r.AttemptDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(r => r.AttemptDate <= dateTo.Value.AddDays(1).AddTicks(-1));

            // 3. Выгружаем отфильтрованные данные
            var results = await query.OrderByDescending(r => r.AttemptDate).ToListAsync();

            // 4. Высчитываем средний балл (теперь он будет считаться по отфильтрованным данным)
            var averageGrades = results
                .GroupBy(r => r.Test.Course.Name)
                .ToDictionary(
                    g => g.Key,
                    g => Math.Round(g.Average(r => r.Score), 2)
                );
            ViewBag.AverageGrades = averageGrades;

            // 5. Передаем данные для формочки поиска (чтобы выбранные значения не сбрасывались после нажатия "Поиск")
            ViewBag.Courses = await _context.Courses.ToListAsync();
            ViewBag.SelectedCourse = courseId;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            return View(results);
        }
    }
}