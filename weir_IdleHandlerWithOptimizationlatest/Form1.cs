
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace i_facility_IdleHandlerWithOptimization
{
    public partial class Form1 : Form
    {
        unitworksccsEntities db = new unitworksccsEntities();

        public Form1()
        {
            InitializeComponent();
            DayTicker();
            string CorrectedDate = "";
            try
            {
                GetPartsandCutting();
                CorrectedDate = GetCorrectedDate();
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }

            Timer MyTimer = new Timer();
            //MyTimer.Interval = (20 * 1000); // 20 seconds
            MyTimer.Interval = (10 * 1000 * 60); // 60 seconds
            MyTimer.Tick += new EventHandler(MyTimer_Tick);
            MyTimer.Start();



            //Timer MyTimer1 = new Timer();
            //MyTimer1.Interval = (60 * 1000 * 1); //1 min          
            //MyTimer1.Enabled = true;
            //MyTimer1.Tick += new EventHandler(MyTimer_Tick1);
            //MyTimer1.Start();
        }

        private string GetCorrectedDate()
        {
            string CorrectedDate = "";
            unitworkccs_tbldaytiming StartTime1 = db.unitworkccs_tbldaytiming.Where(m => m.IsDeleted == 0).FirstOrDefault();
            TimeSpan Start = StartTime1.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            return CorrectedDate;
        }


        private void MyTimer_Tick(object sender, EventArgs e)
        {
            string LCorrectedDate, todaysCorrectedDate;

            try
            {
                #region Shift & CorrectedDate
                todaysCorrectedDate = GetCorrectedDate();
                LCorrectedDate = todaysCorrectedDate;//dummy initializaition;

                string Shift = null;
                string CorrectedDate = GetCorrectedDate();
                using (MsqlConnection mcp = new MsqlConnection())
                {
                    mcp.open();
                    String queryshift = "SELECT ShiftName,StartTime,EndTime FROM ["+ MsqlConnection.DB+ "].[" + MsqlConnection.Schema + "].unitworksccs.`unitworkccs.tblshift_mstr` WHERE IsDeleted = 0";
                    MySqlDataAdapter dashift = new MySqlDataAdapter(queryshift, mcp.sqlConnection);
                    DataTable dtshift = new DataTable();
                    dashift.Fill(dtshift);
                    String[] msgtime = System.DateTime.Now.TimeOfDay.ToString().Split(':');
                    TimeSpan msgstime = System.DateTime.Now.TimeOfDay;
                    //TimeSpan msgstime = new TimeSpan(Convert.ToInt32(msgtime[0]), Convert.ToInt32(msgtime[1]), Convert.ToInt32(msgtime[2]));
                    TimeSpan s1t1 = new TimeSpan(0, 0, 0), s1t2 = new TimeSpan(0, 0, 0), s2t1 = new TimeSpan(0, 0, 0), s2t2 = new TimeSpan(0, 0, 0);
                    TimeSpan s3t1 = new TimeSpan(0, 0, 0), s3t2 = new TimeSpan(0, 0, 0), s3t3 = new TimeSpan(0, 0, 0), s3t4 = new TimeSpan(23, 59, 59);
                    for (int k = 0; k < dtshift.Rows.Count; k++)
                    {
                        if (dtshift.Rows[k][0].ToString().Contains("A"))
                        {
                            String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
                            s1t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                            String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
                            s1t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                        }
                        else if (dtshift.Rows[k][0].ToString().Contains("B"))
                        {
                            String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
                            s2t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                            String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
                            s2t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                        }
                        else if (dtshift.Rows[k][0].ToString().Contains("C"))
                        {
                            String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
                            s3t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                            String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
                            s3t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                        }
                    }

                    if (msgstime >= s1t1 && msgstime < s1t2)
                    {
                        Shift = "A";
                    }
                    else if (msgstime >= s2t1 && msgstime < s2t2)
                    {
                        Shift = "B";
                    }
                    else if ((msgstime >= s3t1 && msgstime <= s3t4) || (msgstime >= s3t3 && msgstime < s3t2))
                    {
                        Shift = "C";
                        if (msgstime >= s3t3 && msgstime < s3t2)
                        {
                            CorrectedDate = System.DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                        }
                    }
                    mcp.close();
                }

                #endregion

                //if (System.DateTime.Now.Hour == 6 || DateTime.Now.Hour==14 || DateTime.Now.Hour==22)
                //{
                    DayTicker();

                //}

                //IsNormalWC = 0 2017-02-10
                //var machineData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0).ToList();
                var machineData = db.unitworkccs_tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();
                foreach (var macrow in machineData)
                {
                    int machineid = macrow.MachineID;

                    //Insert 1 no_code row when the machine is enabled for the 1st time.
                    #region
                    //using (unitworksccsEntities dbquick = new unitworksccsEntities())
                    //{
                    //    var AtLeast1LossRow = dbquick.tbllossofentries.Where(m => m.MachineID == machineid).FirstOrDefault();
                    //    if (AtLeast1LossRow == null)
                    //    {
                    //        tbllossofentry lossentry = new tbllossofentry();
                    //        lossentry.Shift = Shift.ToString();
                    //        lossentry.EntryTime = DateTime.Now.AddMinutes(-1);
                    //        lossentry.StartTime = DateTime.Now.AddMinutes(-1);
                    //        lossentry.EndTime = DateTime.Now.AddSeconds(-59);
                    //        lossentry.CorrectedDate = CorrectedDate;
                    //        lossentry.IsUpdate = 1;
                    //        lossentry.DoneWithRow = 1;
                    //        lossentry.IsStart = 0;
                    //        lossentry.IsScreen = 0;
                    //        lossentry.ForRefresh = 0;
                    //        lossentry.MessageCodeID = 999;
                    //        int abc = Convert.ToInt32(lossentry.MessageCodeID);
                    //        var a = dbquick.tbllossescodes.Find(abc);
                    //        lossentry.MessageDesc = a.LossCodeDesc.ToString();
                    //        lossentry.MessageCode = a.LossCode.ToString();
                    //        lossentry.MachineID = machineid;

                    //        //Session["showIdlePopUp"] = 0;
                    //        dbquick.tbllossofentries.Add(lossentry);
                    //        dbquick.SaveChanges();
                    //    }
                    //}
                    #endregion
                }

                //Here Call the Stored Procedure
                try
                {

                    #region commented by Ashok
                    //string conString = "server = 'localhost' ;userid = 'root' ;Password = 'srks4$' ;database = 'mazakdaq';port = 3306 ;persist security info=False";
                    //using (MySqlConnection databaseConnection = new MySqlConnection(conString))
                    //{
                    //    MySqlCommand cmd = new MySqlCommand("IdleHandler", databaseConnection);
                    //    cmd.Parameters.AddWithValue("Shift", Shift);
                    //    cmd.Parameters.AddWithValue("CorrectedDate", CorrectedDate);
                    //    databaseConnection.Open();
                    //    cmd.CommandType = CommandType.StoredProcedure;
                    //    var a = cmd.ExecuteNonQuery();
                    //    databaseConnection.Close();
                    //}
                    #endregion
                    //idehandler(CorrectedDate, Shift);
                }
                catch (Exception ea)
                {
                    IntoFile(ea.ToString());
                }
                CorrectedDate = GetCorrectedDate();
                //ModeWithLoss(CorrectedDate);
                //ModeVsLossOvelap(CorrectedDate);
                //DeleteExtraLossRows(CorrectedDate);  // Loss Overlap Correction in  livelossofEntry 
            }
            catch (Exception exception)
            {
                IntoFile(exception.ToString());
            }
        }


        private void DayTicker()
        {
            IntoFile("DayTicker:" + DateTime.Now);
            string LCorrectedDate, todaysCorrectedDate;

            try
            {
                #region Shift & CorrectedDate
                todaysCorrectedDate = GetCorrectedDate();
                IntoFile("for todaysCorrectedDate:" + todaysCorrectedDate);
                LCorrectedDate = todaysCorrectedDate;//dummy initializaition;
                IntoFile("for LCorrectedDate:" + LCorrectedDate);
                //correcteddate
                //string correcteddate = null;
                //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
                //TimeSpan Start = StartTime.StartTime;
                //if (Start < DateTime.Now.TimeOfDay)
                //{
                //    correcteddate = DateTime.Now.ToString("yyyy-MM-dd");
                //}
                //else
                //{
                //    correcteddate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                //}

                string Shift = null;
                int shiftID = 1;
                String CorrectedDate = GetCorrectedDate();
                IntoFile("CorrectedDate:" + CorrectedDate);
                using (MsqlConnection mcp = new MsqlConnection())
                {
                    mcp.open();
                    //String queryshift = "SELECT ShiftName,StartTime,EndTime FROM i_facility_tsal.dbo.tblshift_mstr WHERE IsDeleted = 0";
                    String queryshift = "SELECT ShiftName,StartTime,EndTime FROM " + MsqlConnection.DB + "." + MsqlConnection.Schema + ".unitworksccs.`unitworkccs.tblshift_mstr` WHERE IsDeleted = 0";

                    MySqlDataAdapter dashift = new MySqlDataAdapter(queryshift, mcp.sqlConnection);
                    DataTable dtshift = new DataTable();
                    dashift.Fill(dtshift);
                    String[] msgtime = System.DateTime.Now.TimeOfDay.ToString().Split(':');
                    TimeSpan msgstime = System.DateTime.Now.TimeOfDay;
                    //TimeSpan msgstime = new TimeSpan(Convert.ToInt32(msgtime[0]), Convert.ToInt32(msgtime[1]), Convert.ToInt32(msgtime[2]));
                    TimeSpan s1t1 = new TimeSpan(0, 0, 0), s1t2 = new TimeSpan(0, 0, 0), s2t1 = new TimeSpan(0, 0, 0), s2t2 = new TimeSpan(0, 0, 0);
                    TimeSpan s3t1 = new TimeSpan(0, 0, 0), s3t2 = new TimeSpan(0, 0, 0), s3t3 = new TimeSpan(0, 0, 0), s3t4 = new TimeSpan(23, 59, 59);
                    for (int k = 0; k < dtshift.Rows.Count; k++)
                    {
                        if (dtshift.Rows[k][0].ToString().Contains("A") || dtshift.Rows[k][0].ToString().Contains("1"))
                        {
                            String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
                            s1t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                            String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
                            s1t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                        }
                        else if (dtshift.Rows[k][0].ToString().Contains("B") || dtshift.Rows[k][0].ToString().Contains("2"))
                        {
                            String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
                            s2t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                            String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
                            s2t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                        }
                        else if (dtshift.Rows[k][0].ToString().Contains("C") || dtshift.Rows[k][0].ToString().Contains("3"))
                        {
                            String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
                            s3t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                            String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
                            s3t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                        }
                    }

                    if (msgstime >= s1t1 && msgstime < s1t2)
                    {
                        Shift = "A";
                        shiftID = 1;
                    }
                    else if (msgstime >= s2t1 && msgstime < s2t2)
                    {
                        Shift = "B";
                        shiftID = 2;
                    }
                    else if ((msgstime >= s3t1 && msgstime <= s3t4) || (msgstime >= s3t3 && msgstime < s3t2))
                    {
                        Shift = "C";
                        shiftID = 3;
                        if (msgstime >= s3t3 && msgstime < s3t2)
                        {
                            CorrectedDate = System.DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                        }
                    }
                    mcp.close();
                }

                #endregion

                var machineData = db.unitworkccs_tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();
                foreach (var macrow in machineData)
                {
                    int machineid = macrow.MachineID;
                    IntoFile("machineid:" + machineid);
                    //Check for DayChange and replicate BreakdownLoss
                    #region
                    //using (unitworksccsEntities dbquick = new unitworksccsEntities())
                    //{
                    //    var LatestBDData = dbquick.tblbreakdowns.Where(m => m.MachineID == machineid).OrderByDescending(m => m.BreakdownID).FirstOrDefault();
                    //    if (LatestBDData != null)
                    //    {
                    //        string tableCorrectedDate = LatestBDData.CorrectedDate;
                    //        if (LatestBDData.DoneWithRow != 1)
                    //        {
                    //            if (CorrectedDate != tableCorrectedDate)
                    //            {
                    //                DateTime TodayStartDate = Convert.ToDateTime(CorrectedDate + " " + new TimeSpan(6, 0, 0));

                    //                // Update Endtime for Previous Breakdown loss
                    //                LatestBDData.DoneWithRow = 1;
                    //                LatestBDData.EndTime = TodayStartDate.AddSeconds(-1);
                    //                dbquick.Entry(LatestBDData).State = System.Data.Entity.EntityState.Modified;
                    //                dbquick.SaveChanges();

                    //                // Insert New Breakdown loss at Day start Time
                    //                tblbreakdown tblloe = new tblbreakdown();
                    //                tblloe.CorrectedDate = CorrectedDate;
                    //                tblloe.DoneWithRow = 0;
                    //                tblloe.MachineID = LatestBDData.MachineID;
                    //                tblloe.MessageCode = LatestBDData.MessageCode;
                    //                tblloe.BreakDownCode = LatestBDData.BreakDownCode;
                    //                tblloe.MessageDesc = LatestBDData.MessageDesc;
                    //                //tblloe.Shift = "A"; commented by Ashok
                    //                tblloe.Shift = Shift;
                    //                tblloe.StartTime = TodayStartDate;
                    //                dbquick.tblbreakdowns.Add(tblloe);
                    //                dbquick.SaveChanges();


                    //            }
                    //        }
                    //    }
                    //}
                    #endregion

                    //Check for DayChange and replicate Lossrow
                    #region
                    //using (unitworksccsEntities dbquick = new unitworksccsEntities())
                    //{
                    //    var LatestLossData = dbquick.tbllivelossofentries.Where(m => m.MachineID == machineid).OrderByDescending(m => m.LossID).FirstOrDefault();
                    //    if (LatestLossData != null)
                    //    {
                    //        string tableCorrectedDate = LatestLossData.CorrectedDate;
                    //        if (LatestLossData.DoneWithRow != 1)
                    //        {
                    //            if (CorrectedDate != tableCorrectedDate)
                    //            {
                    //                DateTime TodayStartDate = Convert.ToDateTime(CorrectedDate + " " + new TimeSpan(6, 0, 0));

                    //                // Update the Endtime of  Previous Day Loss
                    //                LatestLossData.IsStart = 0;
                    //                LatestLossData.IsScreen = 0;
                    //                LatestLossData.ForRefresh = 0;
                    //                LatestLossData.DoneWithRow = 1;
                    //                LatestLossData.IsUpdate = 1;
                    //                LatestLossData.EndDateTime = TodayStartDate.AddSeconds(-1);
                    //                dbquick.Entry(LatestLossData).State = System.Data.Entity.EntityState.Modified;
                    //                dbquick.SaveChanges();


                    //                tbllivelossofentry tblloe = new tbllivelossofentry();
                    //                tblloe.CorrectedDate = CorrectedDate;
                    //                tblloe.DoneWithRow = 0;
                    //                //tblloe.EndDateTime = TodayStartDate;
                    //                tblloe.EntryTime = TodayStartDate;
                    //                tblloe.ForRefresh = LatestLossData.ForRefresh;
                    //                tblloe.IsScreen = LatestLossData.IsScreen;
                    //                tblloe.IsStart = LatestLossData.IsStart;
                    //                tblloe.IsUpdate = LatestLossData.IsUpdate;
                    //                tblloe.MachineID = LatestLossData.MachineID;
                    //                tblloe.MessageCode = LatestLossData.MessageCode;
                    //                tblloe.MessageCodeID = LatestLossData.MessageCodeID;
                    //                tblloe.MessageDesc = LatestLossData.MessageDesc;
                    //                tblloe.Shift = Shift;
                    //                tblloe.StartDateTime = TodayStartDate;
                    //                // Insert New loss at Day start Time
                    //                dbquick.tbllivelossofentries.Add(tblloe);
                    //                dbquick.SaveChanges();
                    //            }
                    //        }
                    //    }
                    //}
                    #endregion

                    //Check for DayChange and replicate Mode 
                    #region
                    using (unitworksccsEntities dbquick = new unitworksccsEntities())
                    {
                        var LatestModeData = dbquick.unitworkccs_tbllivemode.Where(m => m.MachineID == machineid && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).FirstOrDefault();
                        if (LatestModeData != null)
                        {
                            string tableCorrectedDate = LatestModeData.CorrectedDate.ToString("yyyy-MM-dd");

                            #region shift Base
                            int prevshiftid = LatestModeData.IsShiftEnd;
                            //string PrevShift = LatestModeData.Shift;

                            if (prevshiftid != shiftID )
                            {
                                var shiftmasterdet = new unitworkccs_shift_master();
                                DateTime nowdate = DateTime.Now;

                                using (unitworksccsEntities db = new unitworksccsEntities())
                                {
                                    shiftmasterdet = db.unitworkccs_shift_master.Find(shiftID);
                                }
                                if (shiftmasterdet != null)
                                {
                                    string shiftST = GetCorrectedDate() + " " + shiftmasterdet.StartTime;
                                    DateTime ShiftETDT = Convert.ToDateTime(shiftST);
                                    if (ShiftETDT.Hour <= nowdate.Hour)
                                    {
                                        //if (shiftid == 3)
                                        //{
                                        //    ShiftETDT = ShiftETDT.AddDays(1);
                                        //}
                                        string Et = ShiftETDT.AddSeconds(-1).ToString("HH:mm:ss");
                                        string start = nowdate.ToString("yyyy-MM-dd") + " " + shiftmasterdet.StartTime.ToString();
                                        DateTime st = Convert.ToDateTime(start);
                                        //DateTime NowDateCalc = Convert.ToDateTime(nowdate.ToString("yyyy-MM-dd 0" + (Start - 1) + ":59:59"));
                                        DateTime NowDateCalc = Convert.ToDateTime(nowdate.ToString("yyyy-MM-dd " + Et));
                                        int durationinsec = Convert.ToInt32(NowDateCalc.Subtract(Convert.ToDateTime(st)).TotalSeconds);
                                        String StartTimeNext = nowdate.ToString("yyyy-MM-dd " + st.ToString("HH:mm:ss"));

                                        //update the end time of Previous Day mode 
                                        LatestModeData.IsCompleted = 1;
                                        DateTime dt = ShiftETDT.AddSeconds(-1);
                                        LatestModeData.EndTime = dt;
                                        LatestModeData.ModeTypeEnd = 1;
                                        int duration = (int)Convert.ToDateTime(LatestModeData.EndTime).Subtract(Convert.ToDateTime(LatestModeData.StartTime)).TotalSeconds;
                                        LatestModeData.DurationInSec = duration;
                                        dbquick.Entry(LatestModeData).State = System.Data.Entity.EntityState.Modified;
                                        dbquick.SaveChanges();


                                        var LatestModeData1 = dbquick.unitworkccs_tbllivemode.Where(m => m.MachineID == machineid && m.IsCompleted == 1).OrderByDescending(m => m.ModeID).FirstOrDefault();

                                        //string ET = ShiftETDT.AddSeconds(-1).ToString;
                                        DateTime et = ShiftETDT.AddSeconds(-1);
                                        if (LatestModeData1.EndTime == et)
                                        {
                                            unitworkccs_tbllivemode tblm = new unitworkccs_tbllivemode();
                                            tblm.CorrectedDate = Convert.ToDateTime(CorrectedDate).Date;
                                            tblm.ColorCode = LatestModeData.ColorCode;
                                            //tblm.EndTime = TodayStartDate;
                                            tblm.InsertedBy = 1;
                                            tblm.InsertedOn = DateTime.Now;
                                            tblm.IsCompleted = 0;
                                            tblm.IsDeleted = 0;
                                            tblm.MachineID = LatestModeData.MachineID;
                                            tblm.MacMode = LatestModeData.MacMode;
                                            tblm.ModeType = LatestModeData.MacMode;
                                            //string ST = DateTime.Now.Date.ToString("yyyy-MM-dd") + " 06:00:00";
                                            tblm.StartTime = Convert.ToDateTime(start);
                                            // Insert the New mode at day start time
                                            tblm.IsShiftEnd = shiftID;

                                            using (unitworksccsEntities db = new unitworksccsEntities())
                                            {
                                                db.unitworkccs_tbllivemode.Add(tblm);
                                                db.SaveChanges();
                                            }
                                        }
                                        else
                                        {
                                            LatestModeData.IsCompleted = 1;
                                            //string ET1 = DateTime.Now.Date.ToString("yyyy-MM-dd") + " 05:59:59";
                                            DateTime et1 = ShiftETDT.AddSeconds(-1);
                                            LatestModeData.EndTime = et1;
                                            int duration1 = (int)Convert.ToDateTime(LatestModeData.EndTime).Subtract(Convert.ToDateTime(LatestModeData.StartTime)).TotalSeconds;
                                            LatestModeData.DurationInSec = duration1;
                                            LatestModeData.ModeTypeEnd = 1;
                                            dbquick.Entry(LatestModeData).State = System.Data.Entity.EntityState.Modified;
                                            dbquick.SaveChanges();

                                            unitworkccs_tbllivemode tblm = new unitworkccs_tbllivemode();
                                            tblm.CorrectedDate = Convert.ToDateTime(CorrectedDate).Date;
                                            IntoFile("CorrectedDate while inserting:" + tblm.CorrectedDate);
                                            tblm.ColorCode = LatestModeData.ColorCode;
                                            //tblm.EndTime = TodayStartDate;
                                            tblm.InsertedBy = 1;
                                            tblm.InsertedOn = DateTime.Now;
                                            IntoFile("InsertedOn while inserting:" + tblm.InsertedOn);
                                            tblm.IsCompleted = 0;
                                            tblm.IsDeleted = 0;
                                            tblm.MachineID = LatestModeData.MachineID;
                                            tblm.MacMode = LatestModeData.MacMode;
                                            tblm.ModeType = LatestModeData.MacMode;
                                            //string ST = DateTime.Now.Date.ToString("yyyy-MM-dd") + " 06:00:00";
                                            tblm.StartTime = Convert.ToDateTime(start);
                                            IntoFile("StartTime while inserting:" + start);
                                            tblm.IsShiftEnd = shiftID;
                                            // Insert the New mode at day start time
                                            dbquick.unitworkccs_tbllivemode.Add(tblm);
                                            dbquick.SaveChanges();
                                        }

                                    }
                                }
                            }
                            #endregion
                            LatestModeData = dbquick.unitworkccs_tbllivemode.Where(m => m.MachineID == machineid && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).FirstOrDefault();
                            tableCorrectedDate = LatestModeData.CorrectedDate.ToString("yyyy-MM-dd");
                            #region Day wise 
                            if (CorrectedDate != tableCorrectedDate)
                            {
                                //DateTime TodayStartDate = Convert.ToDateTime(CorrectedDate + " " + new TimeSpan(6, 0, 0));
                                var daytiming = dbquick.unitworkccs_tbldaytiming.Where(m => m.IsDeleted == 0).FirstOrDefault();
                                TimeSpan StartTime = daytiming.StartTime;
                                string Date = CorrectedDate + " " + StartTime;
                                DateTime TodayStartDate = Convert.ToDateTime(Date);

                                //update the end time of Previous Day mode 
                                LatestModeData.IsCompleted = 1;
                                DateTime dt = TodayStartDate.AddSeconds(-1);
                                LatestModeData.EndTime = dt;
                                int duration = (int)Convert.ToDateTime(LatestModeData.EndTime).Subtract(Convert.ToDateTime(LatestModeData.StartTime)).TotalSeconds;
                                LatestModeData.DurationInSec = duration;
                                LatestModeData.ModeTypeEnd = 1;
                                dbquick.Entry(LatestModeData).State = System.Data.Entity.EntityState.Modified;
                                dbquick.SaveChanges();
                                var LatestModeData1 = dbquick.unitworkccs_tbllivemode.Where(m => m.MachineID == machineid && m.IsCompleted == 1).OrderByDescending(m => m.ModeID).FirstOrDefault();

                                string ET = DateTime.Now.Date.ToString("yyyy-MM-dd") + " 05:59:59";
                                DateTime et = Convert.ToDateTime(ET);
                                if (LatestModeData1.EndTime == et)
                                {
                                    unitworkccs_tbllivemode tblm = new unitworkccs_tbllivemode();
                                    tblm.CorrectedDate = Convert.ToDateTime(CorrectedDate).Date;
                                    tblm.ColorCode = LatestModeData.ColorCode;
                                    //tblm.EndTime = TodayStartDate;
                                    tblm.InsertedBy = 1;
                                    tblm.InsertedOn = DateTime.Now;
                                    tblm.IsCompleted = 0;
                                    tblm.IsDeleted = 0;
                                    tblm.MachineID = LatestModeData.MachineID;
                                    tblm.MacMode = LatestModeData.MacMode;
                                    tblm.ModeType = LatestModeData.MacMode;

                                    string ST = DateTime.Now.Date.ToString("yyyy-MM-dd") + " 06:00:00";
                                    tblm.StartTime = Convert.ToDateTime(ST);
                                    tblm.IsShiftEnd = shiftID;
                                    // Insert the New mode at day start time
                                    dbquick.unitworkccs_tbllivemode.Add(tblm);
                                    dbquick.SaveChanges();
                                }
                                else
                                {
                                    LatestModeData.IsCompleted = 1;
                                    string ET1 = DateTime.Now.Date.ToString("yyyy-MM-dd") + " 05:59:59";
                                    DateTime et1 = Convert.ToDateTime(ET);
                                    LatestModeData.EndTime = et1;
                                    int duration1 = (int)Convert.ToDateTime(LatestModeData.EndTime).Subtract(Convert.ToDateTime(LatestModeData.StartTime)).TotalSeconds;
                                    LatestModeData.DurationInSec = duration1;
                                    LatestModeData.ModeTypeEnd = 1;
                                    dbquick.Entry(LatestModeData).State = System.Data.Entity.EntityState.Modified;
                                    dbquick.SaveChanges();

                                    unitworkccs_tbllivemode tblm = new unitworkccs_tbllivemode();
                                    tblm.CorrectedDate = Convert.ToDateTime(CorrectedDate).Date;
                                    IntoFile("CorrectedDate while inserting:" + tblm.CorrectedDate);
                                    tblm.ColorCode = LatestModeData.ColorCode;
                                    //tblm.EndTime = TodayStartDate;
                                    tblm.InsertedBy = 1;
                                    tblm.InsertedOn = DateTime.Now;
                                    IntoFile("InsertedOn while inserting:" + tblm.InsertedOn);
                                    tblm.IsCompleted = 0;
                                    tblm.IsDeleted = 0;
                                    tblm.MachineID = LatestModeData.MachineID;
                                    tblm.MacMode = LatestModeData.MacMode;
                                    tblm.ModeType = LatestModeData.MacMode;
                                    string ST = DateTime.Now.Date.ToString("yyyy-MM-dd") + " 06:00:00";
                                    tblm.StartTime = Convert.ToDateTime(ST);
                                    IntoFile("StartTime while inserting:" + TodayStartDate);
                                    tblm.IsShiftEnd = shiftID;
                                    // Insert the New mode at day start time
                                    dbquick.unitworkccs_tbllivemode.Add(tblm);
                                    dbquick.SaveChanges();
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
            }
            catch (Exception exception)
            {
                IntoFile(exception.ToString());
            }
        }

        public void IntoFile(string Msg)
        {
            try
            {
                string path1 = AppDomain.CurrentDomain.BaseDirectory;
                string appPath = Application.StartupPath + @"\LogFileOfIdleHandler.txt";
                using (StreamWriter writer = new StreamWriter(appPath, true)) //true => Append Text
                {
                    writer.WriteLine(System.DateTime.Now + ":  " + Msg);
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show("IntoFile Error " + e.ToString());
                IntoFile(e.ToString());
            }

        }


        //private void idehandler(string correctedDate, string shift)
        //{
        //    IntoFile("idehandler");
        //    var machineslist = new List<tblmachinedetail>();
        //    var livelossofentryrow = new tbllossofentry();
        //    DateTime Corr = Convert.ToDateTime(correctedDate);
        //    var livemoderow = new tbllivemode();
        //    using (unitworksccsEntities db = new unitworksccsEntities())
        //    {
        //        machineslist = db.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();
        //    }
        //    foreach (var machines in machineslist)
        //    {
        //        bool isIDLE = false;
        //        double Totalsec = 0, TotalModesec = 0, durationofCurrent;
        //        int machineid = machines.MachineID;
        //        using (unitworksccsEntities db = new unitworksccsEntities())
        //        {
        //            livelossofentryrow = db.tbllossofentries.Where(m => m.MachineID == machineid && m.CorrectedDate == correctedDate).OrderByDescending(m => m.LossID).FirstOrDefault();
        //            livemoderow = db.tbllivemode.Where(m => m.MachineID == machineid && m.CorrectedDate == Corr).OrderByDescending(m => m.ModeID).FirstOrDefault();
        //        }
        //        if (livemoderow != null && livelossofentryrow != null)
        //        {
        //            if (livelossofentryrow.IsUpdate == 1 && livelossofentryrow.DoneWithRow == 1)
        //            {
        //                IntoFile("idehandler:" + livelossofentryrow.IsUpdate);
        //                IntoFile("idehandler:" + livelossofentryrow.DoneWithRow);
        //                if (livemoderow.MacMode == "IDLE")
        //                {
        //                    IntoFile("idehandler:" + livemoderow.ModeID);
        //                    //isIDLE = true;
        //                    Totalsec = DateTime.Now.Subtract(Convert.ToDateTime(livemoderow.StartTime)).TotalSeconds;
        //                    TotalModesec = Convert.ToDateTime(livemoderow.InsertedOn).Subtract(Convert.ToDateTime(livemoderow.StartTime)).TotalSeconds;
        //                    durationofCurrent = Totalsec - TotalModesec;
        //                    if (durationofCurrent > 120)
        //                    {
        //                        IntoFile("idehandler:" + durationofCurrent);
        //                        tbllossofentry tblloe = new tbllossofentry();
        //                        tblloe.MessageCodeID = 999;
        //                        tblloe.StartTime = livemoderow.StartTime;
        //                        tblloe.EndTime = livemoderow.StartTime;
        //                        tblloe.EntryTime = livemoderow.StartTime;
        //                        tblloe.CorrectedDate = correctedDate;
        //                        tblloe.DoneWithRow = 0;
        //                        tblloe.IsScreen = 0;
        //                        tblloe.IsStart = 1;
        //                        tblloe.IsUpdate = 0;
        //                        tblloe.ForRefresh = 0;
        //                        tblloe.MachineID = machineid;
        //                        tblloe.MessageDesc = "No Code Entered";
        //                        tblloe.Shift = shift;
        //                        using (unitworksccsEntities db = new unitworksccsEntities())
        //                        {
        //                            db.tbllossofentries.Add(tblloe);
        //                            db.SaveChanges();
        //                        }
        //                    }
        //                }
        //            }   // Inserting Loss row with  NO code Reason
        //            else if (livelossofentryrow.IsUpdate == 0 && livelossofentryrow.DoneWithRow == 0)
        //            {
        //                IntoFile("idehandler:" + livelossofentryrow.IsUpdate);
        //                IntoFile("idehandler:" + livelossofentryrow.DoneWithRow);
        //                if (livemoderow.MacMode == "IDLE")
        //                {
        //                    isIDLE = true;
        //                }
        //                else
        //                {
        //                    IntoFile("idehandler:" + livemoderow.ModeID);
        //                    var livelossofentry = new tbllossofentry();
        //                    using (unitworksccsEntities db = new unitworksccsEntities())
        //                    {
        //                        livelossofentry = db.tbllossofentries.Find(livelossofentryrow.LossID);
        //                    }
        //                    UpdateLossofEntries(livelossofentry.LossID, Convert.ToDateTime(livemoderow.StartTime));
        //                }
        //            }    // To update the Loss Reasonnew code at 1st Time
        //            else if (livelossofentryrow.IsUpdate == 1 && livelossofentryrow.DoneWithRow == 0)
        //            {
        //                IntoFile("idehandler:" + livelossofentryrow.IsUpdate);
        //                IntoFile("idehandler:" + livelossofentryrow.DoneWithRow);
        //                Totalsec = DateTime.Now.Subtract(Convert.ToDateTime(livelossofentryrow.EntryTime)).TotalSeconds;
        //                if (livemoderow.MacMode == "IDLE")
        //                {
        //                    isIDLE = true;
        //                }
        //                else if (!isIDLE)
        //                {
        //                    IntoFile("idehandler:" + livemoderow.ModeID);
        //                    var livelossofentry = new tbllossofentry();
        //                    using (unitworksccsEntities db = new unitworksccsEntities())
        //                    {
        //                        livelossofentry = db.tbllossofentries.Find(livelossofentryrow.LossID);
        //                    }
        //                    UpdateLossofEntries(livelossofentry.LossID, Convert.ToDateTime(livemoderow.StartTime));
        //                }
        //                else if (Totalsec > 120)
        //                {
        //                    var livelossofentry = new tbllossofentry();
        //                    using (unitworksccsEntities db = new unitworksccsEntities())
        //                    {
        //                        livelossofentry = db.tbllossofentries.Find(livelossofentryrow.LossID);
        //                    }
        //                    if (livelossofentry != null)
        //                    {
        //                        livelossofentry.IsScreen = 1;
        //                        using (unitworksccsEntities db1 = new unitworksccsEntities())
        //                        {
        //                            db1.Entry(livelossofentry).State = System.Data.Entity.EntityState.Modified;
        //                            db1.SaveChanges();
        //                        }
        //                    }
        //                }
        //            }    //  To update the new code at 2nd Time and so on.  
        //        }
        //        else if (livelossofentryrow == null && livemoderow != null)
        //        {
        //            if (livemoderow.MacMode == "IDLE")
        //            {
        //                //isIDLE = true;
        //                Totalsec = DateTime.Now.Subtract(Convert.ToDateTime(livemoderow.StartTime)).TotalSeconds;
        //                TotalModesec = Convert.ToDateTime(livemoderow.InsertedOn).Subtract(Convert.ToDateTime(livemoderow.StartTime)).TotalSeconds;
        //                durationofCurrent = Totalsec - TotalModesec;
        //                if (durationofCurrent > 120)
        //                {
        //                    tbllossofentry tblloe = new tbllossofentry();
        //                    tblloe.MessageCodeID = 999;
        //                    tblloe.StartTime = livemoderow.StartTime;
        //                    tblloe.EndTime = livemoderow.StartTime;
        //                    tblloe.EntryTime = livemoderow.StartTime;
        //                    tblloe.CorrectedDate = correctedDate;
        //                    tblloe.DoneWithRow = 0;
        //                    tblloe.IsScreen = 0;
        //                    tblloe.IsStart = 1;
        //                    tblloe.IsUpdate = 0;
        //                    tblloe.ForRefresh = 0;
        //                    tblloe.MachineID = machineid;
        //                    tblloe.MessageDesc = "No Code Entered";
        //                    tblloe.Shift = shift;
        //                    using (unitworksccsEntities db = new unitworksccsEntities())
        //                    {
        //                        db.tbllossofentries.Add(tblloe);
        //                        db.SaveChanges();
        //                    }
        //                }
        //            }
        //        }
        //    }

        //}

        //private void UpdateLossofEntries(int LossID, DateTime ModeStartTime)
        //{
        //    IntoFile("UpdateLossofEntries:" + ModeStartTime);
        //    var livelossofentry = new tbllossofentry();
        //    using (unitworksccsEntities db = new unitworksccsEntities())
        //    {
        //        livelossofentry = db.tbllossofentries.Find(LossID);
        //    }

        //    if (livelossofentry.StartTime <= ModeStartTime)
        //    {
        //        livelossofentry.EndTime = ModeStartTime;
        //        livelossofentry.DoneWithRow = 1;
        //        livelossofentry.IsUpdate = 1;
        //        livelossofentry.IsScreen = 0;
        //        livelossofentry.IsStart = 0;
        //        livelossofentry.ForRefresh = 0;
        //        using (unitworksccsEntities db1 = new unitworksccsEntities())
        //        {
        //            db1.Entry(livelossofentry).State = System.Data.Entity.EntityState.Modified;
        //            db1.SaveChanges();
        //        }
        //    }

        //}

        private void GetPartsandCutting()
        {
            IntoFile("GetPartsandCutting:");
            var machinedet = new List<unitworkccs_tblmachinedetails>();
            // var celldet = new List<tblcell>();

            string correctedDate = GetCorrectedDate();
            //string correctedDate = "2019-03-07";
            using (unitworksccsEntities db = new unitworksccsEntities())
            {
                machinedet = db.unitworkccs_tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsPCB == 1).ToList(); //m.CellID == cell.CellID && 
                                                                                                         //  celldet = db.tblcells.Where(m => m.IsDeleted == 0).ToList();

            }
            foreach (var machine in machinedet)
            {
                int machineid = machine.MachineID;
                //int cellID = celldet.Find(machine.CellID);
                DateTime correctedDateDt = Convert.ToDateTime(correctedDate);
                string PCBIPAddress = machine.IPAddress;
                int opno = Convert.ToInt32(machine.OperationNumber);
                //var scrap = new tblworkorderentry();
                var scrap = new unitworkccs_tblfgpartnodet();
                var scrapqty1 = new List<unitworkccs_tblrejectqty>();
                var cellpartDet = new unitworkccs_tblcellpart();
                var partsDet = new unitworkccs_tblparts();
                var bottleneckmachines = new unitworkccs_tblbottelneck();
                using (unitworksccsEntities db1 = new unitworksccsEntities())
                {
                    //scrap = db1.tblworkorderentries.Where(m => m.MachineID == machineid && m.CorrectedDate == correctedDateDt).OrderByDescending(m => m.HMIID).FirstOrDefault();  //workorder entry
                    string correctedDate1 = Convert.ToString(correctedDateDt);
                    scrap = db1.unitworkccs_tblfgpartnodet.Where(m => m.machineId == machineid && m.correctedDate == correctedDate1).OrderByDescending(m => m.fgPartId).FirstOrDefault();  //workorder entry
                    if (scrap != null)
                    {
                        //partsDet = db.tblparts.Where(m => m.IsDeleted == 0 && m.FGCode == scrap.PartNo).FirstOrDefault();
                        partsDet = db.unitworkccs_tblparts.Where(m => m.IsDeleted == 0 && m.PartID == scrap.partId).FirstOrDefault();
                        //if (partsDet != null)
                        //    bottleneckmachines = db.tblbottelnecks.Where(m => m.PartNo == partsDet.FGCode && m.CellID == scrap.CellID).FirstOrDefault();
                    }
                    else
                    {
                        partsDet = db.unitworkccs_tblparts.Where(m => m.IsDeleted == 0).FirstOrDefault();
                    }
                }
                double IdealTime = Convert.ToDouble(partsDet.IdealCycleTime);
                IdealTime = (IdealTime / 60.0);


                GetPartsCount(correctedDate, PCBIPAddress, machineid, IdealTime);
            }
        }

        private void GetPartsCount(string correcteddate, string PCBIPAddress, int machineid, double idealtime)
        {
            IntoFile("GetPartsCount:" + machineid);
            DateTime starttime = Convert.ToDateTime(correcteddate + " " + "06:00:00");
            DateTime endtime = starttime.AddDays(1);
            string endtme = correcteddate + " " + DateTime.Now.ToString("HH:mm:ss");
            DateTime temp = Convert.ToDateTime(correcteddate + " " + "06:00:00");
            DateTime CrctDate = Convert.ToDateTime(correcteddate);
            temp = temp.AddDays(1);
            if (endtime <= temp)
            {
                var parameterslist = new List<unitworkccs_iotgatwaypacketsdata>();
                DateTime stime = Convert.ToDateTime(correcteddate + " " + "06:00:00");
                DateTime edtime = stime.AddHours(1);
                using (unitworksccsEntities db1 = new unitworksccsEntities())
                {
                    parameterslist = db1.unitworkccs_iotgatwaypacketsdata.Where(m => m.IPAddres == PCBIPAddress && m.CreatedOn >= starttime && m.CreatedOn <= endtime).ToList();

                }
                foreach (var row in parameterslist)
                {
                    string sttime = stime.ToString("yyyy-MM-dd HH:mm:ss");
                    string etime = edtime.ToString("yyyy-MM-dd HH:mm:ss");
                    DateTime St = Convert.ToDateTime(sttime);
                    DateTime Et = Convert.ToDateTime(etime);
                    //DateTime ET = Convert.ToDateTime(endtme);
                    if (edtime <= temp)
                    {
                        var parameterslist1 = parameterslist.Where(m => m.CreatedOn >= St && m.CreatedOn <= Et && (m.AlaramInput1_16 == 1 && m.AlaramInput2_17 == 1 && m.AlaramInput3_18 == 1)).ToList();

                        //  parameterslist1 = parameterslist.Where(m => m.CreatedOn >= St && m.CreatedOn <= Et && (m.ParamPIN == 18 && m.ParamValue == 1)).ToList();
                        //var pin18= parameterslist.Where(m => m.CreatedOn >= starttime && m.CreatedOn <= endtime && (m.ParamPIN.ToString().Contains("17") && m.ParamPIN.ToString().Contains("18") && m.ParamPIN.ToString().Contains("17") && m.ParamValue == 1)).ToList();

                        //var toprow = parameterslist.Where(m => m.MachineID == machineid && m.InsertedOn >= St && m.InsertedOn <= Et).OrderByDescending(m => m.ParameterID).FirstOrDefault();
                        //var lastrow = parameterslist.Where(m => m.MachineID == machineid && m.InsertedOn >= St && m.InsertedOn <= Et).OrderBy(m => m.ParameterID).FirstOrDefault();
                        int PartsCount = 0, CuttingTime = 0;
                        if (parameterslist.Count > 0)
                        {
                            PartsCount = parameterslist1.Count;
                            // CuttingTime = Convert.ToInt32(toprow.CuttingTime) - Convert.ToInt32(lastrow.CuttingTime);
                        }
                        else
                        {
                            stime = edtime;
                            edtime = edtime.AddHours(1);
                            continue;

                        }
                        if (idealtime == 0)
                            idealtime = 1;
                        var targ = (60.00 / idealtime);

                        if (targ > 60)
                            targ = 0;
                        decimal Target = Math.Round((decimal)targ, 0);
                        var parts_cutting = new unitworkccs_tblpartscountandcutting();
                        using (unitworksccsEntities db1 = new unitworksccsEntities())
                        {
                            parts_cutting = db1.unitworkccs_tblpartscountandcutting.Where(m => m.CorrectedDate == CrctDate.Date && m.MachineID == machineid && m.Isdeleted == 0 && m.StartTime >= St && m.EndTime <= Et).FirstOrDefault();
                        }
                        if (parts_cutting == null)
                        {
                            DateTime cremod11 = DateTime.Now;
                            string cremod = cremod11.ToString("yyyy-MM-dd HH:MM:ss");

                            unitworkccs_tblpartscountandcutting parts = new unitworkccs_tblpartscountandcutting();
                            parts.MachineID = machineid;
                            parts.PartCount = PartsCount;
                            parts.CuttingTime = CuttingTime;
                            parts.CorrectedDate = CrctDate.Date;
                            parts.StartTime = Convert.ToDateTime(sttime);
                            parts.EndTime = Convert.ToDateTime(edtime);
                            parts.Isdeleted = 0;
                            parts.CreatedOn = DateTime.Now;
                            parts.CreatedBy = 1;
                            parts.TargetQuantity = Convert.ToInt32(Target);
                            using (unitworksccsEntities db = new unitworksccsEntities())
                            {
                                db.unitworkccs_tblpartscountandcutting.Add(parts);
                                db.SaveChanges();
                                IntoFile("Insertion success tblpartscountandcutting:");
                            }
                        }
                        else
                        {
                            var parts = new unitworkccs_tblpartscountandcutting();
                            using (unitworksccsEntities db = new unitworksccsEntities())
                            {
                                parts = db.unitworkccs_tblpartscountandcutting.Find(parts_cutting.pcid);
                            }
                            parts.PartCount = PartsCount;
                            parts.CuttingTime = CuttingTime;
                            parts.ModifiedOn = DateTime.Now;
                            parts.ModifiedBy = 1;
                            using (unitworksccsEntities db = new unitworksccsEntities())
                            {
                                db.Entry(parts).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                    stime = edtime;
                    edtime = edtime.AddHours(1);
                }

            }
        }
    }
}
