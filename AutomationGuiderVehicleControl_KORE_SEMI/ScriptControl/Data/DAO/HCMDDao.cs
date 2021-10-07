using com.mirle.ibg3k0.sc.Data.SECS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    public class HCMDDao
    {
        public void AddByBatch(DBConnection_EF con, List<HCMD> hcmds)
        {
            con.HCMD.AddRange(hcmds);
            con.SaveChanges();
        }

        public List<HCMD> loadByInsertPeriod(DBConnection_EF con, DateTime dtStart, DateTime dtEnd)
        {
            var query = from cmd in con.HCMD
                        where (cmd.CMD_INSER_TIME > dtStart && cmd.CMD_INSER_TIME < dtEnd)
                           || (cmd.CMD_INSER_TIME > dtEnd && cmd.CMD_INSER_TIME < dtStart)
                        orderby cmd.CMD_INSER_TIME descending
                        select cmd;
            return query.ToList();
        }
    }

}
