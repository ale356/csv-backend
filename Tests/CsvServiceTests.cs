using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace csvBackEnd.Tests
{
  // Model class for User (to match the CSV structure)
  public class User
  {
    public required string FullName { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }


  }

  // Service that handles CSV file upload and validation
  public class CsvService
  {
    public async Task<ValidationResult> ValidateAndParseCsvAsync(List<User> users)
    {
      var result = new ValidationResult();
      foreach (var user in users)
      {
        if (!IsValidEmail(user.Email))
        {
          result.Errors.Add($"{user.Email} is not a valid email.");
        }
      }

      return await Task.FromResult(result);
    }

    private bool IsValidEmail(string email)
    {
      return email.Contains("@") && email.Contains(".");
    }
  }

  // Result object to store validation errors
  public class ValidationResult
  {
    public List<string> Errors { get; set; } = new List<string>();
  }

  public class CsvServiceTests
  {
    [Fact]
    public async Task UploadCsv_WithInvalidEmails_ShouldReturnInvalidEntries()
    {
      // Arrange
      var csvData = new List<User>
            {
                new User { FullName = "John Doe", Username = "jdoe", Email = "invalidEmail", Password = "Password1!" },
                new User { FullName = "Jane Smith", Username = "jsmith", Email = "jane@smith.com", Password = "SecurePass1!" }
            };
      var csvService = new CsvService();

      // Act
      var result = await csvService.ValidateAndParseCsvAsync(csvData);

      // Assert
      Assert.Contains(result.Errors, e => e.Contains("invalidEmail"));
    }
  }
}
