using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Library.Planning.OrderManagement
{
    public class Order
    {
        private readonly SqlRepository _sqlRepository;

        public object JsonRequestBehavior { get; private set; }

        public Order()
        {
            _sqlRepository = new SqlRepository();

        }




        public IEnumerable<object> filters()
        {
            try
            {
                var sql = @"SELECT * FROM ( SELECT  
                                        isnull(e.Id,'') AS EntityId,isnull(e.UserName,'') Entity,
										pln.Id PlantId,Pln.UserName Plant,
                                        isnull(ps.Id,'') AS ProductionStatusId, isnull(ps.UserName,'') AS ProductionStatus
										,PO.Id ProductionOrderId
                                      , ResponsiblePersonId=STUFF((select distinct ','+XMO.ResponsiblePersonId from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join dbo.EmployeeInformation XEmp on XEmp.SystemId=XMO.ResponsiblePersonId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
	                                         , ResponsiblePerson=STUFF((select distinct ','+XEmp.EmployeeName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join dbo.EmployeeInformation XEmp on XEmp.SystemId=XMO.ResponsiblePersonId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                   , Buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

														 SOStatusId=STUFF((select distinct ','+XB.Id from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].OrderStatus XB on XB.Id=XSO.OrderStatusId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


													
																 MOStatusId=STUFF((select distinct ','+XB.Id from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].OrderStatus XB on XB.Id=XMO.OrderStatusId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

		
													 BuyerId=STUFF((select distinct ','+XB.Id from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

																
                                                    CustomerId=STUFF((select distinct ','+XP.Id from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),   
                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                                                 


                                        from trn.ProductionOrder PO
				                                inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
												
				                              
				                                left outer join org.Entity E on e.Id=PO.EntityID
				                             
				                                left outer join org.Plant PLN on pln.Id=E.PlantId
				                                LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId

                              WHERE  PO.ProductionStatusId<>'Closed'
                                ) AS KK							
";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception e)
            {
                throw e;
            }
        }


        public struct Total
        {
            public string Key ;
            public double total;
            public Total(double tot)
            {
                Key = "Total";
                total = tot;
            }
        }

        public List<Dictionary<string, object>> getSlabData(Dictionary<string,string>parameters , string group , out List<Object> totalArr , out List<double[]> chart , string value , string analysis, string type)
        {
            try
            {
                var str = "";
                string filter = "";
                string select = "";
                string groupBy = "";
                string ids = "";
                string date = "";
                string val = "";
                string DDate = "";
                string Dtype = "";
                // The Chart Type
                if(type == "ProductionD")
                {
                    Dtype = "(SELECT MAX(xp1.ProductionDate) FROM ProductionPlanningType1 Xp1 WHERE Xp1.ProductionOrderID = pod.ProductionOrderID)";
                }
                if(type == "ToD")
                {
                    Dtype = "GETDATE()";
                }
                //To set date String
                switch (analysis)
                {
                    case "DeliveryD":
                        date = date + "DeliveryDate";
                        DDate = DDate + "so.DeliveryDate";
                        break;
                    case "CommitmentD":
                        date = date + "CommitmentDate";
                        DDate = DDate + "so.CommitmentDate";
                        break;
                    case "ExFactoryD":
                        date = date + "DDate";
                        DDate = DDate + "case when so.PlanExFactoryDate is null then so.CommitmentDate else PlanExFactoryDate end";
                        break;

                }
                //To Set Filter String
                if (parameters.ContainsKey("PlantId"))
                {
                    string ents = "";
                    if(parameters["ERespId"] != "'',''")
                    {
                        ents = ents +" and e.employeeId in (" +parameters["ERespId"]+")";
                    }

                    filter = @" and isnull(mo.ResponsiblePersonId,'') in(" + parameters["MResId"] + @") and isnull(e.Id,'') in (" + parameters["EntityId"] + @") and isnull(p.Id,'') in (" + parameters["PlantId"] + @") 
                                and isnull(so.OrderStatusId,'') in ( " + parameters["Status"] + @") and isnull(mo.PartyId,'') in (" + parameters["CustomerId"] + @") " + ents + @"";
                }
                
                //To set Select and groupBY string and ids
                if(group == "Delivery")
                {
                        select = @"Select DATENAME(m," + date + @") as Months , YEAR(" + date + @") as Years ,";
                        groupBy = @" group by  DATENAME(MONTH," + date + @") , YEAR(" + date + @") , DATEPART(m," + date + @") , OrderStatusId
                order by Years , DATEPART(m," + date + @")";
                    
                    
                }
                else
                {
                    select = @"Select isnull("+group+",'Not Alotted') as "+group+"  , col,";
                    groupBy = @"group by "+group+ ",col, OrderStatusId order by " + group+"";
                    if(group == "Entity")
                    {
                        ids = ",e.Id as col";
                    }
                    if (group == "Customers")
                    {
                        ids = ",mo.PartyId as col";
                    }
                    if (group == "MResp")
                    {
                        ids = ",mo.ResponsiblePersonId as col";
                    }
                    if (group == "EResp")
                    {
                        ids = ",e.employeeId as col";
                    }
                }

                //To set val  String
                switch(value)
                {
                    case "SO":
                        val = val + "1";
                        break;
                    case "SOQTY":
                        val = val + "Qty";
                        break;
                    case "SORT":
                        val = val + "Qty*Rate";
                        break;
                    case "SOCM":
                        val = val + "Qty*CM";
                        break;
                }
                

                    
                str = @""+select+ @"sum(case when EarlyOrLateBy<-30 then " + val + @" else 0 end) LN30, sum(case when EarlyOrLateBy>-31 and EarlyOrLateBy<-20 then " + val + @" else 0 end) LN30T20
                                , sum(case when EarlyOrLateBy>-21 and EarlyOrLateBy<-10 then " + val+ @" else 0 end) LN20T10, sum(case when EarlyOrLateBy>-11 and EarlyOrLateBy<-5 then " + val + @" else 0 end) LN10T5
                                , sum(case when EarlyOrLateBy>-6 and EarlyOrLateBy<0 then " + val + @" else 0 end) LN5T0, sum(case when EarlyOrLateBy=0 then " + val + @" else 0 end) E0
                                , sum(case when EarlyOrLateBy>0 and EarlyOrLateBy<6 then " + val + @" else 0 end) G0T5, sum(case when EarlyOrLateBy>5 and EarlyOrLateBy<11 then " + val + @" else 0 end) G5T10
                                , sum(case when EarlyOrLateBy>10 and EarlyOrLateBy<16 then " + val + @" else 0 end) G10T15, sum(case when EarlyOrLateBy>15 and EarlyOrLateBy<21 then " + val + @" else 0 end) G15T20
                                , sum(case when EarlyOrLateBy>20 and EarlyOrLateBy<31 then " + val + @" else 0 end) G20T30, sum(case when EarlyOrLateBy>30 then " + val + @" else 0 end) G30
                                ,sum(case when ProductionDate is null then " + val + @" else 0 end) nodates
                                , sum(case when ProductionOrderId is null then " + val + @" else 0 end) NotAlotted
								, sum(case when AddedDate >= DATEADD(DAY, -3 , GETDATE()) then " + val + @" else 0 end) daysthree 
                                ,OrderStatusId
                                from
                                (
                                 Select distinct so.Id ,so.Qty , so.Rate , so.CM , so.DeliveryDate,so.AddedDate , so.CommitmentDate , pod.ProductionOrderID , (SELECT MAX(xp1.ProductionDate) FROM ProductionPlanningType1 Xp1 WHERE Xp1.ProductionOrderID=pod.ProductionOrderID) as ProductionDate, so.OrderStatusId as OrderStatusId ,
                                DateDiff(Day,"+Dtype+", " + DDate+ @") as EarlyOrLateBy , prt.Username as customers , e.UserName as Entity ,  (case when so.PlanExFactoryDate is null then so.CommitmentDate else PlanExFactoryDate end) as DDate , emp.EmployeeName as MResp,
								ee.EmployeeName as EResp " + ids+ @"
                                from trn.MasterOrder mo 
								left join hkp.orderstatus os on os.Id = mo.OrderStatusId
								left outer join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
								inner join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
								left outer join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
								left outer join org.entity e on e.Id = mo.EntityId
								left outer join org.Plant p on p.Id = mo.PlantId
								left outer join hkp.Party prt on prt.Id = mo.PartyId
								left outer join dbo.EmployeeInformation emp on emp.SystemId = mo.ResponsiblePersonId
								left outer join dbo.EmployeeInformation ee on ee.SystemId = e.EmployeeId
								where os.id<> 'Closed' and os.Id <>'Cancelled' and so.OrderStatusId not in ('Closed','Cancelled')
                                " + filter+@"
                                ) as da
                                " + groupBy+"";

                
                //Making of the required Datatable for the SlabGrid
                DataTable tr = _sqlRepository.GetDataTable(str);
                DataTable tt = tr.Clone();
               List<Object> newArr = new List<object>();
                int ini = 2;

                double[] Active = new double[14] ;
                double[] Pending = new double[14] ;
                double[] ToClose = new double[14];
                double[] ToDispatch = new double[14];
                double[] ProductionComplete = new double[14];



                string[] columnNames = tr.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName)
                                 .ToArray();

                //The Stacked Chart Values
                for (int i = 0; i <tr.Rows.Count; i++)
                {
                    if(tr.Rows[i]["OrderStatusId"].ToString() == "Active")
                    {
                        for(int j = 2;j<14; j++)
                        {
                            Active[j] = Active[j] + OTSBD.clsStaticInfo.dbl(tr.Rows[i][j].ToString());
                        }
                    }
                    if (tr.Rows[i]["OrderStatusId"].ToString() == "Pending")
                    {
                        for (int j = 2; j < 14; j++)
                        {
                            Pending[j] = Pending[j] + OTSBD.clsStaticInfo.dbl(tr.Rows[i][j].ToString());
                        }
                    }
                    if (tr.Rows[i]["OrderStatusId"].ToString() == "ToClose")
                    {
                        for (int j = 2; j < 14; j++)
                        {
                            ToClose[j] = ToClose[j] + OTSBD.clsStaticInfo.dbl(tr.Rows[i][j].ToString());
                        }
                    }
                    if (tr.Rows[i]["OrderStatusId"].ToString() == "ToShip")
                    {
                        for (int j = 2; j < 14; j++)
                        {
                            ToDispatch[j] = ToDispatch[j] + OTSBD.clsStaticInfo.dbl(tr.Rows[i][j].ToString());
                        }
                    }
                    if (tr.Rows[i]["OrderStatusId"].ToString() == "ProductionComplete")
                    {
                        for (int j = 2; j < 14; j++)
                        {
                            ProductionComplete[j] = ProductionComplete[j] + OTSBD.clsStaticInfo.dbl(tr.Rows[i][j].ToString());
                        }
                    }
                }
                List<double[]> list = new List<double[]>();
                list.Add(Active);
                list.Add(Pending);
                list.Add(ToClose);
                list.Add(ToDispatch);
                list.Add(ProductionComplete);

                chart = list;

                DataRow dr = null;
                int roww = 0;
                string ch = "";
                for (int i = 0; i<tr.Rows.Count; i++)
                {
                    if(tr.Rows[i][0].ToString() != "")
                    {
                        
                        if(tr.Rows[i][0].ToString() != ch)
                        {
                            dr = tt.NewRow();
                            dr[columnNames[0]] = tr.Rows[i][0].ToString();
                            dr[columnNames[1]] = tr.Rows[i][1].ToString();

                            for (int j = 2; j < 17; j++)
                            {
                                dr[columnNames[j]] = 0;
                            }
                            tt.Rows.Add(dr);
                            roww++;
                        }

                        for(int j = 2; j<17; j++)
                        {
                            double sum = OTSBD.clsStaticInfo.dbl(tt.Rows[roww - 1][j].ToString()) + OTSBD.clsStaticInfo.dbl(tr.Rows[i][j].ToString());
                            tt.Rows[roww - 1][j] = sum; 
                        }

                        ch = tr.Rows[i][0].ToString();
                    }
                }



                //Finding the total of the Row
                if(type == "ProductionD")
                {
                    for (int i = 0; i < tt.Rows.Count; i++)
                    {
                        double jj = 0;
                        for (int j = ini; j < 15; j++)
                        {
                            jj = jj + OTSBD.clsStaticInfo.dbl(tt.Rows[i][j].ToString());
                        }
                        Total t = new Total(jj);
                        newArr.Add(t);

                    }
                }
                else
                {
                    for (int i = 0; i < tt.Rows.Count; i++)
                    {
                        double jj = 0;
                        for (int j = ini; j < 14; j++)
                        {
                            jj = jj + OTSBD.clsStaticInfo.dbl(tt.Rows[i][j].ToString());
                        }
                        Total t = new Total(jj);
                        newArr.Add(t);

                    }
                }
                
                totalArr = newArr;

                tt.Columns.Add("RowTotal", typeof(decimal));
                
                    for (int i = 0; i < tt.Rows.Count; i++)
                    {
                        double jj = 0;
                        for (int j = ini; j < 14; j++)
                        {
                            jj = jj + OTSBD.clsStaticInfo.dbl(tt.Rows[i][j].ToString());
                        }
                        tt.Rows[i]["RowTotal"] = jj;
                    }
                
                

                return Library.Service.Helpers.DataTableExtensions.DataTableToJson(tt);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}