using Library.Model.Enums;
using Library.Model.Parties;
using Syncfusion.XlsIO;
using System;

namespace Library.Service.Parties
{
    public interface IPartyReportService
    {
        IWorkbook GetPartyLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId);
        IWorkbook GetPartyLedgerReportBothCustomerVendor(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId);
        IWorkbook GetPartyLedgerReportXls(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId);
        IWorkbook GetPartyCategoryLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string partyType, string partyCategoryId, string fromDate, string toDate);
        IWorkbook GetPartyLedgerReportGroupByGL(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId);
        IWorkbook GetPartyLedgerReportGroupByGLXls(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId);
        IWorkbook GetInterPartyLedger(string companyGroupId, string CompanyId, string PlantId,string PlantName,string FromDate, string ToDate);

        IWorkbook GetPartyOpeningBalanceLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string partyId, string partyPlantId, string fiscalYearId);

        IWorkbook PartyOutstadningReport(string companyGroupId, string companyId, string plantId,string plantName, string reportName, SourceType sourceType, DateTime postingDate);

        IWorkbook PartyReport(string type, string companyGroupId, string companyId, string plantId);
        IWorkbook GetPartyOutstandingReport(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId);
        IWorkbook GetPartyPaymentStatusReportGL(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId);
        IWorkbook GetPartyPaymentStatusReportGroupByGLXls(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId);
        IWorkbook GetPartyPaymentStatusLedgerReport3(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId);
        IWorkbook GetPartyPaymentStatusLedgerReportXls(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId);
        IWorkbook GetShortPartyPaymentStatusLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId, string partyName);

       IWorkbook GetPartyLedgerReportLongSizeXls(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId);
        IWorkbook GetPartyLedgerReportGroupByGLReportLongSizeXls(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId);
    }
}