using distant.Data;
using distant.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace distant.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses.ToListAsync();
            return View(courses);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Ищем курс по Id
            var course = await _context.Courses
                .Include(c => c.Lectures)
                .Include(c => c.Tests)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.PassedTestIds = await _context.Results
                .Where(r => r.StudentId == userIdString).Select(r => r.TestId).ToListAsync();
            ViewBag.IsCourseCompleted = await _context.CourseResults
                .AnyAsync(cr => cr.StudentId == userIdString && cr.CourseId == id);

            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> ReadLecture(int id)
        {
            // Ищем лекцию по ID и захватываем название курса, к которому она относится
            var lecture = await _context.Lectures
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lecture == null)
            {
                return NotFound();
            }

            return View(lecture);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadLecture(int id)
        {
            // Находим лекцию в базе
            var lecture = await _context.Lectures
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lecture == null)
            {
                return NotFound();
            }

            // 1. Формируем красивый текст для файла
            string fileContent = $"КУРС: {lecture.Course?.Name}\r\n";
            fileContent += $"ЛЕКЦИЯ: {lecture.Title}\r\n";
            fileContent += new string('-', 50) + "\r\n\r\n"; // Разделительная линия
            fileContent += lecture.Content;

            // 2. Превращаем текст в массив байтов (в кодировке UTF-8, чтобы русские буквы читались нормально)
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(fileContent);

            // 3. Формируем имя файла (заменяем пробелы на нижние подчеркивания для красоты)
            string fileName = $"{lecture.Title.Replace(" ", "_")}.txt";

            // 4. Отдаем файл браузеру!
            return File(fileBytes, "text/plain", fileName);
        }
        [HttpGet]
        public async Task<IActionResult> FinishCourse(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Проверяем, не завершил ли студент этот курс ранее
            var existingResult = await _context.CourseResults
                .FirstOrDefaultAsync(cr => cr.CourseId == id && cr.StudentId == userIdString);

            if (existingResult != null)
            {
                return RedirectToAction("Index", "Journal");
            }

            // 2. Достаем все результаты тестов ЭТОГО студента по ЭТОМУ курсу
            var testResults = await _context.Results
                .Include(r => r.Test)
                .Where(r => r.Test.CourseId == id && r.StudentId == userIdString)
                .ToListAsync();

            // Если студент не прошел ни одного теста, не даем завершить курс
            if (!testResults.Any())
            {
                TempData["WarningMessage"] = "Невозможно завершить курс: вы не сдали ни одного теста!";
                return RedirectToAction("Details", new { id = id });
            }

            // 3. Считаем среднюю оценку и округляем до 2 знаков
            double finalGrade = Math.Round(testResults.Average(r => r.Score), 2);

            // 4. Сохраняем итог в базу
            var courseResult = new CourseResult
            {
                CourseId = id,
                StudentId = userIdString,
                FinalGrade = finalGrade,
                CompletionDate = DateTime.Now
            };

            _context.CourseResults.Add(courseResult);
            await _context.SaveChangesAsync();

            // 5. Отправляем в Журнал любоваться результатом
            return RedirectToAction("Index", "Journal");
        }
    }

}