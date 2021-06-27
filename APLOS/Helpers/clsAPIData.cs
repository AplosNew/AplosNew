
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebApi;

namespace ClientDataExchange
{
    public class clsAPIData
    {
        public clsAPIData()
        {

            //clsApplicationConfig.BaseLink = baselink;
        }
        //public readonly string PingAPI = "api/BiometricAccess/GetDeviceLog?plantid={0}&DeviceID={1}";
        public readonly string PingAPI = "api/AplosConnectivity/AplosPing";
        public readonly string APIAplosAuthentication = "api/AplosConnectivity/AplosAuthentication";


        public List<Dictionary<string, object>> GetMenuListForSync()
        {
            //string API =  "api/BiometricAccess/GetDeviceLog?plantid=" + PlantID + "&DeviceID=" + DeviceID;
           
            string clientUrl = "";

            string MainAPI = clientUrl + PingAPI;
            DataSet dsRef = new System.Data.DataSet();
            try
            {
                clsWebApi webApi = new clsWebApi(MainAPI);

                List<Dictionary<string, object>> data = webApi.GetMessage<Dictionary<string, object>>("");



                return data;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
            
        }//End Function
     
        public void ClearDeviceLog(string plantID, string deviceIP)
        {


            //string API = clsApplicationConfig.BaseLink + "api/BiometricAccess/ClearDeviceLog?plantID=" + plantID + "&deviceIP=" + deviceIP;
            DataSet dsRef = new System.Data.DataSet();
            try
            {
                //clsWebApi webApi = new clsWebApi(API);
                //webApi.DeleteMessage<string>("");

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public async void SaveDataSetsForEmp(string plantID, string DeviceID, DataSet dsData)
        {


            //List<AccessControllerEmployeeTag> DataToSave = new List<AccessControllerEmployeeTag>();
            //foreach (DataRow dr in dsData.Tables[0].Rows)
            //{
            //    DataToSave.Add(new AccessControllerEmployeeTag
            //    {

            //        Id = dr["Id"].ToString(),
            //        GroupID = dr["GroupID"].ToString(),
            //        PlantID = dr["PlantID"].ToString(),
            //        EmpInfoSystemID = dr["EmpInfoSystemID"].ToString(),
            //        DeviceSystemID = dr["DeviceSystemID"].ToString(),
            //        RegisterStatus = dr["RegisterStatus"].ToString(),
            //        AddedBy = loginInfo.UserID

            //    });

            //}


            //string API = clsApplicationConfig.BaseLink + "api/BiometricAccess/SaveDataSetsForEmp";
            DataSet dsRef = new System.Data.DataSet();
            try
            {
                //clsWebApi webApi = new clsWebApi(API);
                //await webApi.PostMessageWithResponse("", DataToSave, "");

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public  void SaveDictionaryFromAPI(List<Dictionary<string, object>> DataToSave, string clientUrl)
        {
            string MainAPI = clientUrl + APIAplosAuthentication;
            DataSet dsRef = new System.Data.DataSet();
            try
            {
                clsWebApi webApi = new clsWebApi(MainAPI);
                webApi.PostMessageWithResponse("", DataToSave, "");

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

    }
}
