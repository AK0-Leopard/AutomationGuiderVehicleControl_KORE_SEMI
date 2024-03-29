﻿//*********************************************************************************
//      SysExcuteQualityBLL.cs
//*********************************************************************************
// File Name: SysExcuteQualityBLL.cs
// Description:
//
//(c) Copyright 2021, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
// 2023/09/01    Steven Hong    N/A            A0.01   移除不必要的Redis功能                            
//**********************************************************************************

using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class SysExcuteQualityBLL
    {
        SysExcuteQualityDao sysExeQualityDAO = null;
        private SCApplication scApp = null;

        public SysExcuteQualityBLL()
        {

        }
        public void start(SCApplication app)
        {
            scApp = app;
            sysExeQualityDAO = scApp.SysExcuteQualityDao;
        }


        //A0.01 Start
        //private ASYSEXCUTEQUALITY buildSysExcuteQualityObj(string cmd_id, string cst_id,
        //    string source, string destn,
        //    int total_act_vh, int total_idle_vh, int totla_paking_vh, int total_cycling_vh)
        //{
        //    ASYSEXCUTEQUALITY quality = new ASYSEXCUTEQUALITY()
        //    {
        //        CMD_ID_MCS = cmd_id,
        //        CST_ID = cst_id,
        //        CMD_INSERT_TIME = DateTime.Now,
        //        SOURCE_ADR = source,
        //        DESTINATION_ADR = destn,
        //        TOTAL_ACT_VH_COUNT = total_act_vh,
        //        TOTAL_IDLE_VH_COUNT = total_idle_vh,
        //        PARKING_VH_COUNT = totla_paking_vh,
        //        CYCLERUN_VH_COUNT = total_cycling_vh
        //    };
        //    return quality;
        //}
        //public bool addSysExcuteQuality(ASYSEXCUTEQUALITY quality)
        //{
        //    bool isSuccess = true;
        //    //using (DBConnection_EF con = new DBConnection_EF())
        //    //using (DBConnection_EF con = DBConnection_EF.GetUContext())
        //    //{
        //    //    sysExeQualityDAO.add(con, quality);
        //    //}
        //    sysExeQualityDAO.add(scApp.getRedisCacheManager(), quality);
        //    return isSuccess;
        //}

        //public bool updateSysExecQity_PassSecInfo(
        //    string mcs_cmd_id, string vh_id, string start_sec_id, string[] routeSec_Vh2TO, string[] routeSec_From2To)
        //{
        //    bool isSuccess = true;
        //    //using (DBConnection_EF con = DBConnection_EF.GetUContext())
        //    //{
        //    ASYSEXCUTEQUALITY quality = sysExeQualityDAO.getByID(scApp.getRedisCacheManager(), mcs_cmd_id);
        //    if (quality != null)
        //    {
        //        int sec_cnt2source = 0;
        //        int sec_dis2source = 0;
        //        int sec_cnt2destn = 0;
        //        int sec_dis2destn = 0;
        //        double ohtc_cmd_proc_time = 0;

        //        List<ASECTION> routeSec_Vh2TO_Obj = null;
        //        if (routeSec_Vh2TO != null && routeSec_Vh2TO.Length > 0)
        //            routeSec_Vh2TO_Obj = scApp.MapBLL.loadSectionBySecIDs(routeSec_Vh2TO.ToList());
        //        List<ASECTION> routeSec_From2To_Obj = null;
        //        if (routeSec_From2To != null && routeSec_From2To.Length > 0)
        //            routeSec_From2To_Obj = scApp.MapBLL.loadSectionBySecIDs(routeSec_From2To.ToList());
        //        if (routeSec_Vh2TO_Obj != null)
        //        {
        //            sec_cnt2source = routeSec_Vh2TO_Obj.Count;
        //            sec_dis2source = (int)routeSec_Vh2TO_Obj.Select(sec => sec.SEC_DIS).Sum();
        //        }
        //        if (routeSec_From2To_Obj != null)
        //        {
        //            sec_cnt2destn = routeSec_From2To_Obj.Count;
        //            sec_dis2destn = (int)routeSec_From2To_Obj.Select(sec => sec.SEC_DIS).Sum();
        //        }
        //        ohtc_cmd_proc_time = getWithNowDifferenceSeconds(quality.CMD_INSERT_TIME);

        //        quality.VH_ID = vh_id;
        //        quality.VH_START_SEC_ID = start_sec_id;
        //        quality.SEC_CNT_TO_SOURCE = sec_cnt2source;
        //        quality.SEC_DIS_TO_SOURCE = sec_dis2source;
        //        quality.SEC_CNT_TO_DESTN = sec_cnt2destn;
        //        quality.SEC_DIS_TO_DESTN = sec_dis2destn;
        //        quality.CMD_START_TIME = DateTime.Now;
        //        quality.CMDQUEUE_TIME = ohtc_cmd_proc_time;

        //        sysExeQualityDAO.update(scApp.getRedisCacheManager(), quality);
        //    }
        //    //}
        //    return isSuccess;
        //}

        //private double getWithNowDifferenceSeconds(DateTime? dateTime)
        //{
        //    double diffSec = 0;
        //    if (dateTime.HasValue)
        //    {
        //        diffSec = DateTime.Now.Subtract(dateTime.Value).TotalSeconds;

        //        diffSec = Math.Round(diffSec, 1);
        //    }
        //    return diffSec;
        //}

        //public bool updateSysExecQity_ArrivalSourcePort(string mcs_cmd_id)
        //{
        //    bool isSuccess = true;
        //    ASYSEXCUTEQUALITY quality = sysExeQualityDAO.getByID(scApp.getRedisCacheManager(), mcs_cmd_id);
        //    if (quality != null)
        //    {
        //        quality.MOVE_TO_SOURCE_TIME = getWithNowDifferenceSeconds(quality.CMD_START_TIME);
        //        sysExeQualityDAO.update(scApp.getRedisCacheManager(), quality);
        //    }
        //    else
        //    {
        //        isSuccess = false;
        //    }
        //    return isSuccess;
        //}

        //public bool updateSysExecQity_ArrivalDestnPort(string mcs_cmd_id)
        //{
        //    bool isSuccess = true;
        //    //DBConnection_EF con = DBConnection_EF.GetContext();
        //    //using (DBConnection_EF con = DBConnection_EF.GetUContext())
        //    //{
        //    ASYSEXCUTEQUALITY quality = sysExeQualityDAO.getByID(scApp.getRedisCacheManager(), mcs_cmd_id);
        //    if (quality != null)
        //    {
        //        quality.MOVE_TO_DESTN_TIME = getWithNowDifferenceSeconds(quality.CMD_START_TIME) - quality.MOVE_TO_SOURCE_TIME;
        //        sysExeQualityDAO.update(scApp.getRedisCacheManager(), quality);
        //    }
        //    else
        //    {
        //        isSuccess = false;
        //    }
        //    //}
        //    return isSuccess;
        //}
        //public bool updateSysExecQity_BlockTime2SurceOnTheWay(string mcs_cmd_id, double blockCostlyTime)
        //{
        //    bool isSuccess = true;
        //    //using (DBConnection_EF con = DBConnection_EF.GetUContext())
        //    //{
        //    ASYSEXCUTEQUALITY quality = sysExeQualityDAO.getByID(scApp.getRedisCacheManager(), mcs_cmd_id);
        //    if (quality != null)
        //    {
        //        quality.TOTAL_BLOCK_TIME_TO_SOURCE += blockCostlyTime;
        //        quality.TOTAL_BLOCK_COUNT_TO_SOURCE++;
        //        sysExeQualityDAO.update(scApp.getRedisCacheManager(), quality);
        //    }
        //    //}
        //    return isSuccess;
        //}
        //public bool updateSysExecQity_OCSTime2SurceOnTheWay(string mcs_cmd_id, double OCSCostlyTime)
        //{
        //    bool isSuccess = true;
        //    //using (DBConnection_EF con = DBConnection_EF.GetUContext())
        //    //{
        //    ASYSEXCUTEQUALITY quality = sysExeQualityDAO.getByID(scApp.getRedisCacheManager(), mcs_cmd_id);
        //    if (quality != null)
        //    {
        //        quality.TOTAL_OCS_TIME_TO_SOURCE += OCSCostlyTime;
        //        quality.TOTAL_OCS_COUNT_TO_SOURCE++;
        //        sysExeQualityDAO.update(scApp.getRedisCacheManager(), quality);
        //    }
        //    //}
        //    return isSuccess;
        //}
        //public bool updateSysExecQity_BlockTime2DestnOnTheWay(string mcs_cmd_id, double blockCostlyTime)
        //{
        //    bool isSuccess = true;
        //    //using (DBConnection_EF con = DBConnection_EF.GetUContext())
        //    //{
        //    ASYSEXCUTEQUALITY quality = sysExeQualityDAO.getByID(scApp.getRedisCacheManager(), mcs_cmd_id);
        //    if (quality != null)
        //    {
        //        quality.TOTAL_BLOCK_TIME_TO_DESTN += blockCostlyTime;
        //        quality.TOTAL_BLOCK_COUNT_TO_DESTN++;
        //        sysExeQualityDAO.update(scApp.getRedisCacheManager(), quality);
        //    }
        //    //}
        //    return isSuccess;
        //}
        //public bool updateSysExecQity_OCSTime2DestnOnTheWay(string mcs_cmd_id, double OCSCostlyTime)
        //{
        //    bool isSuccess = true;
        //    //using (DBConnection_EF con = DBConnection_EF.GetUContext())
        //    //{
        //    ASYSEXCUTEQUALITY quality = sysExeQualityDAO.getByID(scApp.getRedisCacheManager(), mcs_cmd_id);
        //    if (quality != null)
        //    {
        //        quality.TOTAL_OCS_TIME_TO_DESTN += OCSCostlyTime;
        //        quality.TOTAL_OCS_COUNT_TO_DESTN++;
        //        sysExeQualityDAO.update(scApp.getRedisCacheManager(), quality);
        //    }
        //    //}
        //    return isSuccess;
        //}
        //public bool updateSysExecQity_PauseTime(string mcs_cmd_id, double pauseCostlyTime)
        //{
        //    bool isSuccess = true;
        //    //using (DBConnection_EF con = DBConnection_EF.GetUContext())
        //    //{
        //    ASYSEXCUTEQUALITY quality = sysExeQualityDAO.getByID(scApp.getRedisCacheManager(), mcs_cmd_id);
        //    if (quality != null)
        //    {
        //        quality.TOTALPAUSE_TIME += pauseCostlyTime;
        //        sysExeQualityDAO.update(scApp.getRedisCacheManager(), quality);
        //    }
        //    //}
        //    return isSuccess;
        //}

        //public bool SysExecQityfinish(string mcs_cmd_id, CompleteStatus completeStatus, int total_cmd_dis)
        //{
        //    var cmdFinishStatus = CMDBLL.CompleteStatusToECmdStatus(completeStatus);
        //    bool is_success = updateSysExecQity_CmdFinish(mcs_cmd_id, cmdFinishStatus, completeStatus, total_cmd_dis, out ASYSEXCUTEQUALITY quality);
        //    if (quality != null)
        //    {
        //        SCUtility.TrimAllParameter(quality);
        //        LogManager.GetLogger("SysExcuteQuality").Info(quality.ToString());
        //    }
        //    return is_success;
        //}
        //private bool updateSysExecQity_CmdFinish(string mcs_cmd_id, E_CMD_STATUS cmdFinishStatus, CompleteStatus completeStatus, int total_cmd_dis, out ASYSEXCUTEQUALITY quality)
        //{
        //    bool isSuccess = true;
        //    //DBConnection_EF con = DBConnection_EF.GetContext();
        //    //using (DBConnection_EF con = new DBConnection_EF())
        //    quality = sysExeQualityDAO.getByID(scApp.getRedisCacheManager(), mcs_cmd_id);
        //    if (quality != null)
        //    {
        //        //quality.CMD_FINISH_STATUS = E_CMD_STATUS.NormalEnd;
        //        quality.CMD_FINISH_STATUS = cmdFinishStatus;
        //        quality.CMD_FINISH_TIME = DateTime.Now;
        //        quality.CMD_TOTAL_EXCUTION_TIME = getWithNowDifferenceSeconds(quality.CMD_START_TIME);
        //        quality.CompleteStatus = completeStatus;
        //        quality.CMD_TOTAL_DIS = total_cmd_dis;
        //        //sysExeQualityDAO.update(scApp.getRedisCacheManager(), quality);
        //        sysExeQualityDAO.delete(scApp.getRedisCacheManager(), quality.CMD_ID_MCS);
        //        using (DBConnection_EF con = DBConnection_EF.GetUContext())
        //        {
        //            ASYSEXCUTEQUALITY qualityTemp = sysExeQualityDAO.getByID(con, mcs_cmd_id);
        //            if (qualityTemp == null)
        //                sysExeQualityDAO.add(con, quality);
        //        }
        //    }
        //    else
        //    {

        //    }

        //    return isSuccess;
        //}


        //public Dictionary<int, double> loadSysExcuteQualityOfCMDQueueTimeByEachHourInDay(int year, int month, int day)
        //{
        //    Dictionary<int, double> dicQuality = null;
        //    using (DBConnection_EF con = new DBConnection_EF())
        //    {
        //        dicQuality = sysExeQualityDAO.loadDicByEachHourInDay(con, year, month, day);
        //    }
        //    return dicQuality;
        //}

        //public Dictionary<string, List<ARPTID>> loadDicRPTIDAndVID()
        //{
        //    Dictionary<string, List<ARPTID>> dicRptidAndVid = null;
        //    using (DBConnection_EF con = new DBConnection_EF())
        //    {

        //    }
        //    return dicRptidAndVid;
        //}
        //A0.01 End

    }
}
