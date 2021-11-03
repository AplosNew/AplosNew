using OTSBD;
using System;
using System.Data;
using System.Drawing;
using System.Web.UI.WebControls;

public class clsCommonUtility
{
    public void ShowShiftInfo(string emppk, string attndDate, clsShiftShowLabel cls)
    {
        clsEmployeeLoad objApp = null;
        DataSet dsEmpShift, dsPreShift = null;
        DataSet dsEmpWeekOff = null;
        try
        {
            objApp = new OTSBD.clsEmployeeLoad();
            objApp.GetEmployeeShiftInfo(emppk, attndDate, out dsEmpShift);

            if (dsEmpShift.Tables[0].Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(dsEmpShift.Tables[0].Rows[0]["EffectiveDate"].ToString()))
                {
                    if (cls.lblCurrentEffectiveDate != null)
                    {
                        cls.lblCurrentEffectiveDate.Text = dsEmpShift.Tables[0].Rows[0]["EffectiveDate"].ToString();
                    }
                }
                if (!string.IsNullOrEmpty(dsEmpShift.Tables[0].Rows[0]["ShiftDefinationDescription"].ToString()))
                {
                    if (cls.lblShiftName != null)
                    {
                        cls.lblShiftName.Text = dsEmpShift.Tables[0].Rows[0]["ShiftDefinationDescription"].ToString();
                    }
                }
                if (!string.IsNullOrEmpty(dsEmpShift.Tables[0].Rows[0]["LeastPunchTime"].ToString()))
                {
                    if (cls.lblLeastPunchTime != null)
                    {
                        cls.lblLeastPunchTime.Text = dsEmpShift.Tables[0].Rows[0]["LeastPunchTime"].ToString();
                    }
                }
                if (!string.IsNullOrEmpty(dsEmpShift.Tables[0].Rows[0]["DayStatus"].ToString()))
                {
                    if (cls.lblDayStatus != null)
                    {
                        cls.lblDayStatus.Text = dsEmpShift.Tables[0].Rows[0]["DayStatus"].ToString();
                    }
                }
                if (bplib.clsWebLib.GetBoolData(dsEmpShift.Tables[0].Rows[0]["IsFix"].ToString()))
                {
                    cls.lblCurrentShift.Text = dsEmpShift.Tables[0].Rows[0]["Fixed"].ToString() + " (Fixed)";
                    ///
                    //weekoff
                    objApp.GetEmployeeWeekOffByDay(emppk, out dsEmpWeekOff);
                    if (dsEmpWeekOff.Tables[0].Rows.Count > 0)
                    {
                        if (bplib.clsWebLib.GetBoolData(dsEmpWeekOff.Tables[0].Rows[0]["AlignWithCC"].ToString()))
                        {
                            if (cls.lblCurrentWeekOff != null)
                            {
                                cls.lblCurrentWeekOff.Text = "As per Company Calendar";
                            }
                        }
                        else
                        {
                            if (cls.lblCurrentWeekOff != null)
                            {
                                cls.lblCurrentWeekOff.Text = dsEmpWeekOff.Tables[0].Rows[0]["FstOffDay"].ToString();
                            }
                        }
                    }
                }
                else
                {
                    if (cls.lblCurrentShift != null)
                    {

                        cls.lblCurrentShift.Text = dsEmpShift.Tables[0].Rows[0]["Roster"].ToString() + " (Roster:" + dsEmpShift.Tables[0].Rows[0]["ShiftRosterName"].ToString() + ")";
                        cls.lblCurrentWeekOff.Text = dsEmpShift.Tables[0].Rows[0]["WeekOff"].ToString();
                    }
                    //weekoff
                    //var _rosterId
                }
            }
            else
            {
                objApp.GetEmployeeShiftInfo(emppk, out dsPreShift);
                if (dsPreShift.Tables[0].Rows.Count > 0)
                {
                    if (cls.lblShiftName!=null)
                    {
                    cls.lblShiftName.Text = dsPreShift.Tables[0].Rows[0]["ShiftName"].ToString();
                    }
                    if (cls.lblCurrentEffectiveDate!=null)
                    {
                    cls.lblCurrentEffectiveDate.Text = dsPreShift.Tables[0].Rows[0]["EffectiveDate"].ToString();
                    }

                }
                else
                {
                    if (cls.lblCurrentEffectiveDate!=null)
                    {
                    cls.lblCurrentEffectiveDate.Text = string.Empty;
                    }
                    if (cls.lblShiftName!=null)
                    {
                    cls.lblShiftName.Text = string.Empty;
                    }
                }
                if (cls.lblLeastPunchTime!=null)
                {
                cls.lblLeastPunchTime.Text = string.Empty;
                }
                if (cls.lblDayStatus!=null)
                {
                    cls.lblDayStatus.Text = string.Empty; 
                }
                if (cls.lblCurrentShift!=null)
                {
                    cls.lblCurrentShift.Text = string.Empty; 
                }
                if (cls.lblCurrentWeekOff!=null)
                {
                    cls.lblCurrentWeekOff.Text = string.Empty; 
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    
    public void EntityDDL(string _groupid, string _companyid, clsEntityDropdownlist cls)
    {
        System.Data.DataSet dsLocal = null;
        clsEmployeeLoad objEmpBasic = null;
        clsStaticInfo obs = null;
        //string _groupid = string.Empty;
        //string _companyid = string.Empty;

        try
        {
            obs = new clsStaticInfo();
            objEmpBasic = new clsEmployeeLoad();
            //_groupid = (string)Session["LOGIN_GROUP_ID"].ToString().Trim();
            //_companyid = (string)Session["COMPANY_ID"].ToString().Trim();

            if (cls.ddlUnit != null)
            {
                objEmpBasic.GetUnitName(_groupid, _companyid, "", out dsLocal);
                obs.LoadDDL(dsLocal, "UserName", "Id", cls.ddlUnit);
            }

            if (cls.ddlDivision != null)
            {
                objEmpBasic.GetDivisionName(_groupid, _companyid, "", out dsLocal);
                obs.LoadDDL(dsLocal, "UserName", "ID", cls.ddlDivision);
            }

            if (cls.ddlDepartment != null)
            {
                objEmpBasic.GetDepartmentName(_groupid, _companyid, "", out dsLocal);
                obs.LoadDDL(dsLocal, "UserName", "Id", cls.ddlDepartment);
            }

            if (cls.ddlSection != null)
            {
                objEmpBasic.GetSectionName(_groupid, _companyid, "", out dsLocal);
                obs.LoadDDL(dsLocal, "UserName", "ID", cls.ddlSection);
            }

            if (cls.ddlSubSection != null)
            {
                objEmpBasic.GetSubSectionName(_groupid, _companyid, "", out dsLocal);
                obs.LoadDDL(dsLocal, "UserName", "ID", cls.ddlSubSection);
            }

            if (cls.ddlLine != null)
            {
                objEmpBasic.GetLineInfo(_groupid, _companyid, "", out dsLocal);
                obs.LoadDDL(dsLocal, "UserName", "ID", cls.ddlLine);
            }

            if (cls.ddlEmpCategor != null)
            {
                objEmpBasic.GetEmployeeCategory("", out dsLocal);
                obs.LoadDDL(dsLocal, "UserName", "ID", cls.ddlEmpCategor);
            }
            if (cls.ddlJobLocation != null)
            {
                objEmpBasic.GetJobLocationPlantWise(cls.ddlPlant.SelectedValue, out dsLocal);
                obs.LoadDDL(dsLocal, "JobLocation", "SystemID", cls.ddlJobLocation);
            }
            if (cls.ddlEntity != null)
            {
                objEmpBasic.GetEntityPlantwise(cls.ddlPlant.SelectedValue, out dsLocal);
                obs.LoadDDL(dsLocal, "UserName", "Id", cls.ddlEntity);
            }
            if (cls.ddlDesignationGroup != null)
            {
                objEmpBasic.GetDesignationGroup(_groupid, "", out dsLocal);
                obs.LoadDDL(dsLocal, "UserName", "ID", cls.ddlDesignationGroup);
            }

            if (cls.ddlDesignationGroup != null && cls.ddlDesignation != null)
            {
                objEmpBasic.GetDesignationName(_groupid, cls.ddlDesignationGroup.SelectedValue, out dsLocal);
                obs.LoadDDL(dsLocal, "UserName", "ID", cls.ddlDesignation);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            dsLocal = null;
            objEmpBasic = null;
        }
    }//End Function

    public void Clear(params CheckBox[] txts)
    {
        foreach (var txt in txts)
        {
            CheckBox t = (CheckBox)txt;
            t.Checked = false;
        }
    }

    public void Clear(params TextBox[] txts)
    {
        foreach (var txt in txts)
        {
            TextBox t = (TextBox)txt;
            t.Text = string.Empty;
        }
    }

    public void Clear(params Label[] txts)
    {
        foreach (var txt in txts)
        {
            Label t = (Label)txt;
            t.Text = string.Empty;
        }
    }

    public void Clear(params DropDownList[] txts)
    {
        foreach (var txt in txts)
        {
            DropDownList t = (DropDownList)txt;
            t.SelectedIndex = -1;
        }
    }

    //public void ShowConfirmation(string message, string Flag)
    //{
    //    lblConfirmationTitle.Text = "Confirmation";
    //    lblConfirmationTitle.ForeColor = Color.Orange;
    //    lblConfirmationTitle.Visible = true;

    //    lblFLAGConfirmation.Text = Flag;
    //    pnlConfirmation.BorderColor = Color.Orange;

    //    //lblMessage.ForeColor = Color.Orange;
    //    //lblMessage.Text = " Are you sure to Delete " + message + " ?";

    //    string msg = @"<div style='color:orange;font-size:medium'>
    //                         " + message + @"
    //                        </div>";
    //    ltrlMessage.Text = msg;

    //    this.pnlConfirmation.Visible = true;
    //    this.PanelSearch.Visible = false;
    //    this.PanelFactory.Visible = false;
    //    this.btnConfirmationNO.Visible = true;
    //    this.btnConfirmationYes.Visible = true;
    //}
}

public class Confirmation
{
    private Panel[] _InvisiblePanels = null;

    //Panel _VisiblePanel = null;
    private Label _Title = null;

    private Literal _ltrl = null;
    private Panel _pnlConfirmation = null;
    private Label _lblflag = null;
    private Button _btnYes = null;
    private Button _btnNo = null;

    public Confirmation(Panel pnlConfirmation, Label Title, Literal ltrl, Label lblflag, Button btnYes, Button btnNo, params Panel[] InvisiblePanels)
    {
        _InvisiblePanels = InvisiblePanels;
        _Title = Title;
        _ltrl = ltrl;
        _pnlConfirmation = pnlConfirmation;
        _lblflag = lblflag;
        _btnNo = btnNo;
        _btnYes = btnYes;
    }

    private void SetPanel(Panel VisiblePanel, Panel OtherPanel, params Panel[] InvisiblePanels)
    {
        foreach (var p in InvisiblePanels)
        {
            p.Visible = false;
        }
        OtherPanel.Visible = false;
        VisiblePanel.Visible = true;
    }

    private void SetTitle(string Title, Color c, bool IsVisible)
    {
        _Title.Text = Title;
        _Title.ForeColor = c;
        _Title.Visible = IsVisible;
    }

    private void SetMessage(string message, string flag, Color c)
    {
        _lblflag.Text = flag;
        _pnlConfirmation.BorderColor = c;
        string msg = @"<div style='color:" + c.Name.ToLower() + @";font-size:16px;'>
                             " + message + @"
                            </div>";
        _ltrl.Text = msg;
    }

    public void ShowConfirmation(string message, string Flag, Panel BasePanel)
    {
        SetTitle("Confirmation", Color.Orange, true);
        SetMessage(message, Flag, Color.Orange);
        SetPanel(_pnlConfirmation, BasePanel, _InvisiblePanels);
        _btnNo.Visible = true;
        _btnYes.Visible = true;
    }

    public void ShowMessage(string message, Panel BasePanel)
    {
        SetTitle("Information", Color.Green, true);
        SetMessage(message, "", Color.Green);
        SetPanel(_pnlConfirmation, BasePanel, _InvisiblePanels);
        _btnNo.Visible = false;
        _btnYes.Visible = true;
    }

    public void ShowMessage(Exception ex, Panel BasePanel)
    {
        SetTitle("Error !!!", Color.Red, true);
        SetMessage(ex.Message, "", Color.Red);
        SetPanel(_pnlConfirmation, BasePanel, _InvisiblePanels);
        _btnNo.Visible = false;
        _btnYes.Visible = true;
    }

    public void MessageClear(Panel BasePanel)
    {
        SetTitle("", Color.Red, false);
        SetMessage("", "", Color.Red);
        SetPanel(BasePanel, _pnlConfirmation, _InvisiblePanels);
    }
}

public class ParamList
{
    public string EmployeeId { get; set; }
    public string UnitId { get; set; }
    public string DivisionId { get; set; }
    public string DepartmentId { get; set; }
    public string SectionId { get; set; }
    public string SubSectionId { get; set; }
    public string LineId { get; set; }
    public string PlantId { get; set; }
    public string SubSecStrucId { get; set; }
    public string EmpCategorId { get; set; }
    public string DesignationGroupId { get; set; }
    public string DesignationId { get; set; }
    public string FromDate { get; set; }
    public string EmpStatus { get; set; }
    public string SalaryProcessId { get; set; }
    public string  CompanyGroupId { get; set; }
    public string CompanyId { get; set; }
    public string ToDate { get; set; }
    public string PayGroup { get; set; }
    public string SystemID { get; set; }
    public string LanguageId { get; set; }
    public string SystemAdmin { get; set; }
    public string ControlAdmin { get; set; }
}

public class clsEntityDropdownlist
{
    public DropDownList ddlUnit { get; set; }
    public DropDownList ddlDivision { get; set; }
    public DropDownList ddlDepartment { get; set; }
    public DropDownList ddlSection { get; set; }
    public DropDownList ddlSubSection { get; set; }
    public DropDownList ddlLine { get; set; }
    public DropDownList ddlPlant { get; set; }
    public DropDownList ddlSubSecStruc { get; set; }
    public DropDownList ddlEmpCategor { get; set; }
    public DropDownList ddlDesignationGroup { get; set; }
    public DropDownList ddlDesignation { get; set; }
    public DropDownList ddlJobLocation { get; set; }
    public DropDownList ddlEntity { get; set; }
    public DropDownList ddlPayGroup { get; set; }

    
}
public class clsShiftShowLabel
{
    public Label lblEmpCode { get; set; }
    public Label lblEmpName { get; set; }
    public Label lblEmpDateOJ { get; set; }
    public Label lblDesignationGroup { get; set; }
    public Label lblDesignation { get; set; }
    public Label lblDepartment { get; set; }
    public Label lblCurrentShift { get; set; }
    public Label lblCurrentWeekOff { get; set; }
    public Label lblShiftName { get; set; }
    public Label lblDayStatus { get; set; }
    public Label lblCurrentEffectiveDate { get; set; }
    public Label lblLeastPunchTime { get; set; }
    public Label lblJbLc { get; set; }
    public Label lblJbLcSystemID { get; set; }
    public Label lblJbLcPlantID { get; set; }
}

public class ParaAttendanceReport
{
    public string UnitId { get; set; }
    public string DivisionId { get; set; }
    public string DepartmentId { get; set; }
    public string SectionId { get; set; }
    public string SubsectionId { get; set; }
    public string LineId { get; set; }
    public string EmpCat { get; set; }
    public string DesignationId { get; set; }
    public string EntityId { get; set; }
    public string JoblocationId { get; set; }
    public string DesignationGroupId { get; set; }
    public string ShiftId { get; set; }
    public string PlantId { get; set; }
    public string ADate { get; set; }
    //string sUnit = ddlUnit.SelectedValue.ToString().Trim();
    //string sDevi = ddlDivision.SelectedValue.ToString().Trim();
    //string sDept = ddlDepartment.SelectedValue.ToString().Trim();
    //string sSect = ddlSection.SelectedValue.ToString().Trim();
    //string sSbSe = ddlSubSection.SelectedValue.ToString().Trim();
    //string sLine = ddlLine.SelectedValue.ToString().Trim();
    ////string sSbSeStr = this.ddlSubSecStruc.SelectedValue.ToString().Trim();
    //string sEmpC = ddlEmpCategor.SelectedValue.ToString().Trim();
    //string sDeGr = ddlDesignationGroup.SelectedValue.ToString().Trim();
    //string sDesi = ddlDesignation.SelectedValue.ToString().Trim();
    //string sEntity = ddlEntity.SelectedValue.ToString().Trim();
    //string sJoblocation = ddlJobLocation.SelectedValue.ToString().Trim();
}
public class ParaMontlyAttendance
{
    public string UnitId { get; set; }
    public string DivisionId { get; set; }
    public string DepartmentId { get; set; }
    public string SectionId { get; set; }
    public string SubsectionId { get; set; }
    public string LineId { get; set; }
    public string EmpCat { get; set; }
    public string DesignationId { get; set; }
    public string DesignationGroupId { get; set; }
    public string EntityId { get; set; }
    public string JoblocationName { get; set; }
    //public string ShiftId { get; set; }
    public string PlantId { get; set; }
    public string AMonth { get; set; }
    public string AYear { get; set; }

    public string FDate { get; set; }
    public string TDate { get; set; }
    

}

