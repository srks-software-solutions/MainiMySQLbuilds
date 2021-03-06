using System.Data.SqlClient;
using SRKSDAQFanuc.ServerModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SRKSDAQFanuc
{
    public partial class Service1 : ServiceBase
    {
        private ushort port;
        private int timeout;
        private MySqlConnection conn;
        private DateTime PrvDT;
        private DateTime PrvOpRun;
        private int counter5;
        private DateTime PrvParaEntryTime = System.DateTime.Now.Date;

        public Service1()
        {
            InitializeComponent();
            MSqlConnection mc = new MSqlConnection();
            conn = mc.MqlConnection;
            port = 3306;
            timeout = 2;
        }
        protected override void OnStart(string[] args)
        {
            System.Timers.Timer T1 = new System.Timers.Timer();
            T1.Interval = (60000);
            T1.AutoReset = true;
            T1.Enabled = true;
            T1.Elapsed += new System.Timers.ElapsedEventHandler(insertdb);
        }

        protected override void OnStop()
        {
            //MsqlConnection mc = new MsqlConnection();
            //mc.open();
            //MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason) VALUES('SRKS DataLogging Service was by stopped','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error')");
            //cmd.ExecuteNonQuery();
            //mc.close();
            InsertOperationLogDetails("SRKS DataLogging Service was by stopped ", System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "M/c Error", 0);
        }

        protected override void OnShutdown()
        {
            //MsqlConnection mc = new MsqlConnection();
            //mc.open();
            //MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason) VALUES('SRKS DataLogging Service was by stopped because of System ShutDown','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error')");
            //cmd.ExecuteNonQuery();
            //mc.close();
            InsertOperationLogDetails("SRKS DataLogging Service was by stopped because of System ShutDown ", System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "M/c Error", 0);
            base.OnShutdown();
        }

        private void insertdb(object sender, System.Timers.ElapsedEventArgs e)
        {
            //DemoEndTime = new DateTime(2017, 06, 30);
            DateTime CurDateTime = System.DateTime.Now.Date;
            //if (CurDateTime <= DemoEndTime)
            {
                //For Cell 1 Machines to Verify the 100MBPS Connection
                //DataTable dt = new DataTable();
                //using (MsqlConnection con1 = new MsqlConnection())
                //{
                //    con1.open();
                //    String query = "SELECT IPAddress,MachineType,MachineID,IsParameters,CurrentControlAxis,MachineLockBit, MachineSetupBit, MachineMaintBit, MachineToolLifeBit, MachineUnlockBit, MachineIdleBit, MachineIdleMin, EnableLockLogic From i_facility.unitworkccs_tblmachinedetailss WHERE IsDeleted = 0 and MachineModelType = 1 order by MachineID";
                //    SqlDataAdapter da = new SqlDataAdapter(query, con1.msqlConnection);
                //    da.Fill(dt);
                //}
                var t = new List<unitworkccs_tblmachinedetails>();
                using (unitworksccsEntities db = new unitworksccsEntities())
                {
                    t = db.unitworkccs_tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineModelType == 1).ToList();
                }
                int count = t.Count;
                IntoFile("Machines Count:" + count);
                CountdownEvent cntevent = new CountdownEvent(count);
                //string Shift = "";
                int shiftid = 1;
                var shiftmasterdet = new unitworkccs_shift_master();
                try
                {
                    shiftid = GetShift();

                    //shiftid = Convert.ToInt32(Shift);
                    IntoFile("ShiftID:" + shiftid);
                    using (unitworksccsEntities db = new unitworksccsEntities())
                    {
                        shiftmasterdet = db.unitworkccs_shift_master.Find(shiftid);
                    }
                }
                catch(Exception ex)
                {
                    IntoFile(ex.ToString());
                }

                for (int j = 0; j < count; j++)
                {
                    string ip = Convert.ToString(t[j].IPAddress);
                    int type = Convert.ToInt32(t[j].MachineType);
                    int mcid = Convert.ToInt32(t[j].MachineID);
                    int ParameterExcep = Convert.ToInt32(t[j].IsParameters);
                    int NoOfAxis = Convert.ToInt32(t[j].CurrentControlAxis);
                    int MacLockbit = Convert.ToInt32(t[j].MachineLockBit);
                    int MacSetupbit = Convert.ToInt32(t[j].MachineSetupBit);
                    int MacMaintbit = Convert.ToInt32(t[j].MachineSetupBit);
                    int MacToolLifebit = Convert.ToInt32(t[j].MachineToolLifeBit);
                    int MacUnlockbit = Convert.ToInt32(t[j].MachineUnlockBit);
                    int MacIdlebit = Convert.ToInt32(t[j].MachineIdleBit);
                    int MacIdleMin = Convert.ToInt32(t[j].MachineIdleMin);
                    int EnableLock = Convert.ToInt32(t[j].EnableLockLogic);
                    int IsShiftWise = Convert.ToInt32(t[j].IsShiftWise);

                    Task.Factory.StartNew(() =>
                    {
                        //Parameters for Functions For Focas Libraries
                        ushort h; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                        short ret; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                        short freeret;

                        //using (MsqlConnection mconn = new MsqlConnection())
                        //{
                        //    mconn.open();
                        //    String getparametersquery = "SELECT InsertedOn From  unitworkccs_parameters_master WHERE MachineID = " + mcid + " order by ParameterID DESC";
                        //    SqlDataAdapter daConn = new SqlDataAdapter(getparametersquery, mconn.sqlConnection);
                        //    DataTable dtConn = new DataTable();
                        //    daConn.Fill(dtConn);
                        //    PrvDT = Convert.ToDateTime(dtConn.Rows[0][0]);
                        //    mconn.close();
                        //}
                        //if (DateTime.Now.Subtract(PrvDT).TotalSeconds >= 60.0)
                        {
                            //PrvDT = DateTime.Now;
                            Focas1.IODBPSD_2 cuttim = new Focas1.IODBPSD_2(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                            Focas1.IODBPSD_2 opttim = new Focas1.IODBPSD_2(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                            Focas1.IODBPSD_2 pot = new Focas1.IODBPSD_2(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                            Focas1.IODBPSD_2 parcout = new Focas1.IODBPSD_2(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                            Focas1.IODBPSD_2 partot = new Focas1.IODBPSD_2(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                            Focas1.ODBUP prodat = new Focas1.ODBUP(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM

                            int stopophisret = 0, startophisret = 0, startophisret1 = 0;
                            int almhisret;
                            int ctti, opti, potret, parcouret, partotret;
                            short cuttt = 6754;
                            short opttt = 6752;
                            if (ParameterExcep == 1)
                            {
                                opttt = 6756;
                            }
                            short powerontime = 6750;
                            short partscount = 6711;
                            short partstotal = 6712;

                            //Data Logging Functions using Focas Libraries
                            ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
                            int ConnectionRet = ret;
                            Focas1.ODBERR ErrNo = new Focas1.ODBERR();
                            short retval = Focas1.cnc_getdtailerr(h, ErrNo);
                            int ConnectionRetErr = ErrNo.err_no;
                            IntoFile("ConnectionRetErr:" + ret);
                            if (ret == 0)
                            {
                                #region Parameters and Program
                                if (ret == 0)
                                {
                                    #region Parameters Collection
                                    //Parameters Collection
                                    ctti = Focas1.cnc_rdparam(h, cuttt, 0, 4 + 8 * Focas1.MAX_AXIS, cuttim); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                                    opti = Focas1.cnc_rdparam(h, opttt, 0, 4 + 8 * Focas1.MAX_AXIS, opttim); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                                    potret = Focas1.cnc_rdparam(h, powerontime, 0, 4 + 8 * Focas1.MAX_AXIS, pot); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                                    parcouret = Focas1.cnc_rdparam(h, partscount, 0, 4 + 8 * Focas1.MAX_AXIS, parcout); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                                    partotret = Focas1.cnc_rdparam(h, partstotal, 0, 4 + 8 * Focas1.MAX_AXIS, partot); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                                    DateTime insertedon = System.DateTime.Now;

                                    //Operating time and Cutting Time
                                    Double operatingtime = Convert.ToDouble(opttim.rdata.prm_val.ToString() + "." + opttim.rdata.dec_val.ToString());
                                    Double cuttingtime = Convert.ToDouble(cuttim.rdata.prm_val.ToString() + "." + cuttim.rdata.dec_val.ToString());
                                    Double powertime = Convert.ToDouble(pot.rdata.prm_val.ToString() + "." + pot.rdata.dec_val.ToString());
                                    Double partcount = Convert.ToDouble(parcout.rdata.prm_val.ToString() + "." + parcout.rdata.dec_val.ToString());
                                    Double parttotal = Convert.ToDouble(partot.rdata.prm_val.ToString() + "." + partot.rdata.dec_val.ToString());


                                    if(parttotal==0)
                                    {
                                        var parameter = new unitworkccs_parameters_master();
                                        using (unitworksccsEntities db = new unitworksccsEntities())
                                        {
                                            parameter = db.unitworkccs_parameters_master.Where(m => m.MachineID == mcid && m.PartsTotal!=0).OrderByDescending(m => m.ParameterID).FirstOrDefault();
                                        }
                                        parttotal = Convert.ToDouble(parameter.PartsTotal);
                                    }

                                    int shift = shiftid;
                                    String CorrectedDate = GetCorrectedDate().ToString("yyyy-MM-dd");
                                    IntoFile("CorrectedDate:" + CorrectedDate);
                                    //if ((operatingtime != 0 && powertime != 0) || ParameterExcep == 1)
                                    if (operatingtime != 0 && powertime != 0)
                                    {
                                        if (cuttingtime != 0)
                                        {
                                            //using (MsqlConnection sa = new MsqlConnection())
                                            //{
                                            //    sa.open();
                                            //    MySqlCommand cmd2 = new MySqlCommand("INSERT INTO  i_facility.unitworkccs_parameters_master(CuttingTime,OperatingTime,InsertedOn, PowerOnTime, PartsCount, PartsTotal,MachineID, Shift, CorrectedDate) VALUES('" + cuttingtime + "','" + operatingtime + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + powertime + "','" + partcount + "','" + parttotal + "'," + mcid + "," + shift + ",'" + CorrectedDate + "')", sa.msqlConnection);
                                            //    cmd2.ExecuteNonQuery();
                                            //    sa.close();
                                            //}
                                            DateTime date = System.DateTime.Now;
                                            string dt1 = date.ToString("yyyy-MM-dd HH:mm:ss");
                                            DateTime dtnew = DateTime.Parse(dt1);
                                            try
                                            {
                                                unitworkccs_parameters_master obj = new unitworkccs_parameters_master();
                                                obj.CuttingTime = Convert.ToString(cuttingtime);
                                                obj.OperatingTime = Convert.ToString(operatingtime);
                                                obj.InsertedOn = dtnew;
                                                obj.PowerOnTime = powertime.ToString();
                                                obj.PartsCount = partcount;
                                                obj.PartsTotal = Convert.ToInt32(parttotal);
                                                obj.MachineID = mcid;
                                                obj.Shift = shift;
                                                obj.CorrectedDate = Convert.ToDateTime(CorrectedDate);
                                                using (unitworksccsEntities db = new unitworksccsEntities())
                                                {
                                                    db.unitworkccs_parameters_master.Add(obj);
                                                    db.SaveChanges();
                                                    IntoFile("unitworkccs_parameters_master Insert Done");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                IntoFile("unitworkccs_parameters_master Insert Exception:" + ex.ToString());
                                            }
                                            PrvParaEntryTime = System.DateTime.Now;
                                        }
                                    }
                                    else
                                    {
                                        //using (MsqlConnection sa = new MsqlConnection())
                                        //{
                                        //    sa.open();
                                        //    MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Parameters Cant be logged, " + ctti + " Return Code from M/c.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Parameter Error'," + mcid + ")", sa.msqlConnection);
                                        //    cmd.ExecuteNonQuery();
                                        //    sa.close();
                                        //}
                                        InsertOperationLogDetails("Parameters Cant be logged, " + ctti + " Return Code from M/c.", System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "Parameter Error", mcid);
                                    }

                                    #endregion
                                    //progret = programfilter1(h, mcid);

                                    //if (progret == 4 || progret == 4)
                                    //{
                                    //    mc1.close();
                                    //    mc1.open();
                                    //    MySqlCommand cmd = new MySqlCommand("INSERT INTO i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('NC Program Cant be read, due to upload error from M/c.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error'," + mcid + ")", mc1.msqlConnection);
                                    //    cmd.ExecuteNonQuery();
                                    //    mc1.close();
                                    //}
                                    //freeret = Focas1.cnc_freelibhndl(h);
                                }
                                else
                                {
                                    //using (MsqlConnection sa = new MsqlConnection())
                                    //{
                                    //    sa.open();
                                    //    MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Connection to M/c Problem. Please check the Ethernet Connection and Power Connection of the CNC Machine.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error'," + mcid + ")", sa.msqlConnection);
                                    //    cmd.ExecuteNonQuery();
                                    //    sa.close();
                                    //}
                                    InsertOperationLogDetails("Connection to M/c Problem. Please check the Ethernet Connection and Power Connection of the CNC Machine.", System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "M/c Error", mcid);
                                }
                            }
                            #endregion
                            double diff = System.DateTime.Now.Subtract(PrvParaEntryTime).TotalMinutes;
                            if (ret == 0 || (ret != 0 && diff > 2))
                            {
                                try
                                {
                                    if (IsShiftWise == 1)
                                        getmachinemode(mcid, ip, h, MacLockbit, MacIdlebit, MacUnlockbit, MacIdleMin, ConnectionRet, ConnectionRetErr, EnableLock, shiftmasterdet);
                                    else
                                        getmachinemodeDayWise(mcid, ip, h, MacLockbit, MacIdlebit, MacUnlockbit, MacIdleMin, ConnectionRet, ConnectionRetErr, EnableLock);
                                }
                                catch (Exception ex)
                                {
                                    //using (MsqlConnection SA = new MsqlConnection())
                                    //{
                                    //    SA.open();
                                    //    MySqlCommand cmd = new MySqlCommand("INSERT INTO i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('" + ex.ToString() + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error'," + mcid + ")", SA.msqlConnection);
                                    //    cmd.ExecuteNonQuery();
                                    //    SA.close();
                                    //}

                                    InsertOperationLogDetails(ex.ToString(), System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "M/c Error", mcid);
                                }
                            }
                            //else if(ret != 0 && diff > 2)
                            //{
                            //    getmachinemode(mcid, ip, h, MacLockbit, MacIdlebit, MacUnlockbit, MacIdleMin, ConnectionRet, ConnectionRetErr, EnableLock);
                            //}
                            #region commented by Ashok
                            // updatemimics(mcid);  
                            #endregion
                            //Program Insert Check
                            //ProgramInsertCheck(h, mcid);

                            //Insert Axis Details
                            //InsertAxisDetails(h, mcid, NoOfAxis);
                            //Insert Servo Details
                            //InsertServoDetails(h, mcid, NoOfAxis);
                            freeret = Focas1.cnc_freelibhndl(h);
                            //Getting Link to the Particular CNC Machine
                            ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                            DateTime CurTime = System.DateTime.Now;
                            double diffAlarm = CurTime.Subtract(PrvOpRun).TotalMinutes;
                            if (counter5 == -1 || diffAlarm > 5)
                            {
                                PrvOpRun = System.DateTime.Now;
                                counter5++;
                                #region Message History And Alarm History
                                if (ret == 0)
                                {
                                    DataTable dtOpMsg = new DataTable();
                                    dtOpMsg.Columns.Add("Meassage", typeof(string));
                                    dtOpMsg.Columns.Add("MessageDate", typeof(string));
                                    dtOpMsg.Columns.Add("MessageTime", typeof(string));
                                    dtOpMsg.Columns.Add("MessageDateTime", typeof(string));
                                    dtOpMsg.Columns.Add("MessageNo", typeof(string));
                                    dtOpMsg.Columns.Add("InsertedOn", typeof(string));
                                    dtOpMsg.Columns.Add("MachineID", typeof(int));
                                    dtOpMsg.Columns.Add("MessageCode", typeof(string));

                                    DataTable dtAlrmMsg = new DataTable();
                                    dtAlrmMsg.Columns.Add("AlarmMessage", typeof(string));
                                    dtAlrmMsg.Columns.Add("AlarmDate", typeof(string));
                                    dtAlrmMsg.Columns.Add("AlarmTime", typeof(string));
                                    dtAlrmMsg.Columns.Add("AlarmDateTime", typeof(string));
                                    dtAlrmMsg.Columns.Add("AlarmNo", typeof(string));
                                    dtAlrmMsg.Columns.Add("Axis_No", typeof(string));
                                    dtAlrmMsg.Columns.Add("Axis_Num", typeof(string));
                                    dtAlrmMsg.Columns.Add("Abs_Pos", typeof(string));
                                    dtAlrmMsg.Columns.Add("InsertedOn", typeof(string));
                                    dtAlrmMsg.Columns.Add("MachineID", typeof(int));
                                    dtAlrmMsg.Columns.Add("AlarmGroup", typeof(string));

                                    try
                                    {
                                        // For Type one that is for Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                                        #region
                                        if (type == 1)
                                        {
                                            //MySqlCommand cmdStrt = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Operation History stopped to data Log the alarm history and message history.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Ope Stopped'," + mcid + ")", mc1.sqlConnection);
                                            //cmdStrt.ExecuteNonQuery();
                                            //mc1.close();

                                            //Stopping Operation History While logging data from Message History and Alarm History
                                            int stopret = 0;
                                            stopoperationhistory:
                                            stopophisret = Focas1.cnc_stopophis(h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM

                                            if (stopophisret == 0)
                                            {
                                                //opmsghisret = formatinsertMsgHis(h, mcid, dtOpMsg);
                                                almhisret = formatinsertAlmHis(h, mcid, dtAlrmMsg);
                                                startophisret = Focas1.cnc_startophis(h);
                                                //insertMsgHisDB(h, mcid, dtOpMsg);
                                                insertAlarmHIsDB(h, mcid, dtAlrmMsg);
                                            }
                                            else if (stopophisret == -1)
                                            {
                                                startophisret = Focas1.cnc_startophis(h);
                                                stopret++;
                                                if (stopret == 4)
                                                {
                                                    goto nex;
                                                }

                                                goto stopoperationhistory;
                                            }
                                            else if (stopophisret == -16)
                                            {
                                                startophisret = Focas1.cnc_startophis(h);
                                                freeret = Focas1.cnc_freelibhndl(h);
                                                ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
                                                stopret = 4;
                                            }
                                            nex:
                                            startophisret = Focas1.cnc_startophis(h);
                                            if (stopret == 4)
                                            {
                                                //using (MsqlConnection SA = new MsqlConnection())
                                                //{
                                                //    SA.open();
                                                //    MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Operation History Couldnot be stopped to data Log the alarm history and message history.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error'," + mcid + ")", SA.msqlConnection);
                                                //    cmd.ExecuteNonQuery();
                                                //    SA.close();
                                                //}
                                                InsertOperationLogDetails("Operation History Couldnot be stopped to data Log the alarm history and message history.", System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "M/c Error", mcid);
                                            }
                                            //mc1.open();
                                            //MySqlCommand cmdStop = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Operation History Started.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Ope Started'," + mcid + ")", mc1.sqlConnection);
                                            //cmdStop.ExecuteNonQuery();
                                            //mc1.close();
                                        }
                                        #endregion
                                        //For Type Two that is for Fanuc Controller 16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MC Slim/21i-TB
                                        #region
                                        else if (type == 2)
                                        {
                                            //mc1.close();
                                            //mc1.open();
                                            //MySqlCommand cmdStrt = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Operator Message History Stopped.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Ope Msg Stopped'," + mcid + ")", mc1.sqlConnection);
                                            //cmdStrt.ExecuteNonQuery();
                                            //mc1.close();

                                            //Stopping Operation History While logging data from Message History 
                                            //    int stopret1 = 0;
                                            //stopoperationhistory1:
                                            //stopophisret1 = Focas1.cnc_stopomhis(h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM

                                            //    if (stopophisret1 == 0)
                                            //{
                                            //    opmsghisret = formatinsertMsgHis2(h, mcid, dtOpMsg);
                                            //    startophisret1 = Focas1.cnc_startomhis(h);
                                            //    //insertMsgHisDB(h, mcid, dtOpMsg);

                                            //}
                                            //else if (stopophisret1 == -1)
                                            //{
                                            //    startophisret1 = Focas1.cnc_startomhis(h);
                                            //    stopret1++;
                                            //    if (stopret1 == 4)
                                            //        goto nex1;
                                            //    goto stopoperationhistory1;
                                            //}
                                            //else if (stopophisret1 == -16)
                                            //{
                                            //    ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
                                            //    stopret1 = 4;
                                            //}
                                            //nex1:
                                            //startophisret1 = Focas1.cnc_startomhis(h);
                                            //if (stopret1 == 4)
                                            //{
                                            //    mc1.close();
                                            //    mc1.open();
                                            //    MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Operator Message History Couldnot be stopped to data Log the message history.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error'," + mcid + ")", mc1.sqlConnection);
                                            //    cmd.ExecuteNonQuery();
                                            //    mc1.close();
                                            //}
                                            //mc1.close();
                                            //mc1.open();
                                            //MySqlCommand cmdStop = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Operation Message History Started.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Ope Msg Started'," + mcid + ")", mc1.sqlConnection);
                                            //cmdStop.ExecuteNonQuery();
                                            //mc1.close();

                                            //mc1.close();
                                            //mc1.open();
                                            //MySqlCommand cmdStrtAlarm = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Operation History Stopped.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Ope Stopped'," + mcid + ")", mc1.sqlConnection);
                                            //cmdStrtAlarm.ExecuteNonQuery();
                                            //mc1.close();
                                            //Stopping Alarm History While logging data from Alarm History
                                            int stopret = 0;
                                            stopoperationhistory:
                                            stopophisret = Focas1.cnc_stopophis(h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM

                                            if (stopophisret == 0)
                                            {
                                                almhisret = formatinsertAlmHis2(h, mcid, dtAlrmMsg);
                                                startophisret = Focas1.cnc_startophis(h);
                                                insertAlarmHIsDB2(h, mcid, dtAlrmMsg);
                                            }
                                            else if (stopophisret == -1)
                                            {
                                                startophisret = Focas1.cnc_startophis(h);
                                                stopret++;
                                                if (stopret == 4)
                                                {
                                                    goto nex;
                                                }
                                                goto stopoperationhistory;
                                            }
                                            else if (stopophisret == -16)
                                            {
                                                ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
                                                stopret = 4;
                                            }
                                            nex:
                                            startophisret = Focas1.cnc_startophis(h);
                                            if (stopret == 4)
                                            {
                                                //using (MsqlConnection SA = new MsqlConnection())
                                                //{
                                                //    SA.open();
                                                //    MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Operation History Couldnot be stopped to data Log the alarm history.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error'," + mcid + ")", SA.msqlConnection);
                                                //    cmd.ExecuteNonQuery();
                                                //    SA.close();
                                                //}
                                                InsertOperationLogDetails("Operation History Couldnot be stopped to data Log the alarm history and message history.", System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "M/c Error", mcid);
                                            }
                                        }
                                        #endregion
                                    }
                                    catch (Exception)
                                    { }
                                    //{
                                    //    mc1.close();
                                    //    mc1.open();
                                    //    String CmdQuery = @"INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('" + ex.Message + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','App Error'," + mcid + ")";
                                    //    MySqlCommand cmd = new MySqlCommand(CmdQuery, mc1.sqlConnection);
                                    //    cmd.ExecuteNonQuery();
                                    //    mc1.close();
                                    //}
                                    if (type == 1)
                                    {
                                        startophisret = Focas1.cnc_startophis(h);
                                    }
                                    else if (type == 2)
                                    {
                                        startophisret1 = Focas1.cnc_startomhis(h);
                                        startophisret = Focas1.cnc_startophis(h);
                                    }
                                    freeret = Focas1.cnc_freelibhndl(h);
                                }
                                else
                                {
                                    //using (MsqlConnection SA = new MsqlConnection())
                                    //{
                                    //    SA.open();
                                    //    MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Connection to M/c Problem. Please check the Ethernet Connection and Power Connection of the CNC Machine.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error'," + mcid + ")", SA.msqlConnection);
                                    //    cmd.ExecuteNonQuery();
                                    //    SA.close();
                                    //}
                                    InsertOperationLogDetails("Connection to M/c Problem. Please check the Ethernet Connection and Power Connection of the CNC Machine.", System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "M/c Error", mcid);
                                }
                                #endregion
                            }
                            freeret = Focas1.cnc_freelibhndl(h);
                        }

                        //ATCCounter
                        //ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
                        //try
                        //{
                        //    if (ret == 0)
                        //        GetToolATC(h, mcid);
                        //}
                        //catch (Exception ex)
                        //{
                        //    mc1.close();
                        //    mc1.open();
                        //    MySqlCommand cmd2 = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('" + @ex.ToString() + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Tool Stop',100" + mcid + ")", mc1.sqlConnection);
                        //    cmd2.ExecuteNonQuery();
                        //    mc1.close();
                        //}
                        freeret = Focas1.cnc_freelibhndl(h);
                        cntevent.Signal();
                    });
                }
                cntevent.Wait();
                cntevent.Dispose();
            }
        }

        //Operator Message History Type 1
        private int formatinsertMsgHis(ushort h, int machineid, DataTable dtOp)
        {
            Focas1.ODBOMHIS2 alarmmsg1 = new Focas1.ODBOMHIS2();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
                                                                //var alarmmsg2 = new Focas1.ODBOMHIS2();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
            ushort msgnumbers;
            int msgnumret = Focas1.cnc_rdomhisno(h, out msgnumbers);
            if (msgnumbers != 0)
            {
                int snoval = 1;
                ushort msglent = (ushort)(4 + alarmmsg1.opm_his.data1.alm_msg.Length * 2); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
                ushort s_no = (ushort)(msgnumbers - snoval);
                ushort e_no = msgnumbers;
                int notinsert = 0;

                DateTime insertedon = System.DateTime.Now;

                for (int i = 0; i < msgnumbers; i++)
                {
                    int alarmmsgret = Focas1.cnc_rdomhistry2(h, e_no, e_no, msglent, alarmmsg1); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
                    if (alarmmsgret == 0)
                    {
                        String opmsgdate1 = null, opmsgtime1 = null, opmsgdatetime1 = null, opmsgmsg1 = null, opmsgno1 = null, opmsgcode1 = null;
                        // saving date from operator msg history
                        if (alarmmsg1.opm_his.data1.day.ToString() != "0")
                        {
                            opmsgdate1 = alarmmsg1.opm_his.data1.year.ToString() + "/" + alarmmsg1.opm_his.data1.month.ToString() + "/" + alarmmsg1.opm_his.data1.day.ToString();
                            opmsgtime1 = alarmmsg1.opm_his.data1.hour.ToString() + ":" + alarmmsg1.opm_his.data1.minute.ToString() + ":" + alarmmsg1.opm_his.data1.second.ToString();
                            String Datessss = alarmmsg1.opm_his.data1.year.ToString() + "-" + alarmmsg1.opm_his.data1.month.ToString() + "-" + alarmmsg1.opm_his.data1.day.ToString();
                            opmsgdatetime1 = Convert.ToDateTime(Datessss).ToString("yyyy-MM-dd") + " " + opmsgtime1;
                            opmsgmsg1 = alarmmsg1.opm_his.data1.alm_msg.ToString();
                            if (opmsgmsg1.Contains('\''))
                            {
                                opmsgmsg1.Replace("\'", "");
                            }
                            opmsgno1 = alarmmsg1.opm_his.data1.om_no.ToString();
                            String[] msgsplit = opmsgmsg1.Trim().Split(' ');
                            opmsgcode1 = msgsplit[0];
                            if (opmsgcode1.Contains('M'))
                            {
                                opmsgcode1 = opmsgcode1.Substring(1);
                            }
                            if (opmsgdate1 != null)
                            {
                                dtOp.Rows.Add(new object[] { opmsgmsg1, opmsgdate1, opmsgtime1, opmsgdatetime1, opmsgno1, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), machineid, opmsgcode1 });
                            }
                        }
                    }
                    else
                    {
                        notinsert++;
                    }
                    e_no = (ushort)(e_no - 1);
                    s_no = (ushort)(e_no - 1);
                    if (e_no == 0)
                    {
                        e_no = 0;
                        s_no = 0;
                    }
                }
                if (notinsert == 0)
                {
                    return 0;
                }
                else
                {
                    return notinsert;
                }
            }
            else
            {
                return 100;
            }
        }
        //10.20.40.9
        //Inserting the Values into the DB after getting the records of Operator Message History
        private void insertMsgHisDB(ushort h, int machineid, DataTable dtOp)
        {
            unitworksccsEntities db = new unitworksccsEntities();
            String opmsgdate1 = null, opmsgtime1 = null, opmsgdatetime1 = null, opmsgmsg1 = null, opmsgno1 = null, opmsgcode1 = null, opInsteredOn = null;
            int opMachineID;
            for (int i = 0; i < dtOp.Rows.Count; i++)
            {
                opmsgdate1 = dtOp.Rows[i][1].ToString();
                opmsgtime1 = dtOp.Rows[i][2].ToString();
                opmsgdatetime1 = dtOp.Rows[i][3].ToString();
                opmsgmsg1 = dtOp.Rows[i][0].ToString();
                opmsgno1 = dtOp.Rows[i][4].ToString();
                opmsgcode1 = dtOp.Rows[i][7].ToString();
                opInsteredOn = dtOp.Rows[i][5].ToString();
                opMachineID = Convert.ToInt32(dtOp.Rows[i][6].ToString());

                //MsqlConnection mc = new MsqlConnection();
                //mc.open();
                if (opmsgdate1 != null)
                {
                    //DataTable dt = new DataTable();
                    //String query = "SELECT Meassage From  i_facility.unitworkccs_message_history_master WHERE MessageDateTime = '" + opmsgdatetime1 + "' AND MachineID = " + machineid + "";
                    //using (MsqlConnection mc = new MsqlConnection())
                    //{
                    //    mc.open();
                    //    SqlDataAdapter da = new SqlDataAdapter(query, mc.msqlConnection);
                    //    da.Fill(dt);
                    //    mc.close();
                    //}
                    var msgs = new List<unitworkccs_message_history_master>();
                    using (unitworksccsEntities db1 = new unitworksccsEntities())
                    {
                        msgs = db.unitworkccs_message_history_master.Where(m => m.MessageDateTime.Equals(opmsgdatetime1) && m.MachineID == machineid).ToList();
                    }

                    if (msgs.Count == 0)
                    {
                        //DataTable dtshift = new DataTable();
                        //String querymsgtyp = "SELECT MessageType,MessageCode From  i_facility.unitworkccs_message_code_master WHERE MessageCode = '" + opmsgcode1 + "'";
                        var msgs1 = new List<unitworkccs_message_code_master>();
                        using (unitworksccsEntities db1 = new unitworksccsEntities())
                        {
                            msgs1 = db.unitworkccs_message_code_master.Where(m => m.MessageCode == opmsgcode1).ToList();
                        }
                        if (opmsgcode1.Length == 6)
                        {
                            //querymsgtyp = "SELECT MessageType,MessageCode From  i_facility.unitworkccs_message_code_master WHERE MessageMCode = 'M" + opmsgcode1 + "'";
                            using (unitworksccsEntities db1 = new unitworksccsEntities())
                            {
                                msgs1 = db.unitworkccs_message_code_master.Where(m => m.MessageCode == "M" + opmsgcode1).ToList();
                            }
                        }
                        //using (MsqlConnection mc = new MsqlConnection())
                        //{

                        //    mc.open();
                        //    SqlDataAdapter damt = new SqlDataAdapter("", mc.msqlConnection);
                        //    DataTable dtmt = new DataTable();
                        //    damt.Fill(dtmt);
                        //    mc.close();
                        //}
                        //String querymsgtyp = "SELECT MessageType From unitworkccs_message_code_master WHERE MessageCode = '" + opmsgcode1 + "'";

                        String msgtype = "";// dtmt.Rows[0][0].ToString();
                        opmsgcode1 = "";// dtmt.Rows[0][1].ToString();
                        String shift = null;
                        //String queryshift = "SELECT ShiftName,StartTime,EndTime FROM i_facility.unitworkccs_shift_master WHERE IsDeleted = 0";

                        //using (MsqlConnection mc = new MsqlConnection())
                        //{
                        //    mc.open();
                        //    SqlDataAdapter dashift = new SqlDataAdapter(queryshift, mc.msqlConnection);
                        //    dashift.Fill(dtshift);
                        //    mc.close();
                        //}
                        var msgs2 = new List<unitworkccs_shift_master>();
                        using (unitworksccsEntities db1 = new unitworksccsEntities())
                        {
                            msgs2 = db.unitworkccs_shift_master.Where(m => m.IsDeleted == 0).ToList();
                        }
                        String[] msgtime = opmsgtime1.Split(':');
                        TimeSpan msgstime = new TimeSpan(Convert.ToInt32(msgtime[0]), Convert.ToInt32(msgtime[1]), Convert.ToInt32(msgtime[2]));
                        TimeSpan s1t1 = new TimeSpan(0, 0, 0), s1t2 = new TimeSpan(0, 0, 0);
                        TimeSpan s2t1 = new TimeSpan(0, 0, 0), s2t2 = new TimeSpan(0, 0, 0);
                        TimeSpan s3t1 = new TimeSpan(0, 0, 0), s3t2 = new TimeSpan(0, 0, 0), s3t3 = new TimeSpan(0, 0, 0), s3t4 = new TimeSpan(23, 59, 59);
                        for (int j = 0; j < msgs2.Count; j++)
                        {
                            if (msgs2[j].ShiftName.ToString().Contains("1"))
                            {
                                String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                                s1t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                                String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                                s1t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                            }
                            if (msgs2[j].ShiftName.ToString().Contains("2"))
                            {
                                String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                                s2t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                                String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                                s2t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                            }
                            if (msgs2[j].ShiftName.ToString().Contains("3"))
                            {
                                String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                                s3t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                                String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                                s3t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                            }
                        }
                        String CorrectedDate = Convert.ToDateTime(opmsgdate1).ToString("yyyy-MM-dd");
                        if (msgstime >= s1t1 && msgstime < s1t2)
                        {
                            shift = "1";
                        }
                        else if (msgstime >= s2t1 && msgstime < s2t2)
                        {
                            shift = "2";
                        }
                        else if ((msgstime >= s3t1 && msgstime <= s3t4) || (msgstime >= s3t3 && msgstime < s3t2))
                        {
                            shift = "3";
                            if (msgstime >= s3t3 && msgstime < s3t2)
                            {
                                CorrectedDate = Convert.ToDateTime(opmsgdate1).AddDays(-1).ToString("yyyy-MM-dd");
                            }
                        }


                        //using (MsqlConnection mc = new MsqlConnection())
                        //{
                        //    mc.open();
                        //    MySqlCommand cmd1 = new MySqlCommand("INSERT INTO  i_facility.unitworkccs_message_history_master(Meassage,MessageDate,MessageTime,MessageDateTime,MessageNo,InsertedOn,MachineID,MessageCode,MessageType,MessageShift,CorrectedDate,IsProgLock)" + //
                        //    "VALUES('" + @opmsgmsg1 + "','" + opmsgdate1 + "','" + opmsgtime1 + "','" + opmsgdatetime1 + "','" + opmsgno1 + "','" + opInsteredOn + "'," + opMachineID + ",'" + opmsgcode1 + "','" + msgtype + "','" + shift + "','" + CorrectedDate + "',0)", mc.msqlConnection); //
                        //    cmd1.ExecuteNonQuery();
                        //    mc.close();
                        //}
                        unitworkccs_message_history_master obj = new unitworkccs_message_history_master();
                        obj.Meassage = @opmsgmsg1;
                        obj.MessageDate = Convert.ToDateTime(opmsgdate1);
                        obj.MessageTime = System.DateTime.Now.TimeOfDay;
                        obj.MessageDateTime = Convert.ToDateTime(opmsgdatetime1);
                        obj.MessageNo = opmsgno1;
                        obj.InsertedOn = Convert.ToDateTime(opInsteredOn);
                        obj.MachineID = opMachineID;
                        obj.MessageCode = opmsgcode1;
                        obj.MessageType = msgtype;
                        obj.MessageShift = shift;
                        obj.CorrectedDate = Convert.ToDateTime(CorrectedDate);
                        obj.IsProgLock = 0;
                        using (unitworksccsEntities db1 = new unitworksccsEntities())
                        {
                            db.unitworkccs_message_history_master.Add(obj);
                            db.SaveChanges();
                        }

                        if (opmsgcode1 == "8006")
                        {
                            //String MsgDbquery = "SELECT MessageID,MessageDateTime From unitworkccs_message_history_master WHERE MessageCode = '8006' AND IsProgLock = 0 AND MachineID = " + machineid + " Order By MessageID DESC;";
                            //SqlDataAdapter MsgDbda = new SqlDataAdapter(MsgDbquery, mc.msqlConnection);
                            //DataTable MsgDbdt = new DataTable();
                            //MsgDbda.Fill(MsgDbdt);
                            int ret = programfilter(h, machineid, opmsgdatetime1);//, Convert.ToInt32(MsgDbdt.Rows[0][0])
                            if (ret == 4 || ret == 4)
                            {
                                //using (MsqlConnection SA = new MsqlConnection())
                                //{
                                //    SA.open();
                                //    MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('NC Program Cant be read, due to upload error from M/c.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error'," + machineid + ")", SA.msqlConnection);
                                //    cmd.ExecuteNonQuery();
                                //    SA.close();
                                //}
                                InsertOperationLogDetails("NC Program Cant be read, due to upload error from M/c. ", System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "M/c Error", machineid);
                            }
                        }
                    }
                }
            }
        }

        //Alarm History Type 1
        private int formatinsertAlmHis(ushort h, int machineid, DataTable dtAlrm)
        {
            Focas1.ODBAHIS5 opmsg = new Focas1.ODBAHIS5();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
            ushort msgnumbers;
            int msgnumret = Focas1.cnc_rdalmhisno(h, out msgnumbers);
            if (msgnumbers != 0)
            {
                int snoval = 1;

                ushort msglen = 4 + (516 * 10);
                ushort s_no = (ushort)(msgnumbers - snoval);
                ushort e_no = msgnumbers;
                int notinsert = 0;
                DateTime insertedon = System.DateTime.Now;

                for (int i = 0; i < msgnumbers; i++)
                {
                    int opmsgret = Focas1.cnc_rdalmhistry5(h, s_no, e_no, msglen, opmsg); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM

                    if (opmsgret == 0)
                    {
                        string alamsgdate1 = null, alamsgtime1 = null, alamsgdatetime1 = null, alamsgalmno1 = null, alamsgaxisno1 = null, alamsgaxisnum1 = null, alamsgmsg1 = null, alamsgabspos1 = null;
                        String alamgrp1 = null;
                        //saving alarm msg history
                        //1st message
                        if (opmsg.alm_his.data1.day.ToString() != "0")
                        {
                            alamsgdate1 = opmsg.alm_his.data1.year.ToString() + "/" + opmsg.alm_his.data1.month.ToString() + "/" + opmsg.alm_his.data1.day.ToString();
                            String datesssss = opmsg.alm_his.data1.year.ToString() + "-" + opmsg.alm_his.data1.month.ToString() + "-" + opmsg.alm_his.data1.day.ToString();
                            alamsgtime1 = opmsg.alm_his.data1.hour.ToString() + ":" + opmsg.alm_his.data1.minute.ToString() + ":" + opmsg.alm_his.data1.second.ToString();
                            alamsgdatetime1 = datesssss + " " + alamsgtime1;
                            alamsgalmno1 = opmsg.alm_his.data1.alm_no.ToString();
                            alamsgaxisno1 = opmsg.alm_his.data1.axis_no.ToString();
                            alamsgaxisnum1 = opmsg.alm_his.data1.axis_num.ToString();
                            alamsgmsg1 = opmsg.alm_his.data1.alm_msg.ToString();
                            alamsgabspos1 = opmsg.alm_his.data1.abs_pos.ToString();
                            alamgrp1 = opmsg.alm_his.data1.alm_grp.ToString();

                            if (alamsgdate1 != null)
                            {
                                dtAlrm.Rows.Add(new object[] { alamsgmsg1, alamsgdate1, alamsgtime1, alamsgdatetime1, alamsgalmno1, alamsgaxisno1, alamsgaxisnum1, alamsgabspos1, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), machineid, alamgrp1 });
                            }
                        }
                    }
                    else
                    {
                        notinsert++;
                    }
                    e_no = (ushort)(e_no - 1);
                    s_no = (ushort)(e_no - 1);
                    if (s_no == 0)
                    {
                        s_no = 1;
                    }
                }
                if (notinsert == 0)
                {
                    return 0;
                }
                else
                {
                    return notinsert;
                }
            }
            else
            {
                return 100;
            }
        }

        //Inserting the Values into the DB after getting the records pf Alarm History
        private void insertAlarmHIsDB(ushort h, int machineid, DataTable dtAlrm)
        {
            string alamsgdate1 = null, alamsgtime1 = null, alamsgdatetime1 = null, alamsgalmno1 = null, alamsgaxisno1 = null, alamsgaxisnum1 = null, alamsgmsg1 = null, alamsgabspos1 = null;
            String alamgrp1 = null, alaInsteredOn = null;
            int alaMachineID;
            for (int i = 0; i < dtAlrm.Rows.Count; i++)
            {
                alamsgdate1 = dtAlrm.Rows[i][1].ToString();
                alamsgtime1 = dtAlrm.Rows[i][2].ToString();
                alamsgdatetime1 = dtAlrm.Rows[i][3].ToString();
                alamsgalmno1 = dtAlrm.Rows[i][4].ToString();
                alamsgaxisno1 = dtAlrm.Rows[i][5].ToString();
                alamsgaxisnum1 = dtAlrm.Rows[i][6].ToString();
                alamsgmsg1 = @dtAlrm.Rows[i][0].ToString().Replace('\'', ' ');
                alamsgabspos1 = dtAlrm.Rows[i][7].ToString();
                alaInsteredOn = dtAlrm.Rows[i][8].ToString();
                alaMachineID = Convert.ToInt32(dtAlrm.Rows[i][9].ToString());
                alamgrp1 = dtAlrm.Rows[i][10].ToString();

                if (alamsgdate1 != null)
                {
                    //DataTable dt = new DataTable();
                    //using (MsqlConnection mc = new MsqlConnection())
                    //{
                    //    mc.open();
                    //    String query = "SELECT AlarmNo From  i_facility.unitworkccs_alarm_history_master WHERE AlarmDateTime = '" + alamsgdatetime1 + "' AND MachineID = " + machineid + " AND Axis_No = '" + alamsgaxisno1 + "'";
                    //    SqlDataAdapter da = new SqlDataAdapter(query, mc.msqlConnection);
                    //    da.Fill(dt);
                    //    mc.close();
                    //}
                    var alarmhis = new List<unitworkccs_alarm_history_master>();
                    using (unitworksccsEntities db1 = new unitworksccsEntities())
                    {
                        alarmhis = db1.unitworkccs_alarm_history_master.Where(m => m.AlarmDateTime == Convert.ToDateTime(alamsgdatetime1) && m.MachineID == machineid && m.Axis_No == alamsgaxisno1).ToList();
                    }
                    if (alarmhis.Count == 0)
                    {
                        //if (alamgrp1 == "6" || alamgrp1 == "9" || alamgrp1 == "0" || alamgrp1 == "1" || alamgrp1 == "5")
                        {
                            String alarmmsgnumber = null;
                            if (alamgrp1 == "6")
                            {
                                alarmmsgnumber = "SV" + alamsgalmno1;
                            }
                            else if (alamgrp1 == "9")
                            {
                                alarmmsgnumber = "SP" + alamsgalmno1;
                            }
                            else if (alamgrp1 == "0")
                            {
                                alarmmsgnumber = "SW" + alamsgalmno1;
                            }
                            else if (alamgrp1 == "1")
                            {
                                alarmmsgnumber = "PW" + alamsgalmno1;
                            }
                            else if (alamgrp1 == "5")
                            {
                                alarmmsgnumber = "OH" + alamsgalmno1;
                            }
                            else
                            {
                                alarmmsgnumber = alamsgalmno1.ToString();
                            }

                            String shift = null;
                            //DataTable dtshift = new DataTable();
                            //using (MsqlConnection mc = new MsqlConnection())
                            //{
                            //    mc.open();
                            //    String queryshift = "SELECT ShiftName,StartTime,EndTime FROM  i_facility.unitworkccs_shift_master WHERE IsDeleted = 0";
                            //    SqlDataAdapter dashift = new SqlDataAdapter(queryshift, mc.msqlConnection);
                            //    dashift.Fill(dtshift);
                            //    mc.close();
                            //}
                            var msgs2 = new List<unitworkccs_shift_master>();
                            using (unitworksccsEntities db1 = new unitworksccsEntities())
                            {
                                msgs2 = db1.unitworkccs_shift_master.Where(m => m.IsDeleted == 0).ToList();
                            }
                            String[] msgtime = alamsgtime1.Split(':');
                            TimeSpan msgstime = new TimeSpan(Convert.ToInt32(msgtime[0]), Convert.ToInt32(msgtime[1]), Convert.ToInt32(msgtime[2]));
                            TimeSpan s1t1 = new TimeSpan(0, 0, 0), s1t2 = new TimeSpan(0, 0, 0), s2t1 = new TimeSpan(0, 0, 0), s2t2 = new TimeSpan(0, 0, 0);
                            TimeSpan s3t1 = new TimeSpan(0, 0, 0), s3t2 = new TimeSpan(0, 0, 0), s3t3 = new TimeSpan(0, 0, 0), s3t4 = new TimeSpan(23, 59, 59);
                            for (int j = 0; j < msgs2.Count; j++)
                            {
                                if (msgs2[j].ShiftName.ToString().Contains("1"))
                                {
                                    String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                                    s1t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                                    String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                                    s1t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                                }
                                if (msgs2[j].ShiftName.ToString().Contains("2"))
                                {
                                    String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                                    s2t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                                    String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                                    s2t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                                }
                                if (msgs2[j].ShiftName.ToString().Contains("3"))
                                {
                                    String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                                    s3t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                                    String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                                    s3t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                                }
                            }
                            String CorrectedDate = Convert.ToDateTime(alamsgdate1).ToString("yyyy-MM-dd");
                            if (msgstime >= s1t1 && msgstime < s1t2)
                            {
                                shift = "1";
                            }
                            else if (msgstime >= s2t1 && msgstime < s2t2)
                            {
                                shift = "2";
                            }
                            else if ((msgstime >= s3t1 && msgstime <= s3t4) || (msgstime >= s3t3 && msgstime < s3t2))
                            {
                                shift = "3";
                                if (msgstime >= s3t3 && msgstime < s3t2)
                                {
                                    CorrectedDate = Convert.ToDateTime(alamsgdate1).AddDays(-1).ToString("yyyy-MM-dd");
                                }
                            }
                            //using (MsqlConnection mc = new MsqlConnection())
                            //{
                            //    mc.open();
                            //    MySqlCommand cmd1 = new MySqlCommand("INSERT INTO  i_facility.unitworkccs_alarm_history_master(AlarmMessage,AlarmDateTime,AlarmNo,Axis_No,Axis_Num,Abs_Pos,InsertedOn,MachineID,Shift,CorrectedDate)" +
                            //        "VALUES('" + @alamsgmsg1 + "','" + alamsgdatetime1 + "','" + alarmmsgnumber + "','" + alamsgaxisno1 + "','" + alamsgaxisnum1 + "','" + alamsgabspos1 + "','" + alaInsteredOn + "'," + alaMachineID + ",'" + shift + "','" + CorrectedDate + "')", mc.msqlConnection);
                            //    cmd1.ExecuteNonQuery();
                            //    mc.close();
                            //}
                            unitworkccs_alarm_history_master obj = new unitworkccs_alarm_history_master();
                            obj.AlarmMessage = @alamsgmsg1;
                            obj.AlarmDateTime = Convert.ToDateTime(alamsgdatetime1);
                            obj.AlarmNo = Convert.ToString(alarmmsgnumber);
                            obj.Axis_No = alamsgaxisno1;
                            obj.Axis_Num = alamsgaxisnum1;
                            obj.Abs_Pos = alamsgabspos1;
                            obj.InsertedOn = Convert.ToDateTime(alaInsteredOn);
                            obj.MachineID = alaMachineID;
                            obj.Shift = shift;
                            obj.CorrectedDate = Convert.ToDateTime(CorrectedDate);
                            using (unitworksccsEntities db1 = new unitworksccsEntities())
                            {
                                db1.unitworkccs_alarm_history_master.Add(obj);
                                db1.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        //Operator Message History Type 2
        private int formatinsertMsgHis2(ushort h, int machineid, DataTable dtOp)
        {
            Focas1.ODBOMHIS alarmmsg1 = new Focas1.ODBOMHIS();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
            Focas1.ODBOMIF msgnumber = new Focas1.ODBOMIF();
            int retNumOfMsgs = Focas1.cnc_rdomhisinfo(h, msgnumber);
            int currentnumber = msgnumber.om_sum;
            if (currentnumber != 0)
            {
                ushort msglent = (ushort)(4 + alarmmsg1.omhis1.om_msg.Length * 2); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
                ushort s_no = 0;
                ushort e_no = 1;
                int notinsert = 0;
                DateTime insertedon = System.DateTime.Now;

                for (int i = 0; i < currentnumber; i++)
                {
                    int almsgyear = 0;
                    int alarmmsgret = Focas1.cnc_rdomhistry(h, s_no, ref e_no, alarmmsg1); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
                    if (alarmmsgret == 0)
                    {
                        String opmsgdate1 = null, opmsgtime1 = null, opmsgdatetime1 = null, opmsgmsg1 = null, opmsgno1 = null, opmsgcode1 = null;
                        // saving date from operator msg history
                        if (alarmmsg1.omhis1.day.ToString() != "0") // && alarmmsg1.omhis1.om_msg.Contains("VV")
                        {
                            almsgyear = alarmmsg1.omhis1.year;
                            if (almsgyear.ToString().Length == 2)
                            {
                                String year = "20" + almsgyear;
                                almsgyear = Convert.ToInt32(year);
                            }
                            else if (almsgyear.ToString().Length == 3)
                            {
                                String year = "2" + almsgyear;
                                almsgyear = Convert.ToInt32(year);
                            }
                            opmsgdate1 = almsgyear + "/" + alarmmsg1.omhis1.month.ToString() + "/" + alarmmsg1.omhis1.day.ToString();
                            opmsgtime1 = alarmmsg1.omhis1.hour.ToString() + ":" + alarmmsg1.omhis1.minute.ToString() + ":" + alarmmsg1.omhis1.second.ToString();
                            String Datessss = almsgyear + "-" + alarmmsg1.omhis1.month.ToString() + "-" + alarmmsg1.omhis1.day.ToString();
                            opmsgdatetime1 = Convert.ToDateTime(Datessss).ToString("yyyy-MM-dd") + " " + opmsgtime1;
                            opmsgmsg1 = alarmmsg1.omhis1.om_msg.ToString();
                            if (opmsgmsg1.Contains('\''))
                            {
                                opmsgmsg1.Replace("\'", "");
                            }
                            opmsgno1 = alarmmsg1.omhis1.om_no.ToString();
                            String[] msgsplit = opmsgmsg1.Trim().Split(' ');
                            opmsgcode1 = msgsplit[0];
                            if (opmsgcode1.Contains('M'))
                            {
                                opmsgcode1 = opmsgcode1.Substring(1);
                            }

                            if (opmsgdate1 != null)
                            {
                                dtOp.Rows.Add(new object[] { opmsgmsg1, opmsgdate1, opmsgtime1, opmsgdatetime1, opmsgno1, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), machineid, opmsgcode1 });
                            }
                        }
                    }
                    else
                    {
                        notinsert++;
                    }
                    s_no = (ushort)(s_no + 1);
                }
                if (notinsert == 0)
                {
                    return 0;
                }
                else
                {
                    return notinsert;
                }
            }
            else
            {
                return 100;
            }
        }

        //Alarm History Type 2
        private int formatinsertAlmHis2(ushort h, int machineid, DataTable dtAlrm)
        {
            Focas1.ODBAHIS opmsg = new Focas1.ODBAHIS();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
            ushort msgnumbers;
            int msgnumret = Focas1.cnc_rdalmhisno(h, out msgnumbers);
            if (msgnumbers != 0)
            {
                int snoval = 1;

                ushort msglen = 4 + (516 * 10);
                ushort s_no = (ushort)(msgnumbers - snoval);
                ushort e_no = msgnumbers;
                int notinsert = 0;
                DateTime insertedon = System.DateTime.Now;

                for (int i = 0; i < msgnumbers; i++)
                {
                    int opmsgret = Focas1.cnc_rdalmhistry(h, e_no, e_no, msglen, opmsg); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM

                    if (opmsgret == 0)
                    {
                        string alamsgdate1 = null, alamsgtime1 = null, alamsgdatetime1 = null, alamsgalmno1 = null, alamsgaxisno1 = null, alamsgaxisnum1 = null, alamsgmsg1 = null, alamsgabspos1 = null;
                        String alamgrp1 = null;
                        //saving alarm msg history
                        //1st message
                        if (opmsg.alm_his.data1.day.ToString() != "0")
                        {
                            alamsgdate1 = opmsg.alm_his.data1.year.ToString() + "/" + opmsg.alm_his.data1.month.ToString() + "/" + opmsg.alm_his.data1.day.ToString();
                            String datesssss = opmsg.alm_his.data1.year.ToString() + "-" + opmsg.alm_his.data1.month.ToString() + "-" + opmsg.alm_his.data1.day.ToString();

                            alamsgtime1 = opmsg.alm_his.data1.hour.ToString() + ":" + opmsg.alm_his.data1.minute.ToString() + ":" + opmsg.alm_his.data1.second.ToString();
                            alamsgdatetime1 = datesssss + " " + alamsgtime1;
                            alamsgalmno1 = opmsg.alm_his.data1.alm_no.ToString();
                            alamsgaxisno1 = opmsg.alm_his.data1.axis_no.ToString();
                            //alamsgaxisnum1 = opmsg.alm_his.data1..ToString();
                            alamsgmsg1 = opmsg.alm_his.data1.alm_msg.ToString();
                            //alamsgabspos1 = opmsg.alm_his.data1.abs_pos.ToString();
                            alamgrp1 = opmsg.alm_his.data1.alm_grp.ToString();
                            //System.Windows.Forms.MessageBox.Show(machineid + "\n" + alamgrp1 + "\n" + alamsgalmno1 + "\n" + alamsgmsg1);
                            if (alamsgdate1 != null)
                            {
                                dtAlrm.Rows.Add(new object[] { alamsgmsg1, alamsgdate1, alamsgtime1, alamsgdatetime1, alamsgalmno1, alamsgaxisno1, alamsgaxisnum1, alamsgabspos1, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), machineid, alamgrp1 });
                            }
                        }
                    }
                    else
                    {
                        notinsert++;
                    }
                    e_no = (ushort)(e_no - 1);
                    s_no = (ushort)(e_no - 1);
                    if (e_no == 0)
                    {
                        e_no = 0;
                        s_no = 0;
                    }
                }

                if (notinsert == 0)
                {
                    return 0;
                }
                else
                {
                    return notinsert;
                }
            }
            else
            {
                return 100;
            }
        }

        //Inserting the Values into the DB after getting the records pf Alarm History
        private void insertAlarmHIsDB2(ushort h, int machineid, DataTable dtAlrm)
        {
            string alamsgdate1 = null, alamsgtime1 = null, alamsgdatetime1 = null, alamsgalmno1 = null, alamsgaxisno1 = null, alamsgaxisnum1 = null, alamsgmsg1 = null, alamsgabspos1 = null;
            String alamgrp1 = null, alaInsteredOn = null;
            int alaMachineID;
            for (int i = 0; i < dtAlrm.Rows.Count; i++)
            {
                alamsgdate1 = DateTime.ParseExact(dtAlrm.Rows[i][1].ToString(), "yy/M/d", CultureInfo.InvariantCulture).ToString("yyyy/MM/dd"); //dtAlrm.Rows[i][1].ToString();
                alamsgtime1 = dtAlrm.Rows[i][2].ToString();
                alamsgdatetime1 = DateTime.ParseExact(dtAlrm.Rows[i][3].ToString(), "yy-M-d H:m:s", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                alamsgalmno1 = dtAlrm.Rows[i][4].ToString();
                alamsgaxisno1 = dtAlrm.Rows[i][5].ToString();
                //alamsgaxisnum1 = dtAlrm.Rows[i][6].ToString();
                alamsgmsg1 = dtAlrm.Rows[i][0].ToString();
                //alamsgabspos1 = dtAlrm.Rows[i][7].ToString();
                alaInsteredOn = dtAlrm.Rows[i][8].ToString();
                alaMachineID = Convert.ToInt32(dtAlrm.Rows[i][9].ToString());
                alamgrp1 = dtAlrm.Rows[i][10].ToString();

                if (alamsgdate1 != null)
                {
                    //DataTable dt = new DataTable();
                    //using (MsqlConnection mc = new MsqlConnection())
                    //{
                    //    mc.open();
                    //    String query = "SELECT AlarmNo From  unitworkccs_alarm_history_master WHERE AlarmDateTime = '" + alamsgdatetime1 + "' AND MachineID = " + machineid + " AND Axis_No = '" + alamsgaxisno1 + "' AND AlarmNo = '" + alamsgalmno1 + "'";
                    //    SqlDataAdapter da = new SqlDataAdapter(query, mc.msqlConnection);
                    //    da.Fill(dt);
                    //    mc.close();
                    //}
                    var alarm = new List<unitworkccs_alarm_history_master>();
                    using (unitworksccsEntities db1 = new unitworksccsEntities())
                    {
                        alarm = db1.unitworkccs_alarm_history_master.Where(m => m.AlarmDateTime == Convert.ToDateTime(alamsgdatetime1) && m.MachineID == machineid && m.Axis_No == alamsgaxisno1 && m.AlarmNo == Convert.ToString(alamsgalmno1)).ToList();
                    }
                    if (alarm.Count == 0)
                    {
                        String alarmmsgnumber = alamsgalmno1;

                        String shift = "3";
                        //DataTable dtshift = new DataTable();
                        //using (MsqlConnection mc = new MsqlConnection())
                        //{
                        //    mc.open();
                        //    String queryshift = "SELECT ShiftName,StartTime,EndTime FROM  unitworkccs_shift_master WHERE IsDeleted = 0";
                        //    SqlDataAdapter dashift = new SqlDataAdapter(queryshift, mc.msqlConnection);
                        //    dashift.Fill(dtshift);
                        //    mc.close();
                        //}
                        var msgs2 = new List<unitworkccs_shift_master>();
                        using (unitworksccsEntities db1 = new unitworksccsEntities())
                        {
                            msgs2 = db1.unitworkccs_shift_master.Where(m => m.IsDeleted == 0).ToList();
                        }
                        String[] msgtime = alamsgtime1.Split(':');
                        TimeSpan msgstime = new TimeSpan(Convert.ToInt32(msgtime[0]), Convert.ToInt32(msgtime[1]), Convert.ToInt32(msgtime[2]));
                        TimeSpan s1t1 = new TimeSpan(0, 0, 0), s1t2 = new TimeSpan(0, 0, 0), s2t1 = new TimeSpan(0, 0, 0), s2t2 = new TimeSpan(0, 0, 0);
                        TimeSpan s3t1 = new TimeSpan(0, 0, 0), s3t2 = new TimeSpan(0, 0, 0), s3t3 = new TimeSpan(0, 0, 0), s3t4 = new TimeSpan(23, 59, 59);
                        for (int j = 0; j < msgs2.Count; j++)
                        {
                            if (msgs2[j].ShiftName.ToString().Contains("1"))
                            {
                                String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                                s1t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                                String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                                s1t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                            }
                            if (msgs2[j].ShiftName.ToString().Contains("2"))
                            {
                                String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                                s2t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                                String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                                s2t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                            }
                            if (msgs2[j].ShiftName.ToString().Contains("3"))
                            {
                                String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                                s3t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                                String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                                s3t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                            }
                        }
                        int almsgyear = Convert.ToInt32(alamsgdate1.Split(new char[] { '/', '-' }, StringSplitOptions.RemoveEmptyEntries)[0]);// opmsg.alm_his.data1.year;
                        if (almsgyear.ToString().Length == 2)
                        {
                            String year = "20" + almsgyear;
                            almsgyear = Convert.ToInt32(year);
                        }
                        else if (almsgyear.ToString().Length == 3)
                        {
                            String year = "2" + almsgyear;
                            almsgyear = Convert.ToInt32(year);
                        }
                        DateTime almmsgdateformat = new DateTime(almsgyear, Convert.ToInt32(alamsgdate1.Split(new char[] { '/', '-' }, StringSplitOptions.RemoveEmptyEntries)[1]), Convert.ToInt32(alamsgdate1.Split(new char[] { '/', '-' }, StringSplitOptions.RemoveEmptyEntries)[2]));
                        String CorrectedDate = almmsgdateformat.ToString("yyyy-MM-dd");
                        if (msgstime >= s1t1 && msgstime < s1t2)
                        {
                            shift = "1";
                        }
                        else if (msgstime >= s2t1 && msgstime < s2t2)
                        {
                            shift = "2";
                        }
                        else if ((msgstime >= s3t1 && msgstime <= s3t4) || (msgstime >= s3t3 && msgstime < s3t2))
                        {
                            shift = "3";
                            if (msgstime >= s3t3 && msgstime < s3t2)
                            {
                                CorrectedDate = almmsgdateformat.AddDays(-1).ToString("yyyy-MM-dd");
                            }
                        }
                        //using (MsqlConnection mc = new MsqlConnection())
                        //{
                        //    mc.open();
                        //    MySqlCommand cmd1 = new MySqlCommand("INSERT INTO  unitworkccs_alarm_history_master(AlarmMessage,AlarmDateTime,AlarmNo,Axis_No,Axis_Num,Abs_Pos,InsertedOn,MachineID,Shift,CorrectedDate)" +
                        //        "VALUES('" + @alamsgmsg1 + "','" + alamsgdatetime1 + "','" + alarmmsgnumber + "','" + alamsgaxisno1 + "','" + alamsgaxisnum1 + "','" + alamsgabspos1 + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + machineid + ",'" + shift + "','" + CorrectedDate + "')", mc.msqlConnection);
                        //    cmd1.ExecuteNonQuery();
                        //    mc.close();
                        //}
                        unitworkccs_alarm_history_master obj = new unitworkccs_alarm_history_master();
                        obj.AlarmMessage = @alamsgmsg1;
                        obj.AlarmDateTime = Convert.ToDateTime(alamsgdatetime1);
                        obj.AlarmNo = Convert.ToString(alarmmsgnumber);
                        obj.Axis_No = alamsgaxisno1;
                        obj.Axis_Num = alamsgaxisnum1;
                        obj.Abs_Pos = alamsgabspos1;
                        obj.InsertedOn = Convert.ToDateTime(alaInsteredOn);
                        obj.MachineID = alaMachineID;
                        obj.Shift = shift;
                        obj.CorrectedDate = Convert.ToDateTime(CorrectedDate);

                        using (unitworksccsEntities db1 = new unitworksccsEntities())
                        {
                            db1.unitworkccs_alarm_history_master.Add(obj);
                            db1.SaveChanges();
                        }
                    }
                }
            }
        }

        //Reading Program when 8006 Code comes into Message history
        private int programfilter(ushort h, int machineid, String MessageDateTime) //, int MessageDBRow
        {
            MSqlConnection mc = new MSqlConnection();
            mc.open();
            String GetMachineDispName = "SELECT MachineDisplayName FROM  unitworksccs.`unitworkccs.tblmachinedetails` WHERE MachineID = " + machineid + ";";
            MySqlDataAdapter daTC = new MySqlDataAdapter(GetMachineDispName, mc.MqlConnection);
            System.Data.DataTable dtTC = new System.Data.DataTable();
            daTC.Fill(dtTC);
            bool SendMail = false;

            String queryProgNo = "SELECT ProgramNum,ProgDBit From  unitworksccs.`unitworkccs.tblmachinedetails` WHERE IsDeleted = 0 AND MachineID = " + machineid + " order by MachineID";
            MySqlDataAdapter daProgNo = new MySqlDataAdapter(queryProgNo, mc.MqlConnection);
            DataTable dtProgNo = new DataTable();
            daProgNo.Fill(dtProgNo);
            int progret, opti1, progret1;
            Focas1.ODBUP prodat = new Focas1.ODBUP(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            short progno = Convert.ToInt16(dtProgNo.Rows[0][0]);
            Focas1.ODBPRO progdata = new Focas1.ODBPRO(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            ushort prolen = 4 + 256;
            int progcount = 0;
            int progcount1 = 0;
            upstart:
            progret = Focas1.cnc_upstart(h, progno); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            if (progret == 0)
            {
                //Reading of the program
                opti1 = Focas1.cnc_upload(h, prodat, ref prolen); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                if (opti1 == 0)
                {
                    //Ending the reading function
                    progret1 = Focas1.cnc_upend(h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                    if (progret1 == 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < 256; i++)
                        {
                            sb.Append(prodat.data[i].ToString());
                        }
                        String[] progsplit = sb.ToString().Split('\n');
                        String[] empcodesplit;
                        String[] shiftsplit;
                        String[] opncodesplit1;
                        String[] WorkOrdsplit1;
                        String[] PartsProsplit1;
                        String[] PartsAccepsplit1;
                        String[] opncodesplit2;
                        String[] WorkOrdsplit2;
                        String[] PartsProsplit2;
                        String[] PartsAccepsplit2;
                        String[] opncodesplit3;
                        String[] WorkOrdsplit3;
                        String[] PartsProsplit3;
                        String[] PartsAccepsplit3;
                        String EmpCode = null;
                        String Shift = null;
                        String OpnCode1 = null;
                        String WorkOrder1 = null;
                        int PartsPro1 = 0;
                        int PartsAccep1 = 0;
                        int PartsRej1 = PartsPro1 - PartsAccep1;
                        String OpnCode2 = null;
                        String WorkOrder2 = null;
                        int PartsPro2 = 0;
                        int PartsAccep2 = 0;
                        int PartsRej2 = PartsPro2 - PartsAccep2;
                        String OpnCode3 = null;
                        String WorkOrder3 = null;
                        int PartsPro3 = 0;
                        int PartsAccep3 = 0;
                        int PartsRej3 = PartsPro3 - PartsAccep3;
                        int emp = 0, shf = 0, op1 = 0, wo1 = 0, pp1 = 0, pa1 = 0, op2 = 0, wo2 = 0, pp2 = 0, pa2 = 0, op3 = 0, wo3 = 0, pp3 = 0, pa3 = 0;
                        foreach (string prog in progsplit)
                        {
                            if (prog.Contains("(SHIFT)S"))
                            {
                                shiftsplit = prog.Split(')');
                                Shift = shiftsplit[1].Substring(1);
                                shf++;
                            }
                            else if (prog.Contains("(EMP CODE)E"))
                            {
                                empcodesplit = prog.Split(')');
                                EmpCode = empcodesplit[1].Substring(1);
                                emp++;
                            }
                            else if (prog.Contains("(WO 1)W") || prog.Contains("(WO1)W"))
                            {
                                WorkOrdsplit1 = prog.Split(')');
                                WorkOrder1 = WorkOrdsplit1[1].Substring(1);
                                wo1++;
                            }
                            else if (prog.Contains("(OPN 1)C") || prog.Contains("(OPN1)C"))
                            {
                                opncodesplit1 = prog.Split(')');
                                OpnCode1 = opncodesplit1[1].Substring(1);
                                op1++;
                            }
                            else if (prog.Contains("(PART PRO 1)PP") || prog.Contains("(PART PRO1)PP"))
                            {
                                PartsProsplit1 = prog.Split(')');
                                try
                                {
                                    PartsPro1 = Convert.ToInt32(PartsProsplit1[1].Substring(2));
                                }
                                catch
                                {
                                    PartsPro1 = Convert.ToInt32(PartsProsplit1[1].Substring(3));
                                }
                                pp1++;
                            }
                            else if (prog.Contains("(PART ACC 1)PA") || prog.Contains("(PART ACC1)PA"))
                            {
                                PartsAccepsplit1 = prog.Split(')');
                                try
                                {
                                    PartsAccep1 = Convert.ToInt32(PartsAccepsplit1[1].Substring(2));
                                }
                                catch
                                {
                                    PartsAccep1 = Convert.ToInt32(PartsAccepsplit1[1].Substring(3));
                                }
                                pa1++;
                            }
                            else if (prog.Contains("(WO 2)W") || prog.Contains("(WO2)W"))
                            {
                                WorkOrdsplit2 = prog.Split(')');
                                if (WorkOrdsplit2[1].Length > 1)
                                {
                                    WorkOrder2 = WorkOrdsplit2[1].Substring(1);
                                }

                                wo2++;
                            }
                            else if (prog.Contains("(OPN 2)C") || prog.Contains("(OPN2)C"))
                            {
                                opncodesplit2 = prog.Split(')');
                                if (opncodesplit2[1].Length > 1)
                                {
                                    OpnCode2 = opncodesplit2[1].Substring(1);
                                }

                                op2++;
                            }
                            else if (prog.Contains("(PART PRO 2)PP") || prog.Contains("(PART PRO2)PP"))
                            {
                                PartsProsplit2 = prog.Split(')');
                                if (PartsProsplit2[1].Length > 1)
                                {
                                    try
                                    {
                                        PartsPro2 = Convert.ToInt32(PartsProsplit2[1].Substring(2));
                                    }
                                    catch
                                    {
                                        PartsPro2 = Convert.ToInt32(PartsProsplit2[1].Substring(3));
                                    }
                                }

                                pp2++;
                            }
                            else if (prog.Contains("(PART ACC 2)PA") || prog.Contains("(PART ACC2)PA"))
                            {
                                PartsAccepsplit2 = prog.Split(')');
                                if (PartsAccepsplit2[1].Length > 1)
                                {
                                    try
                                    {
                                        PartsAccep2 = Convert.ToInt32(PartsAccepsplit2[1].Substring(2));
                                    }
                                    catch
                                    {
                                        PartsAccep2 = Convert.ToInt32(PartsAccepsplit2[1].Substring(3));
                                    }
                                }

                                pa2++;
                            }
                            else if (prog.Contains("(WO 3)W") || prog.Contains("WO3)W"))
                            {
                                WorkOrdsplit3 = prog.Split(')');
                                if (WorkOrdsplit3[1].Length > 1)
                                {
                                    WorkOrder3 = WorkOrdsplit3[1].Substring(1);
                                }

                                wo3++;
                            }
                            else if (prog.Contains("(OPN 3)C") || prog.Contains("(OPN3)C"))
                            {
                                opncodesplit3 = prog.Split(')');
                                if (opncodesplit3[1].Length > 1)
                                {
                                    OpnCode3 = opncodesplit3[1].Substring(1);
                                }

                                op3++;
                            }
                            else if (prog.Contains("(PART PRO 3)PP") || prog.Contains("(PART PRO3)PP"))
                            {
                                PartsProsplit3 = prog.Split(')');
                                if (PartsProsplit3[1].Length > 1)
                                {
                                    try
                                    {
                                        PartsPro3 = Convert.ToInt32(PartsProsplit3[1].Substring(2));
                                    }
                                    catch
                                    {
                                        PartsPro3 = Convert.ToInt32(PartsProsplit3[1].Substring(3));
                                    }
                                }

                                pp3++;
                            }
                            else if (prog.Contains("(PART ACC 3)PA") || prog.Contains("(PART ACC3)PA"))
                            {
                                PartsAccepsplit3 = prog.Split(')');
                                if (PartsAccepsplit3[1].Length > 1)
                                {
                                    try
                                    {
                                        PartsAccep3 = Convert.ToInt32(PartsAccepsplit3[1].Substring(2));
                                    }
                                    catch
                                    {
                                        PartsAccep3 = Convert.ToInt32(PartsAccepsplit3[1].Substring(3));
                                    }
                                }

                                pa3++;
                            }
                        }
                        PartsRej1 = PartsPro1 - PartsAccep1;
                        PartsRej2 = PartsPro2 - PartsAccep2;
                        PartsRej3 = PartsPro3 - PartsAccep3;
                        mc.close();
                        mc.open();
                        #region
                        if (emp == 0)
                        {
                            using (MSqlConnection SA = new MSqlConnection())
                            {
                                SA.open();
                                MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.'unitworkccs.operationlog' (OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('EMP CODE KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','EMP CODE'," + machineid + ")", SA.MqlConnection);
                                cmd.ExecuteNonQuery();
                                SA.close();
                            }
                            SendMail = true;
                        }
                        if (shf == 0)
                        {
                            using (MSqlConnection SA = new MSqlConnection())
                            {
                                SA.open();
                                MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('SHIFT KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','SHIFT'," + machineid + ")", mc.MqlConnection);
                                cmd.ExecuteNonQuery();
                                SA.close();
                            }
                            SendMail = true;
                        }
                        if (op1 == 0)
                        {
                            using (MSqlConnection SA = new MSqlConnection())
                            {
                                SA.open();
                                MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('OPN 1 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','OPN 1'," + machineid + ")", SA.MqlConnection);
                                cmd.ExecuteNonQuery();
                                SA.close();
                            }
                            SendMail = true;
                        }
                        if (op2 == 0)
                        {
                            using (MSqlConnection SA = new MSqlConnection())
                            {
                                SA.open();
                                MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('OPN 2 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','OPN 2'," + machineid + ")", SA.MqlConnection);
                                cmd.ExecuteNonQuery();
                                SA.close();
                            }
                            SendMail = true;
                        }
                        if (op3 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('OPN 3 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','OPN 3'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (wo1 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('WO 1 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Emp Code'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (wo2 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('WO 2 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','WO 2'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (wo3 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('WO 3 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','WO 3'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (pp1 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('PART PRO 1 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','PART PRO 1'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (pp2 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('PART PRO 2 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','PART PRO 2'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (pp3 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('PART PRO 3 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','PART PRO 3'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (pa1 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('PART ACC 1 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','PART ACC 1'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (pa2 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('PART ACC 2 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','PART ACC 2'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (pa3 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('PART ACC 3 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','PART ACC 3'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        #endregion
                        mc.open();
                        if (!SendMail)
                        {
                            //MySqlCommand cmd = new MySqlCommand("INSERT INTO program_temp(ProgramData,ProgramDateTime,MachineID) VALUES('" + sb.ToString() + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + machineid + ")", mc.sqlConnection);
                            //cmd.ExecuteNonQuery();

                            MySqlCommand cmd1 = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.program_master`(EmployeeCode,WorkOrderNo1,PartsProduced1,PartsAccepted1,PartsRejected1,MachineID,InsertedOn,ProgramDate,ProgramTime,ProgramDateTime,OpnCode1,OpnCode2,WorkOrderNo2,PartsProduced2,PartsAccepted2,PartsRejected2,OpnCode3,WorkOrderNo3,PartsProduced3,PartsAccepted3,PartsRejected3,Shift,CreatedOn)" +
                                "VALUES('" + EmpCode + "','" + WorkOrder1 + "'," + PartsPro1 + "," + PartsAccep1 + "," + PartsRej1 + "," + machineid + ",'" + Convert.ToDateTime(MessageDateTime).ToString("yyyy-MM-dd HH:mm:ss") + "','" + Convert.ToDateTime(MessageDateTime).ToString("yyyy-MM-dd") + "','" + Convert.ToDateTime(MessageDateTime).ToString("HH:mm:ss") + "','" + Convert.ToDateTime(MessageDateTime).ToString("yyyy-MM-dd HH:mm:ss") + "','" + OpnCode1 + "','" + OpnCode2 + "','" + WorkOrder2 + "'," + PartsPro2 + "," + PartsAccep2 + "," + PartsRej2 + ",'" + OpnCode3 + "','" + WorkOrder3 + "'," + PartsPro3 + "," + PartsAccep3 + "," + PartsRej3 + ",'" + Shift + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')", mc.MqlConnection);
                            cmd1.ExecuteNonQuery();

                            MySqlCommand cmd2 = new MySqlCommand("Update  unitworksccs.`unitworkccs_message_history_master` SET IsProgLock = 1 WHERE MachineID = " + machineid + " ", mc.MqlConnection);
                            cmd2.ExecuteNonQuery();

                            //var rdpmcdata = new Focas1.IODBPMC0();
                            //short adr_type = 0;
                            //short data_type = 0;
                            //ushort s_number = (ushort)Convert.ToInt16(dtProgNo.Rows[0][1]);
                            //ushort e_number = (ushort)Convert.ToInt16(dtProgNo.Rows[0][1]);
                            //ushort length = 9;
                            //short rdret = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdata);

                            //if (rdret == 0)
                            //{
                            //    var wrpmcdata = rdpmcdata;
                            //    wrpmcdata.cdata[0] = 0;
                            //    {
                            //        short wrret = Focas1.pmc_wrpmcrng(h, length, wrpmcdata);
                            //    }
                            //}
                        }
                        mc.close();
                        if (SendMail)
                        {
                            StringBuilder sb1 = new StringBuilder();
                            for (int i = 0; i < 256; i++)
                            {
                                sb1.Append(prodat.data[i].ToString());
                            }
                            ProgEmailEscalation(sb1.ToString(), dtTC.Rows[0][0].ToString(), machineid);

                            //var rdpmcdata = new Focas1.IODBPMC0();
                            //short adr_type = 9;
                            //short data_type = 0;
                            //ushort s_number = (ushort)Convert.ToInt16(dtProgNo.Rows[0][1]);
                            //ushort e_number = (ushort)Convert.ToInt16(dtProgNo.Rows[0][1]);
                            //ushort length = 9;
                            //short rdret = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdata);

                            //if (rdret == 0)
                            //{
                            //    var wrpmcdata = rdpmcdata;
                            //    wrpmcdata.cdata[0] = 1;
                            //    short wrret = Focas1.pmc_wrpmcrng(h, length, wrpmcdata);
                            //    for (int i = 0; i < 1000; i++)
                            //    {
                            //        int j = 0;
                            //    }
                            //    wrpmcdata.cdata[0] = 0;
                            //    wrret = Focas1.pmc_wrpmcrng(h, length, wrpmcdata);
                            //}
                        }
                        mc.close();
                    }
                }
                // Goto Upstart E2,W3,PP4,PA5
                else if (opti1 == 1)
                {
                    progcount++;
                    if (progcount == 4)
                    {

                        goto next;
                    }
                    goto upstart;
                }
            }
            else if (progret == -1)
            {
                progret1 = Focas1.cnc_upend(h);
                progcount1++;
                if (progcount1 == 4)
                {
                    goto next;
                }

                goto upstart;
            }
            next:
            if (progcount == 4)
            {
                return 4;
            }
            else if (progcount1 == 4)
            {
                return 4;
            }
            else
            {
                return 0;
            }
        }

        //Reading from Program 7531/8531 filtering the Data and Formatting the data and inserting into the DB Regular Period Logging
        private int programfilter1(ushort h, int machineid)
        {
            //Creating the Mail Body And changing the color of the Error Line - Pending

            MSqlConnection mc = new MSqlConnection();
            mc.open();
            String GetMachineDispName = "SELECT MachineDisplayName FROM unitworksccs.`unitworkccs_tblmachinedetails` WHERE MachineID = " + machineid + ";";
            MySqlDataAdapter daTC = new MySqlDataAdapter(GetMachineDispName, mc.MqlConnection);
            System.Data.DataTable dtTC = new System.Data.DataTable();
            daTC.Fill(dtTC);
            bool SendMail = false;
            String queryProgNo = "SELECT ProgramNum From  unitworksccs.`unitworkccs_tblmachinedetails` WHERE IsDeleted = 0 AND MachineID = " + machineid + " order by MachineID";
            MySqlDataAdapter daProgNo = new MySqlDataAdapter(queryProgNo, mc.MqlConnection);
            DataTable dtProgNo = new DataTable();
            daProgNo.Fill(dtProgNo);
            int progret, opti1, progret1;
            Focas1.ODBUP prodat = new Focas1.ODBUP(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            short progno = Convert.ToInt16(dtProgNo.Rows[0][0]);
            Focas1.ODBPRO progdata = new Focas1.ODBPRO(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            ushort prolen = 4 + 256;
            int progcount = 0;
            int progcount1 = 0;
            upstart:
            progret = Focas1.cnc_upstart(h, progno); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            if (progret == 0)
            {
                //Reading of the program
                opti1 = Focas1.cnc_upload(h, prodat, ref prolen); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                if (opti1 == 0)
                {
                    //Ending the reading function
                    progret1 = Focas1.cnc_upend(h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                    if (progret1 == 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < 256; i++)
                        {
                            sb.Append(prodat.data[i].ToString());
                        }
                        String[] progsplit = sb.ToString().Split('\n');
                        String[] empcodesplit;
                        String[] shiftsplit;
                        String[] opncodesplit1;
                        String[] WorkOrdsplit1;
                        String[] PartsProsplit1;
                        String[] PartsAccepsplit1;
                        String[] opncodesplit2;
                        String[] WorkOrdsplit2;
                        String[] PartsProsplit2;
                        String[] PartsAccepsplit2;
                        String[] opncodesplit3;
                        String[] WorkOrdsplit3;
                        String[] PartsProsplit3;
                        String[] PartsAccepsplit3;
                        String EmpCode = null;
                        String Shift = null;
                        String OpnCode1 = null;
                        String WorkOrder1 = null;
                        int PartsPro1 = 0;
                        int PartsAccep1 = 0;
                        int PartsRej1 = PartsPro1 - PartsAccep1;
                        String OpnCode2 = null;
                        String WorkOrder2 = null;
                        int PartsPro2 = 0;
                        int PartsAccep2 = 0;
                        int PartsRej2 = PartsPro2 - PartsAccep2;
                        String OpnCode3 = null;
                        String WorkOrder3 = null;
                        int PartsPro3 = 0;
                        int PartsAccep3 = 0;
                        int PartsRej3 = PartsPro3 - PartsAccep3;
                        int emp = 0, shf = 0, op1 = 0, wo1 = 0, pp1 = 0, pa1 = 0, op2 = 0, wo2 = 0, pp2 = 0, pa2 = 0, op3 = 0, wo3 = 0, pp3 = 0, pa3 = 0;
                        #region
                        foreach (string prog in progsplit)
                        {
                            if (prog.Contains("(SHIFT)S"))
                            {
                                shiftsplit = prog.Split(')');
                                Shift = shiftsplit[1].Substring(1);
                                shf++;
                            }
                            else if (prog.Contains("(EMP CODE)E"))
                            {
                                empcodesplit = prog.Split(')');
                                EmpCode = empcodesplit[1].Substring(1);
                                emp++;
                            }
                            else if (prog.Contains("(WO 1)W") || prog.Contains("(WO1)W"))
                            {
                                WorkOrdsplit1 = prog.Split(')');
                                WorkOrder1 = WorkOrdsplit1[1].Substring(1);
                                wo1++;
                            }
                            else if (prog.Contains("(OPN 1)C") || prog.Contains("(OPN1)C"))
                            {
                                opncodesplit1 = prog.Split(')');
                                OpnCode1 = opncodesplit1[1].Substring(1);
                                op1++;
                            }
                            else if (prog.Contains("(PART PRO 1)PP") || prog.Contains("(PART PRO1)PP"))
                            {
                                PartsProsplit1 = prog.Split(')');
                                try
                                {
                                    PartsPro1 = Convert.ToInt32(PartsProsplit1[1].Substring(2));
                                }
                                catch
                                {
                                    PartsPro1 = Convert.ToInt32(PartsProsplit1[1].Substring(3));
                                }
                                pp1++;
                            }
                            else if (prog.Contains("(PART ACC 1)PA") || prog.Contains("(PART ACC1)PA"))
                            {
                                PartsAccepsplit1 = prog.Split(')');
                                try
                                {
                                    PartsAccep1 = Convert.ToInt32(PartsAccepsplit1[1].Substring(2));
                                }
                                catch
                                {
                                    PartsAccep1 = Convert.ToInt32(PartsAccepsplit1[1].Substring(3));
                                }
                                pa1++;
                            }
                            else if (prog.Contains("(WO 2)W") || prog.Contains("(WO2)W"))
                            {
                                WorkOrdsplit2 = prog.Split(')');
                                if (WorkOrdsplit2[1].Length > 1)
                                {
                                    WorkOrder2 = WorkOrdsplit2[1].Substring(1);
                                }

                                wo2++;
                            }
                            else if (prog.Contains("(OPN 2)C") || prog.Contains("(OPN2)C"))
                            {
                                opncodesplit2 = prog.Split(')');
                                if (opncodesplit2[1].Length > 1)
                                {
                                    OpnCode2 = opncodesplit2[1].Substring(1);
                                }

                                op2++;
                            }
                            else if (prog.Contains("(PART PRO 2)PP") || prog.Contains("(PART PRO2)PP"))
                            {
                                PartsProsplit2 = prog.Split(')');
                                if (PartsProsplit2[1].Length > 1)
                                {
                                    try
                                    {
                                        PartsPro2 = Convert.ToInt32(PartsProsplit2[1].Substring(2));
                                    }
                                    catch
                                    {
                                        PartsPro2 = Convert.ToInt32(PartsProsplit2[1].Substring(3));
                                    }
                                }

                                pp2++;
                            }
                            else if (prog.Contains("(PART ACC 2)PA") || prog.Contains("(PART ACC2)PA"))
                            {
                                PartsAccepsplit2 = prog.Split(')');
                                if (PartsAccepsplit2[1].Length > 1)
                                {
                                    try
                                    {
                                        PartsAccep2 = Convert.ToInt32(PartsAccepsplit2[1].Substring(2));
                                    }
                                    catch
                                    {
                                        PartsAccep2 = Convert.ToInt32(PartsAccepsplit2[1].Substring(3));
                                    }
                                }

                                pa2++;
                            }
                            else if (prog.Contains("(WO 3)W") || prog.Contains("WO3)W"))
                            {
                                WorkOrdsplit3 = prog.Split(')');
                                if (WorkOrdsplit3[1].Length > 1)
                                {
                                    WorkOrder3 = WorkOrdsplit3[1].Substring(1);
                                }

                                wo3++;
                            }
                            else if (prog.Contains("(OPN 3)C") || prog.Contains("(OPN3)C"))
                            {
                                opncodesplit3 = prog.Split(')');
                                if (opncodesplit3[1].Length > 1)
                                {
                                    OpnCode3 = opncodesplit3[1].Substring(1);
                                }

                                op3++;
                            }
                            else if (prog.Contains("(PART PRO 3)PP") || prog.Contains("(PART PRO3)PP"))
                            {
                                PartsProsplit3 = prog.Split(')');
                                if (PartsProsplit3[1].Length > 1)
                                {
                                    try
                                    {
                                        PartsPro3 = Convert.ToInt32(PartsProsplit3[1].Substring(2));
                                    }
                                    catch
                                    {
                                        PartsPro3 = Convert.ToInt32(PartsProsplit3[1].Substring(3));
                                    }
                                }

                                pp3++;
                            }
                            else if (prog.Contains("(PART ACC 3)PA") || prog.Contains("(PART ACC3)PA"))
                            {
                                PartsAccepsplit3 = prog.Split(')');
                                if (PartsAccepsplit3[1].Length > 1)
                                {
                                    try
                                    {
                                        PartsAccep3 = Convert.ToInt32(PartsAccepsplit3[1].Substring(2));
                                    }
                                    catch
                                    {
                                        PartsAccep3 = Convert.ToInt32(PartsAccepsplit3[1].Substring(3));
                                    }
                                }

                                pa3++;
                            }
                        }
                        #endregion
                        PartsRej1 = PartsPro1 - PartsAccep1;
                        PartsRej2 = PartsPro2 - PartsAccep2;
                        PartsRej3 = PartsPro3 - PartsAccep3;
                        mc.close();
                        mc.open();
                        #region
                        if (emp == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('EMP CODE KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','EMP CODE'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (shf == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('SHIFT KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','SHIFT'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (op1 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('OPN 1 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','OPN 1'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (op2 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('OPN 2 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','OPN 2'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (op3 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('OPN 3 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','OPN 3'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (wo1 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('WO 1 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Emp Code'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (wo2 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('WO 2 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','WO 2'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (wo3 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('WO 3 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','WO 3'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (pp1 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('PART PRO 1 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','PART PRO 1'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (pp2 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('PART PRO 2 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','PART PRO 2'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (pp3 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('PART PRO 3 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','PART PRO 3'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (pa1 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('PART ACC 1 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','PART ACC 1'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (pa2 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('PART ACC 2 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','PART ACC 2'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        if (pa3 == 0)
                        {
                            mc.close();
                            mc.open();
                            MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('PART ACC 3 KeyWord Wrong.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','PART ACC 3'," + machineid + ")", mc.MqlConnection);
                            cmd.ExecuteNonQuery();
                            mc.close();
                            SendMail = true;
                        }
                        #endregion
                        mc.open();
                        String query = "SELECT EmployeeCode From unitworksccs.`unitworkccs.program_master` WHERE EmployeeCode = '" + EmpCode + "' AND OpnCode1 = '" + OpnCode1 + "' AND WorkOrderNo1 = '" + WorkOrder1 + "' AND PartsProduced1 = " + PartsPro1 + " AND PartsAccepted1 = " + PartsAccep1 + " AND OpnCode2 = '" + OpnCode2 + "' AND WorkOrderNo2 = '" + WorkOrder2 + "' AND PartsProduced2 = " + PartsPro2 + " AND PartsAccepted2 = " + PartsAccep2 + " AND OpnCode3 = '" + OpnCode3 + "' AND WorkOrderNo3 = '" + WorkOrder3 + "' AND PartsProduced3 = " + PartsPro3 + " AND PartsAccepted3 = " + PartsAccep3 + " AND MachineID = " + machineid + " AND Shift = '" + Shift + "' AND ProgramDate = '" + System.DateTime.Now.ToString("yyyy-MM-dd") + "'";
                        MySqlDataAdapter da = new MySqlDataAdapter(query, mc.MqlConnection);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        mc.close();

                        if (dt.Rows.Count == 0 && !SendMail)
                        {
                            mc.open();
                            //MySqlCommand cmd = new MySqlCommand("INSERT INTO program_temp(ProgramData,ProgramDateTime,MachineID) VALUES('" + sb.ToString() + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + machineid + ")", mc.sqlConnection);
                            //cmd.ExecuteNonQuery();

                            MySqlCommand cmd1 = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.program_master`(EmployeeCode,WorkOrderNo1,PartsProduced1,PartsAccepted1,PartsRejected1,MachineID,InsertedOn,ProgramDate,ProgramTime,ProgramDateTime,OpnCode1,OpnCode2,WorkOrderNo2,PartsProduced2,PartsAccepted2,PartsRejected2,OpnCode3,WorkOrderNo3,PartsProduced3,PartsAccepted3,PartsRejected3,Shift)" +
                                "VALUES('" + EmpCode + "','" + WorkOrder1 + "'," + PartsPro1 + "," + PartsAccep1 + "," + PartsRej1 + "," + machineid + ",'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + OpnCode1 + "','" + OpnCode2 + "','" + WorkOrder2 + "'," + PartsPro2 + "," + PartsAccep2 + "," + PartsRej2 + ",'" + OpnCode3 + "','" + WorkOrder3 + "'," + PartsPro3 + "," + PartsAccep3 + "," + PartsRej3 + ",'" + Shift + "')", mc.MqlConnection);
                            cmd1.ExecuteNonQuery();
                            mc.close();
                        }
                        if (SendMail)
                        {
                            StringBuilder sb1 = new StringBuilder();
                            for (int i = 0; i < 256; i++)
                            {
                                sb1.Append(prodat.data[i].ToString());
                            }
                            ProgEmailEscalation(sb1.ToString(), dtTC.Rows[0][0].ToString(), machineid);
                        }
                        mc.close();
                    }
                }
                // Goto Upstart E2,W3,PP4,PA5
                else if (opti1 == 1)
                {
                    progcount++;
                    if (progcount == 4)
                    {
                        goto next;
                    }

                    goto upstart;
                }
            }
            else if (progret == -1)
            {
                progret1 = Focas1.cnc_upend(h);
                progcount1++;
                if (progcount1 == 4)
                {
                    goto next;
                }

                goto upstart;
            }
            next:
            if (progcount == 4)
            {
                return 4;
            }
            else if (progcount1 == 4)
            {
                return 4;
            }
            else
            {
                return 0;
            }
        }

        //Sending the Program Escalation Email
        private void ProgEmailEscalation(String MailBody, String MachineInv, int machineid)
        {
            MSqlConnection mc = new MSqlConnection();
            mc.open();
            string SQLFromAdd = "SELECT FromEmailAdd,Password,username,domain FROM unitworksccs.`unitworkccs.frommail` Where IsDeleted = 0";
            MySqlDataAdapter daTC = new MySqlDataAdapter(SQLFromAdd, mc.MqlConnection);
            System.Data.DataTable dtTC = new System.Data.DataTable();
            daTC.Fill(dtTC);

            string SQLToAdd = "SELECT EmailID FROM  MailMasterProgEsc Where TOAdd = 1 AND IsDeleted = 0";
            MySqlDataAdapter daTO = new MySqlDataAdapter(SQLToAdd, mc.MqlConnection);
            System.Data.DataTable dtTO = new System.Data.DataTable();
            daTO.Fill(dtTO);

            string SQLCCAdd = "SELECT EmailID FROM  MailMasterProgEsc Where CCAdd = 1 AND IsDeleted = 0";
            MySqlDataAdapter daCC = new MySqlDataAdapter(SQLCCAdd, mc.MqlConnection);
            System.Data.DataTable dtCC = new System.Data.DataTable();
            daCC.Fill(dtCC);

            string SQLBCCAdd = "SELECT EmailID FROM  MailMasterProgEsc Where BCCAdd = 1 AND IsDeleted = 0";
            MySqlDataAdapter daBCC = new MySqlDataAdapter(SQLBCCAdd, mc.MqlConnection);
            System.Data.DataTable dtBCC = new System.Data.DataTable();
            daBCC.Fill(dtBCC);
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();

                message.From = new MailAddress(dtTC.Rows[0][0].ToString());
                for (int i = 0; i < dtTO.Rows.Count; i++)
                {
                    message.To.Add(new MailAddress(dtTO.Rows[i][0].ToString()));
                }
                for (int i = 0; i < dtCC.Rows.Count; i++)
                {
                    message.CC.Add(new MailAddress(dtCC.Rows[i][0].ToString()));
                }
                for (int i = 0; i < dtBCC.Rows.Count; i++)
                {
                    message.Bcc.Add(new MailAddress(dtBCC.Rows[i][0].ToString()));
                }

                String[] progsplit = MailBody.ToString().Split('\n');

                if (!progsplit[2].Contains("(SHIFT)S"))
                {
                    string shift = progsplit[2];
                    progsplit[2] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                if (!progsplit[3].Contains("(EMP CODE)E"))
                {
                    string shift = progsplit[3];
                    progsplit[3] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                if (!progsplit[4].Contains("(WO 1)W"))
                {
                    string shift = progsplit[4];
                    progsplit[4] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                if (!progsplit[5].Contains("(OPN 1)C"))
                {
                    string shift = progsplit[5];
                    progsplit[5] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                if (!progsplit[6].Contains("(PART PRO 1)PP") && !progsplit[6].Contains("(PART PRO1)PP"))
                {
                    string shift = progsplit[6];
                    progsplit[6] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                if (!progsplit[7].Contains("(PART ACC 1)PA") && !progsplit[7].Contains("(PART ACC1)PA"))
                {
                    string shift = progsplit[7];
                    progsplit[7] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                if (!progsplit[8].Contains("(WO 2)W"))
                {
                    string shift = progsplit[8];
                    progsplit[8] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                if (!progsplit[9].Contains("(OPN 2)C"))
                {
                    string shift = progsplit[9];
                    progsplit[9] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                if (!progsplit[10].Contains("(PART PRO 2)PP") && !progsplit[10].Contains("(PART PRO2)PP"))
                {
                    string shift = progsplit[10];
                    progsplit[10] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                if (!progsplit[11].Contains("(PART ACC 2)PA") && !progsplit[11].Contains("(PART ACC2)PA"))
                {
                    string shift = progsplit[11];
                    progsplit[11] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                if (!progsplit[12].Contains("(WO 3)W"))
                {
                    string shift = progsplit[12];
                    progsplit[12] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                if (!progsplit[13].Contains("(OPN 3)C"))
                {
                    string shift = progsplit[13];
                    progsplit[13] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                if (!progsplit[14].Contains("(PART PRO 3)PP") && !progsplit[14].Contains("(PART PRO3)PP"))
                {
                    string shift = progsplit[14];
                    progsplit[14] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                if (!progsplit[15].Contains("(PART ACC 3)PA") && !progsplit[15].Contains("(PART ACC3)PA"))
                {
                    string shift = progsplit[15];
                    progsplit[15] = "<b><span style=\"color:rgb(255,0,0)\">" + shift + "</span></b>";
                }
                String MainMailBody = "<table>";
                MainMailBody += "<tr bgcolor=\"#3bd6c6\"><td><b>Operator Entered Data</b></td><td><b>Standard Format</b></td></tr>";
                MainMailBody += "<tr><td>" + progsplit[0] + "</td><td>%</td></tr>";
                MainMailBody += "<tr bgcolor=\"#d3d3d3\"><td>" + progsplit[1] + "</td><td>O8531(JOB CARD)</td></tr>";
                MainMailBody += "<tr><td>" + progsplit[2] + "</td><td>(SHIFT)Sx</td></tr>";
                MainMailBody += "<tr bgcolor=\"#d3d3d3\"><td>" + progsplit[3] + "</td><td>(EMP CODE)E13xxxxxx</td></tr>";
                MainMailBody += "<tr><td>" + progsplit[4] + "</td><td>(WO 1)W63xxxxx</td></tr>";
                MainMailBody += "<tr bgcolor=\"#d3d3d3\"><td>" + progsplit[5] + "</td><td>(OPN 1)C7xxx</td></tr>";
                MainMailBody += "<tr><td>" + progsplit[6] + "</td><td>(PART PRO 1)PP0</td></tr>";
                MainMailBody += "<tr bgcolor=\"#d3d3d3\"><td>" + progsplit[7] + "</td><td>(PART ACC 1)PA0</td></tr>";
                MainMailBody += "<tr><td>" + progsplit[8] + "</td><td>(WO 2)W63xxxxx</td></tr>";
                MainMailBody += "<tr bgcolor=\"#d3d3d3\"><td>" + progsplit[9] + "</td><td>(OPN 2)C7xxx</td></tr>";
                MainMailBody += "<tr><td>" + progsplit[10] + "</td><td>(PART PRO 2)PP0</td></tr>";
                MainMailBody += "<tr bgcolor=\"#d3d3d3\"><td>" + progsplit[11] + "</td><td>(PART ACC 2)PA0</td></tr>";
                MainMailBody += "<tr><td>" + progsplit[12] + "</td><td>(WO 3)W63xxxxx</td></tr>";
                MainMailBody += "<tr bgcolor=\"#d3d3d3\"><td>" + progsplit[13] + "</td><td>(OPN 3)C7xxx</td></tr>";
                MainMailBody += "<tr><td>" + progsplit[14] + "</td><td>(PART PRO 3)PP0</td></tr>";
                MainMailBody += "<tr bgcolor=\"#d3d3d3\"><td>" + progsplit[15] + "</td><td>(PART ACC 3)PA0</td></tr>";
                MainMailBody += "<tr><td>" + progsplit[16] + "</td><td>%</td></tr>";
                MainMailBody += "</table>";
                message.Subject = "Program Entry Needs to be Corrected for the Machine " + MachineInv + " at " + System.DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");
                message.Body = "<div>" +
                               "<p><b>Dear All,</b></p>" +
                               "<p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Please find the <b><span>Program Details</span></b> data for the " +
                               "&nbsp;<span>Machine Inventory Number</span> &nbsp;<b>" + MachineInv + "</b> at &nbsp;<b>" + System.DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt") + "</b>.</p>" +
                                "<div>" +
                                "<div style=\"float:left\">" +
                                "<pre>" +
                                //"<b>Operator Entered Data</b>\n" +
                                "" + MainMailBody +
                                "</p></pre>" +
                                "<p><font><span style=\"font-family:arial,helvetica,sans-serif\"><span style=\"color:rgb(11,83,148)\"><span style=\"background-color:rgb(255,255,255)\">" +
                                "<i><span><font size=\"2\"><span style=\"font-family:comic sans ms,sans-serif\">“Automatic System generated Mail and No incoming mail facility is available” </span></font></span></i><b><span><br>" +
                                "</span></b></span></span></span></font></p>" +
                                "</div>" +
                                //"<div style=\"float:left\">" +
                                //"<pre>" +
                                //"" + StandardProg +
                                //"</pre>" +
                                //"</div>" +
                                "</div>" +
                                "</div>";
                message.IsBodyHtml = true;

                smtp.Port = 25;
                smtp.Host = "mail.titan.co.in";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(dtTC.Rows[0][2].ToString(), dtTC.Rows[0][1].ToString(), dtTC.Rows[0][3].ToString());
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object s,
                System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                System.Security.Cryptography.X509Certificates.X509Chain chain,
                System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Program Escalation Email was not Sent: " + ex.Message + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','App Error'," + machineid + ")", mc.MqlConnection);
                cmd.ExecuteNonQuery();
            }
            mc.close();
        }

        private void ProgramInsertCheck(ushort h, int machineid)
        {
            MSqlConnection mc = new MSqlConnection();
            mc.open();
            String query = "SELECT MessageID,MessageDateTime From  unitworksccs.`unitworkccs.message_history_master` WHERE MessageCode = '8006' AND IsProgLock = 0 AND MachineID = " + machineid + " Order By MachineID DESC;";
            MySqlDataAdapter da = new MySqlDataAdapter(query, mc.MqlConnection);
            DataTable dt = new DataTable();
            da.Fill(dt);
            mc.close();
            if (dt.Rows.Count != 0)
            {
                int ret = programfilter(h, machineid, dt.Rows[0][1].ToString()); //, Convert.ToInt32(dt.Rows[0][1])
                if (ret == 4)
                {
                    mc.close();
                    mc.open();
                    MySqlCommand cmd = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('NC Program Cant be read, due to upload error from M/c.','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error'," + machineid + ")", mc.MqlConnection);
                    cmd.ExecuteNonQuery();
                    mc.close();
                }
            }
        }

        //Inserting the Axis, Feedrate and Spindle Details
        private void InsertAxisDetails(ushort h, int machineid, int NoOfAxis)
        {
            Focas1.ODBDY2_2 ReadVar = new Focas1.ODBDY2_2();
            short axisdet = 1;
            short datalength = 44;// (28 + 4 * (4 * n));
            int insert = -1;
            for (int i = 0; i < NoOfAxis; i++)
            {
                short posdataret = Focas1.cnc_rddynamic2(h, axisdet, datalength, ReadVar);
                axisdet++;

                double AbsPosDb = Convert.ToDouble(ReadVar.pos.absolute);
                double RelPosDb = Convert.ToDouble(ReadVar.pos.relative);
                double MacPosDb = Convert.ToDouble(ReadVar.pos.machine);
                double DisPosDb = Convert.ToDouble(ReadVar.pos.distance);

                String AbsPos = AbsPosDb.ToString();
                String RelPos = RelPosDb.ToString();
                String MacPos = MacPosDb.ToString();
                String DisPos = DisPosDb.ToString();
                String AxisNo = ReadVar.axis.ToString();

                if (Math.Abs(AbsPosDb).ToString().Length > 3)
                {
                    AbsPos = AbsPos.Insert(AbsPos.Length - 3, ".");
                }
                if (Math.Abs(RelPosDb).ToString().Length > 3)
                {
                    RelPos = RelPos.Insert(RelPos.Length - 3, ".");
                }
                if (Math.Abs(MacPosDb).ToString().Length > 3)
                {
                    MacPos = MacPos.Insert(MacPos.Length - 3, ".");
                }
                if (Math.Abs(DisPosDb).ToString().Length > 3)
                {
                    DisPos = DisPos.Insert(DisPos.Length - 3, ".");
                }

                using (MSqlConnection mc = new MSqlConnection())
                {
                    mc.open();

                    String GetAxis = "SELECT AxisName FROM unitworksccs.`unitworkccs.tbl_axisdet` WHERE MachineID = " + machineid + " and AxisID = " + Convert.ToInt32(AxisNo) + " and IsDeleted = 0;";
                    MySqlDataAdapter daTC = new MySqlDataAdapter(GetAxis, mc.MqlConnection);
                    System.Data.DataTable dtTC = new System.Data.DataTable();
                    daTC.Fill(dtTC);

                    if (dtTC.Rows.Count > 0)
                    {
                        AxisNo = dtTC.Rows[0][0].ToString();
                    }

                    if (AxisNo != "" || AxisNo != null)
                    {
                        MySqlCommand cmd1 = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.tbl_axisdetails1`(MachineID,Axis,AbsPos,RelPos,MacPos,DistPos,StartTime,IsDeleted,InsertedOn)" + //
                            "VALUES('" + machineid + "','" + AxisNo + "','" + AbsPos + "'," + RelPos + ",'" + MacPos + "','" + DisPos + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:00") + "',0,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')", mc.MqlConnection);
                        cmd1.ExecuteNonQuery();
                        insert++;
                    }

                    if (i == 0 || insert == 0)
                    {
                        String FeedRate = ReadVar.actf.ToString();
                        String SpindleSpeed = ReadVar.acts.ToString();
                        short datanum = 10;
                        Focas1.ODBSPLOAD SpiLoad = new Focas1.ODBSPLOAD();
                        short SpiLoadRet = Focas1.cnc_rdspmeter(h, 0, ref datanum, SpiLoad);
                        int SpiLoadI = SpiLoad.spload1.spload.data;
                        int SpiLoadDec = SpiLoad.spload1.spload.dec;
                        String SpiLoadMain = SpiLoadI.ToString().Insert(SpiLoadDec, ".");
                        String SpindleLoad = SpiLoad.spload1.spload.data.ToString();

                        MySqlCommand cmd2 = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.tbl_axisdetails2`(MachineID,FeedRate,SpindleLoad,SpindleSpeed,StartTime,IsDeleted,InsertedOn)" + //
                            "VALUES('" + machineid + "','" + FeedRate + "','" + SpiLoadMain.ToString() + "'," + SpindleSpeed + ",'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:00") + "',0,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')", mc.MqlConnection);
                        cmd2.ExecuteNonQuery();
                    }
                    mc.close();
                }
            }
        }

        //Inserting the Servo Details
        private void InsertServoDetails(ushort h, int machineid, int NoOfAxis)
        {
            short DataLen = 3;
            Focas1.ODBAXDT ServLoad = new Focas1.ODBAXDT();
            Focas1.ODBAXDT ServCurrPer = new Focas1.ODBAXDT();
            Focas1.ODBAXDT ServCurrAmp = new Focas1.ODBAXDT();
            short ServRetLoad = Focas1.cnc_rdaxisdata(h, 2, 0, 1, ref DataLen, ServLoad);
            short ServRetCurPer = Focas1.cnc_rdaxisdata(h, 2, 1, 1, ref DataLen, ServCurrPer);
            short ServRetCurAmp = Focas1.cnc_rdaxisdata(h, 2, 2, 1, ref DataLen, ServCurrAmp);

            for (int i = 0; i < NoOfAxis; i++)
            {
                switch (i)
                {
                    case 0:
                        String AxisName = ServLoad.data1.name;
                        int SpiLoadI = ServLoad.data1.data;
                        int SpiLoadDec = ServLoad.data1.dec;
                        String ServLoadMain = SpiLoadI.ToString();
                        if (SpiLoadDec != 0)
                        {
                            ServLoadMain = SpiLoadI.ToString().Insert(SpiLoadDec, ".");
                        }
                        int ServcurrPerI = ServCurrPer.data1.data;
                        int ServcurrPerD = ServCurrPer.data1.dec;
                        String ServCurPerMain = ServcurrPerI.ToString();
                        if (Math.Abs(ServcurrPerI).ToString().Length > 2 && ServcurrPerD > 0)
                        {
                            ServCurPerMain = ServcurrPerI.ToString().Insert(ServcurrPerD, ".");
                        }
                        int ServcurrAmpI = ServCurrAmp.data1.data;
                        int ServcurrAmpD = ServCurrAmp.data1.dec;
                        String ServcurrAmpMain = ServcurrAmpI.ToString();
                        if (Math.Abs(ServcurrAmpI).ToString().Length > 2 && ServcurrAmpD > 0)
                        {
                            ServcurrAmpMain = ServcurrAmpI.ToString().Insert(ServcurrAmpD, ".");
                        }

                        using (MSqlConnection mc = new MSqlConnection())
                        {
                            if (AxisName != "" || AxisName != null)
                            {
                                mc.open();
                                MySqlCommand cmd1 = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.tbl_servodetails`(MachineID,ServoAxis,ServoLoadMeter,LoadCurrent,LoadCurrentAmp,StartDateTime,IsDeleted,InsertedOn,InsertedBy)" + //
                                        "VALUES('" + machineid + "','" + AxisName + "','" + ServLoadMain + "','" + ServCurPerMain + "','" + ServcurrAmpMain + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',1)", mc.MqlConnection);
                                cmd1.ExecuteNonQuery();
                                mc.close();
                            }
                        }
                        break;
                    case 1:
                        String AxisName2 = ServLoad.data2.name;
                        int SpiLoadI2 = ServLoad.data2.data;
                        int SpiLoadDec2 = ServLoad.data2.dec;
                        String ServLoadMain2 = SpiLoadI2.ToString();
                        if (SpiLoadDec2 != 0)
                        {
                            ServLoadMain2 = SpiLoadI2.ToString().Insert(SpiLoadDec2, ".");
                        }
                        int ServcurrPerI2 = ServCurrPer.data2.data;
                        int ServcurrPerD2 = ServCurrPer.data2.dec;
                        String ServCurPerMain2 = ServcurrPerI2.ToString();
                        if (Math.Abs(ServcurrPerI2).ToString().Length > 2 && ServcurrPerD2 > 0)
                        {
                            ServCurPerMain2 = ServcurrPerI2.ToString().Insert(ServcurrPerD2, ".");
                        }
                        int ServcurrAmpI2 = ServCurrAmp.data2.data;
                        int ServcurrAmpD2 = ServCurrAmp.data2.dec;
                        String ServcurrAmpMain2 = ServcurrAmpI2.ToString();
                        if (Math.Abs(ServcurrAmpI2).ToString().Length > 2 && ServcurrAmpD2 > 0)
                        {
                            ServcurrAmpMain2 = ServcurrAmpI2.ToString().Insert(ServcurrAmpD2, ".");
                        }

                        using (MSqlConnection mc = new MSqlConnection())
                        {
                            if (AxisName2 != "" || AxisName2 != null)
                            {
                                mc.open();
                                MySqlCommand cmd1 = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.tbl_servodetails`(MachineID,ServoAxis,ServoLoadMeter,LoadCurrent,LoadCurrentAmp,StartDateTime,IsDeleted,InsertedOn,InsertedBy)" + //
                                        "VALUES('" + machineid + "','" + AxisName2 + "','" + ServLoadMain2 + "','" + ServCurPerMain2 + "','" + ServcurrAmpMain2 + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',1)", mc.MqlConnection);
                                cmd1.ExecuteNonQuery();
                                mc.close();
                            }
                        }
                        break;
                    case 2:
                        String AxisName3 = ServLoad.data3.name;
                        int SpiLoadI3 = ServLoad.data3.data;
                        int SpiLoadDec3 = ServLoad.data3.dec;
                        String ServLoadMain3 = SpiLoadI3.ToString();
                        if (SpiLoadDec3 != 0)
                        {
                            ServLoadMain3 = SpiLoadI3.ToString().Insert(SpiLoadDec3, ".");
                        }
                        int ServcurrPerI3 = ServCurrPer.data3.data;
                        int ServcurrPerD3 = ServCurrPer.data3.dec;
                        String ServCurPerMain3 = ServcurrPerI3.ToString();
                        if (Math.Abs(ServcurrPerI3).ToString().Length > 2 && ServcurrPerD3 > 0)
                        {
                            ServCurPerMain3 = ServcurrPerI3.ToString().Insert(ServcurrPerD3, ".");
                        }
                        int ServcurrAmpI3 = ServCurrAmp.data3.data;
                        int ServcurrAmpD3 = ServCurrAmp.data3.dec;
                        String ServcurrAmpMain3 = ServcurrAmpI3.ToString();
                        if (Math.Abs(ServcurrAmpI3).ToString().Length > 2 && ServcurrAmpD3 > 0)
                        {
                            ServcurrAmpMain3 = ServcurrAmpI3.ToString().Insert(ServcurrAmpD3, ".");
                        }
                        using (MSqlConnection mc = new MSqlConnection())
                        {
                            if (AxisName3 != "" || AxisName3 != null)
                            {
                                mc.open();
                                MySqlCommand cmd1 = new MySqlCommand("INSERT INTO  unitworksccs.`unitworkccs.tbl_servodetails`(MachineID,ServoAxis,ServoLoadMeter,LoadCurrent,LoadCurrentAmp,StartDateTime,IsDeleted,InsertedOn,InsertedBy)" + //
                                        "VALUES('" + machineid + "','" + AxisName3 + "','" + ServLoadMain3 + "','" + ServCurPerMain3 + "','" + ServcurrAmpMain3 + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',1)", mc.MqlConnection);
                                cmd1.ExecuteNonQuery();
                                mc.close();
                            }
                        }
                        break;
                    case 3:
                        String AxisName4 = ServLoad.data4.name;
                        int SpiLoadI4 = ServLoad.data4.data;
                        int SpiLoadDec4 = ServLoad.data4.dec;
                        String ServLoadMain4 = SpiLoadI4.ToString();
                        if (SpiLoadDec4 != 0)
                        {
                            ServLoadMain4 = SpiLoadI4.ToString().Insert(SpiLoadDec4, ".");
                        }
                        int ServcurrPerI4 = ServCurrPer.data4.data;
                        int ServcurrPerD4 = ServCurrPer.data4.dec;
                        String ServCurPerMain4 = ServcurrPerI4.ToString();
                        if (Math.Abs(ServcurrPerI4).ToString().Length > 2 && ServcurrPerD4 > 0)
                        {
                            ServCurPerMain4 = ServcurrPerI4.ToString().Insert(ServcurrPerD4, ".");
                        }
                        int ServcurrAmpI4 = ServCurrAmp.data4.data;
                        int ServcurrAmpD4 = ServCurrAmp.data4.dec;
                        String ServcurrAmpMain4 = ServcurrAmpI4.ToString();
                        if (Math.Abs(ServcurrAmpI4).ToString().Length > 2 && ServcurrAmpD4 > 0)
                        {
                            ServcurrAmpMain4 = ServcurrAmpI4.ToString().Insert(ServcurrAmpD4, ".");
                        }
                        using (MSqlConnection mc = new MSqlConnection())
                        {
                            if (AxisName4 != "" || AxisName4 != null)
                            {
                                mc.open();
                                MySqlCommand cmd1 = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.tbl_servodetails`(MachineID,ServoAxis,ServoLoadMeter,LoadCurrent,LoadCurrentAmp,StartDateTime,IsDeleted,InsertedOn,InsertedBy)" + //
                                        "VALUES('" + machineid + "','" + AxisName4 + "','" + ServLoadMain4 + "','" + ServCurPerMain4 + "','" + ServcurrAmpMain4 + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',1)", mc.MqlConnection);
                                cmd1.ExecuteNonQuery();
                                mc.close();
                            }
                        }
                        break;
                }
            }
        }

        private void GetToolATC(ushort h, int MachineID)
        {
            short ModalTypeT = 108;
            short ModalBlockT1 = 0;

            Focas1.ODBMDL_3 ModaldataT1 = new Focas1.ODBMDL_3();

            short TModalRet1 = Focas1.cnc_modal(h, ModalTypeT, ModalBlockT1, ModaldataT1);
            int CurATCVal = ModaldataT1.aux.aux_data;
            bool Insertret = IncrementATCCounter(h, MachineID, CurATCVal);
        }

        public bool IncrementATCCounter(ushort h, int MacID, int CurATCVal)
        {
            int CycleStart = GetMachineStatus(h, MacID);
            DataTable dataHolder = new DataTable();
            DataTable dataHolder1 = new DataTable();
            DateTime Correcteddate = GetCorrectedDate();
            int WOID = 0;
            using (MSqlConnection mc = new MSqlConnection())
            {
                mc.open();
                string SelectQuery = "SELECT HMIID FROM  unitworksccs.`unitworkccs.tblworkorderentry` where MachineID = '" + MacID + "' and IsStarted = 1 and IsFinished = 0 and IsHold = 0 order by ToolLifeCycID DESC ;";
                MySqlDataAdapter da1 = new MySqlDataAdapter(SelectQuery, mc.MqlConnection);
                da1.Fill(dataHolder1);

                if (dataHolder1.Rows.Count > 0)
                {
                    WOID = Convert.ToInt32(dataHolder1.Rows[0][0]);
                }
            }

            if (CycleStart == 1 && WOID != 0)
            {
                using (MSqlConnection mc = new MSqlConnection())
                {
                    mc.open();
                    string SelectQuery = "SELECT ToolNo,toollifecounter,ToolLifeID FROM unitworksccs.`unitworkccs.tbltoollifeoperator` where MachineID = '" + MacID + "' and ToolNo = " + CurATCVal + " and HMIID = " + WOID + " and IsCompleted = 0 and IsCycleStart = 0 order by ToolLifeID DESC ;";
                    MySqlDataAdapter da1 = new MySqlDataAdapter(SelectQuery, mc.MqlConnection);
                    da1.Fill(dataHolder);
                    mc.close();
                }
                if (dataHolder.Rows.Count > 0)
                {
                    int prvATCVal = Convert.ToInt32(dataHolder.Rows[0][0]);
                    int CycCounter = Convert.ToInt32(dataHolder.Rows[0][1]) + 1;
                    int ToolLifeID = Convert.ToInt32(dataHolder.Rows[0][3]);
                    using (MSqlConnection mc = new MSqlConnection())
                    {
                        mc.open();
                        MySqlCommand cmdUpdateRows = new MySqlCommand("UPDATE unitworksccs.`unitworkccs.tbltoollifeoperator` Set toollifecounter = " + CycCounter + ", IsCycleStart = 1  Where ToolLifeID = " + ToolLifeID + "");
                        cmdUpdateRows.ExecuteNonQuery();
                        mc.close();
                    }
                }
            }
            else if (CycleStart == 0 && WOID != 0)
            {
                using (MSqlConnection mc = new MSqlConnection())
                {
                    mc.open();
                    MySqlCommand cmdUpdateRows = new MySqlCommand("UPDATE unitworksccs.`unitworkccs.tbltoollifeoperator` Set IsCycleStart = 0 Where HMIID = " + WOID + " and IsCompleted = 0;");
                    cmdUpdateRows.ExecuteNonQuery();
                    mc.close();
                }

                //using (MsqlConnection mc = new MsqlConnection())
                //{
                //    mc.open();
                //    string SelectQuery = "SELECT ToolNo,Counter,CycleStartTime,CycleEndTime,PartCounter, ToolLifeCycID FROM  tblToolLifeCycle where MachineID = '" + MacID + "' and ToolNo = " + CurATCVal + " and WOID = " + WOID + "  order by ToolLifeCycID DESC ;";
                //    SqlDataAdapter da1 = new SqlDataAdapter(SelectQuery, mc.sqlConnection);
                //    da1.Fill(dataHolder);
                //    mc.close();
                //}
                //if (dataHolder.Rows.Count > 0)
                //{
                //    int prvATCVal = Convert.ToInt32(dataHolder.Rows[0][0]);
                //    String CycStrtTime = dataHolder.Rows[0][2].ToString();
                //    String CycEndTime = dataHolder.Rows[0][3].ToString();
                //    int CycCounter = Convert.ToInt32(dataHolder.Rows[0][1]) + 1;
                //    int ToolLifeID = Convert.ToInt32(dataHolder.Rows[0][5]);
                //    int PartCounter = Convert.ToInt32(dataHolder.Rows[0][4]) + 1;
                //    if ((CycStrtTime != "" || CycStrtTime != null) && (CycEndTime != null && CycEndTime != ""))
                //    {
                //        using (MsqlConnection mc = new MsqlConnection())
                //        {
                //            mc.open();
                //            MySqlCommand cmdUpdateRows = new MySqlCommand("UPDATE  tblToolLifeCycle Set CycleCounter = " + PartCounter + ", CycleEndTime = '" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', PartCounter =  Where ToolLifeCycID = " + ToolLifeID + "");
                //            //MySqlCommand cmdUpdateRows = new MySqlCommand("Insert into  tblToolLifeCycle (MachineID,ToolNo,CycleCounter,CycleStartTime,CorrectedDate)" + "" +
                //            //    " values ('" + MacID + "'," + CurATCVal + "," + CycCounter + ",'" + DateTime.Now + "',' " + Correcteddate + "')");
                //            cmdUpdateRows.ExecuteNonQuery();
                //            mc.close();
                //        }
                //    }
                //}
            }
            return false;
        }

        public int GetMachineStatus(ushort h, int MachineID)
        {
            int retstatus = 0;
            Focas1.ODBST MacStatus = new Focas1.ODBST();
            short StatRet = Focas1.cnc_statinfo(h, MacStatus);
            if (MacStatus.aut == 3 && MacStatus.run == 2)
            {
                retstatus = 1;
            }
            else if (MacStatus.aut == 3 && MacStatus.run == 0)
            {
                retstatus = 1;
            }
            else
            {
                retstatus = 0;
            }
            return retstatus;
        }

        //Get the Machine mode - run every minute for Fanuc Controllers
        #region NEW CODE            
        private void getmachinemode(int MacID, string IPAddress, ushort h, int MachineLockBit, int MachineIdleBit, int MachineUnlockBit, int MachineIdleMin, int ConnectionRet, int ConnectionRetErr, int EnableLock, unitworkccs_shift_master shiftmasterdet)
        {
            DateTime correctedDate = DateTime.Now;
            unitworkccs_tbldaytiming Daytimings = new unitworkccs_tbldaytiming();
            using (unitworksccsEntities db = new unitworksccsEntities())
            {
                Daytimings = db.unitworkccs_tbldaytiming.Where(m => m.IsDeleted == 0).FirstOrDefault();
            }
            DateTime Start = Convert.ToDateTime(Daytimings.StartTime.ToString());
            DateTime st = Convert.ToDateTime(Daytimings.StartTime.ToString());
            if (Start <= DateTime.Now)
            {
                correctedDate = DateTime.Now.Date;
            }
            else
            {
                correctedDate = DateTime.Now.AddDays(-1).Date;
            }
            var parameterdet = new List<unitworkccs_parameters_master>();
            var ModeDetList = new List<unitworkccs_tbllivemode>();
            var Machinedet = new unitworkccs_tblmachinedetails();
            using (unitworksccsEntities db = new unitworksccsEntities())
            {
                parameterdet = db.unitworkccs_parameters_master.Where(m => m.MachineID == MacID).OrderByDescending(m => m.ParameterID).ToList();
                ModeDetList = db.unitworkccs_tbllivemode.Where(m => m.MachineID == MacID && m.IsCompleted == 0 || (m.IsCompleted == 1 && m.ModeTypeEnd == 0)).ToList();
                Machinedet = db.unitworkccs_tblmachinedetails.Find(MacID);
            }
            int shiftid = 1;
            if (shiftmasterdet != null)
            {
                shiftid = shiftmasterdet.ShiftID;
            }
            DataTable dtMode = new DataTable();
            DataTable DtModePOFF = new DataTable();
            if (ModeDetList.Count == 1)
            {
                String MacMode = ModeDetList[0].MacMode.ToString();
                int ModeID = Convert.ToInt32(ModeDetList[0].ModeID.ToString());
                String StartTime = ModeDetList[0].StartTime.ToString();
                DateTime CorrectedDateDB = Convert.ToDateTime(ModeDetList[0].CorrectedDate.ToString()).Date;
                String ColoCode = ModeDetList[0].ColorCode.ToString();
                String LossCodeID = ModeDetList[0].LossCodeID.ToString();
                String BDID = ModeDetList[0].BreakdownID.ToString();
                int StartIdle = Convert.ToInt32(ModeDetList[0].StartIdle.ToString());
                String ModeType = ModeDetList[0].ModeType.ToString();
                int PrevShift = ModeDetList[0].IsShiftEnd;  //previous shift
                DateTime nowdate = DateTime.Now;

                if (PrevShift != shiftid)
                {
                    #region shiftEnd
                    if (shiftmasterdet != null)
                    {
                        try
                        {
                            string shiftST = GetCorrectedDate().ToString("yyyy-MM-dd") + " " + shiftmasterdet.StartTime;
                            DateTime ShiftETDT = Convert.ToDateTime(shiftST);
                            if (ShiftETDT.Hour <= nowdate.Hour)
                            {
                                //if (shiftid == 3)
                                //{
                                //    ShiftETDT = ShiftETDT.AddDays(1);
                                //}
                                string Et = ShiftETDT.AddSeconds(-1).ToString("HH:mm:ss");
                                string start = nowdate.ToString("yyyy-MM-dd")+ " "+ shiftmasterdet.StartTime.ToString();
                                st = Convert.ToDateTime(start);
                                //DateTime NowDateCalc = Convert.ToDateTime(nowdate.ToString("yyyy-MM-dd 0" + (Start - 1) + ":59:59"));
                                DateTime NowDateCalc = Convert.ToDateTime(nowdate.ToString("yyyy-MM-dd " + Et));
                                int durationinsec = Convert.ToInt32(NowDateCalc.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
                                String StartTimeNext = nowdate.ToString("yyyy-MM-dd " + st.ToString("HH:mm:ss"));
                                UpdatetbllivemodeDetails(NowDateCalc, durationinsec, ModeID);
                                if (LossCodeID.Length == 0 && BDID.Length == 0)
                                {
                                    INSERTMODE(MacMode, MacID, 1, System.DateTime.Now, correctedDate, Convert.ToDateTime(StartTimeNext), ColoCode, ModeType, shiftid);

                                }
                                else if (LossCodeID.Length != 0)
                                {
                                    unitworkccs_tbllivemode row = new unitworkccs_tbllivemode();
                                    row.MacMode = MacMode;
                                    row.MachineID = MacID;
                                    row.InsertedBy = 1;
                                    row.InsertedOn = System.DateTime.Now;
                                    row.CorrectedDate = correctedDate;
                                    row.StartTime = System.DateTime.Now;
                                    row.ColorCode = ColoCode;
                                    row.LossCodeID = Convert.ToInt32(LossCodeID);
                                    row.ModeType = ModeType;
                                    row.IsShiftEnd = shiftid;//for shift end
                                    using (unitworksccsEntities db1 = new unitworksccsEntities())
                                    {
                                        db1.unitworkccs_tbllivemode.Add(row);
                                        db1.SaveChanges();
                                    }

                                }
                                else if (BDID.Length != 0)
                                {
                                    unitworkccs_tbllivemode row = new unitworkccs_tbllivemode();
                                    row.MacMode = MacMode;
                                    row.MachineID = MacID;
                                    row.InsertedBy = 1;
                                    row.InsertedOn = System.DateTime.Now;
                                    row.CorrectedDate = correctedDate;
                                    row.StartTime = System.DateTime.Now;
                                    row.ColorCode = ColoCode;
                                    row.BreakdownID = Convert.ToInt32(BDID);
                                    row.ModeType = ModeType;
                                    row.IsShiftEnd = shiftid;  //for shift end
                                    using (unitworksccsEntities db1 = new unitworksccsEntities())
                                    {
                                        db1.unitworkccs_tbllivemode.Add(row);
                                        db1.SaveChanges();
                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            IntoFile(ex.ToString());
                        }


                    }
                    #endregion
                }
                if (correctedDate != CorrectedDateDB)
                {
                    try
                    {
                        st = Convert.ToDateTime(Daytimings.StartTime.ToString());
                        string Et = st.AddSeconds(-1).ToString("HH:mm:ss");
                        //DateTime NowDateCalc = Convert.ToDateTime(nowdate.ToString("yyyy-MM-dd 0" + (Start - 1) + ":59:59"));
                        DateTime NowDateCalc = Convert.ToDateTime(nowdate.ToString("yyyy-MM-dd " + Et));
                        int durationinsec = Convert.ToInt32(NowDateCalc.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
                        String StartTimeNext = nowdate.ToString("yyyy-MM-dd " + st.ToString("HH:mm:ss"));
                        UpdatetbllivemodeDetails(NowDateCalc, durationinsec, ModeID);   // Update Previous Mode
                        if (LossCodeID.Length == 0 && BDID.Length == 0)
                        {
                            INSERTMODE(MacMode, MacID, 1, System.DateTime.Now, correctedDate, Convert.ToDateTime(StartTimeNext), ColoCode, ModeType, shiftid);

                        }
                        else if (LossCodeID.Length != 0)
                        {
                            unitworkccs_tbllivemode row = new unitworkccs_tbllivemode();
                            row.MacMode = MacMode;
                            row.MachineID = MacID;
                            row.InsertedBy = 1;
                            row.InsertedOn = System.DateTime.Now;
                            row.CorrectedDate = correctedDate;
                            row.StartTime = System.DateTime.Now;
                            row.ColorCode = ColoCode;
                            row.LossCodeID = Convert.ToInt32(LossCodeID);
                            row.ModeType = ModeType;
                            row.IsShiftEnd = shiftid;
                            using (unitworksccsEntities db1 = new unitworksccsEntities())
                            {
                                db1.unitworkccs_tbllivemode.Add(row);
                                db1.SaveChanges();
                            }
                        }
                        else if (BDID.Length != 0)
                        {
                            unitworkccs_tbllivemode row = new unitworkccs_tbllivemode();
                            row.MacMode = MacMode;
                            row.MachineID = MacID;
                            row.InsertedBy = 1;
                            row.InsertedOn = System.DateTime.Now;
                            row.CorrectedDate = correctedDate;
                            row.StartTime = System.DateTime.Now;
                            row.ColorCode = ColoCode;
                            row.BreakdownID = Convert.ToInt32(BDID);
                            row.ModeType = ModeType;
                            row.IsShiftEnd = shiftid;
                            using (unitworksccsEntities db1 = new unitworksccsEntities())
                            {
                                db1.unitworkccs_tbllivemode.Add(row);
                                db1.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IntoFile(ex.ToString());
                    }
                }

                else if (MacMode != "MNT")
                {
                    try
                    {
                        var ModeDetList1 = new List<unitworkccs_tbllivemode>();
                        using (unitworksccsEntities db = new unitworksccsEntities())
                        {
                            ModeDetList1 = db.unitworkccs_tbllivemode.Where(m => m.MachineID == MacID && m.IsCompleted == 0 || (m.IsCompleted == 1 && m.ModeTypeEnd == 0)).ToList();
                        }
                        MacMode = ModeDetList1[0].MacMode.ToString();
                        ModeID = Convert.ToInt32(ModeDetList1[0].ModeID.ToString());
                        StartTime = ModeDetList1[0].StartTime.ToString();
                        CorrectedDateDB = Convert.ToDateTime(ModeDetList1[0].CorrectedDate.ToString()).Date;
                        ColoCode = ModeDetList1[0].ColorCode.ToString();
                        LossCodeID = ModeDetList1[0].LossCodeID.ToString();
                        BDID = ModeDetList1[0].BreakdownID.ToString();
                        StartIdle = Convert.ToInt32(ModeDetList1[0].StartIdle.ToString());
                        ModeType = ModeDetList[0].ModeType.ToString();
                        int pingCounter = 0;
                        TryPing:
                        //Ping p = new Ping();
                        //PingReply r;
                        //string s;
                        //s = IPAddress;
                        //r = p.Send(s);

                        if (ConnectionRet == 0)
                        {

                            if (parameterdet.Count >= 2)
                            {
                                int prvoptime = Convert.ToInt32(parameterdet[1].OperatingTime);
                                int proptime = Convert.ToInt32(parameterdet[0].OperatingTime);
                                int diff = proptime - prvoptime;
                                if (diff > 0)
                                {
                                    if (MacMode != "PROD")
                                    {
                                        //setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
                                        int durationinsec = Convert.ToInt32(nowdate.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
                                        UpdatetbllivemodeDetails(nowdate, durationinsec, 0, ModeID);
                                        INSERTMODE("PROD", MacID, 1, System.DateTime.Now, correctedDate, nowdate, "GREEN", "PROD", shiftid);
                                    }
                                }
                                else
                                {
                                    if (MacMode != "IDLE")
                                    {
                                        int durationinsec = Convert.ToInt32(nowdate.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
                                        UpdatetbllivemodeDetails(nowdate, durationinsec, 0, ModeID);
                                        INSERTMODE("IDLE", MacID, 1, System.DateTime.Now, correctedDate, nowdate, "YELLOW", "IDLE", shiftid);
                                    }
                                }
                                if (MacMode == "IDLE" && StartIdle == 0)
                                {
                                    int durationinsec = Convert.ToInt32(nowdate.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
                                    double DurinMin = durationinsec / 60;
                                    if (DurinMin >= MachineIdleMin)
                                    {

                                        UpdatetbllivemodeDetails(1, ModeID);
                                    }
                                    else
                                    {
                                        //setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
                                    }
                                }
                            }
                        }
                        else if (MacMode != "POWEROFF")
                        {
                            if (pingCounter < 4)
                            {
                                Thread.Sleep(1000);
                                pingCounter++;
                                goto TryPing;
                            }
                            else
                            {

                                int durationinsec = Convert.ToInt32(nowdate.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
                                ////if (ModeType == "IDLE" || ModeType == "PROD" || ModeType == "POWEROFF")
                                ////{
                                //using (MsqlConnection sa = new MsqlConnection())
                                //{
                                //    sa.open();
                                //    MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE  i_facility.tbllivemode SET EndTime = '" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, DurationInSec = " + durationinsec + ", ModeTypeEnd = 1, StartIdle = 0 Where ModeID = " + ModeID + "", sa.msqlConnection);
                                //    int retend = cmdendprvmode.ExecuteNonQuery();
                                //    sa.close();
                                //    //setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
                                //}
                                UpdatetbllivemodeDetails(nowdate, durationinsec, 0, ModeID);
                                //INSERTMODE("IDLE", MacID, 1, System.DateTime.Now, correctedDate, nowdate, "YELLOW", "IDLE", shiftid);
                                INSERTMODE("POWEROFF", MacID, 1, System.DateTime.Now, correctedDate, nowdate, "BLUE", "POWEROFF", shiftid);
                            }

                            InsertOperationLogDetails("Connection to M/c Problem. Please check the Ethernet Connection and Power Connection of the CNC Machine. " + ConnectionRet + " ErrorNo " + ConnectionRetErr, System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "M/c Error", MacID); ;
                        }
                        else
                        {
                            InsertOperationLogDetails("Connection to M/c Problem. Please check the Ethernet Connection and Power Connection of the CNC Machine. " + ConnectionRet + " ErrorNo " + ConnectionRetErr, System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "M/c Error", MacID); ;
                        }
                    }
                    catch (Exception ex)
                    {
                        IntoFile(ex.ToString());
                    }
                }
            }
            else if (ModeDetList.Count > 1)
            {
                IntoFile("Modecount :" + ModeDetList.Count);
                var dtModeMultiple = new List<unitworkccs_tbllivemode>();
                var dtMode1 = new List<unitworkccs_tbllivemode>();

                using (unitworksccsEntities db = new unitworksccsEntities())
                {
                    dtModeMultiple = db.unitworkccs_tbllivemode.Where(m => m.IsCompleted == 0 && m.MachineID == MacID && m.CorrectedDate <= correctedDate).OrderBy(m => m.ModeID).ToList();
                    dtMode1 = db.unitworkccs_tbllivemode.Where(m => m.IsCompleted == 0 && m.MachineID == MacID).OrderBy(m => m.ModeID).ToList();
                    IntoFile("dtModeMultiple :" + dtModeMultiple.Count);
                    IntoFile("dtMode1 :" + dtMode1.Count);
                }

                for (int i = 0; i < (dtModeMultiple.Count - 1); i++)
                {
                    if (dtModeMultiple[i].MacMode.ToString() == dtModeMultiple[i + 1].MacMode.ToString())
                    {
                        DeleteModeDetails(dtModeMultiple[i].ModeID);
                    }
                }
                for (int i = 0; i < (dtMode1.Count - 2); i++)
                {

                    string shiftst = DateTime.Now.ToString("yyyy-MM-dd")+ shiftmasterdet.StartTime.ToString();
                    DateTime ShiftStDt = Convert.ToDateTime(shiftst);
                    if (dtMode1[i].StartTime.ToString() == ShiftStDt.ToString("yyyy-MM-dd HH:mm:00"))
                    {
                        DeleteModeDetails(dtMode1[i].ModeID);
                    }
                }
            }
            else
            {
                try
                {
                    var ModedetPoff = new unitworkccs_tbllivemode();
                    using (unitworksccsEntities db = new unitworksccsEntities())
                    {
                        ModedetPoff = db.unitworkccs_tbllivemode.Where(m => m.CorrectedDate == correctedDate && m.MachineID == MacID).OrderByDescending(m => m.ModeID).FirstOrDefault();
                    }
                    DateTime nowdate = DateTime.Now;
                    if (DtModePOFF.Rows.Count == 0)
                    {
                        INSERTMODE("POWEROFF", MacID, 1, System.DateTime.Now, correctedDate, nowdate, "BLUE", "POWEROFF", shiftid);
                    }
                    else
                    {
                        INSERTMODE("POWEROFF", MacID, 1, System.DateTime.Now, correctedDate, nowdate, "BLUE", "POWEROFF", shiftid);
                    }
                }
                catch (Exception ex)
                {
                    InsertOperationLogDetails("PowerOFF1 " + ex.Message.ToString(), System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "Parameter Error", MacID); ;
                    IntoFile(ex.ToString());
                }
            }
        }
        #endregion


        #region MODE BY DAy Wise 
        private void getmachinemodeDayWise(int MacID, string IPAddress, ushort h, int MachineLockBit, int MachineIdleBit, int MachineUnlockBit, int MachineIdleMin, int ConnectionRet, int ConnectionRetErr, int EnableLock)
        {


            DateTime correctedDate = DateTime.Now;
            // DataTable dtGDS = new DataTable();
            unitworkccs_tbldaytiming Daytimings = new unitworkccs_tbldaytiming();
            using (unitworksccsEntities db = new unitworksccsEntities())
            {
                Daytimings = db.unitworkccs_tbldaytiming.Where(m => m.IsDeleted == 0).FirstOrDefault();
            }
            //using (MsqlConnection mc = new MsqlConnection())
            //{
            //    mc.open();
            //    String GetDayStartQuery = "SELECT StartTime from i_facility.tbldaytiming where IsDeleted = 0;";
            //    SqlDataAdapter daGDS = new SqlDataAdapter(GetDayStartQuery, mc.msqlConnection);
            //    daGDS.Fill(dtGDS);
            //    mc.close();
            //}
            DateTime Start = Convert.ToDateTime(Daytimings.StartTime.ToString());
            DateTime st = Convert.ToDateTime(Daytimings.StartTime.ToString());
            if (Start <= DateTime.Now)
            {
                correctedDate = DateTime.Now.Date;
            }
            else
            {
                correctedDate = DateTime.Now.AddDays(-1).Date;
            }


            var parameterdet = new List<unitworkccs_parameters_master>();
            var ModeDetList = new List<unitworkccs_tbllivemode>();
            //var Machinedet = new unitworkccs_tblmachinedetails();
            using (unitworksccsEntities db = new unitworksccsEntities())
            {
                parameterdet = db.unitworkccs_parameters_master.Where(m => m.MachineID == MacID).OrderByDescending(m => m.ParameterID).ToList();
                ModeDetList = db.unitworkccs_tbllivemode.Where(m => m.MachineID == MacID && m.IsCompleted == 0 || (m.IsCompleted == 1 && m.ModeTypeEnd == 0)).ToList();
                //Machinedet = db.unitworkccs_tblmachinedetailss.Find(MacID);
            }

            // DataTable dt = new DataTable();
            DataTable dtMode = new DataTable();
            DataTable DtModePOFF = new DataTable();
            // String getmodequery = "SELECT MacMode, ModeID, StartTime, CorrectedDate, ColorCode, LossCodeID, BreakdownID, StartIdle, ModeType From  i_facility.tbllivemode WHERE (IsCompleted = 0 OR (IsCompleted = 1 AND ModeTypeEnd = 0)) and MachineID = " + MacID + " order by ModeID DESC";
            // String getmodequeryPOFF = "SELECT ModeID, StartTime, EndTime From i_facility.tbllivemode WHERE CorrectedDate = '" + correctedDate.ToString("yyyy-MM-dd") + "' and MachineID = " + MacID + " order by ModeID DESC LIMIT 1;";
            //using (MsqlConnection mc = new MsqlConnection())
            //{
            //    mc.open();
            //    String getparametersquery = "SELECT OperatingTime From  i_facility.unitworkccs_parameters_master WHERE MachineID = " + MacID + " order by ParameterID DESC";
            //    SqlDataAdapter da = new SqlDataAdapter(getparametersquery, mc.msqlConnection);
            //    da.Fill(dt);
            //    mc.close();
            //}
            //using (MsqlConnection mc = new MsqlConnection())
            //{
            //    mc.open();
            //    SqlDataAdapter daMode = new SqlDataAdapter(getmodequery, mc.msqlConnection);
            //    daMode.Fill(dtMode);
            //    mc.close();
            //}
            if (ModeDetList.Count == 1)
            {
                String MacMode = ModeDetList[0].MacMode.ToString();
                int ModeID = Convert.ToInt32(ModeDetList[0].ModeID.ToString());
                String StartTime = ModeDetList[0].StartTime.ToString();
                DateTime CorrectedDateDB = Convert.ToDateTime(ModeDetList[0].CorrectedDate.ToString()).Date;
                String ColoCode = ModeDetList[0].ColorCode.ToString();
                String LossCodeID = ModeDetList[0].LossCodeID.ToString();
                String BDID = ModeDetList[0].BreakdownID.ToString();
                int StartIdle = Convert.ToInt32(ModeDetList[0].StartIdle.ToString());
                String ModeType = ModeDetList[0].ModeType.ToString();
                DateTime nowdate = DateTime.Now;
                if (correctedDate != CorrectedDateDB)
                {

                    try
                    {
                        string Et = st.AddSeconds(-1).ToString("HH:mm:ss");
                        //DateTime NowDateCalc = Convert.ToDateTime(nowdate.ToString("yyyy-MM-dd 0" + (Start - 1) + ":59:59"));
                        DateTime NowDateCalc = Convert.ToDateTime(nowdate.ToString("yyyy-MM-dd " + Et));
                        int durationinsec = Convert.ToInt32(NowDateCalc.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
                        String StartTimeNext = nowdate.ToString("yyyy-MM-dd " + st.ToString("HH:mm:ss"));
                        //using (MsqlConnection mc1 = new MsqlConnection())
                        //{
                        //    mc1.open();
                        //    MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE i_facility.tbllivemode SET EndTime = '" + NowDateCalc.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, ModeTypeEnd = 1, DurationInSec = " + durationinsec + " Where ModeID = " + ModeID + "", mc1.msqlConnection);
                        //    int retend = cmdendprvmode.ExecuteNonQuery();
                        //    mc1.close();
                        //

                        #region commented 
                        //tbllivemode rowupdate = new tbllivemode();
                        //using (unitworksccsEntities db = new unitworksccsEntities())
                        //{
                        //    rowupdate = db.tbllivemodes.Find(ModeID);
                        //}
                        //rowupdate.EndTime = NowDateCalc;
                        //rowupdate.DurationInSec = durationinsec;
                        //rowupdate.IsCompleted = 1;
                        //rowupdate.ModeTypeEnd = 1;

                        //using (unitworksccsEntities db1 = new unitworksccsEntities())
                        //{
                        //    db1.Entry(rowupdate).State = System.Data.Entity.EntityState.Modified;
                        //    db1.SaveChanges();
                        //}
                        #endregion

                        UpdatetbllivemodeDetails(NowDateCalc, durationinsec, ModeID);

                        if (LossCodeID.Length == 0 && BDID.Length == 0)
                        {

                            //using (MsqlConnection sa = new MsqlConnection())
                            //{
                            //    sa.open();
                            //    MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO i_facility.tbllivemode " +
                            //        "(MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode,ModeType) VALUES('" + MacMode + "'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate.ToString("yyyy-MM-dd") + "','" + StartTimeNext + "','" + ColoCode + "','" + ModeType + "')", sa.msqlConnection);
                            //    int ret = cmdpoweroff.ExecuteNonQuery();
                            //    sa.close();
                            //}

                            #region commented
                            //tbllivemode row = new tbllivemode();
                            //row.MacMode = MacMode;
                            //row.MachineID = MacID;
                            //row.InsertedBy = 1;
                            //row.InsertedOn = System.DateTime.Now;
                            //row.CorrectedDate = correctedDate;
                            //row.StartTime = System.DateTime.Now;
                            //row.ColorCode = ColoCode;
                            //row.ModeType = ModeType;
                            //using (unitworksccsEntities db1 = new unitworksccsEntities())
                            //{
                            //    db1.tbllivemodes.Add(row);
                            //    db1.SaveChanges();
                            //}
                            #endregion

                            INSERTMODE(MacMode, MacID, 1, System.DateTime.Now, correctedDate, Convert.ToDateTime(StartTimeNext), ColoCode, ModeType);

                        }
                        else if (LossCodeID.Length != 0)
                        {
                            //using (MsqlConnection sa = new MsqlConnection())
                            //{
                            //    sa.open();
                            //    MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO i_facility.tbllivemode " +
                            //        "(MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode, LossCodeID, ModeType) VALUES('" + MacMode + "'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate.ToString("yyyy-MM-dd") + "','" + StartTimeNext + "','" + ColoCode + "'," + Convert.ToInt32(LossCodeID) + ",'" + ModeType + "'", sa.msqlConnection);
                            //    int ret = cmdpoweroff.ExecuteNonQuery();
                            //    sa.close();
                            //}

                            unitworkccs_tbllivemode row = new unitworkccs_tbllivemode();
                            row.MacMode = MacMode;
                            row.MachineID = MacID;
                            row.InsertedBy = 1;
                            row.InsertedOn = System.DateTime.Now;
                            row.CorrectedDate = correctedDate;
                            row.StartTime = System.DateTime.Now;
                            row.ColorCode = ColoCode;
                            row.LossCodeID = Convert.ToInt32(LossCodeID);
                            row.ModeType = ModeType;
                            using (unitworksccsEntities db1 = new unitworksccsEntities())
                            {
                                db1.unitworkccs_tbllivemode.Add(row);
                                db1.SaveChanges();
                            }
                        }
                        else if (BDID.Length != 0)
                        {
                            //using (MsqlConnection sa = new MsqlConnection())
                            //{
                            //    sa.open();
                            //    MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO i_facility.tbllivemode " +
                            //        "(MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode, BreakdownID, ModeType) VALUES('" + MacMode + "'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate.ToString("yyyy-MM-dd") + "','" + StartTimeNext + "','" + ColoCode + "'," + Convert.ToInt32(BDID) + ",'" + ModeType + "')", sa.msqlConnection);
                            //    int ret = cmdpoweroff.ExecuteNonQuery();
                            //    sa.close();
                            //}
                            unitworkccs_tbllivemode row = new unitworkccs_tbllivemode();
                            row.MacMode = MacMode;
                            row.MachineID = MacID;
                            row.InsertedBy = 1;
                            row.InsertedOn = System.DateTime.Now;
                            row.CorrectedDate = correctedDate;
                            row.StartTime = System.DateTime.Now;
                            row.ColorCode = ColoCode;
                            row.BreakdownID = Convert.ToInt32(BDID);
                            row.ModeType = ModeType;
                            using (unitworksccsEntities db1 = new unitworksccsEntities())
                            {
                                db1.unitworkccs_tbllivemode.Add(row);
                                db1.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IntoFile(ex.ToString());
                    }
                }

                else if (MacMode != "MNT")
                {
                    try
                    {
                        //DataTable dtMode1 = new DataTable();
                        var ModeDetList1 = new List<unitworkccs_tbllivemode>();
                        using (unitworksccsEntities db = new unitworksccsEntities())
                        {
                            ModeDetList1 = db.unitworkccs_tbllivemode.Where(m => m.MachineID == MacID && m.IsCompleted == 0 || (m.IsCompleted == 1 && m.ModeTypeEnd == 0)).ToList();
                        }
                        //using (MsqlConnection mc2 = new MsqlConnection())
                        //{
                        //    mc2.open();
                        //    SqlDataAdapter daMode1 = new SqlDataAdapter(getmodequery, mc2.msqlConnection);
                        //    daMode1.Fill(dtMode1);
                        //    mc2.close();
                        //}
                        MacMode = ModeDetList1[0].MacMode.ToString();
                        ModeID = Convert.ToInt32(ModeDetList1[0].ModeID.ToString());
                        StartTime = ModeDetList1[0].StartTime.ToString();
                        CorrectedDateDB = Convert.ToDateTime(ModeDetList1[0].CorrectedDate.ToString()).Date;
                        ColoCode = ModeDetList1[0].ColorCode.ToString();
                        LossCodeID = ModeDetList1[0].LossCodeID.ToString();
                        BDID = ModeDetList1[0].BreakdownID.ToString();
                        StartIdle = Convert.ToInt32(ModeDetList1[0].StartIdle.ToString());
                        ModeType = ModeDetList[0].ModeType.ToString();
                        int pingCounter = 0;
                        TryPing:
                        //Ping p = new Ping();
                        //PingReply r;
                        //string s;
                        //s = IPAddress;
                        //r = p.Send(s);

                        if (ConnectionRet == 0)
                        {
                            //    using (unitworksccsEntities db = new unitworksccsEntities())
                            //    {

                            //        parameterdet = db.unitworkccs_parameters_master.Where(m => m.MachineID == MacID).ToList();
                            //    }
                            if (parameterdet.Count >= 2)
                            {
                                int prvoptime = Convert.ToInt32(parameterdet[1].OperatingTime);
                                int proptime = Convert.ToInt32(parameterdet[0].OperatingTime);
                                int diff = proptime - prvoptime;
                                if (diff > 0)
                                {
                                    if (MacMode != "PROD")
                                    {
                                        //setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
                                        int durationinsec = Convert.ToInt32(nowdate.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
                                        ////if (ModeType == "IDLE" || ModeType == "POWEROFF")
                                        ////{
                                        //using (MsqlConnection sa = new MsqlConnection())
                                        //{
                                        //    sa.open();
                                        //    MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE i_facility.tbllivemode SET" +
                                        //        " EndTime = '" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, DurationInSec = " + durationinsec + ", ModeTypeEnd = 1, StartIdle = 0 Where ModeID = " + ModeID + "", sa.msqlConnection);
                                        //    int retend = cmdendprvmode.ExecuteNonQuery();
                                        //    sa.close();
                                        //}

                                        UpdatetbllivemodeDetails(nowdate, durationinsec, 0, ModeID);

                                        #region commented
                                        //tbllivemode rowupdate = new tbllivemode();
                                        //using (unitworksccsEntities db = new unitworksccsEntities())
                                        //{
                                        //    rowupdate = db.tbllivemodes.Find(ModeID);
                                        //}
                                        //rowupdate.EndTime = nowdate;
                                        //rowupdate.DurationInSec = durationinsec;
                                        //rowupdate.IsCompleted = 1;
                                        //rowupdate.ModeTypeEnd = 1;
                                        //rowupdate.StartIdle = 0;

                                        //using (unitworksccsEntities db1 = new unitworksccsEntities())
                                        //{
                                        //    db1.Entry(rowupdate).State = System.Data.Entity.EntityState.Modified;
                                        //    db1.SaveChanges();
                                        //}
                                        #endregion
                                        ////}
                                        ////else
                                        ////{
                                        ////    using (MsqlConnection sa = new MsqlConnection())
                                        ////    {
                                        ////        sa.open();
                                        ////        MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE tbllivemode SET EndTime = '" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, ModeTypeEnd = 1, DurationInSec = " + durationinsec + " Where ModeID = " + ModeID + "", sa.msqlConnection);
                                        ////        int retend = cmdendprvmode.ExecuteNonQuery();
                                        ////        sa.close();
                                        ////    }
                                        ////}
                                        //using (MsqlConnection mc1 = new MsqlConnection())
                                        //{
                                        //    mc1.open();
                                        //    MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO i_facility.tbllivemode " +
                                        //        "(MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode, ModeType) VALUES('PROD'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate.ToString("yyyy-MM-dd") + "','" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "','GREEN','PROD')", mc1.msqlConnection);
                                        //    int ret = cmdpoweroff.ExecuteNonQuery();
                                        //    mc1.close();
                                        //}
                                        INSERTMODE("PROD", MacID, 1, System.DateTime.Now, correctedDate, nowdate, "GREEN", "PROD");
                                        #region commented
                                        //tbllivemode row = new tbllivemode();
                                        //row.MacMode = "PROD";
                                        //row.MachineID = MacID;
                                        //row.InsertedBy = 1;
                                        //row.InsertedOn = System.DateTime.Now;
                                        //row.CorrectedDate = correctedDate;
                                        //row.StartTime = System.DateTime.Now;
                                        //row.ColorCode = "GREEN";                                        
                                        //row.ModeType = "PROD";
                                        //using (unitworksccsEntities db1 = new unitworksccsEntities())
                                        //{
                                        //    db1.tbllivemodes.Add(row);
                                        //    db1.SaveChanges();
                                        //}
                                        #endregion
                                    }
                                }
                                else
                                {
                                    if (MacMode != "IDLE")
                                    {
                                        int durationinsec = Convert.ToInt32(nowdate.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
                                        ////if (ModeType == "PROD" || ModeType == "POWEROFF")
                                        ////{
                                        //using (MsqlConnection sa = new MsqlConnection())
                                        //{
                                        //    sa.open();
                                        //    MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE i_facility.tbllivemode SET EndTime = '" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, DurationInSec = " + durationinsec + ", ModeTypeEnd = 1, StartIdle = 0 Where ModeID = " + ModeID + "", sa.msqlConnection);
                                        //    int retend = cmdendprvmode.ExecuteNonQuery();
                                        //    sa.close();
                                        //    //setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
                                        //}
                                        UpdatetbllivemodeDetails(nowdate, durationinsec, 0, ModeID);
                                        #region commented
                                        //tbllivemode rowupdate = new tbllivemode();
                                        //using (unitworksccsEntities db = new unitworksccsEntities())
                                        //{
                                        //    rowupdate = db.tbllivemodes.Find(ModeID);
                                        //}
                                        //rowupdate.EndTime = nowdate;
                                        //rowupdate.DurationInSec = durationinsec;
                                        //rowupdate.IsCompleted = 1;
                                        //rowupdate.ModeTypeEnd = 1;
                                        //rowupdate.StartIdle = 0;

                                        //using (unitworksccsEntities db1 = new unitworksccsEntities())
                                        //{
                                        //    db1.Entry(rowupdate).State = System.Data.Entity.EntityState.Modified;
                                        //    db1.SaveChanges();
                                        //}
                                        #endregion
                                        ////}
                                        ////else
                                        ////{
                                        ////    using (MsqlConnection sa = new MsqlConnection())
                                        ////    {
                                        ////        sa.open();
                                        ////        MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE tbllivemode SET EndTime = '" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, DurationInSec = " + durationinsec + ", ModeTypeEnd = 1 Where ModeID = " + ModeID + "", sa.msqlConnection);
                                        ////        int retend = cmdendprvmode.ExecuteNonQuery();
                                        ////        sa.close();
                                        ////        setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
                                        ////    }
                                        ////}
                                        //using (MsqlConnection mc1 = new MsqlConnection())
                                        //{
                                        //    mc1.open();
                                        //    MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO i_facility.tbllivemode " +
                                        //        "(MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode, ModeType) " +
                                        //        "VALUES('IDLE'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate.ToString("yyyy-MM-dd") + "','" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "','YELLOW','IDLE')", mc1.msqlConnection);
                                        //    int ret = cmdpoweroff.ExecuteNonQuery();
                                        //    mc1.close();
                                        //}

                                        INSERTMODE("IDLE", MacID, 1, System.DateTime.Now, correctedDate, nowdate, "YELLOW", "IDLE");
                                    }
                                }
                                if (MacMode == "IDLE" && StartIdle == 0)
                                {
                                    int durationinsec = Convert.ToInt32(nowdate.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
                                    double DurinMin = durationinsec / 60;
                                    if (DurinMin >= MachineIdleMin)
                                    {

                                        UpdatetbllivemodeDetails(1, ModeID);
                                        //using (MsqlConnection mc1 = new MsqlConnection())
                                        //{
                                        //    mc1.open();
                                        //    MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE i_facility.tbllivemode SET StartIdle = 1 Where ModeID = " + ModeID + "", mc1.msqlConnection);
                                        //    int retend = cmdendprvmode.ExecuteNonQuery();
                                        //    mc1.close();
                                        //    if (EnableLock == 1)
                                        //    {
                                        //        //setmachinelock(h, MacID, (ushort)MachineLockBit, (ushort)MachineIdleBit, (ushort)MachineUnlockBit, true);
                                        //    }
                                        //}
                                    }
                                    else
                                    {
                                        //setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
                                    }
                                }
                            }
                        }
                        else if (MacMode != "POWEROFF")
                        {
                            if (pingCounter < 4)
                            {
                                Thread.Sleep(1000);
                                pingCounter++;
                                goto TryPing;
                            }
                            else
                            {

                                int durationinsec = Convert.ToInt32(nowdate.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
                                ////if (ModeType == "IDLE" || ModeType == "PROD" || ModeType == "POWEROFF")
                                ////{
                                //using (MsqlConnection sa = new MsqlConnection())
                                //{
                                //    sa.open();
                                //    MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE  i_facility.tbllivemode SET EndTime = '" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, DurationInSec = " + durationinsec + ", ModeTypeEnd = 1, StartIdle = 0 Where ModeID = " + ModeID + "", sa.msqlConnection);
                                //    int retend = cmdendprvmode.ExecuteNonQuery();
                                //    sa.close();
                                //    //setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
                                //}
                                UpdatetbllivemodeDetails(nowdate, durationinsec, 0, ModeID);
                                ////}
                                ////else
                                ////{
                                ////    using (MsqlConnection sa = new MsqlConnection())
                                ////    {
                                ////        sa.open();
                                ////        MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE  tbllivemode SET EndTime = '" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, ModeTypeEnd = 1, DurationInSec = " + durationinsec + " Where ModeID = " + ModeID + "", sa.msqlConnection);
                                ////        int retend = cmdendprvmode.ExecuteNonQuery();
                                ////        sa.close();
                                ////        sa.Dispose();
                                ////        setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
                                ////    }
                                ////}
                                //using (MsqlConnection mc1 = new MsqlConnection())
                                //{
                                //    mc1.open();
                                //    MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO  i_facility.tbllivemode (MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode,ModeType) VALUES('POWEROFF'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate.ToString("yyyy-MM-dd") + "','" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "','BLUE','POWEROFF')", mc1.msqlConnection);
                                //    int ret = cmdpoweroff.ExecuteNonQuery();
                                //    mc1.close();
                                //}

                                INSERTMODE("POWEROFF", MacID, 1, System.DateTime.Now, correctedDate, nowdate, "BLUE", "POWEROFF");
                            }
                            //using (MsqlConnection mc = new MsqlConnection())
                            //{
                            //    mc.open();
                            //    MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Connection to M/c Problem. Please check the Ethernet Connection and Power Connection of the CNC Machine. " + ConnectionRet + " ErrorNo " + ConnectionRetErr + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error'," + MacID + ")", mc.msqlConnection);
                            //    cmd.ExecuteNonQuery();
                            //    mc.close();
                            //}
                            InsertOperationLogDetails("Connection to M/c Problem. Please check the Ethernet Connection and Power Connection of the CNC Machine. " + ConnectionRet + " ErrorNo " + ConnectionRetErr, System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "M/c Error", MacID); ;
                        }
                        else
                        {
                            //using (MsqlConnection mc1 = new MsqlConnection())
                            //{
                            //    mc1.open();
                            //    MySqlCommand cmd = new MySqlCommand("INSERT INTO i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Connection to M/c Problem. Please check the Ethernet Connection and Power Connection of the CNC Machine. " + ConnectionRet + " ErrorNo " + ConnectionRetErr + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error'," + MacID + ")", mc1.msqlConnection);
                            //    cmd.ExecuteNonQuery();
                            //    mc1.close();
                            //}
                            InsertOperationLogDetails("Connection to M/c Problem. Please check the Ethernet Connection and Power Connection of the CNC Machine. " + ConnectionRet + " ErrorNo " + ConnectionRetErr, System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "M/c Error", MacID); ;
                        }
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.ToString());
                        IntoFile(ex.ToString());
                    }
                }
            }
            else if (dtMode.Rows.Count > 1)
            {
                //String getmodequery1 = "SELECT ModeID,StartTime From i_facility.tbllivemode WHERE IsCompleted = 0 and MachineID = " + MacID + " order by ModeID";
                //string getmodequery2 = "SELECT ModeID,StartTime,MacMode From i_facility.tbllivemode WHERE IsCompleted = 0 and MachineID = " + MacID + " and CorrectedDate<='" + correctedDate.ToString("yyyy-MM-dd") + "' order by ModeID";
                // DataTable dtMode1 = new DataTable();
                // DataTable dtModeMultiple = new DataTable();

                var dtModeMultiple = new List<unitworkccs_tbllivemode>();
                var dtMode1 = new List<unitworkccs_tbllivemode>();

                using (unitworksccsEntities db = new unitworksccsEntities())
                {
                    dtModeMultiple = db.unitworkccs_tbllivemode.Where(m => m.IsCompleted == 0 && m.MachineID == MacID && m.CorrectedDate <= correctedDate).OrderBy(m => m.ModeID).ToList();
                    dtMode1 = db.unitworkccs_tbllivemode.Where(m => m.IsCompleted == 0 && m.MachineID == MacID).OrderBy(m => m.ModeID).ToList();
                }

                //using (MsqlConnection mc = new MsqlConnection())
                //{
                //    mc.open();
                //    SqlDataAdapter daMode1 = new SqlDataAdapter(getmodequery2, mc.msqlConnection);
                //    daMode1.Fill(dtModeMultiple);
                //    mc.close();
                //}

                //using (MsqlConnection mc = new MsqlConnection())
                //{
                for (int i = 0; i < (dtModeMultiple.Count - 1); i++)
                {
                    if (dtModeMultiple[i].MacMode.ToString() == dtModeMultiple[i + 1].MacMode.ToString())
                    {
                        //mc.open();
                        //MySqlCommand cmdpoweroff = new MySqlCommand("DELETE FROM tbllivemode where ModeID = " + Convert.ToInt32(dtModeMultiple.Rows[i][0]), mc.msqlConnection);
                        //int ret = cmdpoweroff.ExecuteNonQuery();
                        //mc.close();
                        DeleteModeDetails(dtModeMultiple[i].ModeID);
                    }
                }
                //}
                //using (MsqlConnection mc = new MsqlConnection())
                //{
                //    mc.open();
                //    SqlDataAdapter daMode1 = new SqlDataAdapter(getmodequery1, mc.msqlConnection);
                //    daMode1.Fill(dtMode1);
                //    mc.close();
                //}
                //using (MsqlConnection mc = new MsqlConnection())
                //{
                for (int i = 0; i < (dtMode1.Count - 2); i++)
                {
                    if (dtMode1[i].StartTime.ToString() == DateTime.Now.ToString("yyyy-MM-dd 07:15:00"))
                    {
                        //mc.open();
                        //MySqlCommand cmdpoweroff = new MySqlCommand("DELETE FROM tbllivemode where ModeID = " + Convert.ToInt32(dtMode1.Rows[i][0]), mc.msqlConnection);
                        //int ret = cmdpoweroff.ExecuteNonQuery();
                        //mc.close();
                        DeleteModeDetails(dtMode1[i].ModeID);
                    }
                }
                //}
            }
            else
            {
                try
                {
                    var ModedetPoff = new unitworkccs_tbllivemode();
                    using (unitworksccsEntities db = new unitworksccsEntities())
                    {
                        ModedetPoff = db.unitworkccs_tbllivemode.Where(m => m.CorrectedDate == correctedDate && m.MachineID == MacID).OrderByDescending(m => m.ModeID).FirstOrDefault();
                    }

                    //using (MsqlConnection mc1 = new MsqlConnection())
                    //{
                    //    mc1.open();
                    //    SqlDataAdapter daModePOff = new SqlDataAdapter(getmodequeryPOFF, mc1.msqlConnection);
                    //    daModePOff.Fill(DtModePOFF);
                    //    mc1.close();
                    //}
                    DateTime nowdate = DateTime.Now;
                    if (DtModePOFF.Rows.Count == 0)
                    {
                        //using (MsqlConnection mc1 = new MsqlConnection())
                        //{
                        //    mc1.open();
                        //    MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO i_facility.tbllivemode (MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode,ModeType) 
                        //VALUES('POWEROFF'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate.ToString("yyyy-MM-dd") + "','" + Start.ToString("yyyy-MM-dd HH:mm:ss") + "','BLUE','POWEROFF')", mc1.msqlConnection);
                        //    int ret = cmdpoweroff.ExecuteNonQuery();
                        //    mc1.close();
                        //}
                        INSERTMODE("POWEROFF", MacID, 1, System.DateTime.Now, correctedDate, nowdate, "BLUE", "POWEROFF");
                    }
                    else
                    {
                        //using (MsqlConnection mc1 = new MsqlConnection())
                        //{
                        //    mc1.open();
                        //    MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO i_facility.tbllivemode (MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode,ModeType) VALUES('POWEROFF'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate.ToString("yyyy-MM-dd") + "','" + DtModePOFF.Rows[0][2].ToString() + "','BLUE','POWEROFF')", mc1.msqlConnection);
                        //    int ret = cmdpoweroff.ExecuteNonQuery();
                        //    mc1.close();
                        //}
                        INSERTMODE("POWEROFF", MacID, 1, System.DateTime.Now, correctedDate, nowdate, "BLUE", "POWEROFF");
                    }
                }
                catch (Exception ex)
                {
                    //using (MsqlConnection mc1 = new MsqlConnection())
                    //{
                    //    mc1.open();
                    //    MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('PowerOFF1 " + ex.Message.ToString() + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Parameter Error'," + MacID + ")", mc1.msqlConnection);
                    //    cmd.ExecuteNonQuery();
                    //    mc1.close();
                    //}
                    InsertOperationLogDetails("PowerOFF1 " + ex.Message.ToString(), System.DateTime.Now, System.DateTime.Now.TimeOfDay, System.DateTime.Now, "Parameter Error", MacID); ;
                    IntoFile(ex.ToString());
                }
            }
        }
        #endregion
        #region Previous Code For MODE
        //Get the Machine mode - run every minute for Fanuc Controllers
        //private void getmachinemode(int MacID, string IPAddress, ushort h, int MachineLockBit, int MachineIdleBit, int MachineUnlockBit, int MachineIdleMin, int ConnectionRet, int ConnectionRetErr, int EnableLock)
        //{
        //    using (MsqlConnection mc = new MsqlConnection())
        //    {
        //        mc.open();
        //        String getparametersquery = "SELECT OperatingTime From  unitworkccs_parameters_master WHERE MachineID = " + MacID + " order by ParameterID DESC";
        //        SqlDataAdapter da = new SqlDataAdapter(getparametersquery, mc.msqlConnection);
        //        DataTable dt = new DataTable();
        //        da.Fill(dt);

        //        String getmodequery = "SELECT MacMode, ModeID, StartTime, CorrectedDate, ColorCode, LossCodeID, BreakdownID, StartIdle, ModeType From  tbllivemode WHERE (IsCompleted = 0 OR (IsCompleted = 1 AND ModeTypeEnd = 0)) and MachineID = " + MacID + " order by ModeID DESC";
        //        SqlDataAdapter daMode = new SqlDataAdapter(getmodequery, mc.msqlConnection);
        //        DataTable dtMode = new DataTable();
        //        daMode.Fill(dtMode);
        //        mc.close();

        //        if (dtMode.Rows.Count > 0)
        //        {
        //            String MacMode = dtMode.Rows[0][0].ToString();
        //            int ModeID = Convert.ToInt32(dtMode.Rows[0][1].ToString());
        //            String StartTime = dtMode.Rows[0][2].ToString();
        //            DateTime CorrectedDateDB = Convert.ToDateTime(dtMode.Rows[0][3].ToString()).Date;
        //            String ColoCode = dtMode.Rows[0][4].ToString();
        //            String LossCodeID = dtMode.Rows[0][5].ToString();
        //            String BDID = dtMode.Rows[0][6].ToString();
        //            int StartIdle = Convert.ToInt32(dtMode.Rows[0][7].ToString());
        //            String ModeType = dtMode.Rows[0][8].ToString();
        //            DateTime nowdate = DateTime.Now;
        //            DateTime correctedDate = DateTime.Now;
        //            String GetDayStartQuery = "SELECT StartTime from tbldaytiming where IsDeleted = 0;";
        //            SqlDataAdapter daGDS = new SqlDataAdapter(GetDayStartQuery, mc.msqlConnection);
        //            DataTable dtGDS = new DataTable();
        //            daGDS.Fill(dtGDS);
        //            DateTime StartDateTime = Convert.ToDateTime(dtGDS.Rows[0][0].ToString());
        //            TimeSpan StartDayTime = Convert.ToDateTime(dtGDS.Rows[0][0].ToString()).TimeOfDay;
        //            int Start = Convert.ToDateTime(dtGDS.Rows[0][0].ToString()).Hour;
        //            if (Start <= DateTime.Now.Hour)
        //            {
        //                correctedDate = DateTime.Now.Date;
        //            }
        //            else
        //            {
        //                correctedDate = DateTime.Now.AddDays(-1).Date;
        //            }
        //            //MessageBox.Show(CorrectedDateDB);
        //            //MessageBox.Show(correctedDate);
        //            if (correctedDate != CorrectedDateDB)
        //            {
        //                mc.open();
        //                DateTime NowDateCalc = Convert.ToDateTime(nowdate.ToString("yyyy-MM-dd "+ StartDateTime.AddSeconds(-1).TimeOfDay));
        //                int durationinsec = Convert.ToInt32(NowDateCalc.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
        //                String StartTimeNext = nowdate.ToString("yyyy-MM-dd " + StartDateTime.TimeOfDay);
        //                MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE tbllivemode SET EndTime = '" + nowdate.ToString("yyyy-MM-dd " + StartDateTime.AddSeconds(-1).TimeOfDay) + "', IsCompleted = 1, ModeTypeEnd = 1, DurationInSec = " + durationinsec + " Where ModeID = " + ModeID + "", mc.msqlConnection);
        //                int retend = cmdendprvmode.ExecuteNonQuery();
        //                if (LossCodeID.Length == 0 && BDID.Length == 0)
        //                {
        //                    MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO tbllivemode (MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode,ModeType) VALUES('" + MacMode + "'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate + "','" + StartTimeNext + "','" + ColoCode + "','" + ModeType + "')", mc.msqlConnection);
        //                    int ret = cmdpoweroff.ExecuteNonQuery();
        //                }
        //                else if (LossCodeID.Length != 0)
        //                {
        //                    MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO tbllivemode (MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode, LossCodeID, ModeType) VALUES('" + MacMode + "'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate + "','" + StartTimeNext + "','" + ColoCode + "'," + Convert.ToInt32(LossCodeID) + ",'" + ModeType + "'", mc.msqlConnection);
        //                    int ret = cmdpoweroff.ExecuteNonQuery();
        //                }
        //                else if (BDID.Length != 0)
        //                {
        //                    MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO tbllivemode (MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode, BreakdownID, ModeType) VALUES('" + MacMode + "'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate + "','" + StartTimeNext + "','" + ColoCode + "'," + Convert.ToInt32(BDID) + ",'" + ModeType + "')", mc.msqlConnection);
        //                    int ret = cmdpoweroff.ExecuteNonQuery();
        //                }
        //                mc.close();
        //            }
        //            else if (MacMode != "MNT")
        //            {
        //                mc.open();
        //                SqlDataAdapter daMode1 = new SqlDataAdapter(getmodequery, mc.msqlConnection);
        //                DataTable dtMode1 = new DataTable();
        //                daMode1.Fill(dtMode1);
        //                mc.close();
        //                MacMode = dtMode1.Rows[0][0].ToString();
        //                ModeID = Convert.ToInt32(dtMode1.Rows[0][1].ToString());
        //                StartTime = dtMode1.Rows[0][2].ToString();
        //                CorrectedDateDB = Convert.ToDateTime(dtMode1.Rows[0][3].ToString()).Date;
        //                ColoCode = dtMode1.Rows[0][4].ToString();
        //                LossCodeID = dtMode1.Rows[0][5].ToString();
        //                BDID = dtMode1.Rows[0][6].ToString();
        //                StartIdle = Convert.ToInt32(dtMode1.Rows[0][7].ToString());
        //                ModeType = dtMode.Rows[0][8].ToString();
        //                int pingCounter = 0;
        //                TryPing:
        //                //Ping p = new Ping();
        //                //PingReply r;
        //                //string s;
        //                //s = IPAddress;
        //                //r = p.Send(s);

        //                if (ConnectionRet == 0)
        //                {
        //                    if (dt.Rows.Count >= 2)
        //                    {
        //                        int prvoptime = Convert.ToInt32(dt.Rows[1][0]);
        //                        int proptime = Convert.ToInt32(dt.Rows[0][0]);
        //                        int diff = proptime - prvoptime;
        //                        if (diff > 0)
        //                        {
        //                            if (MacMode != "PROD")
        //                            {
        //                                setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
        //                                mc.open();
        //                                int durationinsec = Convert.ToInt32(nowdate.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
        //                                if (ModeType == "IDLE" || ModeType == "POWEROFF")
        //                                {
        //                                    MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE tbllivemode SET EndTime = '" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, DurationInSec = " + durationinsec + ", ModeTypeEnd = 1, StartIdle = 0 Where ModeID = " + ModeID + "", mc.msqlConnection);
        //                                    int retend = cmdendprvmode.ExecuteNonQuery();
        //                                }
        //                                else
        //                                {
        //                                    MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE tbllivemode SET EndTime = '" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, ModeTypeEnd = 1, DurationInSec = " + durationinsec + " Where ModeID = " + ModeID + "", mc.msqlConnection);
        //                                    int retend = cmdendprvmode.ExecuteNonQuery();
        //                                }

        //                                MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO tbllivemode (MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode, ModeType) VALUES('PROD'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate + "','" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "','GREEN','PROD')", mc.msqlConnection);
        //                                int ret = cmdpoweroff.ExecuteNonQuery();
        //                                mc.close();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (MacMode != "IDLE")
        //                            {
        //                                mc.open();
        //                                int durationinsec = Convert.ToInt32(nowdate.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
        //                                if (ModeType == "PROD" || ModeType == "POWEROFF")
        //                                {
        //                                    MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE tbllivemode SET EndTime = '" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, DurationInSec = " + durationinsec + ", ModeTypeEnd = 1, StartIdle = 0 Where ModeID = " + ModeID + "", mc.msqlConnection);
        //                                    int retend = cmdendprvmode.ExecuteNonQuery();
        //                                    setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
        //                                }
        //                                else
        //                                {
        //                                    MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE tbllivemode SET EndTime = '" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, DurationInSec = " + durationinsec + ", ModeTypeEnd = 1 Where ModeID = " + ModeID + "", mc.msqlConnection);
        //                                    int retend = cmdendprvmode.ExecuteNonQuery();
        //                                    setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
        //                                }

        //                                MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO tbllivemode (MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode, ModeType) VALUES('IDLE'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate + "','" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "','YELLOW','IDLE')", mc.msqlConnection);
        //                                int ret = cmdpoweroff.ExecuteNonQuery();
        //                                mc.close();
        //                            }
        //                        }
        //                        if (MacMode == "IDLE" && StartIdle == 0)
        //                        {
        //                            int durationinsec = Convert.ToInt32(nowdate.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
        //                            double DurinMin = durationinsec / 60;
        //                            if (DurinMin >= MachineIdleMin)
        //                            {
        //                                mc.open();
        //                                MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE  tbllivemode SET StartIdle = 1 Where ModeID = " + ModeID + "", mc.msqlConnection);
        //                                int retend = cmdendprvmode.ExecuteNonQuery();
        //                                mc.close();
        //                                if (EnableLock == 1)
        //                                {
        //                                    //setmachinelock(h, MacID, (ushort)MachineLockBit, (ushort)MachineIdleBit, (ushort)MachineUnlockBit, true);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                //setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
        //                            }
        //                        }
        //                    }
        //                    mc.close();
        //                }
        //                else if (MacMode != "POWEROFF")
        //                {
        //                    if (pingCounter < 4)
        //                    {
        //                        Thread.Sleep(1000);
        //                        pingCounter++;
        //                        goto TryPing;
        //                    }
        //                    else
        //                    {
        //                        mc.open();
        //                        int durationinsec = Convert.ToInt32(nowdate.Subtract(Convert.ToDateTime(StartTime)).TotalSeconds);
        //                        if (ModeType == "IDLE" || ModeType == "PROD" || ModeType == "POWEROFF")
        //                        {
        //                            MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE  tbllivemode SET EndTime = '" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, DurationInSec = " + durationinsec + ", ModeTypeEnd = 1, StartIdle = 0 Where ModeID = " + ModeID + "", mc.msqlConnection);
        //                            int retend = cmdendprvmode.ExecuteNonQuery();
        //                            setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
        //                        }
        //                        else
        //                        {
        //                            MySqlCommand cmdendprvmode = new MySqlCommand("UPDATE  tbllivemode SET EndTime = '" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "', IsCompleted = 1, ModeTypeEnd = 1, DurationInSec = " + durationinsec + " Where ModeID = " + ModeID + "", mc.msqlConnection);
        //                            int retend = cmdendprvmode.ExecuteNonQuery();
        //                            setmachineUnlock(h, (ushort)MachineLockBit, (ushort)MachineUnlockBit);
        //                        }

        //                        MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO  tbllivemode (MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode,ModeType) VALUES('POWEROFF'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate + "','" + nowdate.ToString("yyyy-MM-dd HH:mm:ss") + "','BLUE','POWEROFF')", mc.msqlConnection);
        //                        int ret = cmdpoweroff.ExecuteNonQuery();
        //                        mc.close();
        //                    }
        //                    mc.close();
        //                    mc.open();
        //                    MySqlCommand cmd = new MySqlCommand("INSERT INTO  i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Connection to M/c Problem. Please check the Ethernet Connection and Power Connection of the CNC Machine. " + ConnectionRet + " ErrorNo " + ConnectionRetErr + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error'," + MacID + ")", mc.msqlConnection);
        //                    cmd.ExecuteNonQuery();
        //                    mc.close();
        //                }
        //                else
        //                {
        //                    mc.close();
        //                    mc.open();
        //                    MySqlCommand cmd = new MySqlCommand("INSERT INTO i_facility.operationlog(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Connection to M/c Problem. Please check the Ethernet Connection and Power Connection of the CNC Machine. " + ConnectionRet + " ErrorNo " + ConnectionRetErr + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error'," + MacID + ")", mc.msqlConnection);
        //                    cmd.ExecuteNonQuery();
        //                    mc.close();
        //                }
        //            }
        //            else
        //            {

        //            }
        //        }
        //        else
        //        {
        //            {
        //                DateTime nowdate = DateTime.Now;
        //                string correctedDate = null;
        //                String GetDayStartQuery = "SELECT StartTime from tbldaytiming where IsDeleted = 0;";
        //                SqlDataAdapter daGDS = new SqlDataAdapter(GetDayStartQuery, mc.msqlConnection);
        //                DataTable dtGDS = new DataTable();
        //                daGDS.Fill(dtGDS);
        //                DateTime Start = Convert.ToDateTime(dtGDS.Rows[0][0].ToString());
        //                if (Start.Hour <= DateTime.Now.Hour)
        //                {
        //                    correctedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //                }
        //                else
        //                {
        //                    correctedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //                }
        //                mc.open();
        //                MySqlCommand cmdpoweroff = new MySqlCommand("INSERT INTO tbllivemode (MacMode,MachineID,InsertedBy,InsertedOn,CorrectedDate,StartTime,ColorCode,ModeType) VALUES('POWEROFF'," + MacID + ",1,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + correctedDate + "','" + nowdate.ToString("yyyy-MM-dd 0" + (Start.Hour) + ":00:00") + "','BLUE','POWEROFF')", mc.msqlConnection);
        //                int ret = cmdpoweroff.ExecuteNonQuery();
        //                mc.close();
        //            }
        //        }
        //    }
        //}
        #endregion
        private DateTime GetCorrectedDate()
        {
            DateTime correctedDate = DateTime.Now;
            unitworkccs_tbldaytiming Daytimings = new unitworkccs_tbldaytiming();
            using (unitworksccsEntities db = new unitworksccsEntities())
            {
                Daytimings = db.unitworkccs_tbldaytiming.Where(m => m.IsDeleted == 0).FirstOrDefault();
            }
            //using (MsqlConnection mc = new MsqlConnection())
            //{
            //    mc.open();
            //    String GetDayStartQuery = "SELECT StartTime from i_facility.tbldaytiming where IsDeleted = 0;";
            //    SqlDataAdapter daGDS = new SqlDataAdapter(GetDayStartQuery, mc.msqlConnection);
            //    daGDS.Fill(dtGDS);
            //    mc.close();
            //}
            DateTime Start = Convert.ToDateTime(Daytimings.StartTime.ToString());
            DateTime st = Convert.ToDateTime(Daytimings.StartTime.ToString());
            if (Start <= DateTime.Now)
            {
                correctedDate = DateTime.Now.Date;
            }
            else
            {
                correctedDate = DateTime.Now.AddDays(-1).Date;
            }

            return correctedDate;
        }

        private void updatemimics(int MacID)
        {
            DataTable dtMode = new DataTable();
            using (MSqlConnection mc = new MSqlConnection())
            {
                mc.open();
                String getmodequery = "SELECT MacMode From unitworksccs.`unitworkccs.tbllivemode` WHERE IsCompleted = 0 and MachineID = " + MacID + " order by ModeID DESC";
                MySqlDataAdapter daMode = new MySqlDataAdapter(getmodequery, mc.MqlConnection);
                daMode.Fill(dtMode);
                mc.close();
            }

            if (dtMode.Rows.Count > 0)
            {
                DateTime nowdate = DateTime.Now;
                DateTime correctedDate = DateTime.Now;
                DataTable dtGDS = new DataTable();
                using (MSqlConnection mc = new MSqlConnection())
                {
                    String GetDayStartQuery = "SELECT StartTime from unitworksccs.`unitworkccs.tbldaytiming` where IsDeleted = 0;";
                    MySqlDataAdapter daGDS = new MySqlDataAdapter(GetDayStartQuery, mc.MqlConnection);

                    daGDS.Fill(dtGDS);
                }
                DateTime Start = Convert.ToDateTime(dtGDS.Rows[0][0].ToString());
                if (Start <= DateTime.Now)
                {
                    correctedDate = DateTime.Now.Date;
                }
                else
                {
                    correctedDate = DateTime.Now.AddDays(-1).Date;
                }
                using (MSqlConnection mc = new MSqlConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand("SP_MinMod", mc.MqlConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MachineID", MacID);
                        cmd.Parameters.AddWithValue("@Cdate", correctedDate);
                        mc.open();
                        cmd.ExecuteNonQuery();
                        mc.close();
                    }
                }
            }
        }

        //Not in Use - 2018-05-03
        private void setmachinelockSec(ushort h, int MacID, ushort LockDBit, ushort IdleDBit, ushort UnLockBit, bool LockStatus)
        {
            Focas1.IODBPMC0 rdpmcdataLockBit = new Focas1.IODBPMC0();
            short adr_type = 9;
            short data_type = 0;
            ushort s_number = LockDBit;
            ushort e_number = LockDBit;
            ushort length = 9;

            Focas1.IODBPMC0 rdpmcdataIdleBit = new Focas1.IODBPMC0();
            short adr_typeIdle = 9;
            short data_typeIdle = 0;
            ushort s_numberIdle = IdleDBit;
            ushort e_numberIdle = IdleDBit;
            ushort lengthIdle = 9;

            //Unlock Bit Parameters
            Focas1.IODBPMC0 rdpmcdataUnLockBit = new Focas1.IODBPMC0();
            ushort s_numberUn = LockDBit;
            ushort e_numberUn = LockDBit;

            short rdretLock = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdataLockBit);

            short rdretUnLock = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberUn, e_numberUn, length, rdpmcdataUnLockBit);

            short rdretIDLE = Focas1.pmc_rdpmcrng(h, adr_typeIdle, data_typeIdle, s_numberIdle, e_numberIdle, lengthIdle, rdpmcdataIdleBit);

            if (LockStatus)
            {
                //Machine UNLOCK D Bit
                Focas1.IODBPMC0 wrpmcdataUn = rdpmcdataUnLockBit;
                wrpmcdataUn.cdata[0] = 1;
                for (int i = 0; i < 100; i++)
                {
                    short wrretLOCK = Focas1.pmc_wrpmcrng(h, length, wrpmcdataUn);
                    if (wrretLOCK == 0)
                    {
                        break;
                    }
                }

                //Machine LOCK D Bit
                Focas1.IODBPMC0 wrpmcdata = rdpmcdataLockBit;
                wrpmcdata.cdata[0] = 1;
                for (int i = 0; i < 10; i++)
                {
                    short wrretLOCK = Focas1.pmc_wrpmcrng(h, length, wrpmcdata);
                }
                //IDLE Message D Bit
                Focas1.IODBPMC0 wrpmcdataIDLE = rdpmcdataIdleBit;
                wrpmcdataIDLE.cdata[0] = 1;
                for (int i = 0; i < 10; i++)
                {
                    short wrretIDLE = Focas1.pmc_wrpmcrng(h, length, wrpmcdataIDLE);
                }

                Thread.Sleep(2000);
                //Lock D Bit reverted back to OFF State
                rdretLock = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdataLockBit);
                Focas1.IODBPMC0 wrpmcdata1 = rdpmcdataLockBit;
                wrpmcdata1.cdata[0] = 0;
                for (int i = 0; i < 10; i++)
                {
                    short wrret = Focas1.pmc_wrpmcrng(h, length, wrpmcdata1);
                }
                rdretIDLE = Focas1.pmc_rdpmcrng(h, adr_typeIdle, data_typeIdle, s_numberIdle, e_numberIdle, lengthIdle, rdpmcdataIdleBit);
                Focas1.IODBPMC0 wrpmcdataIDLE1 = rdpmcdataIdleBit;
                wrpmcdataIDLE1.cdata[0] = 0;
                for (int i = 0; i < 10; i++)
                {
                    short wrretIDLE = Focas1.pmc_wrpmcrng(h, length, wrpmcdataIDLE1);
                }
            }
            else
            {
                Focas1.IODBPMC0 wrpmcdata1 = rdpmcdataLockBit;
                wrpmcdata1.cdata[0] = 0;
                for (int i = 0; i < 10; i++)
                {
                    short wrret = Focas1.pmc_wrpmcrng(h, length, wrpmcdata1);
                }
            }
        }

        //In Use - 2018-05-03
        private void setmachinelock(ushort h, int MacID, ushort LockDBit, ushort IdleDBit, ushort UnLockDBit, bool LockStatus)
        {
            //Focas1.focas_ret retallclibhndl3 = (Focas1.focas_ret)Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
            if (LockStatus)
            {
                //UnLocking D Bit Parameters
                Focas1.IODBPMC0 rdpmcdataUnLockBit = new Focas1.IODBPMC0();
                short adr_typeUn = 9;
                short data_typeUn = 0;
                ushort s_numberUn = UnLockDBit;
                ushort e_numberUn = UnLockDBit;
                ushort lengthUn = 9;

                short rdretUnLock = Focas1.pmc_rdpmcrng(h, adr_typeUn, data_typeUn, s_numberUn, e_numberUn, lengthUn, rdpmcdataUnLockBit);

                Focas1.IODBPMC0 wrpmcdataUn = rdpmcdataUnLockBit;
                wrpmcdataUn.cdata[0] = 0;
                for (int i = 0; i < 100; i++)
                {
                    short wrret = Focas1.pmc_wrpmcrng(h, lengthUn, wrpmcdataUn);
                    if (wrret == 0)
                    {
                        break;
                    }
                }

                //Locking D Bit Parameters
                Focas1.IODBPMC0 rdpmcdataLockBit = new Focas1.IODBPMC0();
                short adr_type = 9;
                short data_type = 0;
                ushort s_number = LockDBit;
                ushort e_number = LockDBit;
                ushort length = 9;

                short rdretLock = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdataLockBit);

                //IdleMessage D Bit Parameters
                Focas1.IODBPMC0 rdpmcdataIdleBit = new Focas1.IODBPMC0();
                short adr_typeIdle = 9;
                short data_typeIdle = 0;
                ushort s_numberIdle = IdleDBit;
                ushort e_numberIdle = IdleDBit;
                ushort lengthIdle = 9;

                short rdretIDLE = Focas1.pmc_rdpmcrng(h, adr_typeIdle, data_typeIdle, s_numberIdle, e_numberIdle, lengthIdle, rdpmcdataIdleBit);

                //Machine LOCK D Bit
                Focas1.IODBPMC0 wrpmcdata = rdpmcdataLockBit;
                wrpmcdata.cdata[0] = 1;
                for (int i = 0; i < 10; i++)
                {
                    short wrretLOCK = Focas1.pmc_wrpmcrng(h, length, wrpmcdata);
                }

                if (IdleDBit != 0)
                {
                    //IDLE Message D Bit
                    Focas1.IODBPMC0 wrpmcdataIDLE = rdpmcdataIdleBit;
                    wrpmcdataIDLE.cdata[0] = 1;
                    for (int i = 0; i < 10; i++)
                    {
                        short wrretIDLE = Focas1.pmc_wrpmcrng(h, length, wrpmcdataIDLE);
                    }

                    wrpmcdataIDLE.cdata[0] = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        short wrretIDLE = Focas1.pmc_wrpmcrng(h, length, wrpmcdataIDLE);
                    }
                }

                Thread.Sleep(500);
                //Lock D Bit reverted back to OFF State
                rdretLock = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdataLockBit);
                Focas1.IODBPMC0 wrpmcdata1 = rdpmcdataLockBit;
                wrpmcdata1.cdata[0] = 0;
                for (int i = 0; i < 10; i++)
                {
                    short wrret = Focas1.pmc_wrpmcrng(h, length, wrpmcdata1);
                }
            }
            else
            {
                //UnLocking D Bit Parameters
                Focas1.IODBPMC0 rdpmcdataLockBit = new Focas1.IODBPMC0();
                short adr_type = 9;
                short data_type = 0;
                ushort s_number = LockDBit;
                ushort e_number = LockDBit;
                ushort length = 9;

                short rdretLock = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdataLockBit);

                Focas1.IODBPMC0 wrpmcdata1 = rdpmcdataLockBit;
                wrpmcdata1.cdata[0] = 0;
                for (int i = 0; i < 100; i++)
                {
                    short wrret = Focas1.pmc_wrpmcrng(h, length, wrpmcdata1);
                    if (wrret == 0)
                    {
                        break;
                    }
                }
            }
        }

        //In Use - 2018-05-03
        private void setmachineUnlock(ushort h, ushort LockDBit, ushort UnLockDBit)
        {
            //Locking D Bit Parameters
            Focas1.IODBPMC0 rdpmcdataLockBit = new Focas1.IODBPMC0();
            short adr_type = 9;
            short data_type = 0;
            ushort s_number = LockDBit;
            ushort e_number = LockDBit;
            ushort length = 9;

            short rdretLock = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdataLockBit);

            Focas1.IODBPMC0 wrpmcdataLock = rdpmcdataLockBit;
            wrpmcdataLock.cdata[0] = 0;
            for (int i = 0; i < 10; i++)
            {
                short wrret = Focas1.pmc_wrpmcrng(h, length, wrpmcdataLock);
            }

            //UnLocking D Bit Parameters
            Focas1.IODBPMC0 rdpmcdataUnLockBit = new Focas1.IODBPMC0();
            short adr_typeUn = 9;
            short data_typeUn = 0;
            ushort s_numberUn = UnLockDBit;
            ushort e_numberUn = UnLockDBit;
            ushort lengthUn = 9;

            short rdretUnLock = Focas1.pmc_rdpmcrng(h, adr_typeUn, data_typeUn, s_numberUn, e_numberUn, lengthUn, rdpmcdataUnLockBit);

            Focas1.IODBPMC0 wrpmcdataUn = rdpmcdataUnLockBit;
            wrpmcdataUn.cdata[0] = 1;
            for (int i = 0; i < 10; i++)
            {
                short wrret = Focas1.pmc_wrpmcrng(h, lengthUn, wrpmcdataUn);
            }

            Thread.Sleep(500);
            //Lock D Bit reverted back to OFF State
            rdretUnLock = Focas1.pmc_rdpmcrng(h, adr_typeUn, data_typeUn, s_numberUn, e_numberUn, lengthUn, rdpmcdataUnLockBit);
            Focas1.IODBPMC0 wrpmcdata1 = rdpmcdataUnLockBit;
            wrpmcdata1.cdata[0] = 0;
            for (int i = 0; i < 10; i++)
            {
                short wrret = Focas1.pmc_wrpmcrng(h, lengthUn, wrpmcdata1);
            }
        }

        public void IntoFile(string Msg)
        {
            try
            {
                string path1 = AppDomain.CurrentDomain.BaseDirectory;
                string appPath = @"C:\\SRKSLog\\LogFile.txt";
                using (StreamWriter writer = new StreamWriter(appPath, true)) //true => Append Text
                {
                    writer.WriteLine(System.DateTime.Now + ":  " + Msg);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show("IntoFile Error " + e.ToString());
            }

        }


        public void INSERTMODE(string MacMode, int MacID, int InsertedBy, DateTime InsertedOn, DateTime correctedDate, DateTime StartTimeNext, string ColoCode, string ModeType)
        {
            try
            {
                unitworkccs_tbllivemode row = new unitworkccs_tbllivemode();
                row.MacMode = MacMode;
                row.MachineID = MacID;
                row.InsertedBy = InsertedBy;
                row.InsertedOn = InsertedOn;
                row.CorrectedDate = correctedDate;
                row.StartTime = StartTimeNext;
                row.ColorCode = ColoCode;
                row.ModeType = ModeType;
                using (unitworksccsEntities db1 = new unitworksccsEntities())
                {
                    db1.unitworkccs_tbllivemode.Add(row);
                    db1.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
        }

        public void INSERTMODE(string MacMode, int MacID, int InsertedBy, DateTime InsertedOn, DateTime correctedDate, DateTime StartTimeNext, string ColoCode, string ModeType,/*bool IsShift,*/int shift)
        {
            try
            {
                unitworkccs_tbllivemode row = new unitworkccs_tbllivemode();
                row.MacMode = MacMode;
                row.MachineID = MacID;
                row.InsertedBy = InsertedBy;
                row.InsertedOn = InsertedOn;
                row.CorrectedDate = correctedDate;
                row.StartTime = StartTimeNext;
                row.ColorCode = ColoCode;
                row.ModeType = ModeType;
                row.IsShiftEnd = shift;
                //if (shift == 1 && IsShift == true)
                //{
                //    row.IsShiftEnd = 1;
                //}
                using (unitworksccsEntities db1 = new unitworksccsEntities())
                {
                    db1.unitworkccs_tbllivemode.Add(row);
                    db1.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
        }

        private void UpdatetbllivemodeDetails(DateTime nowdate, int durationinsec, int StartIDLE, int ModeID)
        {
            try
            {
                unitworkccs_tbllivemode rowupdate = new unitworkccs_tbllivemode();
                using (unitworksccsEntities db = new unitworksccsEntities())
                {
                    rowupdate = db.unitworkccs_tbllivemode.Find(ModeID);
                }
                rowupdate.EndTime = nowdate;
                rowupdate.DurationInSec = durationinsec;
                rowupdate.IsCompleted = 1;
                rowupdate.ModeTypeEnd = 1;
                rowupdate.StartIdle = StartIDLE;

                using (unitworksccsEntities db1 = new unitworksccsEntities())
                {
                    db1.Entry(rowupdate).State = System.Data.Entity.EntityState.Modified;
                    db1.SaveChanges();
                    IntoFile("Update Mode:" + ModeID);


                }
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
        }

        private void UpdatetbllivemodeDetails(int StartIDLE, int ModeID)
        {
            try
            {
                unitworkccs_tbllivemode rowupdate = new unitworkccs_tbllivemode();
                using (unitworksccsEntities db = new unitworksccsEntities())
                {
                    rowupdate = db.unitworkccs_tbllivemode.Find(ModeID);
                }

                rowupdate.StartIdle = StartIDLE;

                using (unitworksccsEntities db1 = new unitworksccsEntities())
                {
                    db1.Entry(rowupdate).State = System.Data.Entity.EntityState.Modified;
                    db1.SaveChanges();
                    IntoFile("Update Mode:" + ModeID);
                }
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
        }

        private void UpdatetbllivemodeDetails(DateTime nowdate, int durationinsec, int ModeID)
        {
            try
            {
                unitworkccs_tbllivemode rowupdate = new unitworkccs_tbllivemode();
                using (unitworksccsEntities db = new unitworksccsEntities())
                {
                    rowupdate = db.unitworkccs_tbllivemode.Find(ModeID);
                }
                rowupdate.EndTime = nowdate;
                rowupdate.DurationInSec = durationinsec;
                rowupdate.IsCompleted = 1;
                rowupdate.ModeTypeEnd = 1;

                using (unitworksccsEntities db1 = new unitworksccsEntities())
                {
                    db1.Entry(rowupdate).State = System.Data.Entity.EntityState.Modified;
                    db1.SaveChanges();
                    IntoFile("Update Mode:" + ModeID);
                }
            }
            catch (Exception ex)
            {
                IntoFile("Update Mode exception:" + ex);
            }
        }


        //private void UpdatetbllivemodeDetails(DateTime nowdate, int durationinsec, int ModeID,bool IsShift,int shift)
        //{
        //    try
        //    {
        //        tbllivemode rowupdate = new tbllivemode();
        //        using (unitworksccsEntities db = new unitworksccsEntities())
        //        {
        //            rowupdate = db.tbllivemodes.Find(ModeID);
        //        }
        //        rowupdate.EndTime = nowdate;
        //        rowupdate.DurationInSec = durationinsec;
        //        rowupdate.IsCompleted = 1;
        //        rowupdate.ModeTypeEnd = 1;
        //        rowupdate.IsShiftEnd = shift;
        //        //if (shift == 1 && IsShift==true)
        //        //{
        //        //    rowupdate.IsShiftEnd = 1;
        //        //}
        //        //else if (shift == 2 && IsShift == true)
        //        //{
        //        //    rowupdate.IsSecondShift = 1;
        //        //}
        //        //else if(shift==3 && IsShift == true)
        //        //{
        //        //    rowupdate.IsThirdShift = 1;
        //        //}

        //        using (unitworksccsEntities db1 = new unitworksccsEntities())
        //        {
        //            db1.Entry(rowupdate).State = System.Data.Entity.EntityState.Modified;
        //            db1.SaveChanges();
        //            IntoFile("Update Mode:" + ModeID);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        IntoFile("Update Mode exception:" + ex);
        //    }
        //}

        private void DeleteModeDetails(int ModeID)
        {
            try
            {
                using (unitworksccsEntities db = new unitworksccsEntities())
                {
                    unitworkccs_tbllivemode row = db.unitworkccs_tbllivemode.Find(ModeID);
                    db.Entry(row).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
        }

        private void InsertOperationLogDetails(string OpMsg, DateTime OpDate, TimeSpan opTime, DateTime OpDateTime, string opReason, int mcid)
        {
            try
            {
                unitworkccs_operationlog row = new unitworkccs_operationlog();
                row.OpMsg = OpMsg;
                row.OpDate = OpDate;
                row.OpTime = opTime;
                row.OpDateTime = OpDateTime;
                row.OpReason = opReason;
                row.MachineID = mcid;

                using (unitworksccsEntities db1 = new unitworksccsEntities())
                {
                    db1.unitworkccs_operationlog.Add(row);
                    db1.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
        }

        public int GetShift()
        {
            string CorrectedDate = GetCorrectedDate().ToString("yyyy-MM-dd");
            int shift = 0;
            var msgs2 = new List<unitworkccs_shift_master>();
            using (unitworksccsEntities db1 = new unitworksccsEntities())
            {
                msgs2 = db1.unitworkccs_shift_master.Where(m => m.IsDeleted == 0).ToList();
            }
            String[] msgtime = DateTime.Now.ToString("HH:mm:ss").Split(':');
            TimeSpan msgstime = new TimeSpan(Convert.ToInt32(msgtime[0]), Convert.ToInt32(msgtime[1]), Convert.ToInt32(msgtime[2]));
            TimeSpan s1t1 = new TimeSpan(0, 0, 0), s1t2 = new TimeSpan(0, 0, 0);
            TimeSpan s2t1 = new TimeSpan(0, 0, 0), s2t2 = new TimeSpan(0, 0, 0);
            TimeSpan s3t1 = new TimeSpan(0, 0, 0), s3t2 = new TimeSpan(0, 0, 0), s3t3 = new TimeSpan(0, 0, 0), s3t4 = new TimeSpan(23, 59, 59);
            for (int j = 0; j < msgs2.Count; j++)
            {
                if (msgs2[j].ShiftName.ToString().Contains("1") || msgs2[j].ShiftName.ToString().Contains("A"))
                {
                    String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                    s1t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                    String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                    s1t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                }
                if (msgs2[j].ShiftName.ToString().Contains("2") || msgs2[j].ShiftName.ToString().Contains("B"))
                {
                    String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                    s2t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                    String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                    s2t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                }
                if (msgs2[j].ShiftName.ToString().Contains("3") ||  msgs2[j].ShiftName.ToString().Contains("C"))
                {
                    String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                    s3t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                    String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                    s3t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                }
            }

            if (msgstime >= s1t1 && msgstime < s1t2)
            {
                shift = 1;
            }
            else if (msgstime >= s2t1 && msgstime < s2t2)
            {
                shift = 2;
            }
            else if ((msgstime >= s3t1 && msgstime <= s3t4) || (msgstime >= s3t3 && msgstime < s3t2))
            {
                shift = 3;
                //if (msgstime >= s3t3 && msgstime < s3t2)
                //{
                //    CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                //}
            }


            return shift;

        }


    }
}
