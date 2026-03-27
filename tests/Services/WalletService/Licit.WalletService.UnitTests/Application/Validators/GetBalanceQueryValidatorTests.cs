using FluentValidation.TestHelper;
using Licit.WalletService.Application.Features.CQRS.Wallet.GetBalance;

namespace Licit.WalletService.UnitTests.Application.Validators;

public class GetBalanceQueryValidatorTests
{
    private readonly GetBalanceQueryValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new GetBalanceQueryRequest(Guid.NewGuid()));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task UserId_WhenEmpty_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new GetBalanceQueryRequest(Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}
