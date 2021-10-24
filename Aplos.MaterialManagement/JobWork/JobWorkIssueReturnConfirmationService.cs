using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Linq;
using System.Data;
using OTSBD;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using Library.Data.UnitOfWorks;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data;
using Library.Service.Logs;
using Library.Service.Enums;
using System.Reflection;
using System.Collections.Specialized;

namespace Library.MaterialManagement.JobWork
{

    public class JobWorkIssueReturnConfirmation
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public JobWorkIssueReturnConfirmation()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        //       FOR JOB WORK MODULE

        public IEnumerable<object> GetSearchedData(string FromDate, string ToDate, string Status, string PartyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "";
                if (Status == "1")
                {
                    sql = @"select ir.Id as IssueId, FORMAT(ir.Date,'dd-MMM-yyyy') as IssueDate,kk.Id as IssueChildId,kk.TotalIssuedQuantity, vc.Id as ValueAddedContractId,p.UserName as Party 
                                 ,jwi.UserName as JWOutputItem,mma.StandardName as Article,kk.ConfirmationQuantity
								 ,ConfirmedQty =CASE WHEN kk.ConfirmationQuantity is not null THEN kk.ConfirmationQuantity ELSE 0 END
								 from dbo.JobWorkIssueReturn ir 
                                left join (	select SUM(quantity) as TotalIssuedQuantity,JobWorkIssueReturnMasterId,Id,ContractLineItemId, ConfirmationQuantity FROM dbo.JobWorkIssueReturnChild group by ContractLineItemId,JobWorkIssueReturnMasterId,Id,ConfirmationQuantity
										) kk on kk.JobWorkIssueReturnMasterId=ir.Id
								 left join dbo.JobWorkValueAddedContractChild mp on mp.Id=kk.ContractLineItemId
								 left join dbo.JobWorkValueAddedContract vc on vc.Id=mp.JobWorkValueAddedContractMasterId
								 left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
								 left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
								 left join HKP.Party p on p.Id=vc.VendorPartyId
								 where (ir.[Date] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
								 and vc.VendorPartyId='" + PartyId + @"' and ir.IsConfirmed='" + Status + @"' and kk.ConfirmationQuantity is not null order by ir.Date desc  ";


                }
                if (Status == "0")
                {
                    sql = @"select ir.Id as IssueId, FORMAT(ir.Date,'dd-MMM-yyyy') as IssueDate,kk.Id as IssueChildId,kk.TotalIssuedQuantity, vc.Id as ValueAddedContractId,p.UserName as Party 
                                 ,jwi.UserName as JWOutputItem,mma.StandardName as Article
								 ,ConfirmedQty =CASE WHEN kk.ConfirmationQuantity is null THEN kk.TotalIssuedQuantity ELSE 0 END
								 from dbo.JobWorkIssueReturn ir 
                                left join (	select SUM(quantity) as TotalIssuedQuantity,JobWorkIssueReturnMasterId,Id,ContractLineItemId, ConfirmationQuantity FROM dbo.JobWorkIssueReturnChild group by ContractLineItemId,JobWorkIssueReturnMasterId,Id,ConfirmationQuantity
										) kk on kk.JobWorkIssueReturnMasterId=ir.Id
								 left join dbo.JobWorkValueAddedContractChild mp on mp.Id=kk.ContractLineItemId
								 left join dbo.JobWorkValueAddedContract vc on vc.Id=mp.JobWorkValueAddedContractMasterId
								 left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
								 left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
								 left join HKP.Party p on p.Id=vc.VendorPartyId
								 where (ir.[Date] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
								 and vc.VendorPartyId='" + PartyId + @"' and kk.ConfirmationQuantity is null order by ir.Date desc ";


                }
                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> LoadAllPartyDetailsForSelection(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select p.Id, p.Sequence, p.Code, p.ShortName, p.StandardName, p.UserName,pg.UserName as PartyGroup
                               from HKP.Party p left join HKP.PartyGroup pg on pg.Id=p.PartyGroupId
							   inner join dbo.JobWorkValueAddedContract vac on vac.VendorPartyId=p.Id
                               WHERE p.CompanyGroupId='" + identity.CompanyGroupId + @"' and p.PartyType='Party'
                               AND isnull(p.Id,'') not in (select isnull(VendorPartyId,'') from dbo.JobWorkValueAddedContract where Id='" + Id + @"')
                               order by p.Sequence ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void SaveConfirmedIssueChildTab(IEnumerable<JobWorkConfirmationIssue> ConfirmedIssueChildData, string IsConfirmed)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet headerexist;
                DataSet childexist;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var IssueId = "' '";
                var IssueChildId = "' '";
                foreach (var get in ConfirmedIssueChildData)
                {
                    IssueId += ",'" + get.IssueId + "' ";
                    IssueChildId += ",'" + get.IssueChildId + "' ";

                }

                con.OpenDataSetThroughAdapter("select * from dbo.JobWorkIssueReturn where Id IN (" + IssueId + ") ", out headerexist, false, "1");
                con.OpenDataSetThroughAdapter("select * from dbo.JobWorkIssueReturnChild where Id IN (" + IssueChildId + ") ", out childexist, false, "1");

                foreach (var item in ConfirmedIssueChildData)
                {
                    headerexist.Tables[0].DefaultView.RowFilter = "Id ='" + item.IssueId + "' ";

                    if (headerexist.Tables[0].DefaultView.Count > 0)
                    {
                        //edit
                        DataRow dr = headerexist.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["IsConfirmed"] = IsConfirmed;

                        dr["ConfirmationBy"] = identity.Name;
                        dr["ConfirmationDate"] = System.DateTime.Now.ToString();
                        dr["ConfirmationIP"] = identity.IPAddress;


                        dr.EndEdit();

                        childexist.Tables[0].DefaultView.RowFilter = "Id ='" + item.IssueChildId + "' ";
                        if (childexist.Tables[0].DefaultView.Count > 0)
                        {
                            //edit
                            DataRow drr = childexist.Tables[0].DefaultView[0].Row;

                            drr.BeginEdit();

                            drr["ConfirmationQuantity"] = item.ConfirmedQty;

                            drr.EndEdit();

                        }

                    }



                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(headerexist, childexist);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // TRANSFORMATION CONFIRMATION ISSUE CHILD

        public IEnumerable<object> GetSearchTransConfirmationIssue(string FromDate, string ToDate, string Status)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "";
                if (Status == "1")
                {
                    sql = @"select tir.Id as IssueId, FORMAT(tir.Date,'dd-MMM-yyyy') as IssueDate,kk.Id as TransIssueChildId, kk.TotalIssuedQuantity, tc.Id as TransContractId
                                                       ,p.UserName as Party, jwi.UserName as JWOutputItem, jwii.UserName as JWInputItem,mma.StandardName as Article
                                                       ,TransConfirmedQty =CASE WHEN kk.ConfirmationQuantity is not null THEN kk.ConfirmationQuantity ELSE 0 END
                                                       from dbo.JobWorkTransformationIssueReturn tir
													   left join (	select SUM(quantity) as TotalIssuedQuantity,TransformationIssueReturnMasterId,Id, MaterialInputId, ConfirmationQuantity FROM dbo.JobWorkTransformationIssueReturnChild group by TransformationIssueReturnMasterId,Id,MaterialInputId,ConfirmationQuantity
									                 	) kk on kk.TransformationIssueReturnMasterId=tir.Id
														left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=kk.MaterialInputId
														left join dbo.OSTransformationPODetail mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
														left join dbo.JobWorkTransformationContract tc on tc.Id=mp.OSTransformationPOId
														left join HKP.Party p on p.Id=tc.VendorPartyId
														left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
														left join hkp.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
														left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
														 where (tir.[Date] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
							                        	 and tir.IsConfirmed='" + Status + @"' and kk.ConfirmationQuantity is not null order by tir.Date desc  ";


                }
                if (Status == "0")
                {
                    sql = @"select tir.Id as IssueId, FORMAT(tir.Date,'dd-MMM-yyyy') as IssueDate,kk.Id as TransIssueChildId, kk.TotalIssuedQuantity, tc.Id as TransContractId
                                                       ,p.UserName as Party, jwi.UserName as JWOutputItem, jwii.UserName as JWInputItem,mma.StandardName as Article
                                                       ,TransConfirmedQty =CASE WHEN kk.ConfirmationQuantity is null THEN kk.TotalIssuedQuantity ELSE 0 END
                                                       from dbo.JobWorkTransformationIssueReturn tir
													   left join (	select SUM(quantity) as TotalIssuedQuantity,TransformationIssueReturnMasterId,Id, MaterialInputId, ConfirmationQuantity FROM dbo.JobWorkTransformationIssueReturnChild group by TransformationIssueReturnMasterId,Id,MaterialInputId,ConfirmationQuantity
									                 	) kk on kk.TransformationIssueReturnMasterId=tir.Id
														left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=kk.MaterialInputId
														left join dbo.OSTransformationPODetail mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
														left join dbo.JobWorkTransformationContract tc on tc.Id=mp.OSTransformationPOId
														left join HKP.Party p on p.Id=tc.VendorPartyId
														left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
														left join hkp.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
														left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
														 where (tir.[Date] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
							                        	 and kk.ConfirmationQuantity is null order by tir.Date desc ";


                }
                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> LoadAllPartyVendorForSelection(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select p.Id, p.Sequence, p.Code, p.ShortName, p.StandardName, p.UserName,pg.UserName as PartyGroup
                               from HKP.Party p left join HKP.PartyGroup pg on pg.Id=p.PartyGroupId
							   inner join dbo.JobWorkTransformationContract tc on tc.VendorPartyId=p.Id
                               WHERE p.CompanyGroupId='" + identity.CompanyGroupId + @"' and p.PartyType='Party'
                               AND isnull(p.Id,'') not in (select isnull(VendorPartyId,'') from dbo.JobWorkTransformationContract where Id='" + Id + @"')
                               order by p.Sequence ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void SaveTransConfirmationIssueChildTab(IEnumerable<JobWorkTransformationConfirmationIssue> TransConfirmedIssueChildData, string IsConfirmed)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet headerexist;
                DataSet childexist;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var IssueId = "' '";
                var IssueChildId = "' '";
                foreach (var get in TransConfirmedIssueChildData)
                {
                    IssueId += ",'" + get.IssueId + "' ";
                    IssueChildId += ",'" + get.TransIssueChildId + "' ";

                }

                con.OpenDataSetThroughAdapter("select * from dbo.JobWorkTransformationIssueReturn where Id IN (" + IssueId + ") ", out headerexist, false, "1");
                con.OpenDataSetThroughAdapter("select * from dbo.JobWorkTransformationIssueReturnChild where Id IN (" + IssueChildId + ") ", out childexist, false, "1");

                foreach (var item in TransConfirmedIssueChildData)
                {
                    headerexist.Tables[0].DefaultView.RowFilter = "Id ='" + item.IssueId + "' ";

                    if (headerexist.Tables[0].DefaultView.Count > 0)
                    {
                        //edit
                        DataRow dr = headerexist.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["IsConfirmed"] = IsConfirmed;

                        dr["ConfirmationBy"] = identity.Name;
                        dr["ConfirmationDate"] = System.DateTime.Now.ToString();
                        dr["ConfirmationIP"] = identity.IPAddress;


                        dr.EndEdit();

                        childexist.Tables[0].DefaultView.RowFilter = "Id ='" + item.TransIssueChildId + "' ";
                        if (childexist.Tables[0].DefaultView.Count > 0)
                        {
                            //edit
                            DataRow drr = childexist.Tables[0].DefaultView[0].Row;

                            drr.BeginEdit();

                            drr["ConfirmationQuantity"] = item.TransConfirmedQty;

                            drr.EndEdit();

                        }

                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(headerexist, childexist);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
public class JobWorkConfirmationIssue
{

    #region Scalar Properties

    public string IssueId { get; set; }
    public string IssueChildId { get; set; }
    public string ConfirmedQty { get; set; }

    #endregion Scalar Properties
}
public class JobWorkTransformationConfirmationIssue
{

    #region Scalar Properties

    public string IssueId { get; set; }
    public string TransIssueChildId { get; set; }
    public string TransConfirmedQty { get; set; }

    #endregion Scalar Properties
}
