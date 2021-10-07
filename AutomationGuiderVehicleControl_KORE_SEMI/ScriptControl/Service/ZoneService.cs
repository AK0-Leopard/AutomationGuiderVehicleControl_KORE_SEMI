using com.mirle.ibg3k0.sc.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Service
{
    public class ZoneService
    {
        ZoneBLL zoneBLL;
        VehicleBLL vehicleBLL;



        //private List<AZONE> LoadCurrentVehicleEnoughZones()
        //{
        //    var zones = zoneBLL.cache.LoadZones();

        //    foreach (var zone in zones)
        //    {
        //        var vh_in_zone = zone.getCurrentVhs(vehicleBLL);
        //        if (vh_in_zone.Count > zone.VehicleCountLowerLimit)//如果比目前的低水位還高的話即可以前往協助其他區
        //        {

        //        }
        //    }
        //}

    }
}
