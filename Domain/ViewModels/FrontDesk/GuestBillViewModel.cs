﻿namespace ESMART.Domain.ViewModels.FrontDesk
{
    public class GuestBillViewModel
    {
        public string? TransactionId { get; set; }
        public string? Guest { get; set; }
        public string? GuestPhoneNo { get; set; }
        public string? ServiceId { get; set; }
        public string? Date { get; set; }
        public string? Amount { get; set; }
        public string? TaxAmount { get; set; }
        public string? ServiceCharge { get; set; }
        public string? Discount { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? BankAccount { get; set; }
        public string? CreatedBy { get; set; }
    }
}
