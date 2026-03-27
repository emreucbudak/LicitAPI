using FluentAssertions;
using FluentValidation.TestHelper;
using Licit.TenderingService.Application.Features.CQRS.Tender.Create;

namespace Licit.TenderingService.UnitTests.Application.Validators;

public class CreateTenderCommandValidatorTests
{
    private readonly CreateTenderCommandValidator _validator = new();

    private CreateTenderCommandRequest CreateValidRequest() => new(
        Title: "Test İhale",
        Description: "Test açıklama",
        StartingPrice: 1000m,
        StartDate: DateTime.UtcNow.AddDays(1),
        EndDate: DateTime.UtcNow.AddDays(30),
        CreatedByUserId: Guid.NewGuid(),
        CategoryId: Guid.NewGuid(),
        Rules: null
    );

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(CreateValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Title_WhenEmpty_ShouldHaveError(string? title)
    {
        var request = CreateValidRequest() with { Title = title! };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Title_WhenExceeds200Chars_ShouldHaveError()
    {
        var request = CreateValidRequest() with { Title = new string('A', 201) };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Title_WhenExactly200Chars_ShouldNotHaveError()
    {
        var request = CreateValidRequest() with { Title = new string('A', 200) };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Description_WhenEmpty_ShouldHaveError(string? description)
    {
        var request = CreateValidRequest() with { Description = description! };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Description_WhenExceeds2000Chars_ShouldHaveError()
    {
        var request = CreateValidRequest() with { Description = new string('A', 2001) };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task StartingPrice_WhenNegative_ShouldHaveError()
    {
        var request = CreateValidRequest() with { StartingPrice = -1m };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.StartingPrice);
    }

    [Fact]
    public async Task StartingPrice_WhenZero_ShouldNotHaveError()
    {
        var request = CreateValidRequest() with { StartingPrice = 0m };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.StartingPrice);
    }

    [Fact]
    public async Task EndDate_WhenBeforeStartDate_ShouldHaveError()
    {
        var start = DateTime.UtcNow.AddDays(10);
        var request = CreateValidRequest() with { StartDate = start, EndDate = start.AddDays(-1) };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public async Task EndDate_WhenEqualToStartDate_ShouldHaveError()
    {
        var date = DateTime.UtcNow.AddDays(10);
        var request = CreateValidRequest() with { StartDate = date, EndDate = date };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public async Task CreatedByUserId_WhenEmpty_ShouldHaveError()
    {
        var request = CreateValidRequest() with { CreatedByUserId = Guid.Empty };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.CreatedByUserId);
    }

    [Fact]
    public async Task CategoryId_WhenEmpty_ShouldHaveError()
    {
        var request = CreateValidRequest() with { CategoryId = Guid.Empty };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }
}
