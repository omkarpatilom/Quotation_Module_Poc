using Quotation_Module_puc.Model.Domain;

namespace Quotation_Module_puc.Repository.Interface
{
    public interface IQuotationService
    {
        List<Quote> GetAllQuotes();
         Task<int> CreateQuote(Quote quote);
        Quote GetQuoteById(int quoteId);
        void UpdateQuote(int quoteId, Quote quote);
        Task DeleteQuote(int quoteId);
    }
}
