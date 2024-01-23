using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace com.mirle.ibg3k0.bc.winform.UI
{
    public partial class PrioritySetting : Form
    {
        #region 全域變數
        BCMainForm mainform;
        List<APORTSTATION> portList = new List<APORTSTATION>();
        SCApplication scApp;
        #endregion 全域變數

        #region 建構子
        public PrioritySetting(BCMainForm _mainForm)
        {
            InitializeComponent();

            dgv_PortList.AutoGenerateColumns = false;

            mainform = _mainForm;

            scApp = _mainForm.BCApp.SCApplication;

            loadAllPort();

            loadIntervalTime();
        }
        #endregion 建構子

        #region 取得所有Port資料
        private void loadAllPort()
        {
            try
            {
                portList = scApp.PortStationBLL.OperateCatch.loadAllPortStation();

                dgv_PortList.DataSource = null;

                if (portList != null && portList.Count > 0)
                {
                    dgv_PortList.DataSource = portList;
                    lb_PortListQty.Text = portList.Count.ToString();
                }
                else
                {
                    lb_PortListQty.Text = "0";
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "Exception");
            }
        }
        #endregion 取得所有Port資料

        #region 取得Interval Time資料
        private void loadIntervalTime()
        {
            try
            {
                AECDATAMAP IntervalTime = scApp.LineBLL.getECData(SCAppConstants.ECID_PRIORITY_ADD_INTERVAL);

                if (IntervalTime != null)
                {
                    lb_IntervalTime.Text = IntervalTime.ECV.Trim();
                }

                AECDATAMAP Variable = scApp.LineBLL.getECData(SCAppConstants.ECID_PRIORITY_ADD_CNT);
                if (Variable != null)
                {
                    lb_Variable.Text = Variable.ECV.Trim();
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "Exception");
            }
        }
        #endregion 取得Interval Time資料

        #region 儲存Priority Setting按鈕事件
        private void btn_PortPrioritySetting_Click(object sender, EventArgs e)
        {
            try
            {
                string sPortID = string.Empty;
                int sPriority = 0;

                if (!SCUtility.isEmpty(lb_PortPriority.Text))
                {
                    sPortID = lb_PortID.Text;
                }
                else
                {
                    sPortID = string.Empty;
                    return;
                }

                if (!SCUtility.isEmpty(newPriorityValue.Text))
                {
                    sPriority = Convert.ToInt32(newPriorityValue.Text);
                }
                else
                {
                    sPriority = 0;
                    return;
                }

                bool result = scApp.PortStationService.doUpdatePortStationPriority(sPortID, sPriority);

                MessageBox.Show(result.ToString(), "Port Priority Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);

                loadAllPort();

                lb_PortID.Text = string.Empty;
                lb_PortPriority.Text = string.Empty;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "Exception");
            }
        }
        #endregion 儲存Priority Setting按鈕事件

        #region 儲存Interval Time與Variable按鈕事件
        private void btn_SaveIntervalTimeAndVariable_Click(object sender, EventArgs e)
        {
            try
            {
                AECDATAMAP _intervalTime = scApp.LineBLL.getECData(SCAppConstants.ECID_PRIORITY_ADD_INTERVAL);
                string sResult1 = string.Empty;
                bool bResult1 = false;
                if (_intervalTime != null)
                {
                    _intervalTime.ECV = newIntervalTime.Value.ToString();
                    bResult1 = scApp.LineBLL.updatePriority(_intervalTime, out sResult1);
                }

                AECDATAMAP _variable = scApp.LineBLL.getECData(SCAppConstants.ECID_PRIORITY_ADD_CNT);
                string sResult2 = string.Empty;
                bool bResult2 = false;

                if (_variable != null)
                {
                    _variable.ECV = newVariable.Value.ToString();
                    bResult2 = scApp.LineBLL.updatePriority(_variable, out sResult2);
                }

                loadIntervalTime();
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "Exception");
            }
        }
        #endregion 儲存Interval Time與Variable按鈕事件

        #region 關閉按鈕事件
        private void btn_Close_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "Exception");
            }
        }
        private void btn_Close2_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "Exception");
            }
        }
        #endregion 關閉按鈕事件

        #region 點選Port List事件
        private void dgv_PortList_Click(object sender, EventArgs e)
        {
            try
            {
                APORTSTATION selectUserGroup = getSelectedRowToTextBox();
                if (selectUserGroup == null)
                {
                    return;
                }
                else
                {
                    lb_PortID.Text = selectUserGroup.PORT_ID.Trim();
                    lb_PortPriority.Text = selectUserGroup.PRIORITY.ToString();
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "Exception");
            }
        }
        private APORTSTATION getSelectedRowToTextBox()
        {
            int selectedRowCnt = dgv_PortList.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if (selectedRowCnt <= 0)
            {
                return null;
            }
            int selectedIndex = dgv_PortList.SelectedRows[0].Index;
            if (portList.Count <= selectedIndex)
            {
                return null;
            }
            APORTSTATION selectUserGroup = portList[selectedIndex];
            return selectUserGroup;
        }
        #endregion 點選Port List事件

        #region 介面關閉
        private void TransferCommandQureyListForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            mainform.removeForm(this.Name);
        }
        #endregion 介面關閉
    }
}
