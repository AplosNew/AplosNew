using Syncfusion.XlsIO;

namespace Library.Service.Organizations
{
    public interface IOrganizationReportService
    {
        IWorkbook GetEntity(string companyGroupId, string companyId);

        IWorkbook GetManpowerBudget(string companyGroupId, string companyId);

        IWorkbook GetDesignationMaster(string companyGroupId);
    }
}