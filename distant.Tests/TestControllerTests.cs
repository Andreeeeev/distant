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
    public class TestControllerTests
    {
        private ApplicationDbContext _context;
        private TestController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestsDatabase")
                .Options;

            _context = new ApplicationDbContext(options);

            // Кладем один фейковый тест в базу
            _context.Tests.Add(new Test { Id = 1, Title = "Тест по физике", CourseId = 1 });
            _context.SaveChanges();

            _controller = new TestController(_context);

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
        public async Task PassTest_ValidId_ReturnsViewWithTestModel()
        {
            // Act: Пытаемся открыть тест с Id = 1
            var result = await _controller.PassTest(1);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult, "Должна открыться HTML-страница");

            var model = viewResult.Model as Test;
            Assert.IsNotNull(model, "На страницу должна передаться модель Test");
            Assert.AreEqual("Тест по физике", model.Title, "Название теста должно совпадать");
        }
    }
}