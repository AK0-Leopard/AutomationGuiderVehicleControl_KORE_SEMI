//------------------------------------------------------------------------------
// <auto-generated>
//    這個程式碼是由範本產生。
//
//    對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//    如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace com.mirle.ibg3k0.sc
{
    using System;
    using System.Collections.Generic;
    
    public partial class HTRANSFER
    {
        public string ID { get; set; }
        public string CARRIER_ID { get; set; }
        public string LOT_ID { get; set; }
        public E_TRAN_STATUS TRANSFERSTATE { get; set; }
        public int COMMANDSTATE { get; set; }
        public string HOSTSOURCE { get; set; }
        public string HOSTDESTINATION { get; set; }
        public int PRIORITY { get; set; }
        public string CHECKCODE { get; set; }
        public string PAUSEFLAG { get; set; }
        public System.DateTime CMD_INSER_TIME { get; set; }
        public Nullable<System.DateTime> CMD_START_TIME { get; set; }
        public Nullable<System.DateTime> CMD_FINISH_TIME { get; set; }
        public int TIME_PRIORITY { get; set; }
        public int PORT_PRIORITY { get; set; }
        public int REPLACE { get; set; }
        public int PRIORITY_SUM { get; set; }
        public string EXCUTE_CMD_ID { get; set; }
        public string RESULT_CODE { get; set; }
    }
}
