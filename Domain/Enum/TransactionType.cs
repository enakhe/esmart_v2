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
        POS,
        Transfer,
        Other
    }

    public enum Category
    {
        Accomodation,
        Reservation,
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
