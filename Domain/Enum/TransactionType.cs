namespace ESMART.Domain.Enum
{
    public enum TransactionType
    {
        Payment,
        Refund,
        Charge,
        Adjustment
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
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
