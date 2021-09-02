using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APLOS;
using Library.HumanResource.NewAttendanceProcess;
using Library.Service.EmployeeServices;

namespace Aplos.Controllers
{
    [BasicAuthforRawData]
    public class AttdnRawDataAPIController : ApiController
    {

        AttdnRawDataUploadService app = new AttdnRawDataUploadService();

      
        public AttdnRawDataAPIController()
        {
            app = new AttdnRawDataUploadService();
        }

      
        [HttpPost]
        public string SaveDataWithEmpId([FromBody] List<AttdnRawData> DataToSave)
        {
            try
            {
                string Id = app.SaveDataWithEmpId(DataToSave); // Expecting EmpSystemId as LogDownLoadNum
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpPost]
        public string SaveDataWithCardNumber([FromBody] List<AttdnRawData> DataToSave)
        {
            try
            {
                string Id = app.SaveDataWithCardNumber(DataToSave); // Expecting Card Number as LogDownLoadNum
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }


    }
}
