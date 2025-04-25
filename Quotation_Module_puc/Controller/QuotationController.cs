using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quotation_Module_puc.Model.Domain;
using Quotation_Module_puc.Repository.Interface;

namespace Quotation_Module_puc.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotationController : ControllerBase
    {
        private readonly IQuotationService _quotationService;

        public QuotationController(IQuotationService quotationService)
        {
            _quotationService = quotationService;
        }
        [HttpGet]
        public ActionResult<IEnumerable<Quote>> GetAllQuotes()
        {
            var quotes = _quotationService.GetAllQuotes();
            return Ok(quotes);
        }

        [HttpPost]
        public ActionResult<int> CreateQuote([FromBody] Quote quote)
        {
            if (quote == null)
            {
                return BadRequest("Invalid quote data.");
            }

            var quoteId = _quotationService.CreateQuote(quote);
            return CreatedAtAction(nameof(GetQuoteById), new { id = quoteId }, quoteId);
        }

        [HttpGet("{id}")]
        public ActionResult<Quote> GetQuoteById(int id)
        {
            var quote = _quotationService.GetQuoteById(id);

            if (quote == null)
            {
                return NotFound($"Quote with ID {id} not found.");
            }

            return Ok(quote);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateQuote(int id, [FromBody] Quote quote)
        {
            if (quote == null)
            {
                return BadRequest("Invalid quote data.");
            }

            _quotationService.UpdateQuote(id, quote);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteQuote(int id)
        {
            _quotationService.DeleteQuote(id);
            return NoContent();
        }
    }
}