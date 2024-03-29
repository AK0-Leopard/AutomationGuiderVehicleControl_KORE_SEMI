﻿using com.mirle.ibg3k0.bcf.App;
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

namespace com.mirle.ibg3k0.sc
{
    public partial class AZONE : BaseEQObject, IAlarmHisList
    {
        public int VehicleCountLowerLimit { get; private set; } = 0;
        public List<string> BorderSections { get; private set; } = null;

        public List<string> getBorderPassAdress(BLL.SectionBLL sectionBLL)
        {
            List<string> border_pass_addrss = new List<string>();
            //如果Section是在同一Zone的話代表就是邊界的入口了
            foreach (string border_section_id in BorderSections)
            {
                var section = sectionBLL.cache.GetSection(border_section_id);
                var connection_sections = sectionBLL.cache.GetSectionsByAddress(section.REAL_FROM_ADR_ID);
                if (connection_sections.
                    Where(sec => !sc.Common.SCUtility.isMatche(sec.ZONE_ID, this.ZONE_ID)).Count() == 0)
                {
                    border_pass_addrss.Add(section.REAL_FROM_ADR_ID);
                }
                else
                {
                    border_pass_addrss.Add(section.REAL_TO_ADR_ID);
                }
            }
            return border_pass_addrss;
        }

        public void setVehicleCountLowerLimit(int lowerLimit)
        {
            VehicleCountLowerLimit = lowerLimit;
        }
        public void setBorderSections(List<string> borderSections)
        {
            BorderSections = borderSections;
        }

        private AlarmHisList alarmHisList = new AlarmHisList();
        public override void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            foreach (IValueDefMapAction action in valueDefMapActionDic.Values)
            {
                action.doShareMemoryInit(runLevel);
            }
            //對sub eqpt進行初始化
            List<ANODE> subNodeList = SCApplication.getInstance().getEQObjCacheManager().getNodeListByZone(ZONE_ID);
            if (subNodeList != null)
            {
                foreach (ANODE node in subNodeList)
                {
                    node.doShareMemoryInit(runLevel);
                }
            }
        }

        public virtual void resetAlarmHis(List<ALARM> AlarmHisList)
        {
            alarmHisList.resetAlarmHis(AlarmHisList);
        }



    }

}
