using AegisFlow.Users.Api.Contracts;
using AegisFlow.Users.Api.Models;
using AegisFlow.Users.Api.Validation;

namespace AegisFlow.Users.Api.Tests;

// Pure unit tests: no web server, no database.
public sealed class UserValidatorTests
{
    [Fact]
    public void Valid_create_request_has_no_errors()
    {
        var errors = UserValidator.Validate(new CreateUserRequest("ada@example.com", "Ada Lovelace"));

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    public void Invalid_email_is_rejected(string? email)
    {
        var errors = UserValidator.Validate(new CreateUserRequest(email, "Ada Lovelace"));

        Assert.Equal(new[] { "email" }, errors.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_full_name_is_rejected(string? fullName)
    {
        var errors = UserValidator.Validate(new CreateUserRequest("ada@example.com", fullName));

        Assert.Equal(new[] { "fullName" }, errors.Keys);
    }

    [Fact]
    public void Full_name_longer_than_the_limit_is_rejected()
    {
        var tooLong = new string('a', User.FullNameMaxLength + 1);

        var errors = UserValidator.Validate(new UpdateUserRequest(tooLong));

        Assert.Equal(new[] { "fullName" }, errors.Keys);
    }
}
