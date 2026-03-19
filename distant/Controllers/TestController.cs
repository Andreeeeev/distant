using distant.Data;
using distant.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace distant.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Отдаем страницу с вопросами теста
        [HttpGet]
        public async Task<IActionResult> PassTest(int id)
        {
            var test = await _context.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null) return NotFound();

            return View(test);
        }

        // 2. Логика подсчета баллов (сюда придут ответы студента)
        [HttpPost]
        public async Task<IActionResult> SubmitTest(int testId, Dictionary<int, int> answers)
        {
            var test = await _context.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null) return NotFound();

            int correctAnswers = 0;
            int totalQuestions = test.Questions.Count;

            // Считаем количество правильных ответов
            foreach (var question in test.Questions)
            {
                if (answers.ContainsKey(question.Id) && answers[question.Id] == question.CorrectOption)
                {
                    correctAnswers++;
                }
            }

            // Динамический перевод в 5-балльную систему по процентам
            int finalGrade = 2; // По умолчанию ставим 2

            if (totalQuestions > 0)
            {
                double percentage = (double)correctAnswers / totalQuestions * 100;

                if (percentage >= 85) finalGrade = 5;       // Снизили порог для пятерки до 85%
                else if (percentage >= 70) finalGrade = 4;  // Четверка от 70%
                else if (percentage >= 50) finalGrade = 3;  // Тройка теперь дается ровно за половину (50%)
                else finalGrade = 2;                        // Двойка только если меньше 50%                       
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Сохраняем в базу уже готовую 5-балльную оценку
            var result = new Result
            {
                TestId = testId,
                Score = finalGrade, // Теперь Score хранит оценку (2, 3, 4 или 5)
                AttemptDate = DateTime.Now,
                StudentId = userIdString
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Journal");
        }
    }
}