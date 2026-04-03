using distant.Controllers;
using distant.Data;
using distant.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace distant.Tests
{
    // [TestFixture] говорит Visual Studio, что в этом классе лежат тесты
    [TestFixture]
    public class CourseControllerTests
    {
        private ApplicationDbContext _context;
        private CourseController _controller;

        // [SetUp] запускается ПЕРЕД каждым тестом (настраиваем всё)
        [SetUp]
        public void Setup()
        {
            // 1. Создаем базу данных "в оперативной памяти" (она будет стираться после тестов)
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);

            // 2. Добавляем туда пару фейковых курсов
            _context.Courses.Add(new Course { Id = 1, Name = "Фейковый Питон" });
            _context.Courses.Add(new Course { Id = 2, Name = "Фейковые Базы Данных" });
            _context.SaveChanges();

            // 3. Создаем наш реальный контроллер, но подсовываем ему фейковую базу
            _controller = new CourseController(_context);
        }

        // [TearDown] запускается ПОСЛЕ каждого теста (убираем за собой)
        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller?.Dispose();
        }

        // [Test] - это и есть сам тест!
        [Test]
        public async Task Index_ReturnsViewResult_WithListOfCourses()
        {
            // ACT (Действие): Вызываем метод Index у контроллера
            var result = await _controller.Index();

            // ASSERT (Проверка): Проверяем результаты

            // 1. Проверяем, что контроллер вернул HTML-страницу (ViewResult)
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult, "Метод Index должен возвращать ViewResult");

            // 2. Проверяем, что на эту страницу передался список из 2-х наших фейковых курсов
            var model = viewResult.Model as List<Course>;
            Assert.IsNotNull(model, "Модель должна быть списком курсов (List<Course>)");
            Assert.AreEqual(2, model.Count, "Должно вернуться ровно 2 курса");
        }
    }
}