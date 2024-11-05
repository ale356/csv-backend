using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using csvBackEnd.Controllers;
using csvBackEnd.Data;
using csvBackEnd.Models;

namespace csvBackEnd.Tests
{
    public class UserServiceTests
    {
        private readonly UsersController _usersController;
        private readonly ApplicationDbContext _context;

        public UserServiceTests()
        {
            // Set up an in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            _usersController = new UsersController(_context);
        }

        [Fact]
        public async Task UploadUserCsv_WithInvalidPassword_ShouldReturnValidationError()
        {
            // Arrange
            var csvContent = "FullName,Username,Email,Password\n" +
                             "Alice Walker,awalker,alice@walker.com,password"; // Invalid password
            var file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes(csvContent)), 0, csvContent.Length, "file", "test.csv");

            // Act
            var result = await _usersController.UploadUserCsv(file);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response.InvalidEntries);
            Assert.Single(response.InvalidEntries);
            Assert.Equal("Alice Walker", response.InvalidEntries[0].User.FullName);
            Assert.Contains("Password", response.InvalidEntries[0].ValidationResults.Keys);
            Assert.False(response.InvalidEntries[0].ValidationResults["Password"]); // Password validation should fail
        }

        [Fact]
        public async Task UploadUserCsv_WithNoFile_ShouldReturnBadRequest()
        {
            // Arrange
            IFormFile file = null;

            // Act
            var result = await _usersController.UploadUserCsv(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No file uploaded.", badRequestResult.Value);
        }

    }
}
