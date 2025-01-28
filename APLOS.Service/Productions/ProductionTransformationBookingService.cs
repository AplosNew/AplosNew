using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data;
using Library.Service.Logs;
using System.Reflection;
using Library.Service.Enums;

namespace Library.Service.Productions
{

    public class ProductionTransformationBooking
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        string TableName = "dbo.ProductionTransformationBooking";
        string TableName2 = "dbo.ProductionTransformationDetailBooking";

        public ProductionTransformationBooking()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> getProcesslist()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select P.Id as Value, P.UserName as Text 
                               from HKP.Process P inner join dbo.ProductionConversionParameter cp on cp.ProcessId=P.Id order by P.UserName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetProcessIdDisplay(string ProcessId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select P.Id as Value, P.UserName as Text 
                               from HKP.Process P where P.Id='"+ ProcessId + @"' order by P.UserName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> getWorkCentreCategoryGrouplist()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id as Value, UserName as Text from HKP.WorkCenterSubCategory order by UserName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> getDependantProcesslist(string MasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select P.Id as Value, P.UserName as Text from HKP.Process P 
                              left join dbo.ProductionTransformationDetailBooking pdb on P.Id=pdb.ProcessId 
                              where pdb.ProductionTransformationMasterId='"+ MasterId + @"' order by P.UserName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> getOutputItemNamelist()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id as Value, ItemName as Text from dbo.ProductionConversionParameter where ItemType='Output' order by ItemName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> getEntryQuantityUOMList(string OutputItenNameId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select uom.Id as Value, uom.UserName as Text from SCS.UnitOfMeasurement uom
                               inner join dbo.ProductionConversionParameter cp on cp.EntryUoMId=uom.Id where cp.Id='"+ OutputItenNameId + @"' order by uom.UserName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetOutputItemParameter(string OutputItenNameId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id as Value, Parameter as Text, OutputValue from dbo.ProductionConversionParameter where Id='" + OutputItenNameId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> getOutputItemUOMList(string OutputItenNameId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select uom.Id as Value, uom.UserName as Text from SCS.UnitOfMeasurement uom 
                               inner join dbo.ProductionConversionParameter cp on cp.UoMId=uom.Id where cp.Id='"+ OutputItenNameId + @"' order by uom.UserName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> getInputItemNameList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id as Value, ItemName as Text from dbo.ProductionConversionParameter where ItemType='Input' order by ItemName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> getInputUOMList(string InputItenNameId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select uom.Id as Value, uom.UserName as Text from SCS.UnitOfMeasurement uom 
                              inner join dbo.ProductionConversionParameter cp on cp.UoMId=uom.Id where cp.Id='"+ InputItenNameId + @"' order by uom.UserName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> getByProductItemNameList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id as Value, ItemName as Text from dbo.ProductionConversionParameter order by ItemName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> getByProductUOMList(string ByProductNameId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select uom.Id as Value, uom.UserName as Text from SCS.UnitOfMeasurement uom
                              inner join dbo.ProductionConversionParameter cp on cp.UoMId=uom.Id where cp.Id='"+ ByProductNameId + @"' order by uom.UserName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetAutoSequence(string ProductionBookingId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT (ISNULL((MAX(ISNULL(Sequence,0))),0)+1) Sequence FROM dbo.ProductionTransformationDetailBooking Where ProductionTransformationMasterId='" + ProductionBookingId + "'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> LoadAllEmpDetails(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.CompanyId='" + identity.CompanyId + @"' and emp.EmployeeStatus='Active' and EMP.EmpType='Local'
                   AND isnull(Emp.SystemID,'') not in (select isnull(PreparedById,'') from dbo.ProductionTransformationBooking where Id='" + Id + @"')
                  order by EMP.EmployeeCode";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select pb.*, emp.EmployeeName as ResponsiblePerson, emp.EmployeeCode, emp.EmployeeStatus
                               from dbo.ProductionTransformationBooking pb left join dbo.EmployeeInformation emp on emp.SystemId=pb.PreparedById
                               WHERE " + strkey + " ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductionTransformationBooking", out sID);
            return sID;
        }

        public void Create(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.ProductionTransformationBooking where ProcessSetStandardName='" + data["ProcessSetStandardName"] + "' AND  Id<>'" + data["Id"] + "' ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same Process Set StandardName already exist.");
                }

                con.OpenDataSetThroughAdapter("select * from dbo.ProductionTransformationBooking where ProcessSetUserName='" + data["ProcessSetUserName"] + "' AND  Id<>'" + data["Id"] + "' ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same Process Set UserName already exist.");
                }

                con.OpenDataSetThroughAdapter("select * from dbo.ProductionTransformationBooking where ProcessSetShortName='" + data["ProcessSetShortName"] + "' AND  Id<>'" + data["Id"] + "' ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same Process Set ShortName already exist.");
                }

                con.OpenDataSetThroughAdapter("select * from dbo.ProductionTransformationBooking where ProcessSetUserCode='" + data["ProcessSetUserCode"] + "' AND  Id<>'" + data["Id"] + "' ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same Process Set UserCode already exist.");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "PB" + GetPK();

                    dr["ProcessSetStandardName"] = data["ProcessSetStandardName"];
                    dr["ProcessSetUserName"] = data["ProcessSetUserName"];
                    dr["ProcessSetShortName"] = data["ProcessSetShortName"];
                    dr["ProcessSetUserCode"] = data["ProcessSetUserCode"];
                    dr["PreparedById"] = data["PreparedById"];
                    dr["Remarks"] = data["Remarks"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    //dr["UpdatedBy"] = identity.Name;
                    //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    //dr["UpdatedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["ProcessSetStandardName"] = data["ProcessSetStandardName"];
                    dr["ProcessSetUserName"] = data["ProcessSetUserName"];
                    dr["ProcessSetShortName"] = data["ProcessSetShortName"];
                    dr["ProcessSetUserCode"] = data["ProcessSetUserCode"];
                    dr["PreparedById"] = data["PreparedById"];
                    dr["Remarks"] = data["Remarks"];

                    //dr["AddedBy"] = identity.Name;
                    //dr["AddedDate"] = System.DateTime.Now.ToString();
                    //dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }
                data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void delete(string Id)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con2 = new ConnectionManager.DAL.ConManager("1");

                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

                if (!string.IsNullOrEmpty(Id))
                {
                    con2.OpenDataSetThroughAdapter("select * from dbo.ProductionTransformationDetailBooking where ProductionTransformationMasterId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Booking Detail Data");
                    }
                }

                con.BeginTransaction();

                con.executeQuery("delete from dbo.ProductionTransformationBooking where Id='" + Id + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Detail Save

        private string GetDetailPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductionTransformationDetailBooking", out sID);
            return sID;
        }

        public void detailSave(Dictionary<string, object> data, string MasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                //con.OpenDataSetThroughAdapter("select * from dbo.ProductionTransformationBooking where ProcessSetStandardName='" + data["ProcessSetStandardName"] + "' AND  Id<>'" + data["Id"] + "' ", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //{
                //    throw new Exception("Same Process Set StandardName already exist.");
                //}

                //con.OpenDataSetThroughAdapter("select * from dbo.ProductionTransformationBooking where ProcessSetUserName='" + data["ProcessSetUserName"] + "' AND  Id<>'" + data["Id"] + "' ", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //{
                //    throw new Exception("Same Process Set UserName already exist.");
                //}

                //con.OpenDataSetThroughAdapter("select * from dbo.ProductionTransformationBooking where ProcessSetShortName='" + data["ProcessSetShortName"] + "' AND  Id<>'" + data["Id"] + "' ", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //{
                //    throw new Exception("Same Process Set ShortName already exist.");
                //}

                //con.OpenDataSetThroughAdapter("select * from dbo.ProductionTransformationBooking where ProcessSetUserCode='" + data["ProcessSetUserCode"] + "' AND  Id<>'" + data["Id"] + "' ", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //{
                //    throw new Exception("Same Process Set UserCode already exist.");
                //}

                con.OpenDataSetThroughAdapter("select * from " + TableName2 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "PBD" + GetDetailPK();

                    dr["ProductionTransformationMasterId"] = MasterId;
                    dr["ProcessId"] = data["ProcessId"];
                    dr["WorkCentreCategoryGroupId"] = data["WorkCentreCategoryGroupId"];
                    dr["DependantProcessId"] = data["DependantProcessId"];
                    dr["OutputItemNameId"] = data["OutputItemNameId"];
                    dr["EntryQuantityUOMId"] = data["EntryQuantityUOMId"];
                    dr["OutputItemParameterId"] = data["OutputItemParameterId"];
                    dr["ConversionFactorId"] = data["ConversionFactorId"];
                    dr["OutputItemUOMId"] = data["OutputItemUOMId"];
                    dr["OutputQuantity"] = data["OutputQuantity"];
                    dr["InputUOMId"] = data["InputUOMId"];
                    dr["GrossConsumptionPerUnitQuantity"] = data["GrossConsumptionPerUnitQuantity"];
                    dr["ByProductItemNameId"] = data["ByProductItemNameId"];
                    dr["ByProductUOMId"] = data["ByProductUOMId"];
                    dr["ByProductQuantity"] = data["ByProductQuantity"];
                    dr["ByProductCategory"] = data["ByProductCategory"];
                    dr["InvisibleLossPercentage"] = data["InvisibleLossPercentage"];
                    dr["ProductionBookingLevel"] = data["ProductionBookingLevel"];
                    dr["IssueConsumptionBooking"] = data["IssueConsumptionBooking"];
                    dr["InputItemNameId"] = data["InputItemNameId"];
                    dr["Sequence"] = data["Sequence"];
                    dr["Remarks"] = data["Remarks"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    //dr["UpdatedBy"] = identity.Name;
                    //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    //dr["UpdatedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["ProductionTransformationMasterId"] = MasterId;
                    dr["ProcessId"] = data["ProcessId"];
                    dr["WorkCentreCategoryGroupId"] = data["WorkCentreCategoryGroupId"];
                    dr["DependantProcessId"] = data["DependantProcessId"];
                    dr["OutputItemNameId"] = data["OutputItemNameId"];
                    dr["EntryQuantityUOMId"] = data["EntryQuantityUOMId"];
                    dr["OutputItemParameterId"] = data["OutputItemParameterId"];
                    dr["ConversionFactorId"] = data["ConversionFactorId"];
                    dr["OutputItemUOMId"] = data["OutputItemUOMId"];
                    dr["OutputQuantity"] = data["OutputQuantity"];
                    dr["InputUOMId"] = data["InputUOMId"];
                    dr["GrossConsumptionPerUnitQuantity"] = data["GrossConsumptionPerUnitQuantity"];
                    dr["ByProductItemNameId"] = data["ByProductItemNameId"];
                    dr["ByProductUOMId"] = data["ByProductUOMId"];
                    dr["ByProductQuantity"] = data["ByProductQuantity"];
                    dr["ByProductCategory"] = data["ByProductCategory"];
                    dr["InvisibleLossPercentage"] = data["InvisibleLossPercentage"];
                    dr["ProductionBookingLevel"] = data["ProductionBookingLevel"];
                    dr["IssueConsumptionBooking"] = data["IssueConsumptionBooking"];
                    dr["InputItemNameId"] = data["InputItemNameId"];
                    dr["Sequence"] = data["Sequence"];
                    dr["Remarks"] = data["Remarks"];

                    //dr["AddedBy"] = identity.Name;
                    //dr["AddedDate"] = System.DateTime.Now.ToString();
                    //dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }
                data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetDetailData(string ProductionBookingId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select pbd.*,P.UserName as Process, p.Id as ProcessIddisplay,wc.UserName as WorkCenter,Pr.UserName as DependantProcess,cp.ItemName as OutputItemName, uom.UserName as EntryUom
                                ,U.UserName as OutputItemUom,inpU.UserName as UoMInput, cpp.ItemName as ByProductItem, bpU.UserName as ByProductUom, inpcp.ItemName as InputItemName
                                from dbo.ProductionTransformationDetailBooking pbd left join HKP.Process P on P.Id=pbd.ProcessId
                                left join HKP.WorkCenterSubCategory wc on wc.Id=pbd.WorkCentreCategoryGroupId
                                left join HKP.Process Pr on Pr.Id=pbd.DependantProcessId
                                left join dbo.ProductionConversionParameter cp on cp.Id=pbd.OutputItemNameId
                                left join SCS.UnitOfMeasurement uom on uom.Id=pbd.EntryQuantityUOMId
                                left join SCS.UnitOfMeasurement U on U.Id=pbd.OutputItemUOMId
                                left join SCS.UnitOfMeasurement inpU on inpU.Id=pbd.InputUOMId
                                left join dbo.ProductionConversionParameter cpp on cpp.Id=pbd.ByProductItemNameId
                                left join SCS.UnitOfMeasurement bpU on bpU.Id=pbd.ByProductUOMId
                                left join dbo.ProductionConversionParameter inpcp on inpcp.Id=pbd.InputItemNameId
                                where pbd.ProductionTransformationMasterId='" + ProductionBookingId + @"'
                                order by pbd.Sequence desc ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void DelBookingDetails(string Id)
        {
            try
            {

                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery("delete from dbo.ProductionTransformationDetailBooking where Id='" + Id + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}