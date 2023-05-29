using Microsoft.EntityFrameworkCore;

namespace InvoiceWebAPI.InvoiceDatabase
{
    public class InvoiceDbContext : DbContext
    {
        public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : base(options) {
        }

        public DbSet<Invoice> Invoices { get; set; }
    }
}
