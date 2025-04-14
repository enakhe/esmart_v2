namespace ESMART.Domain.Enum
{
    public enum TransactionType
    {
        Payment,
        Refund,
        Charge,
        Adjustment
    }

    public enum PaymentMethod
    {
        Cash,
        CreditCard,
        DebitCard,
        BankTransfer,
        MobilePayment,
        Other
    }
}
