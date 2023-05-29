using System.ComponentModel.DataAnnotations;

namespace InvoiceWebAPI
{
    public class Invoice
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Suplier { get; set; }
        [Required]
        public DateTime DateIssued { get; set; }
        [Required]
        public string Currency { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}
