using FluentValidation.TestHelper;
using Licit.WalletService.Application.Features.CQRS.Wallet.GetTransactions;

namespace Licit.WalletService.UnitTests.Application.Validators;

public class GetTransactionsQueryValidatorTests
{
    private readonly GetTransactionsQueryValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new GetTransactionsQueryRequest(Guid.NewGuid(), 1, 20));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task UserId_WhenEmpty_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new GetTransactionsQueryRequest(Guid.Empty, 1, 20));
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Page_WhenNotPositive_ShouldHaveError(int page)
    {
        var result = await _validator.TestValidateAsync(new GetTransactionsQueryRequest(Guid.NewGuid(), page, 20));
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task PageSize_WhenOutOfRange_ShouldHaveError(int pageSize)
    {
        var result = await _validator.TestValidateAsync(new GetTransactionsQueryRequest(Guid.NewGuid(), 1, pageSize));
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}
