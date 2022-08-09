using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Employee
{
    #region FurniturePolicyService
    public class FurniturePolicyService
    {
        private readonly SqlRepository _sqlRepository;
        public FurniturePolicyService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getFurnitureMaster()
        {
            try
            {
                var sql = @"select distinct UserName as Text from HKP.furnitureMaster";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getDesignationMaster()
        {
            try
            {
                var sql = @"select DISTINCT d.UserName as Text from MST.DesignationMaster d ORDER BY Text";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getFurnitureGridView()
        {
            try
            {
                // Add condition if id  all ready there then not show in grid
                //var sql = @"select fm.*, CONVERT(bit,0) IsSelectSlrProc  from HKP.furnitureMaster fm --where fm.UserName = '" + username + "'";
                var sql = @"SELECT fm.*,CONVERT(bit,0) IsSelectSlrProc  from HKP.furnitureMaster fm
                
                --WHERE NOT EXISTS( SELECT * FROM HKP.FurniturePolicyFM AS fpf  WHERE fpf.FurnitureMasterId = fm.Id)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getDesignationGridView(string employeeCategoryId)
        {
            try
            {
                //                var sql = @"select dm.*, dg.UserName as DesignationGroup, dsg.UserName as Designation, ec.UserName as EmployeeCategory from MST.DesignationMaster dm
                //left join HKP.Designation dg on dg.Id = dm.DesignationId
                //left join HKP.DesignationGroup dsg on dsg.Id = dm.DesignationGroupId 
                //left join HKP.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                // --where dm.UserName = '" + username + "'";

                var sql = @"select dm.*, dg.UserName as DesignationGroup, dsg.UserName as Designation, ec.UserName as EmployeeCategory from MST.DesignationMaster dm
left join HKP.Designation dg on dg.Id = dm.DesignationId
left join HKP.DesignationGroup dsg on dsg.Id = dm.DesignationGroupId 
left join HKP.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId

WHERE NOT EXISTS( SELECT * FROM HKP.FurniturePolicyDM AS fpd  WHERE fpd.DesignationMasterId = dm.Id) and (ec.Id = '"+ employeeCategoryId + "')";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object>getEmployee()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string str = @"select EMP.EmployeeCode as Code, EMP.SystemId, EMP.EmployeeName, SC.UserName as Section, GDSG.UserName as Designation, UN.UserName as Entity
                                from EmployeeInformation EMP
                                LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                                LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                                left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                                left join ORG.Entity UN on UN.Id = MBGT.EntityId
                                left join ORG.Department DP on DP.ID = POS.DepartmentId
                                left join ORG.Section SC on SC.Id = POS.SectionId
                                left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                                LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
                                LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                                LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                                left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId



                                left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
                                left join SalaryRuleMaster SRM on srm.systemid = emp.salaryrulemastersystemid
                                left join ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId
                                left join TransportGroup TG on TG.Id = EMP.TransportGroupId
                                where EMP.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEmployeeCategory()
        {
            try 
            {
                var sql = @"select Id as Value, UserName as Text from HKP.EmployeeCategory";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

            public Dictionary<string, object> Save(Dictionary<string, object> data, string responsiblePerson)
        {
            try
            {
                string TableNameHead = "HKP.FurniturePolicy";
                
                DataSet dsMaster;
               

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where StandardName='" + data["StandardName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Standard Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] = "FP" + _Id;
                    data["ResponsiblePerson"] = responsiblePerson;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion FURNITURE POLICY HEAD

               

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // SAVE TAB A
        public List<Dictionary<string, object>> SaveTabA(List<Dictionary<string, object>> childA, string headerId, List<Dictionary<string, string>> designationmasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string TableNameChildA = "HKP.FurniturePolicyDM";
               

                DataSet dsChildA;
               

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string _Id = "";
                 #region CHILD 1
                con.OpenDataSetThroughAdapter("select * from " + TableNameChildA + " where FurniturePolicyId='" + headerId + "'", out dsChildA, false, "1");
               
                for (int i = 0; i < designationmasterId.Count; i++)
                {
                    var jj = designationmasterId[i];
                  
                    DataRow dr = dsChildA.Tables[0].NewRow();
                    dr["Id"] = headerId + "-" + i;
                    dr["FurniturePolicyId"] = headerId;
                    dr["DesignationMasterId"] = designationmasterId[i]["DesignationMasterId"];
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                   
                    dsChildA.Tables[0].Rows.Add(dr);

                }
                #endregion CHILD 1

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChildA);
                
                return childA;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // SAVE TAB B
        public List<Dictionary<string, object>> SaveTabB(List<Dictionary<string, object>> childB, string headerId, List<Dictionary<string, string>> furnituremasterId, List<Dictionary<string, string>> quantity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string TableNameChildB = "HKP.FurniturePolicyFM";
                             
                DataSet dsChildB;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string _Id = "";
               
                #region CHILD 2
                con.OpenDataSetThroughAdapter("select * from " + TableNameChildB + " where FurniturePolicyId='" + headerId + "'", out dsChildB, false, "1");

                
                for (int i = 0; i < furnituremasterId.Count; i++)
                {
                    var jj = furnituremasterId[i];
                    
                    DataRow dr = dsChildB.Tables[0].NewRow();
                    dr["Id"] = headerId + "-" +i;
                    dr["FurniturePolicyId"] = headerId;
                    dr["FurnitureMasterId"] = furnituremasterId[i]["FurnitureMasterId"];
                    dr["Quantity"] = quantity[i]["Quantity"];
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    
                    dsChildB.Tables[0].Rows.Add(dr);

                }
                #endregion CHILD 2


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChildB);
               
                return childB;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            
            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
    }
    #endregion FurniturePolicyService

    #region FurniturePolicyServiceReport
    public class FurniturePolicyReportService
    {
        private readonly SqlRepository _sqlRepository;
        public FurniturePolicyReportService() 
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getEmployeeCategory()
        {
            try
            {
                var sql = @"select Id as Value, UserName as Text from HKP.EmployeeCategory";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getDesignation(string employeeCategoryId)
        {
            try 
            {
                var sql = @"select dm.Id as Value, dm.UserName as Text from MST.DesignationMaster dm 
                            left join  HKP.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                            left join HKP.Designation d on d.Id = dm.DesignationId
                            where ec.Id = '"+ employeeCategoryId + "' order by d.UserName";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public DataTable furnitureWiseReport(string designationId) 
        {
            try
            {
                var str = "";
                if (designationId == null)
                {
                    str = @"select FP.Id, FM.Sequence, FP.StandardName as PolicyName, FM.Category, FM.SubCategory, FM.StandardName as Furniture, FM.Type, FM.Budget,
                            DM.UserName as Designation, FPF.Quantity  from hkp.FurniturePolicy FP
                            left join hkp.FurniturePolicyFM FPF on FPF.FurniturePolicyId = FP.Id
                            left join hkp.FurniturePolicyDM FPD on FPD.FurniturePolicyId = FP.Id
                            left join hkp.furnitureMaster FM on FM.Id = FPF.FurnitureMasterId
                            left join mst.DesignationMaster DM on DM.Id = FPD.DesignationMasterId
                            left join hkp.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
                            left join hkp.Designation D on D.Id = DM.DesignationId
                             order by D.UserName";
                }
                else
                {
                    str = @"select FP.Id, FM.Sequence, FP.StandardName as PolicyName, FM.Category, FM.SubCategory, FM.StandardName as Furniture, FM.Type, FM.Budget,
                            DM.UserName as Designation, FPF.Quantity  from hkp.FurniturePolicy FP
                            left join hkp.FurniturePolicyFM FPF on FPF.FurniturePolicyId = FP.Id
                            left join hkp.FurniturePolicyDM FPD on FPD.FurniturePolicyId = FP.Id
                            left join hkp.furnitureMaster FM on FM.Id = FPF.FurnitureMasterId
                            left join mst.DesignationMaster DM on DM.Id = FPD.DesignationMasterId
                            left join hkp.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
                            left join hkp.Designation D on D.Id = DM.DesignationId
                            where DM.Id = '" + designationId + "' order by D.UserName";
                }
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getPolicyGrid(string designationId)
        {
            try
            {
                var str = "";
                if (designationId == null)
                {
                    str = @"select FP.Id, FM.Sequence, FP.StandardName as PolicyName, FM.Category, FM.SubCategory, FM.StandardName as Furniture, FM.Type, FM.Budget,
                            DM.UserName as Designation, FPF.Quantity  from hkp.FurniturePolicy FP
                            left join hkp.FurniturePolicyFM FPF on FPF.FurniturePolicyId = FP.Id
                            left join hkp.FurniturePolicyDM FPD on FPD.FurniturePolicyId = FP.Id
                            left join hkp.furnitureMaster FM on FM.Id = FPF.FurnitureMasterId
                            left join mst.DesignationMaster DM on DM.Id = FPD.DesignationMasterId
                            left join hkp.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
                            left join hkp.Designation D on D.Id = DM.DesignationId
                             order by D.UserName";
                }
                else
                {
                    str = @"select FP.Id, FM.Sequence, FP.StandardName as PolicyName, FM.Category, FM.SubCategory, FM.StandardName as Furniture, FM.Type, FM.Budget,
                            DM.UserName as Designation, FPF.Quantity  from hkp.FurniturePolicy FP
                            left join hkp.FurniturePolicyFM FPF on FPF.FurniturePolicyId = FP.Id
                            left join hkp.FurniturePolicyDM FPD on FPD.FurniturePolicyId = FP.Id
                            left join hkp.furnitureMaster FM on FM.Id = FPF.FurnitureMasterId
                            left join mst.DesignationMaster DM on DM.Id = FPD.DesignationMasterId
                            left join hkp.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
                            left join hkp.Designation D on D.Id = DM.DesignationId
                            where DM.Id = '" + designationId + "' order by D.UserName";
                }
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable designationWiseReport()
        {
            try
            {
                var str = @"";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    #endregion FurniturePolicyServiceReport
}

