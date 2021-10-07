using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.Data.VO.Interface;
using com.mirle.ibg3k0.sc.ObjectRelay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage
{
    [Serializable]
    public partial class GuideInfo
    {
        public sc.AVEHICLE.GuideData converToGuideData(sc.BLL.ReserveBLL reserveBLL)
        {
            var vh_guide_data = 
                new AVEHICLE.GuideData(reserveBLL, this.GuideSections.ToList(), this.GuideAddresses.ToList());
            return vh_guide_data;
        }

    }

}
