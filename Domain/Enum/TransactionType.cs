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
        DebitCard,
        BankTransfer,
        Other
    }

    public enum Category
    {
        Accomodation,
        FoodAndBeverage,
        Spa,
        Laundry,
        Transportation,
        Other
    }

    public enum TransactionStatus
    {
        Unpaid,
        Paid,
        Pending,
        Refunded
    }
}
