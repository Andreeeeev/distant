using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using distant.Data;
using distant.Models;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace distant.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Если гость - показываем пустую страницу
            if (!User.Identity.IsAuthenticated) return View(new HomeViewModel());

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Ищем завершенные курсы
            var completed = await _context.CourseResults
                .Include(cr => cr.Course)
                .Where(cr => cr.StudentId == userId)
                .OrderByDescending(cr => cr.CompletionDate)
                .ToListAsync();

            var completedIds = completed.Select(c => c.CourseId).ToList();

            // 2. Ищем тесты, которые студент уже сдал
            var passedTestIds = await _context.Results
                .Where(r => r.StudentId == userId)
                .Select(r => r.TestId)
                .ToListAsync();

            // 3. Ищем активные курсы (те, что еще не завершены)
            var allCourses = await _context.Courses
                .Include(c => c.Tests)
                .Where(c => !completedIds.Contains(c.Id))
                .ToListAsync();

            var active = new List<ActiveCourseInfo>();
            foreach (var course in allCourses)
            {
                active.Add(new ActiveCourseInfo
                {
                    Course = course,
                    RemainingTests = course.Tests.Where(t => !passedTestIds.Contains(t.Id)).ToList()
                });
            }

            return View(new HomeViewModel { CompletedCourses = completed, ActiveCourses = active });
        }
    }
}