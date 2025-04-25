using System.ComponentModel.DataAnnotations;

namespace Quotation_Module_puc.Model.Domain
{
    public class Quote
    {
        // Primary Key
        public int? QuoteId { get; set; }

        // General Info
        public DateTime? CreationDate { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public string? Code { get; set; }
        public bool? AutoNumberEnable { get; set; }
        public int? JobNumberMin { get; set; }
        public int? JobNumberMax { get; set; }
        public int? JobNumberNext { get; set; }

        // Division Info
        public int? DivisionId { get; set; }
        public string? DivisionName { get; set; }
        public string? DivisionDescription { get; set; }

        // Associate Info
        public int? AssociateId { get; set; }
        public string? AssociateName { get; set; }
        public decimal? AssociateCommission { get; set; }

        // Contractor Info
        public int? ContractorId { get; set; }
        public string? ContractorName { get; set; }
        public string? ContractorAddress1 { get; set; }
        public string? ContractorAddress2 { get; set; }
        public string? ContractorPostalCode { get; set; }
        public string? ContractorPhone { get; set; }
        public string? ContractorPhone2 { get; set; }
        public string? ContractorFax { get; set; }
        public string? ContractorEmail { get; set; }
        public decimal? ContractorMarkup { get; set; }
        public string? ContractorWorkDown { get; set; }

        // Contact Info
        public string? ContactName { get; set; }

        // Employee Info
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public int? EmployeeDivisionId { get; set; }
        public string? EmployeeEmail { get; set; }
        public string? EmployeeMobile { get; set; }

        // Project Info
        public string? Project { get; set; }
        public string? Note { get; set; }

        // Type Info
        public string? TypeId { get; set; }
        public string? TypeName { get; set; }

        // Job Info
        public int? JobId { get; set; }
        public int? JobIdProjectManagerId { get; set; }
        public string? JobIdProjectManagerName { get; set; }

        // Sales Rep Info
        public int? SalesRepId { get; set; }
        public string? SalesRepName { get; set; }
        public int? SalesRepDivisionId { get; set; }
        public bool? SalesRepActive { get; set; }

        // Project Manager Info
        public int? ProjectManagerId { get; set; }
        public string? ProjectManagerName { get; set; }
        public int? ProjectManagerDivisionId { get; set; }
        public bool? ProjectManagerActive { get; set; }

        // Probability & Status
        public int? ProbabilityId { get; set; }
        public string? ProbabilityNote { get; set; }

        // Site Info
        public int? SiteId { get; set; }
        public string? SiteAddress1 { get; set; }
        public string? SiteAddress2 { get; set; }
        public string? SiteCity { get; set; }
        public string? SiteStateProv { get; set; }
        public string? SiteZipPostalCode { get; set; }
        public string? SiteCountry { get; set; }
        public string? SitePhone { get; set; }
        public string? SiteContactName { get; set; }
        public string? SiteContactEmail { get; set; }
        public string? SiteContactPhone { get; set; }
        public string? SiteContactCell { get; set; }

        // Site Info Usage
        public bool? MustUseSiteInfo { get; set; }

        // Printing Options
        public bool? PrintDimensionDetail { get; set; }
        public bool? PrintProfileDetail { get; set; }
        public bool? PrintMiscDetail { get; set; }
        public bool? PrintSinkDetail { get; set; }
        public bool? PrintGridQuote { get; set; }
    }
}
