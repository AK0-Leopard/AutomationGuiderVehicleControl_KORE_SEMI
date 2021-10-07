using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc.App;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.WebAPI
{
    public class CarrierMaint : NancyModule
    {
        SCApplication app = null;
        const string restfulContentType = "application/json; charset=utf-8";
        const string urlencodedContentType = "application/x-www-form-urlencoded; charset=utf-8";
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public CarrierMaint()
        {
            //app = SCApplication.getInstance();
            RegisterEvent();
            After += ctx => ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");

        }
        private void RegisterEvent()
        {
            Post["CarrierMaint/Install"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string vh_id = Request?.Query?.vh_id?.Value ?? Request?.Form?.vh_id?.Value ?? string.Empty;
                string loc_id = Request?.Query?.loc_id?.Value ?? Request?.Form?.loc_id?.Value ?? string.Empty;
                string cr_id = Request?.Query?.cr_id?.Value ?? Request?.Form?.cr_id?.Value ?? string.Empty;
                try
                {
                    var check_result = scApp.TransferService.ForceInstallCarrierInVehicle(vh_id, loc_id, cr_id);
                    result = check_result.isSuccess ? "OK" : check_result.result;
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
            Post["CarrierMaint/Remove"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string cr_id = Request?.Query?.cr_id?.Value ?? Request?.Form?.cr_id?.Value ?? string.Empty;
                try
                {
                    var check_result = scApp.TransferService.ForceRemoveCarrierInVehicleByOP(cr_id);
                    result = check_result.isSuccess ? "OK" : check_result.result;
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
        }
    }
}
