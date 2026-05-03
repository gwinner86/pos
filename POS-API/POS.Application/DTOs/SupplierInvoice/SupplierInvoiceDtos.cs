using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.SupplierInvoice
{
    public class SupplierInvoiceDto
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public Guid? GoodsReceiptId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal OutstandingAmount => TotalAmount - TotalPaid;
        public string Status { get; set; } = "Unpaid";
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSupplierInvoiceDto
    {
        public Guid SupplierId { get; set; }
        public Guid? GoodsReceiptId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        // Status defaults to Unpaid
    }

    public class UpdateSupplierInvoiceDto
    {
        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; }
    }
}
