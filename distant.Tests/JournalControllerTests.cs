using distant.Controllers;
using distant.Data;
using distant.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace distant.Tests
{
    [TestFixture]
    public class JournalControllerTests
    {
        private ApplicationDbContext _context;
        private JournalController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "JournalDatabase")
                .Options;

            _context = new ApplicationDbContext(options);

            // Создаем фейковые данные для журнала (Курс -> Тест -> Результат)
            var course = new Course { Id = 1, Name = "Тестовый курс" };
            var test = new Test { Id = 1, Title = "Тестовый тест", CourseId = 1, Course = course };
            var result = new Result { Id = 1, StudentId = "test-student-id", Score = 5, AttemptDate = DateTime.Now, TestId = 1, Test = test };

            _context.Courses.Add(course);
            _context.Tests.Add(test);
            _context.Results.Add(result);
            _context.SaveChanges();

            _controller = new JournalController(_context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-student-id")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller?.Dispose();
        }

        [Test]
        public async Task Index_ReturnsListOfResults_ForCurrentStudent()
        {
            // Act: Открываем журнал без фильтров
            var result = await _controller.Index(null, null, null);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as List<Result>;
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Count, "В журнале должна быть ровно 1 оценка этого студента");
            Assert.AreEqual(5, model[0].Score, "Оценка должна быть равна 5");
        }
    }
}