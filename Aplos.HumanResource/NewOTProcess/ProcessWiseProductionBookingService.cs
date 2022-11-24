using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Library.HumanResource.NewOTProcess
{
  public  class ProcessWiseProductionBookingService
    {
        private readonly SqlRepository _sqlRepository;
        public ProcessWiseProductionBookingService()
        {
            _sqlRepository = new SqlRepository();
        }

        
        public IEnumerable<object> getEntity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"Select e.Id as EntityId, e.UserName as EntityName from org.Entity e
                                left join org.Plant p on p.Id = e.PlantId
                                left join org.Company c on c.Id = p.CompanyId
								--where e.IsProduction = 1";

            return _sqlRepository.GetDataCollection(str);
        }

        
        public IEnumerable<object> getDepartment()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select D.Id DepartmentId,D.Code,D.Sequence,D.ShortName,D.StandardName
						                ,D.UserName DepartmentName,D.Description,D.Remarks 
						                from ORG.Department D";

            return _sqlRepository.GetDataCollection(str);
        }

        
        public IEnumerable<object> getShift()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select Id, Description from MST.CompliedShiftGrouping";

            return _sqlRepository.GetDataCollection(str);
        }


        
        public IEnumerable<object> getMachine()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MM.Id as MachineMasterId,MM.Sequence,MM.Code,MM.ShortName 
						                ,MM.StandardName,MM.UserName MachineMaster
						                from mst.MachineMaster MM";

            return _sqlRepository.GetDataCollection(str);
        }


        
        public IEnumerable<object> getProcess(string entityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                try
                {
                    string str = @"select P.Id,P.UserName
                                    from HKP.EntityProcessTag ep
                                    left join HKP.Process P on P.Id = ep.ProcessId
                                    left join ORG.Entity e on e.Id = ep.EntityId
                                    where e.Id = '" + entityId + "'";


                    return _sqlRepository.GetDataCollection(str);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
        public IEnumerable<object> getEmployee()
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
where EMP.EmployeeStatus = 'Active' and x.UserName = 'Staff' and DP.UserName = 'Production'";

            return _sqlRepository.GetDataCollection(str);
        }

       public IEnumerable<object> getArticle()
        {
            try
            {
                var str = "select * FROM MST.MaterialMasterArticle";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void AddNewMachineMasterTransactionRow(DataTable dt, Dictionary<string, object> sourceData)
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

            //dr["UpdatedBy"] = identity.Name;
            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
            //dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }

        private void EditMachineMasterTransactionRow(DataRow dr, Dictionary<string, object> sourceData)
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
        public Dictionary<string, object> Save(Dictionary<string, object> data, string responsiblepersonId)
        {
            try
            {
                string TableName = "TRN.ProcessWiseProductionBooking";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    // genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MachineMasterTransaction", out MachineMasterTransactionId);

                    data["Id"] = "PWPB" + _Id; ;
                    data["ResponsiblePersonId"] = responsiblepersonId;


                    AddNewMachineMasterTransactionRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditMachineMasterTransactionRow(dsMaster.Tables[0].Rows[0], data);
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return data;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public List<Dictionary<string, object>> SaveChild(List<Dictionary<string, object>> workcenterlist, string headerId)
        {
            try
            {
                //Master Table - PMSMaster
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.ProcessWiseWorkCenter WHERE ProcessWiseProductionBookingId ='" + headerId + "'", out dsMaster, false, "1");


                #region data Master update
                int count = 0;

                foreach (var item in workcenterlist)
                {
                    count++;
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = headerId + "-" + count;
                        item["ProcessWiseProductionBookingId"] = headerId;


                        AddNewRow(dsMaster.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                #endregion data update

                return workcenterlist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
        public IEnumerable<object> GetMachineMasterTransaction()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"SELECT E.UserName Entity, FORMAT(pwpb.Date,'dd-MMM-yyyy')[Date],P.UserName Process, EI.EmployeeName ResponsiblePerson,
EI.EmployeeCode ResponsiblePersonCode,  pwpb.Remark, ss.Description as Shift
from TRN.ProcessWiseProductionBooking pwpb
			                            left join ORG.Entity E on E.Id=pwpb.EntityId
																			
										left join HKP.Process P on P.Id=pwpb.ProcessId
										left join MST.CompliedShiftGrouping ss on ss.Id = pwpb.ShiftId													
										left join EmployeeInformation EI on EI.SystemId=pwpb.ResponsiblePersonId
										";

            return _sqlRepository.GetDataCollection(sql);
        }
        //Omar End
       
        public IEnumerable<object> GetWCCbo(string processId, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"SELECT ppw.Id, wc.UserName, wc.Id as WorkCenterId,  wc.ProcessId
FROM  SCS.WorkCenterMaster wc
left join HKP.Process P on P.Id = wc.ProcessId
Outer Apply(Select pw.* from TRN.ProcessWiseProductionBooking pw Where pw.ProcessId = '" + processId + "'  AND  pw.EntityId='"+ entityId + "') PPW where wc.EntityId = '" + entityId + "' and wc.ProcessId = '" + processId + "'";


            return _sqlRepository.GetDataCollection(sql);

        }


        public IEnumerable<object> Delete(string id)
        {
            string strUSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strUSQL = "delete dbo.MachineMasterTransaction Where Id='" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strUSQL, true, "1");
                objCon.CommitTransaction();

                return _sqlRepository.GetDataCollection(strUSQL);
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
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
            //dr["UpdatedBy"] = identity.Name;
            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
            //dr["UpdatedFromIP"] = identity.IPAddress;

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
}
