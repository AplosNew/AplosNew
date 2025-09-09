using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Planning.OrderManagement
{
    public class OS3Dashboard
    {
        private readonly SqlRepository _sqlRepository;
        public OS3Dashboard()
        {
            _sqlRepository = new SqlRepository();

        }


        public IEnumerable<object> filters()
        {
            try
            {
                var str = @"Select distinct isnull(p.id,'') as PlantId, isnull(p.username,'') as Plant, isnull(en.Id,e.Id) as EntityId, isnull(en.Username,e.Username) as Entity ,
                            isnull(cus.Id,'') as CustomerId,isnull(cus.UserName,'') as Customer , isnull(mo.ResponsiblePersonId,'') as MResId , isnull(emp.EmployeeName,'') as MResP  ,
                            isnull(e.EmployeeId,'') as ERespId , isnull(ee.EmployeeName,'') as EResp , so.OrderStatusId as Status
                            from  Trn.SalesOrder so 
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.id = moi.MasterOrderId
                            left join hkp.Party cus on cus.Id = mo.PartyId
                            left outer join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
							LEFT JOIN TRN.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
							left outer join org.entity e on e.Id = mo.EntityId
							left join org.entity en on en.Id = PO.EntityId
                            left join org.Plant p on p.Id = e.PlantId
                            left join dbo.EmployeeInformation emp on emp.SystemId = mo.ResponsiblePersonId 
                            left join dbo.EmployeeInformation ee on ee.SystemId = e.EmployeeId
                            where mo.OrderStatusId<>'Closed' and mo.OrderStatusId<>'Cancelled'
                            and so.OrderStatusId not in ('Closed','Cancelled')";
                return _sqlRepository.GetDataCollection(str);
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
                        ids = ",ISNULL(en.Id,e.Id) as col";
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
                                 Select distinct so.Id ,so.Qty , so.Rate , so.CM , so.DeliveryDate,so.AddedDate , so.CommitmentDate , pod.ProductionOrderID , so.SoProdCompDate as ProductionDate, so.OrderStatusId as OrderStatusId ,
                                DateDiff(Day,so.SoProdCompDate, so.DeliveryDate) as EarlyOrLateBy, prt.Username as customers ,ISNULL(en.UserName,e.UserName) as Entity ,  (case when so.PlanExFactoryDate is null then so.CommitmentDate else PlanExFactoryDate end) as DDate , emp.EmployeeName as MResp,
								ee.EmployeeName as EResp " + ids+ @"
                                from trn.MasterOrder mo 
								left join hkp.orderstatus os on os.Id = mo.OrderStatusId
								left outer join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
								inner join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
								left outer join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
								LEFT JOIN TRN.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
								left outer join org.entity e on e.Id = mo.EntityId
								left join org.entity en on en.Id = PO.EntityId
								left outer join org.Plant p on p.Id = mo.PlantId
								left outer join hkp.Party prt on prt.Id = mo.PartyId
								left outer join dbo.EmployeeInformation emp on emp.SystemId = mo.ResponsiblePersonId
								left outer join dbo.EmployeeInformation ee on ee.SystemId = e.EmployeeId
								where os.id<> 'Closed' and os.Id <>'Cancelled' and so.OrderStatusId not in ('Hold','Cancelled')
                                " + filter+ @"
--UNION 
--								Select distinct so.Id ,so.Qty , so.Rate , so.CM , so.DeliveryDate,so.AddedDate , so.CommitmentDate , pod.ProductionOrderID , (SELECT MAX--(xp1.ProductionDate) FROM ProductionPlanningType1 Xp1 WHERE Xp1.ProductionOrderID=pod.ProductionOrderID) as ProductionDate, so.OrderStatusId as -OrderStatusId ,
--                                DateDiff(Day,(SELECT MAX(xp1.ProductionDate) FROM ProductionPlanningType1 Xp1 WHERE Xp1.ProductionOrderID = pod.ProductionOrderID), --so.DeliveryDate) as EarlyOrLateBy , prt.Username as customers , e.UserName as Entity ,  (case when so.PlanExFactoryDate is null then -so.CommitmentDate else PlanExFactoryDate -end) as DDate , emp.EmployeeName as MResp,
--								ee.EmployeeName as EResp  " + ids + @"
--                                from TRN.ProductionOrder PO 
--							    LEFT JOIN TRN.ProductionOrderDetail pod ON PO.Id=pod.ProductionOrderId
--							    LEFT JOIN TRN.SalesOrder SO  ON pod.SalesOrderId=SO.Id
--							    left join org.entity e on e.Id = PO.EntityId
--								left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
--								left join trn.MasterOrder mo on mo.id = moi.MasterOrderId
--								left join hkp.orderstatus os on os.Id = mo.OrderStatusId
--								left outer join org.Plant p on p.Id = mo.PlantId
--								left outer join hkp.Party prt on prt.Id = mo.PartyId
--								left outer join dbo.EmployeeInformation emp on emp.SystemId = mo.ResponsiblePersonId
--								left outer join dbo.EmployeeInformation ee on ee.SystemId = e.EmployeeId
--								where os.id<> 'Closed' and os.Id <>'Cancelled' and so.OrderStatusId not in ('Closed','Hold','Cancelled')
--                                  " + filter + @" 
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
                string ch1 = "";
                for (int i = 0; i<tr.Rows.Count; i++)
                {
                    if(tr.Rows[i][0].ToString() != "")
                    {
                        
                        if(tr.Rows[i][0].ToString() != ch || tr.Rows[i][1].ToString() != ch1)
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
                        ch1 = tr.Rows[i][1].ToString();
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

        public IEnumerable<object> getClickData(Dictionary<string, string> parameters, string group, string col , string range , string analysis, string type, string entityId)
        {
            try
            {
                string date = "";
                string DDate = "";
                string Dtype = "";
                string ddd = "";

                // The Chart Type
                if (type == "ProductionD")
                {
                    Dtype = "(SELECT MAX(xp1.ProductionDate) FROM ProductionPlanningType1 Xp1 WHERE Xp1.ProductionOrderID = pod.ProductionOrderID)";
                    ddd = "ProductionDate";
                }
                if (type == "ToD")
                {
                    Dtype = "GETDATE()";
                    ddd = "GETDATE()";
                }

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

                var filRange = "where ";
                switch(range)
                {
                    case "<-30": filRange = filRange + "  DateDiff(Day,"+ddd+" , " + date+") <-30";
                        break;
                    case "<-30 TO -20":
                        filRange = filRange + "  DateDiff(Day,"+ddd+" , " + date + ") >-31 and DateDiff(Day," + ddd + " , " + date + ") <-20";
                        break;
                    case "<-20 TO -10":
                        filRange = filRange + "  DateDiff(Day," + ddd + " , " + date + ") >-21 and DateDiff(Day," + ddd + " , " + date + ") <-10";
                        break;
                    case "<-10 TO -5":
                        filRange = filRange + "  DateDiff(Day," + ddd + " , " + date + ") >-11 and DateDiff(Day," + ddd + " , " + date + ") <-5";
                        break;
                    case "<-5 TO -1":
                        filRange = filRange + "  DateDiff(Day," + ddd + " , " + date + ") >-6 and DateDiff(Day," + ddd + " , " + date + ") <0";
                        break;
                    case "= 0":
                        filRange = filRange + "  DateDiff(Day," + ddd + " , " + date + ") = 0";
                        break;
                    case ">0 TO 5":
                        filRange = filRange + "  DateDiff(Day," + ddd + " , " + date + ")> 0  and DateDiff(Day," + ddd + " , " + date + ") <6";
                        break;
                    case ">5 TO 10":
                        filRange = filRange + "  DateDiff(Day," + ddd + " , " + date + ") >5 and DateDiff(Day," + ddd + " , " + date + ") <11";
                        break;
                    case ">10 TO 15":
                        filRange = filRange + "  DateDiff(Day," + ddd + " , " + date + ") >10 and DateDiff(Day," + ddd + " , " + date + ") <16";
                        break;
                    case ">15 TO 20":
                        filRange = filRange + "  DateDiff(Day," + ddd + " , " + date + ") >15 and DateDiff(Day," + ddd + " , " + date + ") <21";
                        break;
                    case ">20 TO 30":
                        filRange = filRange + "  DateDiff(Day," + ddd + " , " + date + ") >20 and DateDiff(Day," + ddd + " , " + date + ") <31";
                        break;
                    case ">30":
                        filRange = filRange + "  DateDiff(Day," + ddd + " , " + date + ") >30 ";
                        break;

                    default:
                        filRange = "";
                        break;
                }
                string diffCols = "";
                if(range == "SO W/O PO")
                {
                    diffCols = diffCols +" and pod.ProductionOrderId is null";
                }
                if (range == "SO W/O Dates")
                {
                    diffCols = diffCols + " and (SELECT MAX(xp1.ProductionDate) FROM ProductionPlanningType1 Xp1 WHERE Xp1.ProductionOrderID=pod.ProductionOrderID) is null";
                }
                if (range == "SO Created Less Than 3 Days")
                {
                    diffCols = diffCols + " and DateDiff(day,so.AddedDate,GETDATE()) <= 3";
                }
                if(range == "Slab Total")
                {
                    diffCols = @"and (SELECT MAX(xp1.ProductionDate) FROM ProductionPlanningType1 Xp1 WHERE Xp1.ProductionOrderID=pod.ProductionOrderID) is not null

                                 and pod.ProductionOrderId is not null ";
                }

                if(range == "Slab Total" && type == "ToD")
                {
                    diffCols = "";
                }

                string filter = "";
                string[] fil = col.Split(':');
                if (parameters.ContainsKey("PlantId"))
                {
                    string ents = "";
                    if (parameters["ERespId"] != "'',''")
                    {
                        ents = ents + " and " + parameters["ERespId"];
                    }

                    if(group == "Delivery")
                    {
                        filter = @" and mo.ResponsiblePersonId in(" + parameters["MResId"] + @") and e.Id in (" + parameters["EntityId"] + @") and p.Id in (" + parameters["PlantId"] + @") 
                                and so.OrderStatusId in ( " + parameters["Status"] + @") and mo.PartyId in (" + parameters["CustomerId"] + @") " + ents + @" 
                                ";
                    }
                    if (group == "Entity")
                    {
                        filter = @"and (e.Id='"+entityId+ @"' or en.Id='" + entityId + @"') and mo.ResponsiblePersonId in(" + parameters["MResId"] + @") and e.Id = '"+fil[1]+@"' and p.Id in (" + parameters["PlantId"] + @") 
                                and so.OrderStatusId in ( " + parameters["Status"] + @") and mo.PartyId in (" + parameters["CustomerId"] + @") " + ents + @"";
                    }
                    if (group == "Customers")
                    {
                        filter = @" and mo.ResponsiblePersonId in(" + parameters["MResId"] + @") and e.Id in (" + parameters["EntityId"] + @") and p.Id in (" + parameters["PlantId"] + @") 
                                and so.OrderStatusId in ( " + parameters["Status"] + @") and mo.PartyId ='" + fil[1] + @"' " + ents + @"";
                    }
                    if (group == "MResp")
                    {
                        filter = @" and mo.ResponsiblePersonId ='" + fil[1] + @"' and isnull(e.Id,'') in (" + parameters["EntityId"] + @") and isnull(p.Id,'') in (" + parameters["PlantId"] + @") 
                                and isnull(so.OrderStatusId,'') in ( " + parameters["Status"] + @") and isnull(mo.PartyId,'') in (" + parameters["CustomerId"] + @") " + ents + @"";
                    }
                    if (group == "EResp")
                    {
                        filter = @" and mo.ResponsiblePersonId in(" + parameters["MResId"] + @") and e.Id in (" + parameters["EntityId"] + @") and p.Id in (" + parameters["PlantId"] + @") 
                                and so.OrderStatusId in ( " + parameters["Status"] + @") and mo.PartyId in (" + parameters["CustomerId"] + @") " + ents + @"";
                    }

                }
               
                string timing ="";
                if (group == "Delivery")
                {
                    if(filRange != "")
                    {
                        timing = timing + "and ";
                    }

                    if(filRange == "")
                    {
                        timing = timing + "where";
                    }
                    timing = timing + " Year(" + date + @") = '" + fil[1] + @"' and DateName(m," + date + @") = '" + fil[0] + @"'  ";
                }


                var str = @"Select * from (Select isnull(en.UserName,e.UserName) as Entity,prt.Username as Customers,b.UserName as Buyer, mo.BuyerReferenceNo,mo.OwnReferenceNo
								,moi.BuyerReferenceNo as IBuyerReferenceNo,moi.OwnReferenceNo as IOwnReferenceNo,mma.StandardName Article,so.Id SONo, SO.LineItemReference, so.Qty,Pk.DispatchBalance, format(so.DeliveryDate,'dd-MMM-yyyy') as DeliveryDate, format(so.CommitmentDate,'dd-MMM-yyyy') as CommitmentDate 
								--,format((SELECT MAX(xp1.ProductionDate) FROM ProductionPlanningType1 Xp1 WHERE Xp1.ProductionOrderID=pod.ProductionOrderID),'dd-MMM-yyyy') as ProductionDate
,so.SoProdCompDate as ProductionDate
,  format((case when so.PlanExFactoryDate is null then so.CommitmentDate else PlanExFactoryDate end) , 'dd-MMM-yyyy') as DDate
								,mo.Id as OrderNo,moi.Id as ItemNo,po.Id as PRNo,emp.EmployeeName as MResp
--,DateDiff(Day,(SELECT MAX(xp1.ProductionDate) FROM ProductionPlanningType1 Xp1 WHERE Xp1.ProductionOrderID = pod.ProductionOrderID), so.DeliveryDate) as EarlyOrLateBy 
,EarlyOrLateBy=DateDiff(Day,SO.SOProdCompDate, so.DeliveryDate) 
								,so.OrderStatusId as OrderStatusId,ps.UserName as POStatus,OC.UserName OrderType,SO.ProductionType,ShipmentFromStock=CASE WHEN SO.ShipmentFromStock=1 THEN 'Yes' ELSE 'No' END,so.AddedDate  , pod.ProductionOrderID , os.Username as MOOrderStatusId ,ee.EmployeeName as EResp ,mo.BuyerId,rem.Remarks
								
                               from trn.MasterOrder mo 
								left join hkp.orderstatus os on os.Id = mo.OrderStatusId
								left outer join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
								left JOIN mst.MaterialMasterArticle AS mma ON mma.Id=moi.ArticleId
								inner join trn.SalesOrder so on so.MasterOrderItemId = moi.Id								
								LEFT JOIN (SELECT A.SONo,A.Qty,DispatchBalance=ISNULL(A.Qty-SUM(A.TotalQtyNetWeight),0) FROM (
                                SELECT 
                                so.Id SONo, so.Qty,POLR.TotalQtyNetWeight
                                FROM trn.SalesOrder so 								
                                LEFT JOIN trn.PackingLineItem PLI ON PLI.SOId=SO.Id
                                LEFT JOIN 
                                (							
                                Select (sc.NetWeight * Count(sc.RefNo)) as TotalQtyNetWeight,PackingLineItemId from trn.POLotReference po
                                left join dbo.ItemScanChild sc on sc.PackingId = po.Id
                                GROUP BY PackingLineItemId,sc.NetWeight
                                )POLR ON POLR.PackingLineItemId=PLI.PackingLineItemId
                                )A
                                GROUP BY A.SONo,A.Qty )PK ON PK.SoNo=SO.Id
                                LEFT JOIN HKP.OrderCategory OC ON OC.Id = SO.OrderCategoryId
								left outer join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
								left outer join org.entity e on e.Id = mo.EntityId
								left outer join org.entity en on en.Id = po.EntityId
								left outer join org.Plant p on p.Id = mo.PlantId
								left outer join hkp.Party prt on prt.Id = mo.PartyId
								left outer join dbo.EmployeeInformation emp on emp.SystemId = mo.ResponsiblePersonId
								left outer join dbo.EmployeeInformation ee on ee.SystemId = e.EmployeeId
								left outer join hkp.ProductionStatus ps on ps.Id = po.ProductionStatusId
                                left join hkp.Buyer b on b.Id = mo.BuyerId
                                left join (Select  oc.SalesOrderId ,
                                 (Select top 1 Concat( format(cr.AddedDate,'dd/MMM/yy') ,' - ' ,cr.Remarks) as Remarks from dbo.OrderControlRemarks cr left join dbo.OrderControl c on c.Id = cr.OrderControlId
								where c.SalesOrderId = oc.SalesOrderId order by cr.AddedDate desc
                                ) as Remarks
                                from dbo.OrderControl oc
                                left join dbo.OrderControlRemarks ocr on ocr.OrderControlId = oc.Id
                                where oc.SalesOrderId is not null
                                group by oc.SalesOrderId
								) as rem on rem.SalesOrderId = so.Id
                                
								where os.id<> 'Closed' and os.Id <>'Cancelled' and so.OrderStatusId not in ('Hold','Cancelled')
                                " + filter+@" "+diffCols+ @") da 
                                " + filRange + " " + timing + @"";
                
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getControlList(string pr)
        {
            try
            {
                var str = @"Select oc.ProductionOrderId,format(oc.AddedDate,'dd-MMM-yyyy') as DateAdded,(Case When oct.ControlType ='MainRMInHouse' then oc.Status end) as MainRMInHouse , 
                                (Case When oct.ControlType ='OtherRMInHouse' then oc.Status end) as OtherRMInHouse , 
                                (Case When oct.ControlType ='MainRMShipment' then oc.Status end) as MainRMShipment , 
                                (Case When oct.ControlType ='OtherRMShipment' then oc.Status end) as OtherRMShipment ,
                                (Case When oct.ControlType ='BaseProcessInput' then oc.Status end) as BaseProcessInput,
                                ocr.Remarks 
                                from 
                                dbo.OrderControl oc
                                left join dbo.OrderControlTypes oct on oct.Id = oc.ControlTypeId
                                left join dbo.OrderControlRemarks ocr on ocr.OrderControlId = oc.Id
                                where oc.ProductionOrderId is not null and oc.ProductionOrderId ='" + pr+@"'
								order by oc.AddedDate desc
								";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}



//var str = @"
//Select DATENAME(MONTH,MaxDeliveryDate) as Months , YEAR(MaxDeliveryDate) as Years ,sum(case when EarlyOrLateBy<-30 then 1 else 0 end) LN30, sum(case when EarlyOrLateBy>-30 and EarlyOrLateBy<-20 then 1 else 0 end) LN30T20
//                     , sum(case when EarlyOrLateBy>-21 and EarlyOrLateBy<-10 then 1 else 0 end) LN20T10, sum(case when EarlyOrLateBy>-11 and EarlyOrLateBy<-5 then 1 else 0 end) LN10T5
//                     , sum(case when EarlyOrLateBy>-6 and EarlyOrLateBy<0 then 1 else 0 end) LN5T0, sum(case when EarlyOrLateBy=0 then 1 else 0 end) E0
//                     , sum(case when EarlyOrLateBy>0 and EarlyOrLateBy<6 then 1 else 0 end) G0T5, sum(case when EarlyOrLateBy>5 and EarlyOrLateBy<11 then 1 else 0 end) G5T10
//                     , sum(case when EarlyOrLateBy>10 and EarlyOrLateBy<16 then 1 else 0 end) G10T15, sum(case when EarlyOrLateBy>15 and EarlyOrLateBy<21 then 1 else 0 end) G15T20
//                     , sum(case when EarlyOrLateBy>20 and EarlyOrLateBy<31 then 1 else 0 end) G20T30, sum(case when EarlyOrLateBy>30 then 1 else 0 end) G30
//,sum(case when EarlyOrLateBy < 0 then 1 else 0 end) negs,sum(case when EarlyOrLateBy >0 and EarlyOrLateBy<11 then 1 else 0 end) tens
//,sum(case when EarlyOrLateBy > 10 then 1 else 0 end) poss
//                     from
//                     (
//                     Select distinct  p1.ProductionOrderID ,
//                      (SELECT MAX(xso.DeliveryDate) FROM trn.SalesOrder AS xso
//                     JOIN trn.ProductionOrderDetail AS xpod ON xso.Id=xpod.SalesOrderId
//                      where xpod.ProductionOrderId=p1.productionorderid ) AS MaxDeliveryDate,

//                        (SELECT MAX(xp1.ProductionDate) FROM ProductionPlanningType1 Xp1 WHERE Xp1.ProductionOrderID=p1.ProductionOrderID)
//                        AS MaxProductionDate,

//                       DATEDIFF(DAY, (SELECT MAX(xp1.ProductionDate) FROM ProductionPlanningType1 Xp1 WHERE Xp1.ProductionOrderID=p1.ProductionOrderID),
//                        (SELECT MAX(xso.DeliveryDate) FROM trn.SalesOrder AS xso
//                     JOIN trn.ProductionOrderDetail AS xpod ON xso.Id=xpod.SalesOrderId
//                      where xpod.ProductionOrderId=p1.productionorderid )
//                        ) AS EarlyOrLateBy

//                        from ProductionPlanningType1 p1
//                     ) da
//                     group by  DATENAME(MONTH,MaxDeliveryDate) , YEAR(MaxDeliveryDate) , DATEPART(m,MaxDeliveryDate)
//                     order by Years , DATEPART(m,MaxDeliveryDate)
