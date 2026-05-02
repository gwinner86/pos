using FluentValidation;

namespace POS.Application.DTOs.SupplierInvoice.Validators
{
    public class CreateSupplierInvoiceDtoValidator : AbstractValidator<CreateSupplierInvoiceDto>
    {
        public CreateSupplierInvoiceDtoValidator()
        {
            RuleFor(x => x.SupplierId).NotEmpty().WithMessage("Supplier ID is required.");
            RuleFor(x => x.InvoiceNumber).NotEmpty().WithMessage("Invoice Number is required.");
            RuleFor(x => x.TotalAmount).GreaterThan(0).WithMessage("Total Amount must be greater than zero.");
            RuleFor(x => x.InvoiceDate).NotEmpty().WithMessage("Invoice Date is required.");
            RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.InvoiceDate)
                .WithMessage("Due Date cannot be before Invoice Date.");
        }
    }

    public class UpdateSupplierInvoiceDtoValidator : AbstractValidator<UpdateSupplierInvoiceDto>
    {
        public UpdateSupplierInvoiceDtoValidator()
        {
             RuleFor(x => x.TotalAmount)
                .GreaterThan(0).When(x => x.TotalAmount.HasValue)
                .WithMessage("Total Amount must be greater than zero.");
             
             RuleFor(x => x.Status)
                 .Must(status => new[] { "Unpaid", "PartiallyPaid", "Paid", "Overdue" }.Contains(status))
                 .When(x => !string.IsNullOrEmpty(x.Status))
                 .WithMessage("Invalid Status.");
        }
    }
}
