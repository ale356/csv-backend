using Microsoft.AspNetCore.Mvc;
using csvBackEnd.Data;
using csvBackEnd.Models;
using CsvHelper;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

namespace CsvBackEnd.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UsersController(ApplicationDbContext context) : ControllerBase
  {
    private readonly ApplicationDbContext _context = context;

    // Uploads the user CSV file to the database.
    [HttpPost("upload")]
    public async Task<IActionResult> UploadUserCsv(IFormFile file)
    {
      if (file == null || file.Length == 0)
        return BadRequest("No file uploaded.");

      using var reader = new StreamReader(file.OpenReadStream());
      using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
      var users = new List<User>();

      try
      {
        users = csv.GetRecords<User>().ToList();

        // Validate each user in the list.
        var validationResults = new List<ValidationResult>();

        foreach (var user in users)
        {
          // Create a new ValidationContext for each user instance.
          var validationContext = new ValidationContext(user);
          
          // Validate the user model.
          var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);
          if (!isValid)
          {
            // Return the validation errors.
            return BadRequest(validationResults);
          }
        }

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Return or process further as needed.
        return Ok(users);
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }
  }
}
