#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    /// <summary>   Interface for SOP Item service. </summary>
    public interface ISkillMatrixService : IService<SkillMatrix> 
    {
        //GridModel Query(GridParameter parameters, string companyGroupId);

        //void InsertGraph(SOPItem entity, IEnumerable<SOPAttachmentDetail> sopAttachmentDetail);

        //void UpdateGraph(SOPItem entity, IEnumerable<SOPAttachmentDetail> sopAttachmentDetail);

        //GridModel Query(GridParameter parameters, string companyGroupId, string[] sopItemIds);

        IEnumerable<object> GetSkillMaster();
        IEnumerable<object> GetSkillMasterDetail(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess);
        IEnumerable<object> GetGraphDetails(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess);


		IEnumerable<object> GetEntiryWiseData(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess);

		

		IEnumerable<object> GetGraphDetails1();

        IEnumerable<object> GetProcess();
        IEnumerable<object> GetEntity();

        //decimal GetAutoSequence(); 

        IEnumerable<object> GetSkillMasterDetailSummary(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess);
        
        IEnumerable<object> Designation();
        IWorkbook MatrixReport(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string MaterialId, string ArticleId, string queryString);



    }
}