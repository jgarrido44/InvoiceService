using InvoiceWebAPI.Controllers;
using InvoiceWebAPI.InvoiceDatabase;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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

        public Invoice RegisterInvoiceInDb(
            string suplier,
            string currency,
            decimal amount,
            string? description)
        {
            try
            {
                UseSqlServer();

                Invoice invoice = new()
                {
                    Id = Guid.NewGuid(),
                    Suplier = suplier,
                    DateIssued = DateTime.UtcNow,
                    Currency = currency,
                    Amount = (decimal)amount,
                    Description = description ?? "Empty Field"

                };

                if (!IsValidCurrencyCode(currency))
                {
                    throw new Exception("Invalid currency code. Currency codes must be composed by 3 uppercase letters");
                }

                using (var dbContext = new InvoiceDbContext(_optionsBuilder.Options))
                {
                    dbContext.Invoices.Add(invoice);
                    dbContext.SaveChanges();
                    return invoice;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<Invoice> UpdateInvoiceInDbAsync(
            Guid id, 
            string? suplier = null,  
            string? currency = null, 
            decimal? amount = null, 
            string? description = null)
        {
            try
            {
                UseSqlServer();

                using (var dbContext = new InvoiceDbContext(_optionsBuilder.Options))
                {
                    Invoice existingInvoice = await RetrieveInvoiceByIdAsync(id) ?? 
                        throw new Exception($"Could not find the invoice with id {id}");

                    if (!string.IsNullOrEmpty(suplier))
                    {
                        existingInvoice.Suplier = suplier;
                    }

                    if (!string.IsNullOrEmpty(currency))
                    {
                        if (!IsValidCurrencyCode(currency))
                        {
                            throw new Exception("Invalid currency code. Currency codes must be composed by 3 uppercase letters");
                        }
                        else
                        {
                            existingInvoice.Currency = currency;
                        }
                    }

                    if (amount != null)
                    {
                        existingInvoice.Amount = (decimal)amount;
                    }

                    if (!string.IsNullOrEmpty(description))
                    {
                        existingInvoice.Description = description;
                    }

                    dbContext.Invoices.Update(existingInvoice);
                    dbContext.SaveChanges();
                    return existingInvoice;
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

        private static bool IsValidCurrencyCode(string currencyCode)
        {
            string pattern = @"^[A-Z]{3}$"; // Three uppercase letters

            Regex regex = new(pattern);

            return regex.IsMatch(currencyCode);

        }
    }
}
