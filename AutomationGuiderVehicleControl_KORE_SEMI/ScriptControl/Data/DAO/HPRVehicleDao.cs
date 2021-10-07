// ***********************************************************************
// Assembly         : ScriptControl
// Author           : 
// Created          : 03-31-2016
//
// Last Modified By : 
// Last Modified On : 03-24-2016
// ***********************************************************************
// <copyright file="ParkAdrDao.cs" company="">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.Data;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.VO;
using NLog;
using com.mirle.ibg3k0.bcf.Common;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    /// <summary>
    /// Class ParkAdrDao.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.bcf.Data.DaoBase" />
    public class HPRVehicleDao : DaoBase
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();



        public bool isHPRVehicle(string vhid)//查詢該AGV是否在config中設定為HPR專用車
        {
            try
            {
                DataTable dt = SCApplication.getInstance().OHxCConfig.Tables["HPRVEHICLE"];
                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("VEHICLEID").Trim() == vhid.Trim()
                            select new HPRVehicle
                            {
                    VehicleID = c.Field<string>("VEHICLEID"),
                                isHPRVehicle = (c.Field<string>("ISHPRVEHICLE") == "Y")
                            };
                HPRVehicle hPRVehicle = query.SingleOrDefault();
                if (hPRVehicle == null)
                {
                    return false;
                }
                else
                {
                    return hPRVehicle.isHPRVehicle;
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        //public string VehicleID;
        //public bool isHPRVehicle;


    }
}
