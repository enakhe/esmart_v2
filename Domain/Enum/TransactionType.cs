namespace ESMART.Domain.Enum
{
    public enum TransactionType
    {
        RoomCharge,
        BarOrder,
        RestaurantOrder,
        Laundry,
        TopUp,
        Payment,
        Refund,
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
        Deposit,
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
