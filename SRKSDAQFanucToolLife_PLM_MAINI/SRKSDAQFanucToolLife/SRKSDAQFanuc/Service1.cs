using System;
using System.Data;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Z.BulkOperations;
using OfficeOpenXml;


namespace SRKSDAQFanucToolLife
{
    public partial class Service1 : ServiceBase
    {
        ushort port;
        int timeout;
        MySqlConnection conn;
        DateTime PrvDT;
        List<AxisDetails1> AxisDetlist1 = new List<AxisDetails1>();
        List<AxisDetails2> AxisDetlist2 = new List<AxisDetails2>();
        List<ServoDeatailsModel> ServoDetList = new List<ServoDeatailsModel>();
        List<MachineStatusModel> MachineStatList = new List<MachineStatusModel>();
        public Service1()
        {
            InitializeComponent();
            MsqlConnection mc = new MsqlConnection();
            conn = mc.msqlConnection;
            port = 3306;
            timeout = 1;
        }
        protected override void OnStart(string[] args)
        {
            System.Timers.Timer T1 = new System.Timers.Timer();
            T1.Interval = (1000 * 1);  //1sec
            T1.AutoReset = true;
            T1.Enabled = true;
            T1.Elapsed += new System.Timers.ElapsedEventHandler(insertdb);

            System.Timers.Timer T2 = new System.Timers.Timer();
            T2.Interval = (1000 * 60 * 5);  //5min
            T2.AutoReset = true;
            T2.Enabled = true;
            T2.Elapsed += new System.Timers.ElapsedEventHandler(GetDataIntoEXCEL_DB);
        }

        protected override void OnStop()
        {
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason) VALUES('SRKS DataLogging Service was by stopped','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error')");
                cmd.ExecuteNonQuery();
                mc.close();
            }
        }

        protected override void OnShutdown()
        {
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason) VALUES('SRKS DataLogging Service was by stopped because of System ShutDown','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','M/c Error')");
                cmd.ExecuteNonQuery();
                mc.close();
            }
            base.OnShutdown();
        }

        private void insertdb(object sender, System.Timers.ElapsedEventArgs e)
        {
            {
                DataTable dt = new DataTable();
                //For Cell 1 Machines to Verify the 100MBPS Connection
                String query = "SELECT IPAddress,MachineType,MachineID,IsParameters,CurrentControlAxis,MachineLockBit, MachineSetupBit, MachineMaintBit, MachineToolLifeBit, MachineUnlockBit, MachineIdleBit, MachineIdleMin, EnableLockLogic, EnableToolLife  From unitworksccs.`unitworkccs.tblmachinedetails` WHERE IsDeleted = 0  order by MachineID";
                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    MySqlDataAdapter da = new MySqlDataAdapter(query, mc.msqlConnection);
                    da.Fill(dt);
                    mc.close();
                }
                int count = dt.Rows.Count;
                CountdownEvent cntevent = new CountdownEvent(count);
                for (int j = 0; j < count; j++)
                {
                    string ip = Convert.ToString(dt.Rows[j][0]);
                    int type = Convert.ToInt32(dt.Rows[j][1]);
                    int mcid = Convert.ToInt32(dt.Rows[j][2]);
                    int ParameterExcep = Convert.ToInt32(dt.Rows[j][3]);
                    int NoOfAxis = Convert.ToInt32(dt.Rows[j][4]);
                    int MacLockbit = Convert.ToInt32(dt.Rows[j][5]);
                    int MacSetupbit = Convert.ToInt32(dt.Rows[j][6]);
                    int MacMaintbit = Convert.ToInt32(dt.Rows[j][7]);
                    int MacToolLifebit = Convert.ToInt32(dt.Rows[j][8]);
                    int MacUnlockbit = Convert.ToInt32(dt.Rows[j][9]);
                    int MacIdlebit = Convert.ToInt32(dt.Rows[j][10]);
                    int MacIdleMin = Convert.ToInt32(dt.Rows[j][11]);
                    int EnableLock = Convert.ToInt32(dt.Rows[j][12]);
                    int EnableTool = Convert.ToInt32(dt.Rows[j][13]);
                    if (EnableTool == 1)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            //MsqlConnection mc1 = new MsqlConnection();
                            //mc1.open();
                            //Parameters for Functions For Focas Libraries
                            ushort h; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                            short ret; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                            short freeret;

                            //ATCCounter
                            ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
                            try
                            {
                                if (ret == 0)
                                {
                                    try
                                    {
                                        GetToolATC(h, mcid);
                                    }
                                    catch (Exception ex)
                                    {
                                        using (MsqlConnection mcConn = new MsqlConnection())
                                        {
                                            mcConn.close();
                                            mcConn.open();
                                            MySqlCommand cmd2 = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Tool : " + @ex.ToString() + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Tool Stop',100" + mcid + ")", mcConn.msqlConnection);
                                            cmd2.ExecuteNonQuery();
                                            mcConn.close();
                                        }
                                    }
                                    freeret = Focas1.cnc_freelibhndl(h);
                                    ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
                                    try
                                    {
                                        InsertAxisDetails(h, mcid, NoOfAxis);
                                    }
                                    catch (Exception ex)
                                    {
                                        using (MsqlConnection mcConn = new MsqlConnection())
                                        {
                                            mcConn.close();
                                            mcConn.open();
                                            MySqlCommand cmd2 = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Axis : " + @ex.ToString() + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Axis Stop',200" + mcid + ")", mcConn.msqlConnection);
                                            cmd2.ExecuteNonQuery();
                                            mcConn.close();
                                        }
                                    }
                                    freeret = Focas1.cnc_freelibhndl(h);
                                    ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
                                    try
                                    {
                                        InsertServoDetails(h, mcid, NoOfAxis);
                                    }
                                    catch (Exception ex)
                                    {
                                        using (MsqlConnection mcConn = new MsqlConnection())
                                        {
                                            mcConn.close();
                                            mcConn.open();
                                            MySqlCommand cmd2 = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Servo : " + @ex.ToString() + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Servo Stop',300" + mcid + ")", mcConn.msqlConnection);
                                            cmd2.ExecuteNonQuery();
                                            mcConn.close();
                                        }
                                    }
                                    freeret = Focas1.cnc_freelibhndl(h);
                                    ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
                                    try
                                    {
                                        String MachineStatus = "";
                                        String MachineEmergency = "";
                                        String MachineAlm = "";

                                        InsertMachineStatus(h, mcid, out MachineStatus, out MachineEmergency, out MachineAlm);

                                        MachineStatusModel machinemodel = new MachineStatusModel();
                                        machinemodel.MachineID = mcid;
                                        machinemodel.MachineStatus = MachineStatus;
                                        machinemodel.MachineEmergency = MachineEmergency;
                                        machinemodel.MachineAlarm = MachineAlm;
                                        machinemodel.CreatedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        machinemodel.CreatedBy = 0;
                                        machinemodel.CorrectedDate = GetCorrectedDate().ToString("yyyy-MM-dd");
                                        MachineStatList.Add(machinemodel);
                                        //using (MsqlConnection mcConn = new MsqlConnection())
                                        //{
                                        //    mcConn.open();
                                        //    SqlCommand MachineStatuscmd = new SqlCommand("INSERT INTO tbl_machinestatusrealtime(MachineID,MachineStatus,MachineEmergency,MachineAlarm,CreatedOn,CreatedBy,CorrectedDate)" +
                                        //                            "VALUES(" + mcid + ",'" + MachineStatus + "','" + MachineEmergency + "','" + MachineAlm + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0,'" + GetCorrectedDate().ToString("yyyy-MM-dd") + "')", mcConn.msqlConnection);
                                        //    MachineStatuscmd.ExecuteNonQuery();
                                        //    mcConn.close();
                                        //}
                                    }
                                    catch (Exception ex)
                                    {
                                        using (MsqlConnection mcConn = new MsqlConnection())
                                        {
                                            mcConn.close();
                                            mcConn.open();
                                            MySqlCommand cmd2 = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Status : " + @ex.ToString() + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Status Stop',300" + mcid + ")", mcConn.msqlConnection);
                                            cmd2.ExecuteNonQuery();
                                            mcConn.close();
                                        }
                                    }
                                    freeret = Focas1.cnc_freelibhndl(h);
                                }
                                else
                                {
                                    using (MsqlConnection mcConn = new MsqlConnection())
                                    {
                                        mcConn.close();
                                        mcConn.open();
                                        MySqlCommand cmd2 = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('Connection not Established','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','No Connection',100" + mcid + ")", mcConn.msqlConnection);
                                        cmd2.ExecuteNonQuery();
                                        mcConn.close();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                using (MsqlConnection mcConn = new MsqlConnection())
                                {
                                    mcConn.close();
                                    mcConn.open();
                                    MySqlCommand cmd2 = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.operationlog`(OpMsg,OpDate,OpTime,OpDateTime,OpReason,MachineID) VALUES('" + @ex.ToString() + "','" + System.DateTime.Now.ToString("yyyy-MM-dd") + "','" + System.DateTime.Now.ToString("HH:mm:ss") + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','Tool Stop',100" + mcid + ")", mcConn.msqlConnection);
                                    cmd2.ExecuteNonQuery();
                                    mcConn.close();
                                }
                            }
                            freeret = Focas1.cnc_freelibhndl(h);
                            //mc1.close();
                            cntevent.Signal();
                        });
                    }
                }
                cntevent.Wait();
                cntevent.Dispose();
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
            DataTable dataHolderTool = new DataTable();
            DateTime Correcteddate = GetCorrectedDate();
            int WOID = 0;
            int PrvToolNum = -1;
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                string SelectQuery = "SELECT HMIID FROM unitworksccs.`unitworkccs.tblworkorderentry` where MachineID = '" + MacID + "' and IsStarted = 1 and IsFinished = 0 and IsHold = 0 order by HMIID DESC ;";
                MySqlDataAdapter da1 = new MySqlDataAdapter(SelectQuery, mc.msqlConnection);
                da1.Fill(dataHolder1);
                mc.close();
            }
            if (dataHolder1.Rows.Count > 0)
            {
                WOID = Convert.ToInt32(dataHolder1.Rows[0][0]);
            }
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                string PrvToolQuery = "SELECT PresentToolNum FROM unitworksccs.`unitworkccs.tblPresentTool` where MachineID = '" + MacID + "';";
                MySqlDataAdapter daPrvTool = new MySqlDataAdapter(PrvToolQuery, mc.msqlConnection);
                daPrvTool.Fill(dataHolderTool);
                mc.close();
            }
            if (dataHolderTool.Rows.Count > 0)
            {
                PrvToolNum = Convert.ToInt32(dataHolderTool.Rows[0][0]);
            }

            if (CycleStart == 1 && WOID != 0 && PrvToolNum != CurATCVal)
            {

                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    string SelectQuery = "SELECT ToolNo,toollifecounter,ToolLifeID FROM unitworksccs.`unitworkccs.tbltoollifeoperator` where MachineID = '" + MacID + "' and ToolNo = " + CurATCVal + " and HMIID = " + WOID + " and IsCompleted = 'FALSE' and IsCycleStart = 'FALSE' order by ToolLifeID DESC ;";
                    MySqlDataAdapter da1 = new MySqlDataAdapter(SelectQuery, mc.msqlConnection);
                    da1.Fill(dataHolder);
                    mc.close();
                }

                if (dataHolder.Rows.Count > 0)
                {
                    //int prvATCVal = Convert.ToInt32(dataHolder.Rows[0][0]);
                    int CycCounter = Convert.ToInt32(dataHolder.Rows[0][1]) + 1;
                    int ToolLifeID = Convert.ToInt32(dataHolder.Rows[0][2]);
                    using (MsqlConnection mc = new MsqlConnection())
                    {
                        mc.open();
                        MySqlCommand cmdUpdateRows = new MySqlCommand("UPDATE unitworksccs.`unitworkccs.tbltoollifeoperator` Set toollifecounter = " + CycCounter + ", IsCycleStart = 'TRUE'  Where ToolLifeID = " + ToolLifeID + "", mc.msqlConnection);
                        cmdUpdateRows.ExecuteNonQuery();
                        mc.close();
                    }
                }

                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    MySqlCommand cmdUpdateRows = new MySqlCommand("UPDATE unitworksccs.`unitworkccs.tblPresentTool` set PresentToolNum = " + CurATCVal + " WHERE MachineID = " + MacID + "", mc.msqlConnection);
                    if (PrvToolNum == -1)
                    {
                        cmdUpdateRows = new MySqlCommand("INSERT INTO unitworksccs.`unitworkccs.tblPresentTool` (MachineID,PresentToolNum) VALUES(" + MacID + "," + CurATCVal + ")", mc.msqlConnection);
                    }
                    cmdUpdateRows.ExecuteNonQuery();
                    mc.close();
                }
            }
            else if (CycleStart == 0 && WOID != 0)
            {
                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    MySqlCommand cmdUpdateRows = new MySqlCommand("UPDATE unitworksccs.`unitworkccs.tbltoollifeoperator` Set IsCycleStart = 'FALSE' Where HMIID = " + WOID + " and IsCompleted = 0;", mc.msqlConnection);
                    cmdUpdateRows.ExecuteNonQuery();
                    mc.close();
                }
                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    MySqlCommand cmdUpdateRows = new MySqlCommand("UPDATE unitworksccs.`unitworkccs.tblPresentTool` set PresentToolNum = 0 WHERE MachineID = " + MacID + "", mc.msqlConnection);
                    cmdUpdateRows.ExecuteNonQuery();
                    mc.close();
                }
            }
            return false;
        }

        public int GetMachineStatus(ushort h, int MachineID)
        {
            int retstatus = 0;
            Focas1.ODBST MacStatus = new Focas1.ODBST();
            short StatRet = Focas1.cnc_statinfo(h, MacStatus);
            if (MacStatus.aut == 1 && MacStatus.run == 3)
            {
                retstatus = 1;
            }
            else if (MacStatus.aut == 1 && MacStatus.run == 0)
            {
                retstatus = 1;
            }
            else
            {
                retstatus = 0;
            }
            return retstatus;
        }

        private DateTime GetCorrectedDate()
        {
            DateTime correctedDate = DateTime.Now;
            //tbldaytiming StartTime = _UWcontext.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
            DataTable dtMode = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                string SelectQuery = "SELECT StartTime FROM unitworksccs.`unitworkccs.tbldaytiming` where IsDeleted = 0;";
                MySqlDataAdapter da1 = new MySqlDataAdapter(SelectQuery, mc.msqlConnection);
                da1.Fill(dtMode);
                mc.close();
            }

            DateTime Start = Convert.ToDateTime(dtMode.Rows[0][0].ToString());
            if (Start.Hour <= DateTime.Now.Hour)
            {
                correctedDate = DateTime.Now.Date;
            }
            else
            {
                correctedDate = DateTime.Now.AddDays(-1).Date;
            }
            return correctedDate;
        }

        //Inserting the Axis, Feedrate and Spindle Details
        private void InsertAxisDetails(ushort h, int machineid, int NoOfAxis)
        {
            Focas1.ODBDY2_2 ReadVar = new Focas1.ODBDY2_2();
            short datalength = (short)44;// (28 + 4 * (4 * n));
            int insert = -1;
            for (int i = 1; i <= NoOfAxis; i++)
            {
                short posdataret = Focas1.cnc_rddynamic2(h, (short)i, datalength, ReadVar);

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
                System.Data.DataTable dtTC = new System.Data.DataTable();
                String GetAxis = "SELECT AxisName FROM unitworksccs.`unitworkccs.tbl_axisdet` WHERE MachineID = " + machineid + " and AxisID = " + Convert.ToInt32(AxisNo) + " and IsDeleted = 0;";
                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    MySqlDataAdapter daTC = new MySqlDataAdapter(GetAxis, mc.msqlConnection);
                    daTC.Fill(dtTC);
                    mc.close();
                }
                if (dtTC.Rows.Count > 0)
                {
                    AxisNo = dtTC.Rows[0][0].ToString();
                }


                if (AxisNo != "" || AxisNo != null)
                {
                    AxisDetails1 ax = new AxisDetails1();
                    ax.MachineID = machineid;
                    ax.Axis = AxisNo;
                    ax.AbsPos = Convert.ToDecimal(AbsPos);
                    ax.RelPos = Convert.ToDecimal(RelPos);
                    ax.MacPos = Convert.ToDecimal(MacPos);
                    ax.DistPos = Convert.ToDecimal(DisPos);
                    ax.StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ax.InsertedOn = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ax.IsDeleted = 0;
                    ax.CorrectedDate = GetCorrectedDate().ToString("yyyy-MM-dd");
                    AxisDetlist1.Add(ax);

                    //SqlCommand cmd1 = new SqlCommand("INSERT INTO tbl_axisdetails1(MachineID,Axis,AbsPos,RelPos,MacPos,DistPos,StartTime,IsDeleted,InsertedOn,CorrectedDate)" + //
                    //    "VALUES('" + machineid + "','" + AxisNo + "','" + AbsPos + "'," + RelPos + ",'" + MacPos + "','" + DisPos + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:00") + "',0,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + GetCorrectedDate().ToString("yyyy-MM-dd") + "')", mc.msqlConnection);
                    //cmd1.ExecuteNonQuery();
                    insert++;
                }

                if (i == 1 || insert == 0)
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

                    //Focas1.ODBACT FR = new Focas1.ODBACT();
                    //var ActFr = Focas1.cnc_actf(h, FR);
                    //FeedRate Unit
                    //FWLIBAPI short WINAPI cnc_rdaxisdata(unsigned short FlibHndl, short cls, short* type, short num, short* len, ODBAXDT* axdata);
                    string FeedRateUnitVal = null;
                    string FeedRateUnitError = null;
                    short DataLen = 64;
                    Focas1.ODBAXDT FeedRateUnit = new Focas1.ODBAXDT();
                    short GModalRet = Focas1.cnc_rdaxisdata(h, 5, 0, 1, ref DataLen, FeedRateUnit);
                    switch (GModalRet)
                    {
                        case 0:
                            {
                                short unitVal = (short)FeedRateUnit.data1.unit;
                                if (unitVal != 0)
                                {
                                    switch (unitVal)
                                    {
                                        case 3:
                                            FeedRateUnitVal = "mm/minute";
                                            break;
                                        case 4:
                                            FeedRateUnitVal = "inch/minute";
                                            break;
                                        case 6:
                                            FeedRateUnitVal = "mm/round";
                                            break;
                                        case 7:
                                            FeedRateUnitVal = "inch/round";
                                            break;
                                    }
                                }
                                break;
                            }

                        case 2:
                            FeedRateUnitError = "Number of axis(*len) is less or equal 0. ";
                            break;
                        case 3:
                            FeedRateUnitError = "Data class(cls) is wrong. ";
                            break;
                        case 4:
                            FeedRateUnitError = "Kind of data(type) is wrong, or The number of kind(num) exceeds 4. ";
                            break;
                        case 6:
                            FeedRateUnitError = "Required option to read data is not specified. ";
                            break;
                    }

                    //FeedRate = FeedRate + " " + FeedRateUnitVal; //FeedRate Actual 4

                    //Spindle Temperature Capture
                    int SpndlTempVal = 0;
                    ReadDiagServTempData(h, (short)403, (short)i, out SpndlTempVal);

                    AxisDetails2 ax2 = new AxisDetails2();
                    ax2.MachineID = machineid;
                    ax2.FeedRate = FeedRate;
                    ax2.SpindleLoad = SpindleLoad;
                    ax2.StartTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ax2.IsDeleted = 0;
                    ax2.InsertedOn = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ax2.SpindleTemperature = SpndlTempVal;
                    ax2.CorrectedDate = GetCorrectedDate().ToString("yyyy-MM-dd");
                    ax2.FeedRateUnit = FeedRateUnitVal;
                    AxisDetlist2.Add(ax2);
                    //SqlCommand cmd2 = new SqlCommand("INSERT INTO  tbl_axisdetails2(MachineID,FeedRate,SpindleLoad,SpindleSpeed,StartTime,IsDeleted,InsertedOn,SpindleTemperature,CorrectedDate,FeedRateUnit)" + //
                    //    "VALUES('" + machineid + "','" + FeedRate + "','" + SpiLoadMain.ToString() + "'," + SpindleSpeed + ",'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:00") + "',0,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + SpndlTempVal + ",'" + GetCorrectedDate().ToString("yyyy-MM-dd") + "','" + FeedRateUnitVal + "')", mc.msqlConnection);
                    //cmd2.ExecuteNonQuery();

                }
            }
        }

        //Inserting the Servo Details
        private void InsertServoDetails(ushort h, int machineid, int NoOfAxis)
        {
            short DataLen = 3;
            Focas1.ODBDGN_4 ServoTemp = new Focas1.ODBDGN_4();
            Focas1.ODBAXDT ServLoad = new Focas1.ODBAXDT();
            Focas1.ODBAXDT ServCurrPer = new Focas1.ODBAXDT();
            Focas1.ODBAXDT ServCurrAmp = new Focas1.ODBAXDT();
            var ServRetLoad = Focas1.cnc_rdaxisdata(h, 2, 0, 1, ref DataLen, ServLoad);
            var ServRetCurPer = Focas1.cnc_rdaxisdata(h, 2, 1, 1, ref DataLen, ServCurrPer);
            var ServRetCurAmp = Focas1.cnc_rdaxisdata(h, 2, 2, 1, ref DataLen, ServCurrAmp);

            for (int i = 0; i < NoOfAxis; i++)
            {
                short AxisNO = (short)(i + 1);
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

                        //Servo Temperature Capture
                        int ServTempVal = 0;
                        ReadDiagServTempData(h, (short)308, AxisNO, out ServTempVal);

                        //Servo Module Cooling Fan Speed Capture
                        int ServModCFSVal = 0;
                        ReadDiagServTempData(h, (short)1711, AxisNO, out ServModCFSVal);

                        //DC LINK Voltage Capture
                        int DCLinkVoltVal = 0;
                        ReadDiagServTempData(h, (short)752, AxisNO, out DCLinkVoltVal);


                        //using (MsqlConnection mc = new MsqlConnection())
                        //{
                        if (AxisName != "" || AxisName != null)
                        {
                            ServoDeatailsModel serv = new ServoDeatailsModel();
                            serv.MachineID = machineid;
                            serv.ServoAxis = AxisName;
                            serv.ServoLoadMeter = ServLoadMain;
                            serv.LoadCurrent = ServCurPerMain;
                            serv.LoadCurrentAmp = ServCurPerMain;
                            serv.StartDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            serv.IsDeleted = 0;
                            serv.InsertedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            serv.Insertedby = 1;
                            serv.ServoTemperature = ServTempVal;
                            serv.ServoCoolingFanSpeed = ServModCFSVal;
                            serv.CorrectedDate = GetCorrectedDate().ToString("yyyy-MM-dd");
                            serv.DCLinkVoltage = DCLinkVoltVal;
                            ServoDetList.Add(serv);
                            //mc.open();
                            //SqlCommand cmd1 = new SqlCommand("INSERT INTO tbl_servodetails(MachineID,ServoAxis,ServoLoadMeter,LoadCurrent,LoadCurrentAmp,StartDateTime,IsDeleted,InsertedOn,InsertedBy,ServoTemperature,ServoCoolingFanSpeed,CorrectedDate,DCLinkVoltage)" + //
                            //        "VALUES('" + machineid + "','" + AxisName + "','" + ServLoadMain + "','" + ServCurPerMain + "','" + ServcurrAmpMain + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',1," + ServTempVal + "," + ServModCFSVal + ",'" + GetCorrectedDate().ToString("yyyy-MM-dd") + "'," + DCLinkVoltVal + ")", mc.msqlConnection);
                            //cmd1.ExecuteNonQuery();
                            //mc.close();
                            //}
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
                        int ServTempVal1 = 0;
                        ReadDiagServTempData(h, (short)308, AxisNO, out ServTempVal1);

                        int ServModCFSVal1 = 0;
                        ReadDiagServTempData(h, (short)1711, AxisNO, out ServModCFSVal1);

                        //DC LINK Voltage Capture
                        int DCLinkVoltVal1 = 0;
                        ReadDiagServTempData(h, (short)752, AxisNO, out DCLinkVoltVal1);


                        //using (MsqlConnection mc = new MsqlConnection())
                        //{
                        if (AxisName2 != "" || AxisName2 != null)
                        {

                            ServoDeatailsModel serv = new ServoDeatailsModel();
                            serv.MachineID = machineid;
                            serv.ServoAxis = AxisName2;
                            serv.ServoLoadMeter = ServLoadMain2;
                            serv.LoadCurrent = ServCurPerMain2;
                            serv.LoadCurrentAmp = ServCurPerMain2;
                            serv.StartDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            serv.IsDeleted = 0;
                            serv.InsertedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            serv.Insertedby = 1;
                            serv.ServoTemperature = ServTempVal1;
                            serv.ServoCoolingFanSpeed = ServModCFSVal1;
                            serv.CorrectedDate = GetCorrectedDate().ToString("yyyy-MM-dd");
                            serv.DCLinkVoltage = DCLinkVoltVal1;
                            ServoDetList.Add(serv);

                            //mc.open();
                            //SqlCommand cmd1 = new SqlCommand("INSERT INTO  tbl_servodetails(MachineID,ServoAxis,ServoLoadMeter,LoadCurrent,LoadCurrentAmp,StartDateTime,IsDeleted,InsertedOn,InsertedBy,ServoTemperature,ServoCoolingFanSpeed,CorrectedDate,DCLinkVoltage)" + //
                            //        "VALUES('" + machineid + "','" + AxisName2 + "','" + ServLoadMain2 + "','" + ServCurPerMain2 + "','" + ServcurrAmpMain2 + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',1," + ServTempVal1 + "," + ServModCFSVal1 + ",'" + GetCorrectedDate().ToString("yyyy-MM-dd") + "'," + DCLinkVoltVal1 + ")", mc.msqlConnection);
                            //cmd1.ExecuteNonQuery();
                            //mc.close();
                        }
                        //}
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
                        int ServTempVal2 = 0;
                        ReadDiagServTempData(h, (short)308, AxisNO, out ServTempVal2);

                        int ServModCFSVal2 = 0;
                        ReadDiagServTempData(h, (short)1711, AxisNO, out ServModCFSVal2);

                        //DC LINK Voltage Capture
                        int DCLinkVoltVal2 = 0;
                        ReadDiagServTempData(h, (short)752, AxisNO, out DCLinkVoltVal2);
                        //using (MsqlConnection mc = new MsqlConnection())
                        //{
                        if (AxisName3 != "" || AxisName3 != null)
                        {

                            ServoDeatailsModel serv = new ServoDeatailsModel();
                            serv.MachineID = machineid;
                            serv.ServoAxis = AxisName3;
                            serv.ServoLoadMeter = ServLoadMain3;
                            serv.LoadCurrent = ServCurPerMain3;
                            serv.LoadCurrentAmp = ServCurPerMain3;
                            serv.StartDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            serv.IsDeleted = 0;
                            serv.InsertedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            serv.Insertedby = 1;
                            serv.ServoTemperature = ServTempVal2;
                            serv.ServoCoolingFanSpeed = ServModCFSVal2;
                            serv.CorrectedDate = GetCorrectedDate().ToString("yyyy-MM-dd");
                            serv.DCLinkVoltage = DCLinkVoltVal2;
                            ServoDetList.Add(serv);
                            //mc.open();
                            //SqlCommand cmd1 = new SqlCommand("INSERT INTO tbl_servodetails(MachineID,ServoAxis,ServoLoadMeter,LoadCurrent,LoadCurrentAmp,StartDateTime,IsDeleted,InsertedOn,InsertedBy,ServoTemperature,ServoCoolingFanSpeed,CorrectedDate,DCLinkVoltage)" + //
                            //        "VALUES('" + machineid + "','" + AxisName3 + "','" + ServLoadMain3 + "','" + ServCurPerMain3 + "','" + ServcurrAmpMain3 + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',1," + ServTempVal2 + "," + ServModCFSVal2 + ",'" + GetCorrectedDate().ToString("yyyy-MM-dd") + "'," + DCLinkVoltVal2 + ")", mc.msqlConnection);
                            //cmd1.ExecuteNonQuery();
                            //mc.close();
                        }
                        //}
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
                        int ServTempVal3 = 0;
                        ReadDiagServTempData(h, (short)308, AxisNO, out ServTempVal3);

                        int ServModCFSVal3 = 0;
                        ReadDiagServTempData(h, (short)1711, AxisNO, out ServModCFSVal3);

                        //DC LINK Voltage Capture
                        int DCLinkVoltVal3 = 0;
                        ReadDiagServTempData(h, (short)752, AxisNO, out DCLinkVoltVal3);
                        //using (MsqlConnection mc = new MsqlConnection())
                        //{
                        if (AxisName4 != "" || AxisName4 != null)
                        {
                            ServoDeatailsModel serv = new ServoDeatailsModel();
                            serv.MachineID = machineid;
                            serv.ServoAxis = AxisName4;
                            serv.ServoLoadMeter = ServLoadMain4;
                            serv.LoadCurrent = ServCurPerMain4;
                            serv.LoadCurrentAmp = ServCurPerMain4;
                            serv.StartDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            serv.IsDeleted = 0;
                            serv.InsertedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            serv.Insertedby = 1;
                            serv.ServoTemperature = ServTempVal3;
                            serv.ServoCoolingFanSpeed = ServModCFSVal3;
                            serv.CorrectedDate = GetCorrectedDate().ToString("yyyy-MM-dd");
                            serv.DCLinkVoltage = DCLinkVoltVal3;
                            ServoDetList.Add(serv);

                            //mc.open();
                            //SqlCommand cmd1 = new SqlCommand("INSERT INTO tbl_servodetails(MachineID,ServoAxis,ServoLoadMeter,LoadCurrent,LoadCurrentAmp,StartDateTime,IsDeleted,InsertedOn,InsertedBy,ServoTemperature,ServoCoolingFanSpeed,CorrectedDate,DCLinkVoltage)" + //
                            //        "VALUES('" + machineid + "','" + AxisName4 + "','" + ServLoadMain4 + "','" + ServCurPerMain4 + "','" + ServcurrAmpMain4 + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0,'" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',1," + ServTempVal3 + "," + ServModCFSVal3 + ",'" + GetCorrectedDate().ToString("yyyy-MM-dd") + "'," + DCLinkVoltVal3 + ")", mc.msqlConnection);
                            //cmd1.ExecuteNonQuery();
                            //mc.close();
                        }
                        //}
                        break;
                }
            }
        }

        private void ReadDiagServTempData(ushort h, short DiagNum, short Axis, out int ServTempVal)
        {
            Focas1.ODBDGN_1 DiagServTemp = new Focas1.ODBDGN_1();
            short ServTemp = DiagNum;
            short ServLength = 8;

            short DiagRet = Focas1.cnc_diagnoss(h, ServTemp, Axis, ServLength, DiagServTemp);
            ServTempVal = DiagServTemp.ldata;
            //MessageBox.Show("X Axis Temp: " + DiagServTempX.ldata + "\nY Axis Temp: " + DiagServTempY.ldata + "\nZ Axis Temp: " + DiagServTempZ.ldata + "\nA Axis Temp: " + DiagServTempA);
        }

        //private void TVSParameterCheck(ushort h)
        //{
        //    var rdpmcdataMAP = new Focas1.IODBPMC0();
        //    var rdpmcdataPCP = new Focas1.IODBPMC0();
        //    var rdpmcdataPSP = new Focas1.IODBPMC0();
        //    var rdpmcdataLOL = new Focas1.IODBPMC0();
        //    var rdpmcdataCOL = new Focas1.IODBPMC0();
        //    var rdpmcdataLOP = new Focas1.IODBPMC0();
        //    var rdpmcdataLOP1 = new Focas1.IODBPMC0();
        //    short adr_type = 9;
        //    short data_type = 0;
        //    ushort s_numberMAP = (ushort)8888;
        //    ushort e_numberMAP = (ushort)8888;
        //    ushort s_numberPCP = (ushort)8889;
        //    ushort e_numberPCP = (ushort)8889;
        //    ushort s_numberPSP = (ushort)8890;
        //    ushort e_numberPSP = (ushort)8890;
        //    ushort s_numberLOL = (ushort)8892;
        //    ushort e_numberLOL = (ushort)8892;
        //    ushort s_numberCOL = (ushort)8893;
        //    ushort e_numberCOL = (ushort)8893;
        //    ushort s_numberLOP = (ushort)8891;
        //    ushort e_numberLOP = (ushort)8891;
        //    ushort s_numberLOP1 = (ushort)8894;
        //    ushort e_numberLOP1 = (ushort)8894;
        //    ushort length = 9;
        //    short rdret = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberMAP, e_numberMAP, length, rdpmcdataMAP);
        //    short rdret1 = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberPCP, e_numberPCP, length, rdpmcdataPCP);
        //    short rdret2 = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberPSP, e_numberPSP, length, rdpmcdataPSP);
        //    short rdret3 = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberLOL, e_numberLOL, length, rdpmcdataLOL);
        //    short rdret4 = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberCOL, e_numberCOL, length, rdpmcdataCOL);
        //    short rdret5 = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberLOP, e_numberLOP, length, rdpmcdataLOP);
        //    short rdret6 = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberLOP1, e_numberLOP1, length, rdpmcdataLOP1);

        //    if (Convert.ToInt32(rdpmcdataMAP.cdata[0]) == 1)
        //    {
        //        MAPlbl.Text = "OK";
        //        MAPlbl.BackColor = Color.Green;
        //    }
        //    else
        //    {
        //        MAPlbl.Text = "NOK";
        //        MAPlbl.BackColor = Color.Red;
        //    }

        //    if (Convert.ToInt32(rdpmcdataPCP.cdata[0]) == 1)
        //    {
        //        PCPlbl.Text = "OK";
        //        PCPlbl.BackColor = Color.Green;
        //    }
        //    else
        //    {
        //        PCPlbl.Text = "NOK";
        //        PCPlbl.BackColor = Color.Red;
        //    }

        //    if (Convert.ToInt32(rdpmcdataPSP.cdata[0]) == 1)
        //    {
        //        PSPlbl.Text = "OK";
        //        PSPlbl.BackColor = Color.Green;
        //    }
        //    else
        //    {
        //        PSPlbl.Text = "NOK";
        //        PSPlbl.BackColor = Color.Red;
        //    }

        //    if (Convert.ToInt32(rdpmcdataLOL.cdata[0]) == 0)
        //    {
        //        LOLlbl.Text = "OK";
        //        LOLlbl.BackColor = Color.Green;
        //    }
        //    else
        //    {
        //        LOLlbl.Text = "NOK";
        //        LOLlbl.BackColor = Color.Red;
        //    }

        //    if (Convert.ToInt32(rdpmcdataCOL.cdata[0]) == 0)
        //    {
        //        COLlbl.Text = "OK";
        //        COLlbl.BackColor = Color.Green;
        //    }
        //    else
        //    {
        //        COLlbl.Text = "NOK";
        //        COLlbl.BackColor = Color.Red;
        //    }

        //    if (Convert.ToInt32(rdpmcdataLOP.cdata[0]) == 1 && Convert.ToInt32(rdpmcdataLOP1.cdata[0]) == 1)
        //    {
        //        LOPlbl.Text = "OK";
        //        LOPlbl.BackColor = Color.Green;
        //    }
        //    else if (Convert.ToInt32(rdpmcdataLOP1.cdata[0]) == 1 && Convert.ToInt32(rdpmcdataLOP.cdata[0]) == 0)
        //    {
        //        LOPlbl.Text = "NOK";
        //        LOPlbl.BackColor = Color.Red;
        //    }
        //    else if (Convert.ToInt32(rdpmcdataLOP1.cdata[0]) == 0)
        //    {
        //        LOPlbl.Text = "OK";
        //        LOPlbl.BackColor = Color.Yellow;
        //    }
        //}

        // INSERT Machine Status
        private void InsertMachineStatus(ushort h, int machineid, out String MachineStatus, out String MachineEmergency, out String MachineAlm)
        {
            Focas1.ODBST MacStatus = new Focas1.ODBST();
            short StatRet = Focas1.cnc_statinfo(h, MacStatus);

            //Machine Status
            switch (MacStatus.aut)
            {
                case 0:
                    //statetb.Text = "MDI";
                    MachineStatus = "MDI"; //10
                    break;
                case 1:
                    //statetb.Text = "MEM";
                    MachineStatus = "MEM"; //10
                    break;
                case 2:
                    //statetb.Text = "****";
                    MachineStatus = "****"; //10
                    break;
                case 3:
                    //statetb.Text = "EDIT";
                    MachineStatus = "EDIT"; //10
                    break;
                case 4:
                    //statetb.Text = "HND";
                    MachineStatus = "HND"; //10
                    break;
                case 5:
                    //statetb.Text = "JOG";
                    MachineStatus = "JOG"; //10
                    break;
                case 6:
                    //statetb.Text = "Teach JOG";
                    MachineStatus = "Teach JOG"; //10
                    break;
                case 7:
                    //statetb.Text = "Teach HND";
                    MachineStatus = "Teach HND"; //10
                    break;
                case 8:
                    //statetb.Text = "INC Feed";
                    MachineStatus = "INC Feed"; //10
                    break;
                case 9:
                    //statetb.Text = "REF";
                    MachineStatus = "REF"; //10
                    break;
                case 10:
                    //statetb.Text = "RMT";
                    MachineStatus = "RMT"; //10
                    break;
                default:
                    MachineStatus = "****"; //10
                    break;
            }
            //Emergency
            switch (MacStatus.emergency)
            {
                case 0:
                    //emertb.Text = "****";
                    MachineEmergency = "****"; //13
                    break;
                case 1:
                    //emertb.Text = "EMG";
                    //emertb.ForeColor = Color.DarkRed;
                    MachineEmergency = "EMG"; //13
                    break;
                case 2:
                    //emertb.Text = "ReSET";
                    MachineEmergency = "ReSET"; //13
                    break;
                default:
                    MachineEmergency = "****"; //13
                    break;
            }
            //Alarm
            switch (MacStatus.alarm)
            {
                case 0:
                    //alarmtb.Text = "****";
                    MachineAlm = "****"; //12
                    break;
                case 1:
                    //alarmtb.Text = "ALM";
                    //alarmtb.ForeColor = Color.DarkRed;
                    MachineAlm = "ALM"; //12
                    break;
                case 2:
                    //alarmtb.Text = "BATLOW";
                    //alarmtb.ForeColor = Color.DarkRed;
                    MachineAlm = "BATLOW"; //12
                    break;
                case 3:
                    //alarmtb.Text = "FANALM";
                    //alarmtb.ForeColor = Color.DarkRed;
                    MachineAlm = "FANALM"; //12
                    break;
                default:
                    MachineAlm = "****"; //12
                    break;
            }
        }

        //Timer Event for Inserting the Data into DB  Every 5 min
        public async void GetDataIntoEXCEL_DB(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                await InsertData();

            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
        }

        // INSERT DATA into DB
        public async Task InsertData()
        {
            try
            {
                IntoFile("AxisDetails1");
                Task output = Task.Factory.StartNew(AxisDetails1ToDB);
                output.Wait();
                // bool axisdet1 = await AxisDetails1ToDB();


            }
            catch (Exception ex) { IntoFile(ex.ToString()); }
            try
            {
                IntoFile("AxisDetails2");
                Task output = Task.Factory.StartNew(AxisDetails2ToDB);
                output.Wait();
                // bool axisDet2 = await AxisDetails2ToDB();
            }
            catch (Exception ex) { IntoFile(ex.ToString()); }
            try
            {

                IntoFile("ServoDetailsToDB");
                Task output = Task.Factory.StartNew(ServoDetailsToDB);
                output.Wait();
                // bool ServoDet = await ServoDetailsToDB();
            }
            catch (Exception ex) { IntoFile(ex.ToString()); }
            try
            {

                IntoFile("MachineRealTimeStatusToDB");
                Task output = Task.Factory.StartNew(MachineRealTimeStatusToDB);
                output.Wait();
                // bool MachineStatus = await MachineRealTimeStatusToDB();
            }
            catch (Exception ex) { IntoFile(ex.ToString()); }

        }

        //Inserting AxisDetails1
        public async Task<bool> AxisDetails1ToDB()
        {
            bool res = false;
            if (AxisDetlist1.Count > 0)
            {
                //Task.Delay(5000);
                DataTable dt = await ToDataTable(AxisDetlist1);
                //ToDataTable(ServoDetList).Wait();
                // ToEXCEL(dt, "AxisDetails1");
               
                INSERDATAINTOTABLE(dt, "tbl_axisdetails1").Wait();
                AxisDetlist1 = new List<AxisDetails1>();
                res = true;
            }
            return res;
        }

        //Inserting AxisDetails2
        public async Task<bool> AxisDetails2ToDB()
        {
            bool res = false;
            if (AxisDetlist2.Count > 0)
            {

                //Task.Delay(5000);
                DataTable dt1 = await ToDataTable(AxisDetlist2);
               // ToDataTable(ServoDetList).Wait();
               // ToEXCEL(dt1, "AxisDetails2");
                INSERDATAINTOTABLE(dt1, "unitworksccs.`unitworkccs.tbl_axisdetails2`").Wait();
                AxisDetlist2 = new List<AxisDetails2>();
                res = true;
            }
            return res;
        }

        //Inserting ServoDetails
        public async Task<bool> ServoDetailsToDB()
        {
            bool res = false;
            if (ServoDetList.Count > 0)
            {
                //Task.Delay(5000);
                DataTable dt3 = await ToDataTable(ServoDetList);
                //ToDataTable(ServoDetList).Wait();
               // ToEXCEL(dt3, "ServoDetails");
                INSERDATAINTOTABLE(dt3, "unitworksccs.`unitworkccs.tbl_servodetails`").Wait();
                ServoDetList = new List<ServoDeatailsModel>();
                res = true;
            }
            return res;
        }

        //Inserting MachineRealtimeStatus
        public async Task<bool> MachineRealTimeStatusToDB()
        {
            bool res = false;
            if (MachineStatList.Count > 0)
            {
                //Task.Delay(5000);
                DataTable dt2 = await ToDataTable(MachineStatList);
                INSERDATAINTOTABLE(dt2, "unitworksccs.`unitworkccs.tbl_machinestatusrealtime`").Wait();
                // ToEXCEL(dt2, "MachineRealTimeStatus");
                MachineStatList = new List<MachineStatusModel>();
                res = true;
            }
            return res;
        }

        // Push the EXCEL DATA inot Data Table
        public DataTable ExcelToDataTable(string path)
        {
            var pck = new OfficeOpenXml.ExcelPackage();
            pck.Load(File.OpenRead(path));
            var ws = pck.Workbook.Worksheets.First();
            DataTable tbl = new DataTable();
            bool hasHeader = true;
            foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
            {
                tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
            }
            var startRow = hasHeader ? 2 : 1;
            for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
            {
                var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                var row = tbl.NewRow();
                foreach (var cell in wsRow)
                {
                    row[cell.Start.Column - 1] = cell.Text;
                }
                tbl.Rows.Add(row);
            }
            pck.Dispose();
            return tbl;
        }

        // INSERT Bulk Data into the DB
        private async Task<bool> INSERDATAINTOTABLE(DataTable dt, string TableName)
        {
            bool res = false;
            try
            {
                using (MsqlConnection con = new MsqlConnection())
                {
                    con.open();
                    var bulk = new BulkOperation(con.msqlConnection);
                    bulk.DestinationTableName = "unitworkccs."+TableName;
                    bulk.BulkInsert(dt);
                    con.close();
                    res = true;
                }
                IntoFile("unitworkccs."+TableName + " Completed");
                //using (MsqlConnection con = new MsqlConnection())
                //{
                //    con.open();
                //    using (MySqlTransaction tran = con.msqlConnection.BeginTransaction(System.Data.IsolationLevel.Serializable))
                //    {
                //        using (SqlCommand cmd = new SqlCommand())
                //        {
                //            cmd.Connection = con.msqlConnection;
                //            cmd.Transaction = tran;
                //            cmd.CommandText = "SELECT * FROM " + TableName + "";
                //            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                //            {
                //                da.UpdateBatchSize = 1000;
                //                using (MySqlCommandBuilder cb = new MySqlCommandBuilder(da))
                //                {
                //                    da.Update(dt);
                //                    tran.Commit();
                //                    IntoFile(TableName + " Completed");
                //                }
                //            }
                //        }
                //    }
                //    con.close();
                //}
            }
            catch (Exception ex) { IntoFile(TableName + " " + ex.ToString()); }

            return res;
        }

        // Convert List TO DataTable
        public async Task<DataTable> ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        //push the data into EXCEL
        private void ToEXCEL(DataTable dt, string FileName)
        {

            try
            {
                FileInfo templateFile = new FileInfo(@"C:\AxisDetails\Template\" + FileName + ".xlsx");
                ExcelPackage templatep = new ExcelPackage(templateFile);
                ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];

                String FileDir = @"C:\AxisDetails";
                //String FileDir = @"C:\inetpub\ContiAndonWebApp\Reports\" + System.DateTime.Now.ToString("yyyy");

                bool exists = System.IO.Directory.Exists(FileDir);

                if (!exists)
                    System.IO.Directory.CreateDirectory(FileDir);

                FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, FileName + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
                if (newFile.Exists)
                {
                    try
                    {
                        newFile.Delete();  // ensures we create a new workbook
                        newFile = new FileInfo(System.IO.Path.Combine(FileDir, FileName + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
                    }
                    catch
                    {
                        //MessageBox.Show("Excel with same date is already open, please close it and try to generate!!!!");
                        //return View();
                    }
                }
                //Using the File for generation and populating it
                ExcelPackage p = null;
                p = new ExcelPackage(newFile);
                ExcelWorksheet worksheet = null;

                //Creating the WorkSheet for populating
                try
                {
                    worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("yyyyMMddHHmmss"), Templatews);
                }
                catch { }

                if (worksheet == null)
                {
                    worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("yyyyMMddHHmmss"), Templatews);
                }

                worksheet.Cells["A1"].LoadFromDataTable(dt, true);


                p.Save();
                //Downloding Excel
                string path1 = System.IO.Path.Combine(FileDir, FileName + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx");
                System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
                string Outgoingfile = FileName + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            }
            catch (Exception ex)
            {
                //  MessageBox.Show(ex.ToString());
            }
        }
        public void IntoFile(string Msg)
        {
            try
            {
                string appPath = "C:/Axislog.txt";
                using (StreamWriter writer = new StreamWriter(appPath, true)) //true => Append Text
                {
                    writer.WriteLine(System.DateTime.Now + ":  " + Msg + "\r \n");
                }
            }
            catch (Exception e7)
            {
                IntoFile("IntoFile" + e7.ToString());
            }

        }
    }
}
