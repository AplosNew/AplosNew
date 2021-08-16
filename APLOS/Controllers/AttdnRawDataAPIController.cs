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
        public string SaveData([FromBody] List<AttdnRawData> DataToSave)
        {
            try
            {
                string Id = app.SaveData(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }


    }
}
