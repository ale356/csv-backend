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
      var validUsers = new List<User>();
      var invalidUsers = new List<object>();

      try
      {
        users = csv.GetRecords<User>().ToList();

        foreach (var user in users)
        {
          var validationResults = new List<ValidationResult>();
          var validationContext = new ValidationContext(user);

          // Dictionary to store the validation status of each property.
          var propertyValidation = user.GetType()
                                       .GetProperties()
                                       .ToDictionary(prop => prop.Name, prop => true);

          if (Validator.TryValidateObject(user, validationContext, validationResults, true))
          {
            validUsers.Add(user);
          }
          else
          {
            // Mark failed properties as false in propertyValidation dictionary.
            foreach (var validationResult in validationResults)
            {
              foreach (var memberName in validationResult.MemberNames)
              {
                propertyValidation[memberName] = false;
              }
            }

            invalidUsers.Add(new
            {
              User = user,
              ValidationResults = propertyValidation
            });
          }
        }

        if (validUsers.Any())
        {
          await _context.Users.AddRangeAsync(validUsers);
          await _context.SaveChangesAsync();
        }

        // Return both saved users and invalid entries.
        return Ok(new { SavedUsers = validUsers, InvalidEntries = invalidUsers });
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

  }
}
