namespace Licit.WalletService.Domain.Exceptions;

public class InvalidAmountException(string operation)
    : Exception($"Geçersiz tutar: {operation} işlemi için tutar sıfırdan büyük olmalıdır.");
