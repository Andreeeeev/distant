using distant.Data;
using distant.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace distant.Controllers
{
    [Authorize]
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
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Блокируем, если тест уже сдан
            if (await _context.Results.AnyAsync(r => r.TestId == id && r.StudentId == userIdString))
                return RedirectToAction("Index", "Journal");

            var test = await _context.Tests.Include(t => t.Questions).FirstOrDefaultAsync(t => t.Id == id);
            if (test == null) return NotFound();

            // Блокируем, если курс уже завершен
            if (await _context.CourseResults.AnyAsync(cr => cr.CourseId == test.CourseId && cr.StudentId == userIdString))
                return RedirectToAction("Index", "Journal");

            return View(test);
        }

        // 2. Логика подсчета баллов (сюда придут ответы студента)
        [HttpPost]
        public async Task<IActionResult> SubmitTest(int testId, Dictionary<int, string> answers)
        {
            var test = await _context.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null) return NotFound();

            int correctAnswers = 0;
            int totalQuestions = test.Questions.Count;

            foreach (var question in test.Questions)
            {
                if (answers.ContainsKey(question.Id) && !string.IsNullOrWhiteSpace(answers[question.Id]))
                {
                    string studentAnswer = answers[question.Id].Trim();
                    string correctAnswer = question.CorrectAnswerText.Trim();

                    switch (question.QuestionType)
                    {
                        case 0: // Один верный ответ
                            if (studentAnswer == correctAnswer) correctAnswers++;
                            break;

                        case 1: // Несколько верных ответов (приходят через запятую, например "1,3")
                                // Сортируем ответы, чтобы "1,3" и "3,1" считались одинаково верными
                            var studentAnsList = studentAnswer.Split(',').Select(a => a.Trim()).OrderBy(a => a);
                            var correctAnsList = correctAnswer.Split(',').Select(a => a.Trim()).OrderBy(a => a);

                            if (studentAnsList.SequenceEqual(correctAnsList)) correctAnswers++;
                            break;

                        case 2: // Текст: Точное совпадение (игнорируем регистр)
                            if (studentAnswer.ToLower() == correctAnswer.ToLower()) correctAnswers++;
                            break;

                        case 3: // Текст: Ключевые слова (если хотя бы одно слово есть в ответе)
                            var keywords = correctAnswer.Split(',').Select(k => k.Trim().ToLower());
                            if (keywords.Any(k => studentAnswer.ToLower().Contains(k))) correctAnswers++;
                            break;
                    }
                }
            }

            // Динамический перевод в 5-балльную систему по процентам
            int finalGrade = 2; // По умолчанию ставим 2

            if (totalQuestions > 0)
            {
                double percentage = (double)correctAnswers / totalQuestions * 100;

                if (percentage >= 85) finalGrade = 5;
                else if (percentage >= 70) finalGrade = 4;  // Четверка от 70%
                else if (percentage >= 50) finalGrade = 3;
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