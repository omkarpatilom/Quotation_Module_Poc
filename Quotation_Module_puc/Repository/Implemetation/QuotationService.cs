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
        using var cmd = new SqlCommand("SELECT TOP 1000 * FROM Quote_Info order by CreationDate desc ;", conn);
        //using var cmd = new SqlCommand("SELECT TOP 50 * FROM Quote_Info WHERE AcceptedDate IS NOT NULL and AssociateName is not null ORDER BY QuoteId DESC;", conn);
        //using var cmd = new SqlCommand("SELECT TOP 50 * FROM Quote_Info WHERE AcceptedDate IS NULL ORDER BY QuoteId DESC;", conn);
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

    public  void UpdateQuote(int quoteId, Quote quote)
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

    private void AddQuoteParameters(SqlCommand cmd, Quote quote)
    {
        cmd.Parameters.AddWithValue("@CreationDate", quote.CreationDate);
        cmd.Parameters.AddWithValue("@DivisionId", quote.DivisionId);
        cmd.Parameters.AddWithValue("@ContractorId", quote.ContractorId);
        cmd.Parameters.AddWithValue("@AssociateId", (object?)quote.AssociateId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ContactName", (object?)quote.ContactName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@EmployeeId", (object?)quote.EmployeeId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Project", (object?)quote.Project ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SiteId", (object?)quote.SiteId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Note", (object?)quote.Note ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TypeId", quote.TypeId);
        cmd.Parameters.AddWithValue("@UseSiteInfo", quote.MustUseSiteInfo);
        cmd.Parameters.AddWithValue("@PrintDimensionDetail", quote.PrintDimensionDetail);
        cmd.Parameters.AddWithValue("@PrintProfileDetail", quote.PrintProfileDetail);
        cmd.Parameters.AddWithValue("@PrintMiscDetail", quote.PrintMiscDetail);
        cmd.Parameters.AddWithValue("@PrintSinkDetail", quote.PrintSinkDetail);
        cmd.Parameters.AddWithValue("@ProbabilityId", (object?)quote.ProbabilityId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ProbabilityNote", (object?)quote.ProbabilityNote ?? DBNull.Value);
    }


    private Quote MapQuote(SqlDataReader reader)
    {
        var quote = new Quote();

        bool? GetBool(string column) => reader[column] != DBNull.Value ? Convert.ToBoolean(reader[column]) : (bool?)null;

        // Primary Key
        quote.QuoteId = reader["QuoteId"] != DBNull.Value ? Convert.ToInt32(reader["QuoteId"]) : (int?)null;

        // General Info
        quote.CreationDate = reader["CreationDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreationDate"]) : (DateTime?)null;
        quote.AcceptedDate = reader["AcceptedDate"] != DBNull.Value ? Convert.ToDateTime(reader["AcceptedDate"]) : (DateTime?)null;
        quote.Code = reader["Code"] != DBNull.Value ? reader["Code"].ToString() : null;
        quote.AutoNumberEnable = reader["AutoNumberEnable"] != DBNull.Value ? Convert.ToBoolean(reader["AutoNumberEnable"]) : (bool?)null;
        quote.JobNumberMin = reader["JobNumberMin"] != DBNull.Value ? Convert.ToInt32(reader["JobNumberMin"]) : (int?)null;
        quote.JobNumberMax = reader["JobNumberMax"] != DBNull.Value ? Convert.ToInt32(reader["JobNumberMax"]) : (int?)null;
        quote.JobNumberNext = reader["JobNumberNext"] != DBNull.Value ? Convert.ToInt32(reader["JobNumberNext"]) : (int?)null;

        // Division Info
        quote.DivisionId = reader["DivisionId"] != DBNull.Value ? Convert.ToInt32(reader["DivisionId"]) : (int?)null;
        quote.DivisionName = reader["DivisionName"] != DBNull.Value ? reader["DivisionName"].ToString() : null;
        quote.DivisionDescription = reader["DivisionDescription"] != DBNull.Value ? reader["DivisionDescription"].ToString() : null;

        // Associate Info
        quote.AssociateId = reader["AssociateId"] != DBNull.Value ? Convert.ToInt32(reader["AssociateId"]) : (int?)null;
        quote.AssociateName = reader["AssociateName"] != DBNull.Value ? reader["AssociateName"].ToString() : null;
        quote.AssociateCommission = reader["AssociateCommission"] != DBNull.Value ? Convert.ToDecimal(reader["AssociateCommission"]) : (decimal?)null;

        // Contractor Info
        quote.ContractorId = reader["ContractorId"] != DBNull.Value ? Convert.ToInt32(reader["ContractorId"]) : (int?)null;
        quote.ContractorName = reader["ContractorName"] != DBNull.Value ? reader["ContractorName"].ToString() : null;
        quote.ContractorAddress1 = reader["ContractorAddress_1"] != DBNull.Value ? reader["ContractorAddress_1"].ToString() : null;
        quote.ContractorAddress2 = reader["ContractorAddress_2"] != DBNull.Value ? reader["ContractorAddress_2"].ToString() : null;
        quote.ContractorPostalCode = reader["ContractorPostalCode"] != DBNull.Value ? reader["ContractorPostalCode"].ToString() : null;
        quote.ContractorPhone = reader["ContractorPhone"] != DBNull.Value ? reader["ContractorPhone"].ToString() : null;
        quote.ContractorPhone2 = reader["ContractorPhone_2"] != DBNull.Value ? reader["ContractorPhone_2"].ToString() : null;
        quote.ContractorFax = reader["ContractorFax"] != DBNull.Value ? reader["ContractorFax"].ToString() : null;
        quote.ContractorEmail = reader["ContractorEmail"] != DBNull.Value ? reader["ContractorEmail"].ToString() : null;
        quote.ContractorMarkup = reader["ContractorMarkup"] != DBNull.Value ? Convert.ToDecimal(reader["ContractorMarkup"]) : (decimal?)null;
        quote.ContractorWorkDown = reader["ContractorWorkDown"] != DBNull.Value ? reader["ContractorWorkDown"].ToString() : null;

        // Contact Info
        quote.ContactName = reader["ContactName"] != DBNull.Value ? reader["ContactName"].ToString() : null;

        // Employee Info
        quote.EmployeeId = reader["EmployeeId"] != DBNull.Value ? Convert.ToInt32(reader["EmployeeId"]) : (int?)null;
        quote.EmployeeName = reader["EmployeeName"] != DBNull.Value ? reader["EmployeeName"].ToString() : null;
        quote.EmployeeDivisionId = reader["EmployeeDivisionId"] != DBNull.Value ? Convert.ToInt32(reader["EmployeeDivisionId"]) : (int?)null;
        quote.EmployeeEmail = reader["EmployeeEmail"] != DBNull.Value ? reader["EmployeeEmail"].ToString() : null;
        quote.EmployeeMobile = reader["EmployeeMobile"] != DBNull.Value ? reader["EmployeeMobile"].ToString() : null;

        // Project Info
        quote.Project = reader["Project"] != DBNull.Value ? reader["Project"].ToString() : null;
        quote.Note = reader["Note"] != DBNull.Value ? reader["Note"].ToString() : null;

        // Type Info
        quote.TypeId = reader["TypeId"] != DBNull.Value ? reader["TypeId"].ToString() : null;
        quote.TypeName = reader["TypeName"] != DBNull.Value ? reader["TypeName"].ToString() : null;

        // Job Info
        quote.JobId = reader["JobId"] != DBNull.Value ? reader["JobId"].ToString() : null;
        quote.JobIdProjectManagerId = reader["JobIdProjectManagerId"] != DBNull.Value ? Convert.ToInt32(reader["JobIdProjectManagerId"]) : (int?)null;
        quote.JobIdProjectManagerName = reader["JobIdProjectManagerName"] != DBNull.Value ? reader["JobIdProjectManagerName"].ToString() : null;

        // Sales Rep Info
        quote.SalesRepId = reader["SalesRepId"] != DBNull.Value ? Convert.ToInt32(reader["SalesRepId"]) : (int?)null;
        quote.SalesRepName = reader["SalesRepName"] != DBNull.Value ? reader["SalesRepName"].ToString() : null;
        quote.SalesRepDivisionId = reader["SalesRepDivisionId"] != DBNull.Value ? Convert.ToInt32(reader["SalesRepDivisionId"]) : (int?)null;
        quote.SalesRepActive = reader["SalesRepActive"] != DBNull.Value ? Convert.ToBoolean(reader["SalesRepActive"]) : (bool?)null;

        // Project Manager Info
        quote.ProjectManagerId = reader["ProjectManagerId"] != DBNull.Value ? Convert.ToInt32(reader["ProjectManagerId"]) : (int?)null;
        quote.ProjectManagerName = reader["ProjectManagerName"] != DBNull.Value ? reader["ProjectManagerName"].ToString() : null;
        quote.ProjectManagerDivisionId = reader["ProjectManagerDivisionId"] != DBNull.Value ? Convert.ToInt32(reader["ProjectManagerDivisionId"]) : (int?)null;
        quote.ProjectManagerActive = reader["ProjectManagerActive"] != DBNull.Value ? Convert.ToBoolean(reader["ProjectManagerActive"]) : (bool?)null;

        // Probability & Status
        quote.ProbabilityId = reader["ProbabilityId"] != DBNull.Value ? Convert.ToInt32(reader["ProbabilityId"]) : (int?)null;
        quote.ProbabilityNote = reader["ProbabilityNote"] != DBNull.Value ? reader["ProbabilityNote"].ToString() : null;

        // Site Info
        quote.SiteId = reader["SiteId"] != DBNull.Value ? Convert.ToInt32(reader["SiteId"]) : (int?)null;
        quote.SiteAddress1 = reader["SiteAddress_1"] != DBNull.Value ? reader["SiteAddress_1"].ToString() : null;
        quote.SiteAddress2 = reader["SiteAddress_2"] != DBNull.Value ? reader["SiteAddress_2"].ToString() : null;
        quote.SiteCity = reader["SiteCity"] != DBNull.Value ? reader["SiteCity"].ToString() : null;
        quote.SiteStateProv = reader["SiteStateProv"] != DBNull.Value ? reader["SiteStateProv"].ToString() : null;
        quote.SiteZipPostalCode = reader["SiteZipPostalCode"] != DBNull.Value ? reader["SiteZipPostalCode"].ToString() : null;
        quote.SiteCountry = reader["SiteCountry"] != DBNull.Value ? reader["SiteCountry"].ToString() : null;
        quote.SitePhone = reader["SitePhone"] != DBNull.Value ? reader["SitePhone"].ToString() : null;
        quote.SiteContactName = reader["SiteContactName"] != DBNull.Value ? reader["SiteContactName"].ToString() : null;
        quote.SiteContactEmail = reader["SiteContactEmail"] != DBNull.Value ? reader["SiteContactEmail"].ToString() : null;
        quote.SiteContactPhone = reader["SiteContactPhone"] != DBNull.Value ? reader["SiteContactPhone"].ToString() : null;
        quote.SiteContactCell = reader["SiteContactCell"] != DBNull.Value ? reader["SiteContactCell"].ToString() : null;

        // Site Info Usage
        quote.MustUseSiteInfo = reader["MustUseSiteInfo"] != DBNull.Value ? Convert.ToBoolean(reader["MustUseSiteInfo"]) : (bool?)null;

        // Printing Options
        quote.PrintDimensionDetail = reader["PrintDimensionDetail"] != DBNull.Value ? Convert.ToBoolean(reader["PrintDimensionDetail"]) : (bool?)null;
        quote.PrintProfileDetail = reader["PrintProfileDetail"] != DBNull.Value ? Convert.ToBoolean(reader["PrintProfileDetail"]) : (bool?)null;
        quote.PrintMiscDetail = reader["PrintMiscDetail"] != DBNull.Value ? Convert.ToBoolean(reader["PrintMiscDetail"]) : (bool?)null;
        quote.PrintSinkDetail = reader["PrintSinkDetail"] != DBNull.Value ? Convert.ToBoolean(reader["PrintSinkDetail"]) : (bool?)null;
        //quote.PrintGridQuote = GetBool("PrintGridQuote");
        //quote.PrintGridQuote = reader["PrintGridQuote"] != DBNull.Value ? Convert.ToBoolean(reader["PrintGridQuote"]) : (bool?)null;

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
