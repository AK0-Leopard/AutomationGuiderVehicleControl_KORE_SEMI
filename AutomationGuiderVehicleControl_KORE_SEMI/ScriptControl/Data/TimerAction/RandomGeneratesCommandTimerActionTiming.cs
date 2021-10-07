// ***********************************************************************
// Assembly         : ScriptControl
// Author           : 
// Created          : 03-31-2016
//
// Last Modified By : 
// Last Modified On : 03-24-2016
// ***********************************************************************
// <copyright file="BCSystemStatusTimer.cs" company="">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.Data.TimerAction;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.SECS;
using NLog;

namespace com.mirle.ibg3k0.sc.Data.TimerAction
{
    /// <summary>
    /// Class BCSystemStatusTimer.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.bcf.Data.TimerAction.ITimerAction" />
    public class RandomGeneratesCommandTimerActionTiming : ITimerAction
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The sc application
        /// </summary>
        protected SCApplication scApp = null;

        public Dictionary<int, List<TranTask>> dicTranTaskSchedule = null;
        public int MCS_TaskIndex = 0;//420


        /// <summary>
        /// Initializes a new instance of the <see cref="BCSystemStatusTimer"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="intervalMilliSec">The interval milli sec.</param>
        public RandomGeneratesCommandTimerActionTiming(string name, long intervalMilliSec)
            : base(name, intervalMilliSec)
        {

        }
        /// <summary>
        /// Initializes the start.
        /// </summary>
        public override void initStart()
        {
            scApp = SCApplication.getInstance();
            //dicTranTaskSchedule = scApp.CMDBLL.loadTranTaskSchedule_24Hour();

        }
        /// <summary>
        /// Timer Action的執行動作
        /// </summary>
        /// <param name="obj">The object.</param>
        private long syncPoint = 0;





        Random rnd_Index = new Random(Guid.NewGuid().GetHashCode());


        List<APORTSTATION> wait_unload_agv_station = null;

        public async override void doProcess(object obj)
        {

            if (System.Threading.Interlocked.Exchange(ref syncPoint, 1) == 0)
            {
                try
                {
                    if (!DebugParameter.CanAutoRandomGeneratesCommand)
                    {
                        agvStations = null;
                        return;
                    }

                    switch (DebugParameter.CycleRunType)
                    {
                        //case DebugParameter.CycleRunTestType.Normal:
                        //    CycleRunTest();
                        //    break;
                        case DebugParameter.CycleRunTestType.IgnorePortStatus:
                            CycleRunIgnorePortStatus();
                            break;
                        case DebugParameter.CycleRunTestType.OnlyMove:
                            //CycleRunOnlyMove();
                            CycleRunOnlyMove();
                            break;
                        case DebugParameter.CycleRunTestType.MoveBySelectPort:
                            CycleRunMoveOnlySelectedPort();
                            break;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncPoint, 0);
                }
            }
        }

        private async void CycleRunTest()
        {
            scApp.PortStationBLL.updatePortStatusByRedis();
            List<AZONE> zones = scApp.ZoneBLL.cache.LoadZones();
            foreach (var zone in zones)
            {
                Task<(bool is_success, object result)> find_idle_vh_result = findIdleForCycleVehicle();
                Task<(bool is_success, object result)> find_can_unload_agv_station_result = findCanUnloadAGVStation(zone.ZONE_ID);
                Task<(bool is_success, object result)> find_can_load_agv_station_result = findCanLoadAGVStation(zone.ZONE_ID);

                var check_results = await Task.WhenAll(find_idle_vh_result, find_can_unload_agv_station_result, find_can_load_agv_station_result);
                bool is_success = check_results.Where(result => result.is_success == false).Count() == 0;
                if (is_success)
                {
                    List<AVEHICLE> idle_vhs = check_results[0].result as List<AVEHICLE>;
                    if (idle_vhs == null || idle_vhs.Count == 0) continue;
                    AVEHICLE idle_vh = idle_vhs[0];
                    APORTSTATION can_unload_agv_station_port = check_results[1].result as APORTSTATION;
                    APORTSTATION can_load_agv_station_port = check_results[2].result as APORTSTATION;
                    if (idle_vh == null || can_unload_agv_station_port == null || can_load_agv_station_port == null) continue;

                    //foreach (var idle_vh in idle_vhs)
                    //{
                    if (scApp.GuideBLL.IsRoadWalkable(idle_vh.CUR_ADR_ID, can_load_agv_station_port.ADR_ID) &&
                        scApp.GuideBLL.IsRoadWalkable(can_load_agv_station_port.ADR_ID, can_unload_agv_station_port.ADR_ID))
                    {
                        scApp.VehicleService.Command.Loadunload(idle_vh.VEHICLE_ID,
                        can_load_agv_station_port.CassetteID,
                        can_load_agv_station_port.ADR_ID, can_unload_agv_station_port.ADR_ID,
                        can_load_agv_station_port.PORT_ID, can_unload_agv_station_port.PORT_ID);
                    }
                    else
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
                           Data: $"Can't find the path.");
                    }
                    //}
                }
            }
        }


        List<APORTSTATION> agvStations = null;
        private async void CycleRunIgnorePortStatus()
        {
            //List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadAllVh();

            //List<AZONE> zones = scApp.ZoneBLL.cache.LoadZones();
            //foreach (var zone in zones)
            //{
            Task<(bool is_success, object result)> find_idle_vh_result = findIdleForCycleLoadUnload();
            Task<(bool is_success, object result)> find_has_cst_agv_station_result = findHasCSTAndInculdCycleTestPortStation();
            Task<(bool is_success, object result)> find_no_cst_agv_station_result = findNoCSTAndInculdCycleTestPortStation();
            var check_results = await Task.WhenAll(find_idle_vh_result, find_has_cst_agv_station_result, find_no_cst_agv_station_result);
            bool is_success = check_results.Where(result => result.is_success == false).Count() == 0;
            if (is_success)
            {
                List<AVEHICLE> idle_vhs = check_results[0].result as List<AVEHICLE>;
                if (idle_vhs == null || idle_vhs.Count == 0) return;
                AVEHICLE idle_vh = idle_vhs[0];
                APORTSTATION has_cst_agv_station_port = check_results[1].result as APORTSTATION;
                APORTSTATION no_cst_agv_station_port = check_results[2].result as APORTSTATION;
                if (idle_vh == null || has_cst_agv_station_port == null || no_cst_agv_station_port == null) return;
                if (scApp.GuideBLL.IsRoadWalkable(idle_vh.CUR_ADR_ID, no_cst_agv_station_port.ADR_ID) &&
                    scApp.GuideBLL.IsRoadWalkable(no_cst_agv_station_port.ADR_ID, has_cst_agv_station_port.ADR_ID))
                {
                    scApp.VehicleService.Command.Loadunload(idle_vh.VEHICLE_ID,
                    has_cst_agv_station_port.CST_ID,
                    has_cst_agv_station_port.ADR_ID, no_cst_agv_station_port.ADR_ID,
                    has_cst_agv_station_port.PORT_ID, no_cst_agv_station_port.PORT_ID);

                    //has_cst_agv_station_port.TestTimes++;
                    no_cst_agv_station_port.TestTimes++;
                }
                else
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
                       Data: $"Can't find the path.");
                }
            }
            //}


            //foreach (AVEHICLE vh in vhs)
            //{
            //    if (vh.isTcpIpConnect &&
            //        vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote &&
            //        vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.NoCommand &&
            //        !SCUtility.isEmpty(vh.CUR_ADR_ID) &&
            //        scApp.CMDBLL.canAssignCmdNew(vh.VEHICLE_ID, E_CMD_TYPE.Move).canAssign)
            //    {
            //        //找一份目前儲位的列表
            //        if (agvStations == null || agvStations.Count == 0)
            //        {
            //            agvStations = scApp.PortStationBLL.OperateCatch.loadAGVPortStation().ToList();
            //            agvStations = agvStations.Where(port => port.IncludeCycleTest).ToList();
            //        }
            //        //如果取完還是空的 就跳出去
            //        if (agvStations == null || agvStations.Count == 0)
            //            return;
            //        APORTSTATION has_cst_port_station = agvStations.Where(port => !SCUtility.isEmpty(port.CST_ID)).FirstOrDefault();
            //        if (has_cst_port_station == null)
            //        {
            //            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
            //               Data: $"No find has cst port");
            //            return;
            //        }
            //        else
            //        {
            //            agvStations.Remove(has_cst_port_station);
            //        }
            //        //隨機找出讓車子移至下一個Port的地方
            //        int task_RandomIndex = rnd_Index.Next(agvStations.Count - 1);
            //        APORTSTATION p = agvStations[task_RandomIndex];
            //        bool isSuccess = true;
            //        if (scApp.GuideBLL.IsRoadWalkable(vh.CUR_ADR_ID, has_cst_port_station.ADR_ID) &&
            //            scApp.GuideBLL.IsRoadWalkable(has_cst_port_station.ADR_ID, p.ADR_ID))
            //        {
            //            isSuccess &= scApp.VehicleService.Command.Loadunload(vh.VEHICLE_ID,
            //                has_cst_port_station.CST_ID,
            //                has_cst_port_station.ADR_ID, p.ADR_ID,
            //                has_cst_port_station.PORT_ID, p.PORT_ID);
            //            agvStations.Remove(p);
            //        }
            //        else
            //        {
            //            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
            //               Data: $"Has path not enable");
            //        }
            //    }
            //}
        }


        private async void CycleRunMoveOnlySelectedPort()
        {
            //List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadAllVh();

            //List<AZONE> zones = scApp.ZoneBLL.cache.LoadZones();
            //foreach (var zone in zones)
            //{
            Task<(bool is_success, object result)> find_idle_vh_result = findIdleForCycleVehicle();
            Task<(bool is_success, object result)> find_port_station_result = findCycleTestPortStation();
            //Task<(bool is_success, object result)> find_no_cst_agv_station_result = findNoCSTAndInculdCycleTestPortStation();
            var check_results = await Task.WhenAll(find_idle_vh_result, find_port_station_result);
            bool is_success = check_results.Where(result => result.is_success == false).Count() == 0;
            if (is_success)
            {
                List<AVEHICLE> idle_vhs = check_results[0].result as List<AVEHICLE>;
                if (idle_vhs == null || idle_vhs.Count == 0) return;
                AVEHICLE idle_vh = idle_vhs[0];
                APORTSTATION port_station = check_results[1].result as APORTSTATION;
                //APORTSTATION no_cst_agv_station_port = check_results[2].result as APORTSTATION;
                if (idle_vh == null || port_station == null) return;
                if (scApp.GuideBLL.IsRoadWalkable(idle_vh.CUR_ADR_ID, port_station.ADR_ID))
                {
                    scApp.VehicleService.Command.Move(idle_vh.VEHICLE_ID,
                    port_station.ADR_ID);
                    port_station.TestTimes++;
                }
                else
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
                       Data: $"Can't find the path.");
                }
            }

        }

        private void CycleRunOnlyMove()
        {
            List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadAllVh();
            foreach (AVEHICLE vh in vhs)
            {
                if (vh.isTcpIpConnect &&
                    vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote &&
                    vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.NoCommand &&
                    !SCUtility.isEmpty(vh.CUR_ADR_ID) &&
                    //scApp.CMDBLL.canAssignCmdNew(vh.VEHICLE_ID, E_CMD_TYPE.Move).canAssign)
                    scApp.CMDBLL.canAssignCmdNew(vh, E_CMD_TYPE.Move).canAssign)
                {
                    //找一份目前儲位的列表
                    if (agvStations == null || agvStations.Count == 0)
                        agvStations = scApp.PortStationBLL.OperateCatch.loadAllPortStation().ToList();
                    //如果取完還是空的 就跳出去
                    if (agvStations == null || agvStations.Count == 0)
                        return;
                    //刪除目前Vh所在的port位置
                    foreach (var port_station in agvStations.ToList())
                    {
                        if (SCUtility.isMatche(port_station.ADR_ID, vh.CUR_ADR_ID))
                        {
                            agvStations.Remove(port_station);
                        }
                    }

                    //隨機找出讓車子移至下一個Port的地方
                    int task_RandomIndex = rnd_Index.Next(agvStations.Count - 1);
                    APORTSTATION p = agvStations[task_RandomIndex];
                    bool isSuccess = true;
                    if (scApp.GuideBLL.IsRoadWalkable(vh.CUR_ADR_ID, p.ADR_ID))
                    {
                        isSuccess &= scApp.VehicleService.Command.Move(vh.VEHICLE_ID, p.ADR_ID).isSuccess;
                    }
                    agvStations.Remove(p);
                }
            }
        }
        private void CycleRunOnlyMoveByZone_Old()
        {
            List<AZONE> zones = scApp.ZoneBLL.cache.LoadZones();
            foreach (var zone in zones)
            {
                (bool is_success, object result) find_idle_vh_result = findIdleForCycleVehicle().Result;
                if (!find_idle_vh_result.is_success) continue;

                AVEHICLE find_vh = find_idle_vh_result.result as AVEHICLE;
                (bool is_success, object result) find_cycle_run_port = findInculdCycleTestPort(zone.ZONE_ID, find_vh.CUR_ADR_ID).Result;
                if (!find_cycle_run_port.is_success) continue;

                bool is_success = find_idle_vh_result.is_success && find_cycle_run_port.is_success;
                if (is_success)
                {
                    //AVEHICLE idle_vh = find_idle_vh_result.result as AVEHICLE;
                    List<AVEHICLE> idle_vhs = find_idle_vh_result.result as List<AVEHICLE>;
                    APORTSTATION choose_port = find_cycle_run_port.result as APORTSTATION;
                    foreach (var idle_vh in idle_vhs)
                    {
                        if (scApp.GuideBLL.IsRoadWalkable(idle_vh.CUR_ADR_ID, choose_port.ADR_ID))
                        {
                            scApp.VehicleService.Command.Move(idle_vh.VEHICLE_ID, choose_port.ADR_ID);
                            choose_port.TestTimes++;
                            return;
                        }
                        else
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
                               Data: $"Can't find the path.");
                        }
                    }
                }
            }
        }
        private void CycleRunOnlyMoveByZone()
        {
            List<AZONE> zones = scApp.ZoneBLL.cache.LoadZones();
            foreach (var zone in zones)
            {
                (bool is_success, object result) find_idle_vh_result = findIdleForCycleVehicle().Result;
                if (!find_idle_vh_result.is_success) continue;

                bool is_success = find_idle_vh_result.is_success;
                if (is_success)
                {
                    //AVEHICLE idle_vh = find_idle_vh_result.result as AVEHICLE;
                    List<AVEHICLE> idle_vhs = find_idle_vh_result.result as List<AVEHICLE>;
                    foreach (var idle_vh in idle_vhs)
                    {
                        (bool is_success, object result) find_cycle_run_port = findInculdCycleTestPort(zone.ZONE_ID, idle_vh.CUR_ADR_ID).Result;
                        if (!find_cycle_run_port.is_success) continue;

                        APORTSTATION choose_port = find_cycle_run_port.result as APORTSTATION;

                        if (scApp.GuideBLL.IsRoadWalkable(idle_vh.CUR_ADR_ID, choose_port.ADR_ID))
                        {
                            scApp.VehicleService.Command.Move(idle_vh.VEHICLE_ID, choose_port.ADR_ID);
                            choose_port.TestTimes++;
                            return;
                        }
                        else
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
                               Data: $"Can't find the path.");
                        }
                    }
                }
            }
        }


        private Task<(bool is_success, object result)> findCanLoadAGVStation(string zoneID)
        {
            var can_load_agv_station = scApp.PortStationBLL.OperateCatch.getAGVPortStationWaitOutOK(scApp.CMDBLL, zoneID);
            if (can_load_agv_station == null)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
                   Data: $"No find can load agv station,{zoneID}");
            }
            return Task.FromResult((can_load_agv_station != null, (object)can_load_agv_station));
        }
        private Task<(bool is_success, object result)> findHasCSTAndInculdCycleTestPortStation()
        {
            //var agv_stations = scApp.PortStationBLL.OperateCatch.loadAGVPortStation(zoneID);
            var port_stations = scApp.PortStationBLL.OperateCatch.loadAllPortStation();
            port_stations = port_stations.OrderBy(p => p.TestTimes).ToList();
            APORTSTATION port_station = null;
            if (port_stations == null)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
                   Data: $"No find can any port station.");
            }
            else
            {
                foreach (var port in port_stations)
                {
                    //要找出有貨的
                    if (SCUtility.isEmpty(port.CST_ID))
                        continue;
                    if (!port.IncludeCycleTest)
                        continue;
                    if (scApp.CMDBLL.hasExcuteCMDBySourcePortOrDestinationPort(port.PORT_ID))
                        continue;
                    port_station = port;
                    break;
                }
            }
            return Task.FromResult((port_station != null, (object)port_station));
        }

        private Task<(bool is_success, object result)> findCycleTestPortStation()
        {
            //var agv_stations = scApp.PortStationBLL.OperateCatch.loadAGVPortStation(zoneID);
            var port_stations = scApp.PortStationBLL.OperateCatch.loadAllPortStation();

            APORTSTATION port_station = null;
            if (port_stations == null)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
                   Data: $"No find can any port station.");
            }
            else
            {
                port_station = port_stations.Where(station => station.IncludeCycleTest).
                                           OrderBy(station => station.TestTimes).
                                           FirstOrDefault();
            }
            return Task.FromResult((port_station != null, (object)port_station));
        }

        private Task<(bool is_success, object result)> findCanUnloadAGVStation(string zoneID)
        {
            var can_unload_agv_station = scApp.PortStationBLL.OperateCatch.getAGVPortStationCanUnloadForCycleRun(scApp.CMDBLL, zoneID);
            if (can_unload_agv_station == null)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
                   Data: $"No find can unload agv station,{zoneID}");
            }
            return Task.FromResult((can_unload_agv_station != null, (object)can_unload_agv_station));
        }
        private Task<(bool is_success, object result)> findNoCSTAndInculdCycleTestPortStation()
        {
            var port_stations = scApp.PortStationBLL.OperateCatch.loadAllPortStation();
            APORTSTATION port_station = null;
            if (port_stations == null)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
                   Data: $"No find can unload port station");
            }
            else
            {
                port_stations = port_stations.OrderBy(p => p.TestTimes).ToList();
                foreach (var port in port_stations)
                {
                    //要找出沒有貨的
                    if (!SCUtility.isEmpty(port.CST_ID))
                        continue;
                    if (!port.IncludeCycleTest)
                        continue;

                    var source_cmd = scApp.CMDBLL.getExcuteCMDBySourcePort(port.PORT_ID);
                    if (source_cmd != null)
                    {
                        var vh = scApp.VehicleBLL.cache.getVehicle(source_cmd.VH_ID);
                        if (SCUtility.isMatche(vh.CST_ID_L, source_cmd.CARRIER_ID) ||
                            SCUtility.isMatche(vh.CST_ID_R, source_cmd.CARRIER_ID))
                        {
                            //如果已經上車代表這個Port已經是可以放貨物的
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (scApp.CMDBLL.hasExcuteCMDByDestinationPort(port.PORT_ID))
                        continue;
                    port_station = port;
                    break;
                }
                //port_station = port_stations.Where(station => SCUtility.isEmpty(station.CST_ID) && station.IncludeCycleTest).
                //                           OrderBy(station => station.TestTimes).
                //                           FirstOrDefault();
            }
            return Task.FromResult((port_station != null, (object)port_station));
        }

        private Task<(bool is_success, object result)> findIdleForCycleVehicle()
        {
            List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadAllVh();
            var result = vhs.Where(vh => vh.isTcpIpConnect &&
                            !SCUtility.isEmpty(vh.CUR_ADR_ID) &&
                            //SCUtility.isMatche(vh.CUR_ZONE_ID, zoneID) &&
                            //SCUtility.isMatche(vh.getZoneID(scApp.SectionBLL), zoneID) &&
                            vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote &&
                            vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.NoCommand &&
                            //scApp.CMDBLL.canAssignCmdNew(vh.VEHICLE_ID, E_CMD_TYPE.Move).canAssign).
                            scApp.CMDBLL.canAssignCmdNew(vh, E_CMD_TYPE.Move).canAssign).
                         ToList();
            if (result == null)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
                   Data: $"Can't find idle vh.");
            }
            return Task.FromResult((result != null && result.Count != 0, (object)result));
        }
        private Task<(bool is_success, object result)> findIdleForCycleLoadUnload()
        {
            List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadAllVh();
            var result = vhs.Where(vh => vh.isTcpIpConnect &&
                            !SCUtility.isEmpty(vh.CUR_ADR_ID) &&
                            //SCUtility.isMatche(vh.CUR_ZONE_ID, zoneID) &&
                            //SCUtility.isMatche(vh.getZoneID(scApp.SectionBLL), zoneID) &&
                            vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote &&
                            //vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.NoCommand &&
                            //scApp.CMDBLL.canAssignCmdNew(vh.VEHICLE_ID, E_CMD_TYPE.Move).canAssign).
                            scApp.CMDBLL.canAssignCmdNew(vh, E_CMD_TYPE.LoadUnload).canAssign).
                         ToList();
            if (result == null)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
                   Data: $"Can't find idle vh.");
            }
            return Task.FromResult((result != null && result.Count != 0, (object)result));
        }

        private bool IsInZone(AVEHICLE vh, string zoneID)
        {
            string current_adr = SCUtility.Trim(vh.CUR_ADR_ID, true);
            if (SCUtility.isEmpty(current_adr)) return false;
            if (current_adr.StartsWith("9"))
            {
                current_adr = current_adr.Remove(0, 1);
                current_adr = "1" + current_adr;
            }
            var sections = scApp.SectionBLL.cache.GetSectionsByFromAddress(current_adr);
            //濾掉9開頭的路段
            sections = sections.
                       Where(sec => !sec.SEC_ID.StartsWith("9")).
                       ToList();
            return true;
        }

        private void updatePortStatusByRedis()
        {
            var ports_plc_info = scApp.PortStationBLL.redis.getCurrentPortsInfo();
            var port_stations = scApp.PortStationBLL.OperateCatch.loadAGVPortStation().ToList();
            foreach (var port_station in port_stations)
            {
                var port_plc_info = ports_plc_info.Where(port => SCUtility.isMatche(port.PortID, port_station.PORT_ID)).
                                                   FirstOrDefault();
                if (port_plc_info != null)
                {
                    port_station.SetPortInfo(port_plc_info);
                }
                else
                {
                    port_station.ResetPortInfo();
                }

            }
        }

        private Task<(bool is_success, object result)> findInculdCycleTestPort(string zoneID, string byPassAdrID)
        {
            var all_port_in_zone = scApp.PortStationBLL.OperateCatch.loadAllPortStation(zoneID);
            APORTSTATION port = null;
            if (all_port_in_zone == null)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(RandomGeneratesCommandTimerActionTiming), Device: string.Empty,
                   Data: $"No find any port,{zoneID}");
            }
            else
            {
                port = all_port_in_zone.Where(station => station.IncludeCycleTest &&
                                                                !SCUtility.isMatche(station.ADR_ID, byPassAdrID)).
                                        OrderBy(station => station.TestTimes).
                                        FirstOrDefault();
            }
            return Task.FromResult((port != null, (object)port));
        }

    }

}

