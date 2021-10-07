using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using Mirle.Hlts.Utils;
using System;
using System.Text;
using System.Linq;
using NLog;
using Google.Protobuf.Collections;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System.Collections.Generic;
using Quartz.Impl.Triggers;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class ReserveBLL
    {
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private Mirle.Hlts.ReserveSection.Map.ViewModels.HltMapViewModel mapAPI { get; set; }

        private EventHandler reserveStatusChange;
        private object _reserveStatusChangeEventLock = new object();
        public event EventHandler ReserveStatusChange
        {
            add
            {
                lock (_reserveStatusChangeEventLock)
                {
                    reserveStatusChange -= value;
                    reserveStatusChange += value;
                }
            }
            remove
            {
                lock (_reserveStatusChangeEventLock)
                {
                    reserveStatusChange -= value;
                }
            }
        }

        private void onReserveStatusChange()
        {
            reserveStatusChange?.Invoke(this, EventArgs.Empty);
        }

        public ReserveBLL()
        {
        }
        public void start(SCApplication _app)
        {
            mapAPI = _app.getReserveSectionAPI();
        }

        public bool DrawAllReserveSectionInfo()
        {
            bool is_success = false;
            try
            {
                mapAPI.RedrawBitmap(false);
                mapAPI.DrawOverlapRR();
                mapAPI.RefreshBitmap();
                mapAPI.ClearOverlapRR();
                is_success = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                is_success = false;
            }
            return is_success;

        }

        public System.Windows.Media.Imaging.BitmapSource GetCurrentReserveInfoMap()
        {
            return mapAPI.MapBitmapSource;
        }

        double MAX_X = double.MinValue;
        public double GetMaxHltMapAddress_x()
        {
            if (MAX_X == double.MinValue)
                return mapAPI.HltMapAddresses.Max(adr => adr.X);
            else
                return MAX_X;
        }

        public (bool isSuccess, double x, double y, bool isTR50) GetHltMapAddress(string adrID)
        {
            var hlt_address = mapAPI.HltMapAddresses.Where(adr => SCUtility.isMatche(adr.ID, adrID)).FirstOrDefault();
            if (hlt_address == null)
                return (false, double.MinValue, double.MinValue, false);
            bool is_exist = false;
            double x = double.MaxValue;
            double y = double.MaxValue;
            bool is_tr_50 = false;
            var adr_obj = mapAPI.GetAddressObjectByID(adrID);

            if (adr_obj != null)
            {
                is_exist = true;
                x = adr_obj.X;
                y = adr_obj.Y;
                is_tr_50 = adr_obj.IsTR50;
            }

            return (is_exist, x, y, is_tr_50);
        }
        public HltResult TryAddVehicleOrUpdateResetSensorForkDir(string vhID)
        {
            var hltvh = mapAPI.HltVehicles.Where(vh => SCUtility.isMatche(vh.ID, vhID)).SingleOrDefault();
            var clone_hltvh = hltvh.DeepClone();
            clone_hltvh.SensorDirection = HltDirection.None;
            clone_hltvh.ForkDirection = HltDirection.None;
            HltResult result = mapAPI.TryAddOrUpdateVehicle(clone_hltvh);
            return result;
        }
        public HltResult TryAddVehicleOrUpdate(string vhID,
                                               string currentSectionID, double vehicleX, double vehicleY, float vehicleAngle, double speedMmPerSecond,
                                               HltDirection sensorDir, HltDirection forkDir)
        {
            LogHelper.Log(logger: logger, LogLevel: NLog.LogLevel.Debug, Class: nameof(ReserveBLL), Device: "AGV",
               Data: $"add vh in reserve system: vh:{vhID},x:{vehicleX},y:{vehicleY},angle:{vehicleAngle},speedMmPerSecond:{speedMmPerSecond},sensorDir:{sensorDir},forkDir:{forkDir}",
               VehicleID: vhID);

            var regular_result = regularAngle(vehicleAngle);
            if (!regular_result.isSuccess)
            {
                currentSectionID = null;
            }
            else
            {
                vehicleAngle = regular_result.angle;
            }
            //HltResult result = mapAPI.TryAddVehicleOrUpdate(vhID, vehicleX, vehicleY, vehicleAngle, sensorDir, forkDir);
            var hlt_vh = new HltVehicle(vhID, vehicleX, vehicleY, vehicleAngle, speedMmPerSecond, sensorDirection: sensorDir, forkDirection: forkDir, currentSectionID: currentSectionID);
            HltResult result = mapAPI.TryAddOrUpdateVehicle(hlt_vh);
            mapAPI.KeepRestSection(hlt_vh);
            onReserveStatusChange();

            return result;
        }
        private (bool isSuccess, float angle) regularAngle(float vehicleAngle)
        {
            if (vehicleAngle >= 0 && vehicleAngle < 45)
            {
                return (true, 0);
            }
            else if (vehicleAngle >= 45 && vehicleAngle < 135)
            {
                return (true, 90);
            }
            else if (vehicleAngle >= 135 && vehicleAngle < 225)
            {
                return (true, 180);
            }

            else if (vehicleAngle < 0 && vehicleAngle > -45)
            {
                return (true, 0);
            }
            else if (vehicleAngle <= -45 && vehicleAngle > -135)
            {
                return (true, -90);
            }
            else if (vehicleAngle <= -135 && vehicleAngle > -225)
            {
                return (true, -180);
            }
            else
            {
                return (false, vehicleAngle);
            }

        }
        public HltResult TryAddVehicleOrUpdate(string vhID, string adrID)
        {
            var adr_obj = mapAPI.GetAddressObjectByID(adrID);
            var hlt_vh = new HltVehicle(vhID, adr_obj.X, adr_obj.Y, 90, sensorDirection: Mirle.Hlts.Utils.HltDirection.NESW);
            //HltResult result = mapAPI.TryAddVehicleOrUpdate(vhID, adr_obj.X, adr_obj.Y, 0, vehicleSensorDirection: Mirle.Hlts.Utils.HltDirection.NESW);
            HltResult result = mapAPI.TryAddOrUpdateVehicle(hlt_vh);
            onReserveStatusChange();

            return result;
        }


        public void RemoveManyReservedSectionsByVIDSID(string vhID, string sectionID)
        {
            //int sec_id = 0;
            //int.TryParse(sectionID, out sec_id);
            string sec_id = SCUtility.Trim(sectionID);
            mapAPI.RemoveManyReservedSectionsByVIDSID(vhID, sec_id);
            onReserveStatusChange();
        }
        public List<string> loadCurrentReserveSections(string vhID)
        {
            StringBuilder sb = new StringBuilder();
            var current_reserve_sections = mapAPI.HltReservedSections;
            var vh_of_current_reserve_sections = current_reserve_sections.
                                                 Where(reserve_info => SCUtility.isMatche(reserve_info.RSVehicleID, vhID)).
                                                 Select(reserve_info => reserve_info.RSMapSectionID).
                                                 ToList();
            return vh_of_current_reserve_sections;
        }

        public void RemoveVehicle(string vhID)
        {
            var vh = mapAPI.GetVehicleObjectByID(vhID);
            if (vh != null)
            {
                mapAPI.RemoveVehicle(vh);
            }
        }

        public string GetCurrentReserveSectionString()
        {
            StringBuilder sb = new StringBuilder();
            var current_reserve_sections = mapAPI.HltReservedSections;
            sb.AppendLine("Current reserve section");
            foreach (var reserve_section in current_reserve_sections)
            {
                sb.AppendLine($"section id:{reserve_section.RSMapSectionID} vh id:{reserve_section.RSVehicleID}");
            }
            return sb.ToString();
        }

        public List<string> GetCurrentReserveSectionList(string byPassVh = null)
        {
            var current_reserve_sections = mapAPI.HltReservedSections.
                Where(reseredSection => !SCUtility.isMatche(reseredSection.RSVehicleID, byPassVh)).
                Select(reseredSection => reseredSection.RSMapSectionID).
                ToList();
            return current_reserve_sections;
        }
        public int GetCurrentReserveSectionCount(string vhID)
        {
            var current_reserve_section_count = mapAPI.HltReservedSections.
                Where(reseredSection => SCUtility.isMatche(reseredSection.RSVehicleID, vhID)).Count();
            return current_reserve_section_count;
        }
        public HltVehicle GetHltVehicle(string vhID)
        {
            return mapAPI.GetVehicleObjectByID(vhID);
        }

        public HltResult TryAddReservedSection(string vhID, string sectionID, HltDirection sensorDir = HltDirection.None, HltDirection forkDir = HltDirection.None, bool isAsk = false)
        {
            //int sec_id = 0;
            //int.TryParse(sectionID, out sec_id);
            string sec_id = SCUtility.Trim(sectionID);
            var vh_obj = mapAPI.GetVehicleObjectByID(vhID);
            var source_acc = vh_obj.SpeedMmPerSecond;

            vh_obj.SpeedMmPerSecond = 1;
            mapAPI.TryAddOrUpdateVehicle(vh_obj);

            if (isReserveAgain(vhID, sectionID))
            {
                mapAPI.RemoveManyReservedSectionsByVIDSID(vhID, sec_id);
            }
            HltResult result = mapAPI.TryAddReservedSection(vhID, sec_id, sensorDir, forkDir, isAsk);
            onReserveStatusChange();

            vh_obj.SpeedMmPerSecond = source_acc;
            mapAPI.TryAddOrUpdateVehicle(vh_obj);

            return result;
        }
        private bool isReserveAgain(string vhID, string sectionID)
        {
            var current_reserve_sections = mapAPI.HltReservedSections;
            int count = current_reserve_sections.Where(reserve => SCUtility.isMatche(reserve.Vehicle, vhID) &&
                                                                 SCUtility.isMatche(reserve.RSMapSectionID, sectionID))
                                                .Count();
            return count > 0;
        }

        public HltResult RemoveAllReservedSectionsBySectionID(string sectionID)
        {
            //int sec_id = 0;
            //int.TryParse(sectionID, out sec_id);
            string sec_id = SCUtility.Trim(sectionID);
            HltResult result = mapAPI.RemoveAllReservedSectionsBySectionID(sec_id);
            onReserveStatusChange();
            return result;

        }

        public void RemoveAllReservedSectionsByVehicleID(string vhID)
        {

            mapAPI.RemoveAllReservedSectionsByVehicleID(vhID);
            onReserveStatusChange();
        }
        public void RemoveAllReservedSections()
        {

            mapAPI.RemoveAllReservedSections();
            onReserveStatusChange();
        }

        public bool IsR2000Section(string sectionID)
        {
            var hlt_section_obj = mapAPI.HltMapSections.Where(sec => SCUtility.isMatche(sec.ID, sectionID)).FirstOrDefault();
            return SCUtility.isMatche(hlt_section_obj.Type, HtlSectionType.R2000.ToString());
        }

        enum HtlSectionType
        {
            Horizontal,
            Vertical,
            R2000
        }

        public (bool isSuccess, string reservedVhID, string reservedSecID) IsReserveSuccessTest(string vhID, RepeatedField<ReserveInfo> reserveInfos)
        {
            return IsReserveSuccessNew(vhID, reserveInfos);
        }
        public (bool isSuccess, string reservedVhID, string reservedSecID) askReserveSuccess(SectionBLL sectionBLL, string vhID, string sectionID, string addressID)
        {
            RepeatedField<ReserveInfo> reserveInfos = new RepeatedField<ReserveInfo>();
            ASECTION current_section = sectionBLL.cache.GetSection(sectionID);
            DriveDirction driveDirction = SCUtility.isMatche(current_section.FROM_ADR_ID, addressID) ?
                DriveDirction.DriveDirForward : DriveDirction.DriveDirReverse;
            ReserveInfo info = new ReserveInfo()
            {
                //DriveDirction = DriveDirction.DriveDirForward,
                DriveDirction = driveDirction,
                ReserveSectionID = sectionID
            };
            reserveInfos.Add(info);
            return IsReserveSuccessNew(vhID, reserveInfos, isAsk: true);
        }
        public (bool isSuccess, string reservedVhID, string reservedSecID) IsReserveSuccessNew(string vhID, RepeatedField<ReserveInfo> reserveInfos, bool isAsk = false)
        {
            try
            {
                if (DebugParameter.isForcedPassReserve)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(ReserveBLL), Device: "AGV",
                       Data: "test flag: Force pass reserve is open, will driect reply to vh pass",
                       VehicleID: vhID);
                    return (true, string.Empty, string.Empty);
                }

                //強制拒絕Reserve的要求
                if (DebugParameter.isForcedRejectReserve)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(ReserveBLL), Device: "AGV",
                       Data: "test flag: Force reject reserve is open, will driect reply to vh can't pass",
                       VehicleID: vhID);
                    return (false, string.Empty, string.Empty);
                }

                if (reserveInfos == null || reserveInfos.Count == 0) return (false, string.Empty, string.Empty);
                string reserve_section_id = reserveInfos[0].ReserveSectionID;

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(ReserveBLL), Device: "AGV",
                   Data: $"vh:{vhID} Try add reserve section:{reserve_section_id}...",
                   VehicleID: vhID);
                Mirle.Hlts.Utils.HltDirection hltDirection = HltDirection.None;
                var result = TryAddReservedSection(vhID, reserve_section_id,
                                                                    sensorDir: hltDirection,
                                                                    forkDir: hltDirection,
                                                                    isAsk: isAsk);

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(ReserveBLL), Device: "AGV",
                   Data: $"vh:{vhID} Try add reserve section:{reserve_section_id},result:{result.ToString()}",
                   VehicleID: vhID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(ReserveBLL), Device: "AGV",
                   Data: $"current reserve section:{GetCurrentReserveSectionString()}",
                   VehicleID: vhID);
                return (result.OK, result.VehicleID, reserve_section_id);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(ReserveBLL), Device: "AGV",
                   Data: ex,
                   Details: $"process function:{nameof(IsReserveSuccessNew)} Exception");
                return (false, string.Empty, string.Empty);
            }
        }
        //const int MAX_RESERVE_SECTION_COUNT = 3;//20210701 車輛會根據距離來取得預約，所以取消Section數量限制。
        public (bool isSuccess, string reservedVhID, string reservedFailSection, RepeatedField<ReserveInfo> reserveSuccessInfos) IsMultiReserveSuccess
                (SCApplication scApp, string vhID, RepeatedField<ReserveInfo> reserveInfos, bool isAsk = false)
        {
            try
            {
                if (DebugParameter.isForcedPassReserve)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(ReserveBLL), Device: "AGV",
                       Data: "test flag: Force pass reserve is open, will driect reply to vh pass",
                       VehicleID: vhID);
                    return (true, string.Empty, string.Empty, reserveInfos);
                }


                //強制拒絕Reserve的要求
                if (DebugParameter.isForcedRejectReserve)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(ReserveBLL), Device: "AGV",
                       Data: "test flag: Force reject reserve is open, will driect reply to vh can't pass",
                       VehicleID: vhID);
                    return (false, string.Empty, string.Empty, null);
                }
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vhID);
                if (vh.isGuideInfoRequesting)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(ReserveBLL), Device: "AGV",
                       Data: $"vh:{vhID} is guide info requesting, will driect reply to vh can't pass",
                       VehicleID: vhID);
                    return (false, string.Empty, string.Empty, null);
                }
                if (vh.isProcessingResetReservedByReservedStopOn)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(ReserveBLL), Device: "AGV",
                       Data: $"vh:{vhID} is processing reset reserved by reserved stop on, will driect reply to vh can't pass",
                       VehicleID: vhID);
                    return (false, string.Empty, string.Empty, null);
                }
                if (reserveInfos == null || reserveInfos.Count == 0) return (false, string.Empty, string.Empty, null);

                var reserve_success_section = new RepeatedField<ReserveInfo>();
                bool has_success = false;
                string final_blocked_vh_id = string.Empty;
                string reserve_fail_section = "";
                Mirle.Hlts.Utils.HltResult result = default(Mirle.Hlts.Utils.HltResult);
                int current_reserve_count = GetCurrentReserveSectionCount(vhID);

                //if (current_reserve_count >= MAX_RESERVE_SECTION_COUNT)
                //{
                //    has_success |= false;
                //    final_blocked_vh_id = "";
                //    reserve_fail_section = "";
                //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(ReserveBLL), Device: "AGV",
                //       Data: $"vh:{vhID} reserved section is over:{MAX_RESERVE_SECTION_COUNT} sections ...",
                //       VehicleID: vhID);
                //    return (has_success, final_blocked_vh_id, reserve_fail_section, reserve_success_section);
                //}

                foreach (var reserve_info in reserveInfos)
                {
                    string reserve_section_id = reserve_info.ReserveSectionID;


                    var reserve_enhance_check_result = IsReserveBlockSuccess(scApp, vhID, reserve_section_id);
                    //var reserve_enhance_check_result = IsReserveBlockSuccessNew(vh, reserve_section_id, drive_dirction);
                    if (!reserve_enhance_check_result.isSuccess)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(ReserveBLL), Device: "AGV",
                           Data: $"vh:{vhID} Try add reserve enhance section:{reserve_section_id} fail. reserved vh id:{reserve_enhance_check_result.reservedVhID}",
                           VehicleID: vhID);
                        has_success |= false;
                        final_blocked_vh_id = reserve_enhance_check_result.reservedVhID;
                        reserve_fail_section = reserve_section_id;

                        break;
                    }


                    //Mirle.Hlts.Utils.HltDirection hltDirection = Mirle.Hlts.Utils.HltDirection.None;
                    Mirle.Hlts.Utils.HltDirection hltDirection = Mirle.Hlts.Utils.HltDirection.ForwardReverse;
                    var check_result = vh.CurrentGuideData.tryGetWalkDirOnSection(reserve_section_id);
                    if (check_result.isExist)
                    {
                        switch (check_result.dir)
                        {
                            case DriveDirction.DriveDirForward:
                                hltDirection = HltDirection.Forward;
                                break;
                            case DriveDirction.DriveDirReverse:
                                hltDirection = HltDirection.Reverse;
                                break;
                        }
                    }

                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(ReserveBLL), Device: "AGV",
                       Data: $"vh:{vhID} Try add reserve section:{reserve_section_id} ,hlt dir:{hltDirection}...",
                       VehicleID: vhID);
                    result = TryAddReservedSection(vhID, reserve_section_id,
                                                   sensorDir: hltDirection,
                                                   isAsk: isAsk);


                    if (result.OK)
                    {
                        current_reserve_count++;
                        reserve_success_section.Add(reserve_info);
                        has_success |= true;
                    }
                    else
                    {
                        has_success |= false;
                        final_blocked_vh_id = result.VehicleID;
                        reserve_fail_section = reserve_section_id;
                        break;
                    }
                    //if (current_reserve_count >= MAX_RESERVE_SECTION_COUNT)
                    //{
                    //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(ReserveBLL), Device: "AGV",
                    //       Data: $"vh:{vhID} reserved section is over:{MAX_RESERVE_SECTION_COUNT} sections ...",
                    //       VehicleID: vhID);
                    //    has_success |= false;
                    //    final_blocked_vh_id = result.VehicleID;
                    //    reserve_fail_section = reserve_section_id;
                    //    break;
                    //}
                }

                return (has_success, final_blocked_vh_id, reserve_fail_section, reserve_success_section);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(ReserveBLL), Device: "AGV",
                   Data: ex,
                   Details: $"process function:{nameof(IsMultiReserveSuccess)} Exception");
                return (false, string.Empty, string.Empty, null);
            }
        }

        private (bool isSuccess, string reservedVhID) IsReserveBlockSuccess(SCApplication scApp, string vhID, string reserveSectionID)
        {
            AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vhID);
            string vh_id = vh.VEHICLE_ID;
            var block_control_check_result = scApp.getCommObjCacheManager().IsBlockControlSection(reserveSectionID);
            if (block_control_check_result.isBlockControlSec)
            {
                List<string> reserve_enhance_sections = block_control_check_result.enhanceInfo.EnhanceControlSections.ToList();
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(ReserveBLL), Device: "AGV",
                   Data: $"reserve section:{reserveSectionID} is reserve enhance section, group:{string.Join(",", reserve_enhance_sections)}",
                   VehicleID: vh_id);
                //用來判斷是否車子是否有在Block裡面，決定是否要直接讓車子通過
                bool can_direct_pass = true;
                //if (vh.IsOnAdr)
                //{
                //}
                //else
                //{
                //    string cur_sec_id = SCUtility.Trim(vh.CUR_SEC_ID, true); //需確認如何判斷在Block裡面的車子要讓她不會因Block要不到路權
                //    var current_vh_section_is_in_req_block_control_check_result =
                //        scApp.getCommObjCacheManager().IsBlockControlSection(block_control_check_result.enhanceInfo.BlockID, cur_sec_id);
                //    if (!current_vh_section_is_in_req_block_control_check_result.isBlockControlSec)
                //    {
                //        can_direct_pass = false;
                //    }
                //}
                var related_sec = scApp.SectionBLL.cache.GetSectionsByAddress(vh.CUR_ADR_ID);
                foreach (var sec in related_sec)
                {
                    var current_vh_section_is_in_req_block_control_check_result =
                        scApp.getCommObjCacheManager().IsBlockControlSection(block_control_check_result.enhanceInfo.BlockID, sec.SEC_ID);
                    if (!current_vh_section_is_in_req_block_control_check_result.isBlockControlSec)
                    {
                        can_direct_pass = false;
                        break;
                    }
                }

                if (can_direct_pass)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(ReserveBLL), Device: "AGV",
                       Data: $"reserve section:{reserveSectionID} is reserve enhance section, vh:{vh.VEHICLE_ID} is in here, direct pass.",
                       VehicleID: vh_id);

                    return (true, "");
                }



                foreach (var enhance_section in reserve_enhance_sections)
                {
                    var check_one_direct_result = scApp.ReserveBLL.TryAddReservedSection(vh_id, enhance_section,
                                                                    sensorDir: HltDirection.None,
                                                                    isAsk: true);
                    if (!check_one_direct_result.OK)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(ReserveBLL), Device: "AGV",
                           Data: $"vh:{vh_id} Try add reserve section:{reserveSectionID} , it is reserve enhance section," +
                                 $" try to reserve section:{reserveSectionID} fail,result:{check_one_direct_result}.",
                           VehicleID: vh_id);
                        return (false, check_one_direct_result.VehicleID);
                    }
                }
            }
            return (true, "");
        }

        public virtual (bool isExist, HltMapSection section) GetHltMapSections(string secID)
        {
            var sec_obj = mapAPI.HltMapSections.Where(sec => SCUtility.isMatche(sec.ID, secID)).FirstOrDefault();
            return (sec_obj != null, sec_obj);
        }


    }

}