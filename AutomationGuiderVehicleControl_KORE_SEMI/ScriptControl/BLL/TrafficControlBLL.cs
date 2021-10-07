using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class TrafficControlBLL
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private SCApplication scApp;
        public Database dataBase { get; private set; }
        public Cache cache { get; private set; }
        public Redis redis { get; private set; }
        public TrafficControlBLL()
        {
        }
        public void start(SCApplication _app)
        {
            scApp = _app;
            cache = new Cache(scApp.getCommObjCacheManager());
            redis = new Redis(scApp.getRedisCacheManager(), scApp.SectionBLL);
        }


        public class Database
        {

        }
        public class Cache
        {
            CommObjCacheManager CommObjCacheManager = null;
            public Cache(CommObjCacheManager commObjCacheManager)
            {
                CommObjCacheManager = commObjCacheManager;
            }


        }
        public class Redis
        {
            TimeSpan timeOut_10min = new TimeSpan(0, 10, 0);

            RedisCacheManager redisCacheManager = null;
            SectionBLL sectionBLL = null;
            public Redis(RedisCacheManager _redisCacheManager, SectionBLL _sectionBLL)
            {
                redisCacheManager = _redisCacheManager;
                sectionBLL = _sectionBLL;
            }




        }



    }
}
