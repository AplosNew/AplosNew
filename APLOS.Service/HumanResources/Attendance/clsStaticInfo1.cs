using System;
using System.Data;

   public class clsStaticInfo
    {


        public void xSaveDataSets(params System.Data.DataSet[] dsRef)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveData(ref dsRef[i]);
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }

        } // End Function
        public void SaveDataSets(params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }

        }//End Function  
        public void RunRawSQL(string str)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;

                objCon.ExecuteNonQueryWrapper(str, true, "1");
               
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }

        }//End Function  
        public void sampleSearch(string fromDate, string todate, string entityID, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            string strSQL = "";
            string strfromdate = "";
            string strtodate = "";
            try
            {
                if (strKey == "")
                {
                    strSQL = @"SELECT SystemID, refNo, left(replace(upper(convert(varchar,InvoiceDate,113)),' ','-'),11) as InvoiceDate, 
                                Narration, RefID,  ISNULL(Amount,0) AS Amount
                                FROM Voucher 
                                WHERE " + strKey + " AND entityID='" + entityID + @"'
                                ORDER BY refNo";

                }

                else
                {
                    strSQL = @"SELECT SystemID, refNo, left(replace(upper(convert(varchar,InvoiceDate,113)),' ','-'),11) as InvoiceDate, 
                                Narration, RefID,  ISNULL(Amount,0) AS Amount
                                FROM Voucher 
                                WHERE " + strKey + " AND entityID='" + entityID + @"'
                                ORDER BY refNo";

                }

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                //objCon.getDataSet(strSQL, out dsRef);
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
                objCon.CommitTransaction();


            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function
        public void sampleData(string strEntityID, string refNo, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = @"SELECT V.refNo, ii.LotNo, ii.ItemID, IC.ItemDescription, ItemCode,
                            ISNULL(II.Rate,0) AS Rate, ISNULL(II.Quantity,0) AS Quantity, 
                            SUM(ISNULL(II.UpQuantity,0)) AS UpQuantity, SUM(ISNULL(II.DownQuantity,0)) AS DownQuantity 
                            FROM Voucher AS V
                            LEFT OUTER JOIN InventoryItems AS II ON V.SystemID=II.VoucherSystemID
                            LEFT OUTER JOIN ItemChild AS IC ON II.ItemID=IC.SystemID
                            WHERE refNo='" + refNo + @"' AND V.EntityID='" + strEntityID + @"'
                            GROUP BY V.refNo, ii.LotNo, ii.ItemID, IC.ItemDescription, ItemCode,
                            II.Rate, Quantity";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function 
        public void GetGroupID(string CompanyGroupID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = @"select id from hkp.CompanyGroup where id='"+ CompanyGroupID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                //objCon.CommitTransaction();
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void sampleDelete(string strEntityID, string strItemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                objCon.BeginTransaction();

                objCon.executeQuery("DELETE FROM ItemChild WHERE EntityID = '" + strEntityID + "' AND SystemID = '" + strItemID + "'");

                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {

                objCon = null;
            }
        }//End of function


        public void GetUserAuthenticationData(string GroupID, string AuthenticationID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = @"select 
                                u.id
                                ,u.userid
                                ,u.CompanyGroupId
                                ,u.UserLocked
                                ,u.UserLockedDate
                                ,u.AuthToken
                                ,u.AuthTokenLocked
                                ,u.AuthTokenLockedDate
                                ,u.SysAdmin
                                ,u.Active
                                ,u.ARCHIVE
                                ,g.ShortName
                                ,g.Code
                                ,g.StandardName
                                ,c.PwdLockTimeDifference
                                ,c.AuthTokenLockTimeDifference
                                ,c.Active
                                ,c.ARCHIVE
                                ,c.CompanyGroupId

                                 from (select * from sec.[user] where Active=1 and ARCHIVE=0)  u
                                 left outer join (select * from hkp.CompanyGroup where Active=1 and ARCHIVE=0)  g on g.id=u.CompanyGroupId
                                 left outer join (select * from sec.CredentialPolicy where Active=1 and ARCHIVE=0)c on c.CompanyGroupId= u.CompanyGroupId and c.CompanyGroupId=g.ID";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

    }

