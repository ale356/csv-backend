using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using csvBackEnd.Controllers;
using csvBackEnd.Data;
using csvBackEnd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class UsersControllerTests
{
  [Fact]
  public async Task UploadUserCsv_ValidUsers_ShouldReturnOkResult()
  {
    // Arrange: Set up the in-memory database and controller
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDb") // In-memory database for testing
        .Options;

    using var context = new ApplicationDbContext(options);
    var controller = new UsersController(context);

    // Create a FormFile simulating the uploaded CSV data
    var usersCsv = "FullName,Username,Email,Password\nMark Brown,mbrown,mark@brown.com,Password123!";
    var csvFile = new FormFile(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(usersCsv)), 0, usersCsv.Length, "file", "users.csv")
    {
      Headers = new HeaderDictionary(),
      ContentType = "text/csv"
    };

    // Act: Call the UploadUserCsv method to process the uploaded CSV file
    var result = await controller.UploadUserCsv(csvFile);

    // Assert: Verify the result and check that the data was saved in the in-memory database
    var okResult = Assert.IsType<OkObjectResult>(result);
    var response = okResult.Value as dynamic;
    Assert.NotNull(response);
    Assert.NotEmpty(response.SavedUsers);
    Assert.Empty(response.InvalidEntries);

    var savedUsers = context.Users.ToList();
    Assert.Single(savedUsers);
    Assert.Equal("mbrown", savedUsers[0].Username);
  }

  [Fact]
  public async Task UploadUserCsv_InvalidUsers_ShouldReturnInvalidUsers()
  {
    // Arrange: Set up the in-memory database and controller with a unique name
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique database for each test
        .Options;

    using var context = new ApplicationDbContext(options);
    var controller = new UsersController(context);

    // Create a CSV file with both valid and invalid entries
    var fileContent = "FullName,Username,Email,Password\n" +
                      "Mark Brown,mbrown,mark@brown.com,Password123!\n" + // valid user
                      "Invalid User,,invalid-email,,\n"; // invalid user with missing fields

    var file = new FormFile(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)), 0, fileContent.Length, "file", "test.csv");

    // Act
    var result = await controller.UploadUserCsv(file);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    var returnValue = okResult.Value; // Capture the return value directly

    // Check the type of returnValue to be an anonymous type
    var response = returnValue as dynamic;

    Assert.NotNull(response);
    Assert.NotNull(response.InvalidEntries); // Ensure InvalidEntries is not null
    Assert.NotEmpty(response.InvalidEntries); // Check that there are invalid users returned
  }
}
