using Microsoft.AspNetCore.Mvc;

namespace InvoiceWebAPI.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    public class InvoiceController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<InvoiceController> _logger;
        private readonly InvoiceManager _invoiceManager;

        public InvoiceController(IConfiguration configuration, ILogger<InvoiceController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _invoiceManager = new InvoiceManager(configuration, logger);
        }

        [HttpGet ("get")]
        public IActionResult GetInvoices()
        {
            try
            {
                List<Invoice> invoices = _invoiceManager.RetrieveInvoicesFromDb();
                _logger.LogInformation("All invoices retrieved successfully");

                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Failed to retrieve invoices");
            }

        }

        [HttpGet ("get/{id}")]
        public async Task<IActionResult> GetInvoiceAsync(Guid id, string? currency = null)
        {
            try
            {
                Invoice invoice = await _invoiceManager.RetrieveInvoiceByIdAsync(id, currency).ConfigureAwait(false);

                //_logger.LogInformation($"The invoice with Id {id} was retrieved successfully");

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Failed to retrieve invoice from database");

                return StatusCode(500, $"Failed to retrieve the invoice with Id {id}. {ex.Message}");
            }
        }

        [HttpPost ("register")]
        public IActionResult RegisterInvoice(Invoice invoice)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (_invoiceManager.RegisterInvoiceInDb(invoice))
                {
                    return CreatedAtAction(nameof(GetInvoiceAsync), new { id = invoice.Id }, invoice);
                }
                else
                {
                    return StatusCode(500, "Failed to register the invoice in the DataBase.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to register the invoice. {ex.Message}");
            }
        }

        [HttpPut ("update/{id}")]
        public async Task<IActionResult> UpdateInvoiceAsync(Guid id, Invoice updatedInvoice)
        {
            try
            {
                if(!await _invoiceManager.UpdateInvoiceInDbAsync(id, updatedInvoice).ConfigureAwait(false))
                {
                    throw (new Exception("Something went wrong while updating the invoice"));
                }

                return Ok($"Invoice {id} successfully updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest($"Could not update Invoice {id}. {ex.Message}");
            }
        }

        [HttpDelete ("delete/{id}")]
        public async Task<IActionResult> DeleteInvoiceAsync(Guid id)
        {
            try
            {
                if (!await _invoiceManager.DeleteInvoiceInDbAsync(id).ConfigureAwait(false))
                {
                    throw (new Exception("Could not delete the selected invoice"));
                }

                return Ok($"Invoice {id} was deleted from the DataBase");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest($"Could not delete Invoice {id}.");
            }
        }
    }
}