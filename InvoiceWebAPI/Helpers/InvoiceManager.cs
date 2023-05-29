using InvoiceWebAPI.Controllers;
using InvoiceWebAPI.InvoiceDatabase;
using Microsoft.EntityFrameworkCore;

namespace InvoiceWebAPI
{
    public class InvoiceManager
    {
        private readonly DbContextOptionsBuilder<InvoiceDbContext> _optionsBuilder = new();
        private readonly IConfiguration _configuration;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceManager(IConfiguration configuration, ILogger<InvoiceController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public List<Invoice> RetrieveInvoicesFromDb()
        {
            try
            {
                UseSqlServer();

                using (var dbContext = new InvoiceDbContext(_optionsBuilder.Options))
                {
                    List<Invoice> invoices = dbContext.Invoices.ToList();

                    return invoices;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<Invoice> RetrieveInvoiceByIdAsync(Guid id, string? currency = null)
        {
            try
            {
                UseSqlServer();

                using (var dbContext = new InvoiceDbContext(_optionsBuilder.Options))
                {
                    Invoice invoice = dbContext.Invoices.FirstOrDefault(i => i.Id == id) ?? throw new Exception($"The invoice with Id {id} was not found.");

                    if (currency != null && !currency.Equals(string.Empty) && currency != invoice.Currency)
                    {
                        CurrencyExchanger _currencyExchanger = new(_configuration, _logger);

                        decimal currencyExchange = await _currencyExchanger.GetExchangeRateAsync(invoice.Currency, currency).ConfigureAwait(false);
                        invoice.Amount *= currencyExchange;
                        invoice.Currency = currency;
                    }

                    return invoice;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public bool RegisterInvoiceInDb(Invoice invoice)
        {
            try
            {
                UseSqlServer();

                using (var dbContext = new InvoiceDbContext(_optionsBuilder.Options))
                {
                    dbContext.Invoices.Add(invoice);
                    dbContext.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateInvoiceInDbAsync(Guid id, Invoice updatedInvoice)
        {
            try
            {
                UseSqlServer();

                using (var dbContext = new InvoiceDbContext(_optionsBuilder.Options))
                {
                    Invoice existingInvoice = await RetrieveInvoiceByIdAsync(id) ?? 
                        throw new Exception($"Could not find the invoice with id {id}");

                    existingInvoice.Currency = updatedInvoice.Currency;
                    existingInvoice.Amount = updatedInvoice.Amount;
                    existingInvoice.Suplier = updatedInvoice.Suplier;
                    existingInvoice.Description = updatedInvoice.Description;

                    if (!(existingInvoice.Id == updatedInvoice.Id) || !(existingInvoice.DateIssued == updatedInvoice.DateIssued))
                    {
                        throw new Exception("Can not update Invoice Id or Date Issued");
                    }

                    dbContext.Invoices.Update(existingInvoice);
                    dbContext.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteInvoiceInDbAsync(Guid id)
        {
            try
            {
                UseSqlServer();

                using (var dbContext = new InvoiceDbContext(_optionsBuilder.Options))
                {
                    Invoice invoice = await RetrieveInvoiceByIdAsync(id) ?? 
                        throw new Exception($"Could not find the invoice with id {id}");

                    dbContext.Invoices.Remove(invoice);
                    dbContext.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        private void UseSqlServer()
        {
            try
            {
                _optionsBuilder.UseSqlServer(_configuration.GetValue<string>("ConnectionString"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Connection to DB failed.");
            }
        }
    }
}
