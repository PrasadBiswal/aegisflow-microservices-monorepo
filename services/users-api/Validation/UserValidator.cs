using System.Net.Mail;
using AegisFlow.Users.Api.Contracts;
using AegisFlow.Users.Api.Models;

namespace AegisFlow.Users.Api.Validation;

// Returns field -> error messages. An empty result means the request is valid.
// Keys are camelCase to match the JSON field names clients send.
public static class UserValidator
{
    public static Dictionary<string, string[]> Validate(CreateUserRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        AddEmailErrors(request.Email, errors);
        AddFullNameErrors(request.FullName, errors);
        return errors;
    }

    public static Dictionary<string, string[]> Validate(UpdateUserRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        AddFullNameErrors(request.FullName, errors);
        return errors;
    }

    private static void AddEmailErrors(string? email, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            errors["email"] = ["Email is required."];
        }
        else if (email.Trim().Length > User.EmailMaxLength)
        {
            errors["email"] = [$"Email must be at most {User.EmailMaxLength} characters."];
        }
        else if (!MailAddress.TryCreate(email.Trim(), out _))
        {
            errors["email"] = ["Email is not a valid email address."];
        }
    }

    private static void AddFullNameErrors(string? fullName, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            errors["fullName"] = ["Full name is required."];
        }
        else if (fullName.Trim().Length > User.FullNameMaxLength)
        {
            errors["fullName"] = [$"Full name must be at most {User.FullNameMaxLength} characters."];
        }
    }
}
