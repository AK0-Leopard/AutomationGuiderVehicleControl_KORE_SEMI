using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class BlockControlBLL
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        SCApplication scApp = null;
        BlockZoneMasterDao blockZoneMasterDao = null;
        BlockZoneDetailDao blockZoneDetaiDao = null;
        BlockZoneQueueDao blockZoneQueueDao = null;

        public void start(SCApplication app)
        {
            scApp = app;
            blockZoneMasterDao = scApp.BlockZoneMasterDao;
            blockZoneDetaiDao = scApp.BolckZoneDetaiDao;
            blockZoneQueueDao = scApp.BlockZoneQueueDao;
        }


    }
}
