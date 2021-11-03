using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

public class FunctionPara
{
    public string PlantId { get; set; }
    public string FromDate { get; set; }
    public string ToDate { get; set; }
    public string GroupId { get; set; }
    public string lblSalaryProcSystemId { get; set; }
    public string lblSalaryProcId { get; set; }
    public DataSet dsGrid { get; set; }
    public string lblTaxYearID { get; set; }
    public string lblLocalCurrencyID { get; set; }
    public string txtForeignCurRate { get; set; }
    public string lblLocalCurRate { get; set; }
    public string lblUseFrgCurID { get; set; }
    public string USER { get; set; }
    public string lblForeignCurrencyID { get; set; }
    public string lblTaxPeriod { get; set; }
    public string lblEmpCount { get; set; }
    public string ShowLog { get; set; }//
    public string txtDescription { get; set; }//txtDescription
    //public string txtReportDownloadFlag { get; set; }//txtDescription
    public bool IsOTEntitled { get; set; }
    public bool IsMaternity { get; set; }
    public bool IsSeparated { get; set; }
    public bool IsMaternityReturn { get; set; }
    public decimal OTRate { get; set; }
    public bool IsNegativeSalaryApplicable { get; set; } = false;
    public string NegativeSalaryHeadId { get; set; }
    public IclsAdvanceProcess ParaclsAdvanceProcess { get; set; }
    public ISalaryHeadWiseAmountTransaction ParaSalaryHeadWiseAmountTransaction { get; set; }
    public ISalaryHeadWiseFixedService ParaSalaryHeadWiseFixedService { get; set; }
    public ISalaryHeadWiseDailyService ParaSalaryHeadWiseDailyService { get; set; }
}
