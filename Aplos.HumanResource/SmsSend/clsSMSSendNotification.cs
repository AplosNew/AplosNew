using Library.Crosscutting.Security;
using Library.Data.Sql;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.SmsSend
{
    public class clsSMSSendNotification
    {
        ISqlRepository _sqlRepository;
        public clsSMSSendNotification()
        {
            _sqlRepository = new SqlRepository();
        }
        public async Task SendSMSService()
        {


            sendSMS();

        }
        public async void sendSMS()
        {
            DataTable dsApiSql = null;
            DataTable dsEmpList = null;
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            //string Apisql = "SELECT * FROM [MMS].[ModuleExtended] where CompanyGroupId='" + identity.CompanyGroupId + "' ";
            string Apisql = "SELECT * FROM [MMS].[ModuleExtended]";

            string sqlFormat1 = @"--Format 3 -- IN is there, but OUT is not there, notify after 14 hours from IN Time
                                    SELECT  * FROM (Select A.EmpSystemID,E.EmployeeCode,E.EmployeeName,format(A.WorkDate,'dd-MMM-yyyy')WorkDate,A.InTime,A.OutTime,CONVERT(date,DATEADD(DAY, -2, GETDATE())) as BaseDate
                                    ,'Format3' as Type,Format(DATEADD(minute, 10, GETDATE()),'dd/MM/yyyy HH:mm') ScheduleTime
									,Format(l.WorkDate,'dd-MMM-yyyy')PreviousWorkDate,e.CardNumber,e.CellPhnNo
                                    from [dbo].[AttdnProcessData] A
                                    Left Join EmployeeInformation E on A.EmpSystemID=E.SystemId

									LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
									LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id
									Left join ORG.Unit u on u.Id=EN.UnitId 

									left join [dbo].[AttdnProcessData] L ON L.EmpSystemID=A.EmpSystemID
									and L.EmpSystemID+convert(varchar(30),L.WorkDate)=(select TOP 1 LX.EmpSystemID+convert(varchar(30),LX.WorkDate) from [dbo].[AttdnProcessData] LX 
									join DayType dx on dx.DayType=lx.DayStatus 
									where dx.Category IN ('Present','Late') AND ISNULL(E.CellPhnNo,'')<>''
									AND LX.EmpSystemID=A.EmpSystemID and convert(date,LX.WorkDate)<CONVERT(date,A.WorkDate) ORDER BY LX.WorkDate DESC)
                                    where A.InTime is not null and A.OutTime is null and A.WorkDate > CONVERT(date,DATEADD(DAY, -2, GETDATE())) 
                                    and DATEDIFF(HOUR,A.InTime,GETDATE())>14
                                    and A.EmpSystemID+convert(varchar(30),a.WorkDate) NOT IN (Select  B.EmpSystemID+convert(varchar(30),B.WorkDate) from SMSNotification B where A.EmpSystemID=B.EmpSystemId and A.WorkDate=B.WorkDate)
									and E.PlantId = '202016' and u.Id = '20205'

                                    union
                                    --Format 2 -- In is not there, but OUT is there, notify immediately after getting OUT Time
                                    Select  A.EmpSystemID,E.EmployeeCode,E.EmployeeName,format(A.WorkDate,'dd-MMM-yyyy')WorkDate,A.InTime,A.OutTime,CONVERT(date,DATEADD(DAY, -2, GETDATE())) as BaseDate,'Format2' as Type
                                    ,Format(DATEADD(minute, 10, GETDATE()),'dd/MM/yyyy HH:mm') ScheduleTime,Format(l.WorkDate,'dd-MMM-yyyy')PreviousWorkDate,e.CardNumber,e.CellPhnNo
                                    from [dbo].[AttdnProcessData] A
                                    Left Join EmployeeInformation E on A.EmpSystemID=E.SystemId

									LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
									LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id
									Left join ORG.Unit u on u.Id=EN.UnitId 

									left join [dbo].[AttdnProcessData] L ON L.EmpSystemID=A.EmpSystemID
									and L.EmpSystemID+convert(varchar(30),L.WorkDate)=(select TOP 1 LX.EmpSystemID+convert(varchar(30),LX.WorkDate) from [dbo].[AttdnProcessData] LX 
									join DayType dx on dx.DayType=lx.DayStatus 
									where dx.Category IN ('Present','Late') AND ISNULL(E.CellPhnNo,'')<>''
									AND LX.EmpSystemID=A.EmpSystemID and convert(date,LX.WorkDate)<CONVERT(date,A.WorkDate) ORDER BY LX.WorkDate DESC)
                                    where A.InTime is null and A.OutTime is not null and A.WorkDate > CONVERT(date,DATEADD(DAY, -2, GETDATE())) 
                                     and A.EmpSystemID+convert(varchar(30),a.WorkDate) NOT IN (Select  B.EmpSystemID+convert(varchar(30),B.WorkDate) from SMSNotification B where A.EmpSystemID=B.EmpSystemId and A.WorkDate=B.WorkDate)
									 and E.PlantId = '202016' and u.Id = '20205'

                                    union
                                    --Format 1 -- No IN and OUT, but Absent, notify after 30 hours from the assigned Shift IN Time
                                    Select A.EmpSystemID,E.EmployeeCode,E.EmployeeName,format(A.WorkDate,'dd-MMM-yyyy')WorkDate,A.InTime,A.OutTime
                                    ,CONVERT(date,DATEADD(DAY, -2, GETDATE())) as BaseDate,'Format1' as Type,
                                    Format(DATEADD(minute, 10, GETDATE()),'dd/MM/yyyy HH:mm') ScheduleTime
                                    ,Format(l.WorkDate,'dd-MMM-yyyy')PreviousWorkDate,e.CardNumber,e.CellPhnNo
									from [dbo].[AttdnProcessData] A
                                    Left Join EmployeeInformation E on A.EmpSystemID=E.SystemId

									LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
									LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id
									Left join ORG.Unit u on u.Id=EN.UnitId 

                                    Left Join ShiftDefination SD on A.ShiftSystemID=SD.SystemID
                                    Left Join ShiftTimeChgMaster STCM on SD.SystemID=STCM.ShiftDefinationID
                                    Left Join ShiftTimeChgChild STCC on SD.SystemID=STCM.ShiftDefinationID and A.WorkDate=STCC.ShiftDate
									left join [dbo].[AttdnProcessData] L ON L.EmpSystemID=A.EmpSystemID
									and L.EmpSystemID+convert(varchar(30),L.WorkDate)=(select TOP 1 LX.EmpSystemID+convert(varchar(30),LX.WorkDate) from [dbo].[AttdnProcessData] LX 
									join DayType dx on dx.DayType=lx.DayStatus 
									where dx.Category IN ('Present','Late') AND ISNULL(E.CellPhnNo,'')<>''
									AND LX.EmpSystemID=A.EmpSystemID and convert(date,LX.WorkDate)<CONVERT(date,A.WorkDate) ORDER BY LX.WorkDate DESC)
                                    Where A.InTime is null and A.OutTime is null and A.WorkDate > CONVERT(date,DATEADD(DAY, -2, GETDATE())) 
                                    and DATEDIFF(HOUR,DATEADD(minute,DATEPART(minute, isnull(STCM.InTime, SD.Intime)), DATEADD(hour,DATEPART(hour, isnull(STCM.InTime, SD.Intime)),A.WorkDate)),GETDATE())>30
                                    and A.DayStatus='A'
                                     and A.EmpSystemID+convert(varchar(30),a.WorkDate) NOT IN (Select  B.EmpSystemID+convert(varchar(30),B.WorkDate) from SMSNotification B where A.EmpSystemID=B.EmpSystemId and A.WorkDate=B.WorkDate)
									 and E.PlantId = '202016' and u.Id = '20205'
                                
                                    ) AS K  ";

            //0=name,1=card no,2=workdate,3=last workday,
            string MessageBodyFormat1 = @"नाम :- {0} कार्ड नंबर :- {1} आपके द्वारा  दिनांक {2}  को कार्य पर आते / जाते समय पंचिंग नहीं की गई है । आपकी अंतिम  उपस्थिति दिनांक {3} की है ।  अतः इस हेतु आप निर्धारित टाइम ऑफिस में तुरंत संपर्क करें ।";

            string MessageBodyFormat2 = @"नाम :-{0} कार्ड नंबर :- {1} आपके द्वारा  दिनांक {2} को कार्य पर आते  समय पंचिंग नहीं की गई है । आपकी अंतिम  उपस्थिति दिनांक  {3} की है । अतः इस हेतु आप निर्धारित टाइम ऑफिस में तुरंत संपर्क करें ।";

            string MessageBodyFormat3 = @"नाम :- {0} कार्ड नंबर :- {1} आपके द्वारा  दिनांक  {2}  को कार्य  से  जाते समय पंचिंग नहीं की गई है । आपकी अंतिम  उपस्थिति दिनांक  {3} की है ।  अतः इस हेतु आप निर्धारित टाइम ऑफिस में तुरंत संपर्क करें ।";


            ConnectionManager.DAL.ConManager con1 = new ConnectionManager.DAL.ConManager("1");
            dsApiSql = _sqlRepository.GetDataTable(Apisql);
            dsEmpList = _sqlRepository.GetDataTable(sqlFormat1);
          

            for (int i = 0; i < dsEmpList.Rows.Count; i++)
            {
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        //call API HERE
                        string Name = dsEmpList.Rows[i]["EmployeeName"].ToString(),
                            CardNo = dsEmpList.Rows[i]["EmployeeCode"].ToString(),
                            WorkDate = dsEmpList.Rows[i]["WorkDate"].ToString(),
                            LastPresentDate = dsEmpList.Rows[i]["PreviousWorkDate"].ToString();
                        string MobileNo = dsEmpList.Rows[i]["CellPhnNo"].ToString();//"9479871783";//
                        string FinalMessage = "";
                        string ScheduleDate = dsEmpList.Rows[i]["ScheduleTime"].ToString();

                        if (dsEmpList.Rows[i]["CellPhnNo"].ToString() == "")
                            throw new Exception("Cell no not found");


                        if (dsEmpList.Rows[i]["Type"].ToString().ToUpper() == "FORMAT1")
                            FinalMessage = string.Format(MessageBodyFormat1, Name, CardNo, WorkDate, LastPresentDate);

                        else if (dsEmpList.Rows[i]["Type"].ToString().ToUpper() == "FORMAT2")
                            FinalMessage = string.Format(MessageBodyFormat2, Name, CardNo, WorkDate, LastPresentDate);
                        else if (dsEmpList.Rows[i]["Type"].ToString().ToUpper() == "FORMAT3")
                            FinalMessage = string.Format(MessageBodyFormat3, Name, CardNo, WorkDate, LastPresentDate);


                        ////string apipath = @"http:// msg.msgclub.net/rest/services/sendSMS/sendGroupSms?AUTH_KEY=b8394a967b42149cd2d9518bb9c4faa5&message=नाम :- Akanksha कार्ड नंबर :-100 आपके द्वारा दिनांक 19-Jun-2021 को कार्य पर आते / जाते समय पंचिंग नहीं की गई है । आपकी अंतिम उपस्थिति दिनांक 18-Jun-2021 की है । अतः इस हेतु आप निर्धारित टाइम ऑफिस में तुरंत संपर्क करें । PRATBH&senderId=PRATBH&routeId=3&mobileNos=9479871783&smsContentType=Unicode&scheduleddate=19/06/2021%2112:00";
                        string apipath = @"" + dsApiSql.Rows[0]["SMSEndPoint"].ToString() + "?" + dsApiSql.Rows[0]["APIKeyWithValue"].ToString() + "&message=" + FinalMessage + @"। PRATBH&senderId=" + dsApiSql.Rows[0]["SenderId"].ToString() + "&routeId=3&mobileNos=" + MobileNo + "&smsContentType=Unicode&scheduleddate=" + ScheduleDate + "";

                        //call api

                        var client = new RestClient(apipath);
                        client.Timeout = -1;
                        var request = new RestRequest(Method.GET);
                        //request.AddHeader("Cookie", "JSESSIONID=36FFF38DBC63B107D53B3BCD3DE037E0.node3");
                        IRestResponse response = client.Execute(request);
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                            throw new Exception("API invocation error");

                        string sqlInsert = @"INSERT INTO SMSNotification
                                        (
	                                        EmpSystemId,WorkDate,[Description],SendFlag, AddedBy,AddedDate,AddedFromIP,UpdatedBy,UpdatedDate,UpdatedFromIP,SMSBody,SMSAPI
                                        )
                                        VALUES
                                        (
	                                        '" + dsEmpList.Rows[i]["EmpSystemID"].ToString() + @"',
                                           '" + dsEmpList.Rows[i]["WorkDate"].ToString() + @"',
                                            NULL,
                                            'Sent',
                                            'scheduler',
                                            '" + DateTime.Now + @"',
                                           ':::',
                                           'scheduler',
                                           '" + DateTime.Now + @"',
                                           ':::',
                                           N'" + FinalMessage + @"',
                                           N'" + apipath + @"'
                                        )
                                        ";

                        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                        con.BeginTransaction();
                        con.ExecuteNonQueryWrapper(sqlInsert, true, "1");
                        con.CommitTransaction();
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
        }
    }
}
