using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using distant.Data;
using System.Threading.Tasks;

namespace distant.Controllers
{
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
    }
}