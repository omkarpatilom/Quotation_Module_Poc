using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Quotation_Module_puc.Model.Domain;
using Quotation_Module_puc.Repository.Interface;

public class QuotationService : IQuotationService
{
    private readonly string _connectionString;

    public QuotationService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }


    public List<Quote> GetAllQuotes()
    {
        var quotes = new List<Quote>();

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("SELECT TOP 50 * FROM Quote_Info WHERE AcceptedDate IS NULL ORDER BY QuoteId DESC;", conn);
        //using var cmd = new SqlCommand("SELECT * FROM Quote_Info WHERE (AcceptedDate IS NULL) ORDER BY QuoteId Desc", conn);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        // Iterate over all rows and map each row to a Quote object
        while (reader.Read())
        {
            var quote = MapQuote(reader);
            quotes.Add(quote);
        }

        return quotes;
    }


    public async Task<int> CreateQuote(Quote quote)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(@"
            INSERT INTO QUOTES (
                CreationDate, DivisionId, ContractorId, AssociateId, ContactName,
                EmployeeId, Project, SiteId, Note,
                TypeId, MustUseSiteInfo, PrintDimensionDetail,
                PrintProfileDetail, PrintMiscDetail, PrintSinkDetail,
                ProbabilityId, ProbabilityNote
            ) VALUES (
                @CreationDate, @DivisionId, @ContractorId, @AssociateId, @ContactName,
                @EmployeeId, @Project, @SiteId, @Note,
                @TypeId, @UseSiteInfo, @PrintDimensionDetail,
                @PrintProfileDetail, @PrintMiscDetail, @PrintSinkDetail,
                @ProbabilityId, @ProbabilityNote
            );
            SELECT SCOPE_IDENTITY();
        ", conn);

        AddQuoteParameters(cmd, quote);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Quote GetQuoteById(int quoteId)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("SELECT * FROM Quote_Info WHERE QuoteId = @QuoteId", conn);
        cmd.Parameters.AddWithValue("@QuoteId", quoteId);
        conn.Open();
        using var reader =  cmd.ExecuteReader();
        return reader.Read() ? MapQuote(reader) : null;
    }

    public async Task UpdateQuote(int quoteId, Quote quote)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(@"
            UPDATE QUOTES SET
                CreationDate = @CreationDate, DivisionId = @DivisionId, ContractorId = @ContractorId,
                AssociateId = @AssociateId, ContactName = @ContactName, EmployeeId = @EmployeeId,
                Project = @Project, SiteId = @SiteId, Note = @Note,
                TypeId = @TypeId, MustUseSiteInfo = @UseSiteInfo,
                PrintDimensionDetail = @PrintDimensionDetail, PrintProfileDetail = @PrintProfileDetail,
                PrintMiscDetail = @PrintMiscDetail, PrintSinkDetail = @PrintSinkDetail,
                ProbabilityId = @ProbabilityId, ProbabilityNote = @ProbabilityNote
            WHERE QuoteId = @QuoteId;
        ", conn);

        AddQuoteParameters(cmd, quote);
        cmd.Parameters.AddWithValue("@QuoteId", quoteId);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public async Task DeleteQuote(int quoteId)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        var tran = conn.BeginTransaction();
        try
        {
            new SqlCommand(@"
                DELETE FROM QUOTES_REVISIONS_DETAILS
                WHERE RevisionId IN (SELECT DISTINCT RevisionId FROM QUOTES_REVISIONS WHERE QuoteId = @QuoteId)", conn, tran)
                .AddParam("@QuoteId", quoteId).ExecuteNonQuery();

            new SqlCommand("DELETE FROM QUOTES_REVISIONS WHERE QuoteId = @QuoteId", conn, tran)
                .AddParam("@QuoteId", quoteId).ExecuteNonQuery();

            new SqlCommand("DELETE FROM QUOTES_PRICINGS WHERE QuoteId = @QuoteId", conn, tran)
                .AddParam("@QuoteId", quoteId).ExecuteNonQuery();

            new SqlCommand("DELETE FROM QUOTES WHERE QuoteId = @QuoteId", conn, tran)
                .AddParam("@QuoteId", quoteId).ExecuteNonQuery();

            tran.Commit();
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    private async Task AddQuoteParameters(SqlCommand cmd, Quote quote)
    {
        cmd.Parameters.AddWithValue("@CreationDate", quote.CreationDate);
        cmd.Parameters.AddWithValue("@DivisionId", quote.DivisionId);
        cmd.Parameters.AddWithValue("@ContractorId", quote.ContractorId);
        cmd.Parameters.AddWithValue("@AssociateId", quote.AssociateId);
        cmd.Parameters.AddWithValue("@ContactName", quote.ContactName);
        cmd.Parameters.AddWithValue("@EmployeeId", quote.EmployeeId);
        cmd.Parameters.AddWithValue("@Project", quote.Project);
        cmd.Parameters.AddWithValue("@SiteId", quote.SiteId);
        cmd.Parameters.AddWithValue("@Note", quote.Note);
        cmd.Parameters.AddWithValue("@TypeId", quote.TypeId);
        cmd.Parameters.AddWithValue("@UseSiteInfo", quote.MustUseSiteInfo);
        cmd.Parameters.AddWithValue("@PrintDimensionDetail", quote.PrintDimensionDetail);
        cmd.Parameters.AddWithValue("@PrintProfileDetail", quote.PrintProfileDetail);
        cmd.Parameters.AddWithValue("@PrintMiscDetail", quote.PrintMiscDetail);
        cmd.Parameters.AddWithValue("@PrintSinkDetail", quote.PrintSinkDetail);
        cmd.Parameters.AddWithValue("@ProbabilityId", quote.ProbabilityId);
        cmd.Parameters.AddWithValue("@ProbabilityNote", quote.ProbabilityNote);
    }

    private Quote MapQuote(SqlDataReader reader)
    {
        var quote = new Quote();

        // Handle QuoteId safely with TryParse (if it's not guaranteed to be an int)
        quote.QuoteId = reader["QuoteId"] != DBNull.Value ? Convert.ToInt32(reader["QuoteId"]) : (int?)null;

        // Handle DateTime safely
        quote.CreationDate = reader["CreationDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreationDate"]) : (DateTime?)null;

        // Handle DivisionId safely with TryParse
        quote.DivisionId = reader["DivisionId"] != DBNull.Value ? Convert.ToInt32(reader["DivisionId"]) : (int?)null;

        // Handle ContractorId safely with TryParse
        quote.ContractorId = reader["ContractorId"] != DBNull.Value ? Convert.ToInt32(reader["ContractorId"]) : (int?)null;

        // Handle AssociateId safely with TryParse
        quote.AssociateId = reader["AssociateId"] != DBNull.Value ? Convert.ToInt32(reader["AssociateId"]) : (int?)null;

        // Handle ContactName as a string (if nullable)
        quote.ContactName = reader["ContactName"] != DBNull.Value ? reader["ContactName"].ToString() : null;

        // Handle EmployeeId safely with TryParse
        quote.EmployeeId = reader["EmployeeId"] != DBNull.Value ? Convert.ToInt32(reader["EmployeeId"]) : (int?)null;

        // Handle Project as a string (if nullable)
        quote.Project = reader["Project"] != DBNull.Value ? reader["Project"].ToString() : null;

        // Handle SiteId safely with TryParse
        quote.SiteId = reader["SiteId"] != DBNull.Value ? Convert.ToInt32(reader["SiteId"]) : (int?)null;

        // Handle Note as a string (if nullable)
        quote.Note = reader["Note"] != DBNull.Value ? reader["Note"].ToString() : null;

        // Handle TypeId safely with TryParse
        quote.TypeId = reader["TypeId"] != DBNull.Value ? reader["TypeId"].ToString() : null;

        // Handle MustUseSiteInfo safely with TryParse
        quote.MustUseSiteInfo = reader["MustUseSiteInfo"] != DBNull.Value ? Convert.ToBoolean(reader["MustUseSiteInfo"]) : (bool?)null;

        // Handle PrintDimensionDetail safely with TryParse
        quote.PrintDimensionDetail = reader["PrintDimensionDetail"] != DBNull.Value ? Convert.ToBoolean(reader["PrintDimensionDetail"]) : (bool?)null;

        // Handle PrintProfileDetail safely with TryParse
        quote.PrintProfileDetail = reader["PrintProfileDetail"] != DBNull.Value ? Convert.ToBoolean(reader["PrintProfileDetail"]) : (bool?)null;

        // Handle PrintMiscDetail safely with TryParse
        quote.PrintMiscDetail = reader["PrintMiscDetail"] != DBNull.Value ? Convert.ToBoolean(reader["PrintMiscDetail"]) : (bool?)null;

        // Handle PrintSinkDetail safely with TryParse
        quote.PrintSinkDetail = reader["PrintSinkDetail"] != DBNull.Value ? Convert.ToBoolean(reader["PrintSinkDetail"]) : (bool?)null;

        // Handle ProbabilityId safely with TryParse
        quote.ProbabilityId = reader["ProbabilityId"] != DBNull.Value ? Convert.ToInt32(reader["ProbabilityId"]) : (int?)null;

        // Handle ProbabilityNote as a string (if nullable)
        quote.ProbabilityNote = reader["ProbabilityNote"] != DBNull.Value ? reader["ProbabilityNote"].ToString() : null;

        return quote;
    }


}

// Helper Extension Method
public static class SqlCommandExtensions
{
    public static SqlCommand AddParam(this SqlCommand cmd, string name, object value)
    {
        cmd.Parameters.AddWithValue(name, value);
        return cmd;
    }
}
