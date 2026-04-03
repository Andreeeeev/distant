using distant.Controllers;
using distant.Data;
using distant.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Security.Claims;
using System.Threading.Tasks;

namespace distant.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private ApplicationDbContext _context;
        private HomeController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "HomeDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new HomeController(_context);

            // Имитируем авторизованного пользователя (Студента)
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
        public async Task Index_AuthenticatedUser_ReturnsHomeViewModel()
        {
            // Act: Вызываем загрузку главной страницы
            var result = await _controller.Index();

            // Assert: Проверяем, что вернулась HTML-страница с моделью HomeViewModel
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult, "Должен вернуться ViewResult");

            var model = viewResult.Model as HomeViewModel;
            Assert.IsNotNull(model, "Модель должна быть типа HomeViewModel");
            Assert.IsNotNull(model.CompletedCourses, "Список завершенных курсов не должен быть NULL");
            Assert.IsNotNull(model.ActiveCourses, "Список активных курсов не должен быть NULL");
        }
    }
}