using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using MimicsUpdation.ServerModel;

namespace MimicsUpdation
{
    public partial class UpdateMimics : Form
    {
        unitworksccsEntities1 db = new unitworksccsEntities1();

        public UpdateMimics()
        {
            InitializeComponent();

            try
            {
                List<unitworkccs_tblmachinedetails> machines = getmachines();
                string CorrectedDate = "2019-06-21";
                //CorrectedDate = DateTime.Now.Date.ToString("yyyy-MM-dd");

                //string CorrectedDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
                DateTime correctedDate = getcorrecteddate();
                UpdateMimicsdetails(machines, correctedDate);
            }
            catch(Exception ex)
            {
                IntoFile(ex.ToString());
                //MessageBox.Show(ex.ToString());
            }

            try
            {
                GetPartsandCutting();
            }
            catch(Exception ex)
            {
                IntoFile(ex.ToString());
            }
            //Timer MyTimer = new Timer();
            //MyTimer.Interval = (60 * 1000); //1min            
            //MyTimer.Enabled = true;
            //MyTimer.Tick += new EventHandler(MyTimer_Tick);
            //MyTimer.Start();


            //Timer MyTimer1 = new Timer();
            //MyTimer1.Interval = (60 * 1000 * 5); //5 min          
            //MyTimer1.Enabled = true;
            //MyTimer1.Tick += new EventHandler(MyTimer_Tick1);
            //MyTimer1.Start();
        }


        private List<unitworkccs_tblmachinedetails> getmachines()
        {
            string CorrectedDate = "2019-06-21";
            //CorrectedDate = DateTime.Now.Date.ToString("yyyy-MM-dd");

            //string CorrectedDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            //CorrectedDate = "2019-06-21";
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                List<unitworkccs_tblmachinedetails> machinedetails = db.unitworkccs_tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0).ToList();

                return machinedetails;
            }
        }

        private void UpdateMimicsdetails(List<unitworkccs_tblmachinedetails> machines, DateTime correctedDate)
        {
            foreach (var row in machines)
            {

                decimal OperatingTime = 0;
                decimal LossTime = 0;
                decimal MntTime = 0;
                decimal SetupTime = 0;
                decimal SetupMinorTime = 0;
                decimal PowerOffTime = 0;
                decimal PowerONTime = 0;
                int MachineID = row.MachineID;
                //int MachineID = 24;
                var GetModeDurations = new List<unitworkccs_tbllivemode>();
                string correctedDatestr = correctedDate.ToString("yyyy-MM-dd");
                DateTime correcteddate = Convert.ToDateTime(correctedDatestr);
                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                {
                    GetModeDurations = db.unitworkccs_tbllivemode.Where(m => m.MachineID == MachineID && m.CorrectedDate == correcteddate && m.IsCompleted == 1).ToList();
                }
                OperatingTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ColorCode == "green" || m.ColorCode == "GREEN").ToList().Sum(m => m.DurationInSec));
                PowerOffTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ColorCode == "blue" || m.ColorCode == "BLUE").ToList().Sum(m => m.DurationInSec));
                MntTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ColorCode == "red" || m.ColorCode == "RED").ToList().Sum(m => m.DurationInSec));
                LossTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ColorCode == "yellow" || m.ColorCode == "YELLOW").ToList().Sum(m => m.DurationInSec));
                //var modeids = new List<int>();
                //using (unitworksccsEntities1 db = new unitworksccsEntities1())
                //{
                //    modeids = db.unitworkccs_tbllivemodedbs.Where(m => m.MachineID == row.MachineID && m.CorrectedDate == correctedDatestr && m.IsCompleted == 1).Select(m => m.ModeID).ToList();
                //}
                //var setuptimelist = db.tblsetupmaints.Where(m => m.IsCompleted == 1 && modeids.Contains(m.ModeID) && m.MachineID == row.MachineID).ToList();
                //foreach (var row1 in setuptimelist)
                //{
                //    DateTime startTime = Convert.ToDateTime(row1.StartTime);
                //    DateTime endTime = Convert.ToDateTime(row1.EndTime);
                //    SetupMinorTime += Convert.ToDecimal(endTime.Subtract(startTime).TotalSeconds);
                //}

                //foreach (var ModeRow in GetModeDurations)
                //{
                //    if (ModeRow.Mode == "SETUP")
                //    {
                //        try
                //        {
                //            SetupTime += (decimal)Convert.ToDateTime(ModeRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(ModeRow.StartTime)).TotalSeconds;
                //            //SetupMinorTime += (decimal)(db.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60.00);
                //        }
                //        catch { }
                //    }

                //}
                var GetModeDurationsRunning = new List<unitworkccs_tbllivemode>();
                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                {
                    GetModeDurationsRunning = db.unitworkccs_tbllivemode.Where(m => m.MachineID == MachineID && m.CorrectedDate == correcteddate && m.IsCompleted == 0).ToList();
                }
                foreach (var ModeRow in GetModeDurationsRunning)
                {
                    String ColorCode = ModeRow.ColorCode;
                    DateTime StartTime = (DateTime)ModeRow.StartTime;
                    decimal Duration = (decimal)System.DateTime.Now.Subtract(StartTime).TotalSeconds;
                    if (ColorCode == "yellow" || ColorCode=="YELLOW")
                    {
                        LossTime += Duration;
                    }
                    else if (ColorCode == "green" || ColorCode=="GREEN")
                    {
                        OperatingTime += Duration;
                    }
                    else if (ColorCode == "red" || ColorCode=="RED")
                    {
                        MntTime += Duration;
                    }
                    else if (ColorCode == "blue" || ColorCode=="BLUE")
                    {
                        PowerOffTime += Duration;
                    }
                }

                PowerONTime = OperatingTime + LossTime;
                int IdleTime = Convert.ToInt32(LossTime + SetupTime + SetupMinorTime);
                OperatingTime = Math.Round((OperatingTime / 60), 2);
                PowerOffTime = (PowerOffTime / 60);
                MntTime = (MntTime / 60);
                IdleTime = (IdleTime / 60);
                PowerONTime = (PowerONTime / 60);


                string correctedDt = correctedDate.Date.ToString("yyyy-MM-dd");
                var mimicsdata = new unitworkccs_tblmimics();
                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                {
                    mimicsdata = db.unitworkccs_tblmimics.Where(m => m.MachineID == MachineID && m.CorrectedDate == correctedDt).FirstOrDefault();
                }
                if (mimicsdata == null)
                {

                    unitworkccs_tblmimics mimics = new unitworkccs_tblmimics();
                    mimics.MachineID = MachineID;
                    mimics.CorrectedDate = correctedDt;
                    mimics.OperatingTime = Convert.ToInt32(OperatingTime);
                    mimics.BreakdownTime = Convert.ToInt32(MntTime);
                    mimics.MachineOffTime = Convert.ToInt32(PowerOffTime);
                    mimics.IdleTime = Convert.ToInt32(IdleTime);
                    mimics.SetupTime = Convert.ToInt32(SetupTime);
                    mimics.MachineOnTime = Convert.ToInt32(PowerONTime);
                    using (unitworksccsEntities1 db = new unitworksccsEntities1())
                    {
                        db.unitworkccs_tblmimics.Add(mimics);
                        db.SaveChanges();
                    }

                }
                else
                {
                    var mimicsrow = new unitworkccs_tblmimics();
                    using (unitworksccsEntities1 db = new unitworksccsEntities1())
                    {
                        mimicsrow = db.unitworkccs_tblmimics.Find(mimicsdata.mid);
                    }
                    mimicsrow.MachineID = MachineID;
                    mimicsrow.CorrectedDate = correctedDt;
                    mimicsrow.OperatingTime = Convert.ToInt32(OperatingTime);
                    mimicsrow.BreakdownTime = Convert.ToInt32(MntTime);
                    mimicsrow.MachineOffTime = Convert.ToInt32(PowerOffTime);
                    mimicsrow.IdleTime = Convert.ToInt32(IdleTime);
                    mimicsrow.SetupTime = Convert.ToInt32(SetupTime);
                    mimicsrow.MachineOnTime = Convert.ToInt32(PowerONTime);
                    using (unitworksccsEntities1 db = new unitworksccsEntities1())
                    {
                        db.Entry(mimicsrow).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }


        private void MyTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                List<unitworkccs_tblmachinedetails> machines = getmachines();
                DateTime correctedDate = getcorrecteddate();
                UpdateMimicsdetails(machines, correctedDate);
            }
            catch(Exception ex)
            {
                IntoFile(ex.ToString());
            }
        }

        private void MyTimer_Tick1(object sender, EventArgs e)
        {
            try
            {
                GetPartsandCutting();
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
        }

        private DateTime getcorrecteddate()
        {
            DateTime correctedDate = DateTime.Now;
            var daytimings = db.unitworkccs_tbldaytiming.Where(m => m.IsDeleted == 0).FirstOrDefault();
            if (daytimings != null)
            {
                DateTime Start = Convert.ToDateTime(correctedDate.ToString("yyyy-MM-dd") + " " + daytimings.StartTime);


                //DateTime Start = Convert.ToDateTime(dtMode.Rows[0][0].ToString());
                if (Start <= DateTime.Now)
                {
                    correctedDate = DateTime.Now.Date;
                }
                else
                {
                    correctedDate = DateTime.Now.AddDays(-1).Date;
                }
            }
            return correctedDate;
        }

        private void GetPartsandCutting()
        {
            var machinedet = new List<unitworkccs_tblmachinedetails>();
            // var celldet = new List<tblcell>();
        
            string correctedDate = getcorrecteddate().ToString("yyyy-MM-dd");
            DateTime correcteddate = Convert.ToDateTime(correctedDate);
            //string correctedDate = "2019-03-07";
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                machinedet = db.unitworkccs_tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0).ToList(); //m.CellID == cell.CellID && 
                                                                                                              //  celldet = db.tblcells.Where(m => m.IsDeleted == 0).ToList();

            }
            foreach (var machine in machinedet)
            {
                int machineid = machine.MachineID;
                //int cellID = celldet.Find(machine.CellID);
                int opno = Convert.ToInt32(machine.OperationNumber);
                var scrap = new unitworkccs_tblfgpartnodet();
                var scrapqty1 = new List<unitworkccs_tblrejectqty>();
                var cellpartDet = new unitworkccs_tblcellpart();
                var partsDet = new unitworkccs_tblparts();
                var bottleneckmachines = new unitworkccs_tblbottelneck();
         

                using (unitworksccsEntities1 db1 = new unitworksccsEntities1())
                {
                    scrap = db1.unitworkccs_tblfgpartnodet.Where(m => m.machineId == machineid && m.correctedDate == correctedDate).OrderByDescending(m => m.fgPartId).FirstOrDefault();  //workorder entry
                    if (scrap != null)
                    {
                        partsDet = db.unitworkccs_tblparts.Where(m => m.IsDeleted == 0 && m.PartID == scrap.partId).FirstOrDefault();
                        //if (partsDet != null)
                        //    bottleneckmachines = db.unitworkccs_tblbottelnecks.Where(m => m.PartNo == partsDet.FGCode && m.CellID == scrap.CellID).FirstOrDefault();
                    }
                    else
                    {
                        partsDet = db.unitworkccs_tblparts.Where(m => m.IsDeleted == 0 ).FirstOrDefault();
                    }
                    //else
                    //{
                    //    cellpartDet = db.unitworkccs_tblcellparts.Where(m => m.CellID == machine.CellID && m.IsDefault == 1 && m.IsDeleted == 0).FirstOrDefault();
                    //    if (cellpartDet != null)
                    //        bottleneckmachines = db.unitworkccs_tblbottelnecks.Where(m => m.PartNo == cellpartDet.partNo && m.CellID == cellpartDet.CellID).FirstOrDefault();
                    //    //string Operationnum = bottleneckmachines.unitworkccs_tblmachinedetails.OperationNumber.ToString();
                    //    string Operationnum = machine.OperationNumber.ToString();
                    //    partsDet = db.unitworkccs_tblpartss.Where(m => m.IsDeleted == 0 && m.FGCode == cellpartDet.partNo && m.OperationNo == Operationnum).FirstOrDefault();

                    //}
                }
                
                double IdealTime = Convert.ToDouble(partsDet.IdealCycleTime);
                IdealTime = (IdealTime / 60.0);


                GetPartsCount(correctedDate, machineid, IdealTime);
            }
        }

        private void GetPartsCount(string correcteddate, int machineid, double idealtime)
        {           
            DateTime starttime = Convert.ToDateTime(correcteddate + " " + "06:00:00");
            DateTime endtime = starttime.AddDays(1);
            string endtme = correcteddate + " " + DateTime.Now.ToString("HH:mm:ss");
            DateTime temp = Convert.ToDateTime(correcteddate + " " + "06:00:00");
            DateTime CrctDate = Convert.ToDateTime(correcteddate);
            temp = temp.AddDays(1);
            if (endtime <= temp)
            {
                var parameterslist = new List<unitworkccs_parameters_master>();
                DateTime stime = Convert.ToDateTime(correcteddate + " " + "06:00:00");
                DateTime edtime = stime.AddHours(1);
                //using (unitworksccsEntities1 db1 = new unitworksccsEntities1())
                //{
                    parameterslist = db.unitworkccs_parameters_master.Where(m => m.MachineID == machineid && m.CorrectedDate == CrctDate.Date && m.InsertedOn >= starttime && m.InsertedOn <= endtime).ToList();
                //}
                foreach (var row in parameterslist)
                {
                    int PartPerCycle = 1;
                    var WorkOrderEntryDet = db.unitworkccs_tblfgpartnodet.Where(m => m.isClosed == null && m.correctedDate == correcteddate && m.machineId == machineid).FirstOrDefault(); // to mutiple by partpercycle
                    if(WorkOrderEntryDet!=null)
                    {
                        PartPerCycle = Convert.ToInt32(WorkOrderEntryDet.noOfPartsPerCycle);
                    }
                    string sttime = stime.ToString("yyyy-MM-dd HH:mm:ss");
                    string etime = edtime.ToString("yyyy-MM-dd HH:mm:ss");
                    DateTime St = Convert.ToDateTime(sttime);
                    DateTime Et = Convert.ToDateTime(etime);
                    //DateTime ET = Convert.ToDateTime(endtme);
                    if (edtime <= temp)
                    {
                        var LastRow = parameterslist.Where(m => m.MachineID == machineid && m.InsertedOn >= St && m.InsertedOn <= Et).OrderByDescending(m => m.ParameterID).FirstOrDefault();
                        var FirstRow = parameterslist.Where(m => m.MachineID == machineid && m.InsertedOn >= St && m.InsertedOn <= Et).OrderBy(m => m.ParameterID).FirstOrDefault();
                        int PartsCount = 0, CuttingTime = 0;
                        
                        if (LastRow != null && FirstRow != null)
                        {
                            PartsCount = Convert.ToInt32(LastRow.PartsTotal - FirstRow.PartsTotal);
                            CuttingTime = Convert.ToInt32(LastRow.CuttingTime) - Convert.ToInt32(FirstRow.CuttingTime);
                            if (FirstRow.PartsTotal > LastRow.PartsTotal)
                            {
                                using (unitworksccsEntities1 db1 = new unitworksccsEntities1())
                                {
                                    parameterslist = db1.unitworkccs_parameters_master.Where(m => m.MachineID == machineid && m.CorrectedDate == CrctDate.Date && m.InsertedOn >= starttime && m.InsertedOn <= endtime).ToList();
                                    var Zero = parameterslist.Where(m => m.PartsTotal == '0').Select(m => m.ParameterID).FirstOrDefault();
                                    var prevrecord = db1.unitworkccs_parameters_master.Where(m => m.ParameterID < Zero && m.MachineID == machineid).OrderByDescending(m => m.ParameterID).Select(m => m.PartsTotal).FirstOrDefault();
                                    var upfirst = FirstRow.PartsTotal - prevrecord;
                                    var updatedPartsCount = upfirst + LastRow.PartsTotal;
                                    PartsCount = (int)updatedPartsCount;
                                    CuttingTime = Convert.ToInt32(LastRow.CuttingTime) - Convert.ToInt32(FirstRow.CuttingTime);
                                }
                            }
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
                        using (unitworksccsEntities1 db1 = new unitworksccsEntities1())
                        {
                            parts_cutting = db1.unitworkccs_tblpartscountandcutting.Where(m => m.CorrectedDate == CrctDate.Date && m.MachineID == machineid && m.Isdeleted == 0 && m.StartTime >= St && m.EndTime <= Et).FirstOrDefault();
                        }
                        if (parts_cutting == null)
                        {
                            DateTime cremod11 = DateTime.Now;
                            string cremod = cremod11.ToString("yyyy-MM-dd HH:MM:ss");
                            int Tgt = Convert.ToInt32(Target);
                            unitworkccs_tblpartscountandcutting parts = new unitworkccs_tblpartscountandcutting();
                            parts.MachineID = machineid;
                            parts.PartCount = (PartPerCycle * PartsCount);
                            parts.CuttingTime = CuttingTime;
                            parts.CorrectedDate = CrctDate.Date;
                            parts.StartTime = Convert.ToDateTime(sttime);
                            parts.EndTime = Convert.ToDateTime(edtime);
                            parts.Isdeleted = 0;
                            parts.CreatedOn = DateTime.Now;
                            parts.CreatedBy = 1;
                            parts.ShiftName = ShiftDet();
                            parts.TargetQuantity = Convert.ToInt32(Target);
                            if(PartPerCycle>1)
                            {
                                parts.PartsPerCyscleEnteredTime = WorkOrderEntryDet.startDate;
                                parts.WoPartCount= (PartPerCycle * PartsCount);
                                parts.woTargetQty = (PartPerCycle * Tgt);
                            }
                            else
                            {
                                parts.WoPartCount = PartsCount;
                                parts.woTargetQty =  Tgt;
                            }
                            using (unitworksccsEntities1 db = new unitworksccsEntities1())
                            {
                                db.unitworkccs_tblpartscountandcutting.Add(parts);
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            var parts = new unitworkccs_tblpartscountandcutting();
                            using (unitworksccsEntities1 db = new unitworksccsEntities1())
                            {
                                parts = db.unitworkccs_tblpartscountandcutting.Find(parts_cutting.pcid);
                            }
                            parts.PartCount = (PartPerCycle * PartsCount);
                            parts.CuttingTime = CuttingTime;
                            parts.ModifiedOn = DateTime.Now;
                            parts.ModifiedBy = 1;
                            if (PartPerCycle > 1)
                            {
                                parts.PartsPerCyscleEnteredTime = WorkOrderEntryDet.startDate;
                                parts.WoPartCount = (PartPerCycle * PartsCount);
                            }                            
                            using (unitworksccsEntities1 db = new unitworksccsEntities1())
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


        private string ShiftDet()
        {
            string ShiftValue = "";
            DateTime DateNow = DateTime.Now;
            var ShiftDetails=db.unitworkccs_tblshift_mstr.Where(m => m.IsDeleted == 0).ToList();
            foreach(var row in ShiftDetails)
            {
                int ShiftStartHour = row.StartTime.Value.Hours;
                int ShiftEndHour = row.EndTime.Value.Hours;
                int CurrentHour = DateNow.Hour;
                if(CurrentHour>=ShiftStartHour && CurrentHour<=ShiftEndHour)
                {
                    ShiftValue = row.ShiftName;
                }
            }

            return ShiftValue;
        }



        private void GetPartsCountPrevious(string correcteddate, int machineid, int idletime)
        {
            DateTime starttime = Convert.ToDateTime(correcteddate + " " + "06:00:00");
            DateTime endtime = starttime.AddDays(1);
            string endtme = DateTime.Now.ToString("HH:mm:ss");
            DateTime temp = Convert.ToDateTime(correcteddate + " " + "06:00:00");
            DateTime CrctDate = Convert.ToDateTime(correcteddate);
            temp = temp.AddDays(1);
            if (endtime <= temp)
            {
                using (unitworksccsEntities1 db1 = new unitworksccsEntities1())
                {
                    DateTime stime = Convert.ToDateTime(correcteddate + " " + "06:00:00");
                    DateTime edtime = stime.AddHours(1);
                    var parameterslist = db1.unitworkccs_parameters_master.Where(m => m.MachineID == machineid && m.CorrectedDate == CrctDate.Date && m.InsertedOn >= starttime && m.InsertedOn <= endtime).ToList();

                    //foreach (var row in parameterslist)
                    {
                        string sttime = stime.ToString("yyyy-MM-dd HH:mm:ss");
                        string etime = edtime.ToString("yyyy-MM-dd HH:mm:ss");
                        DateTime St = Convert.ToDateTime(sttime);
                        DateTime Et = Convert.ToDateTime(etime);
                        if (edtime <= temp)
                        {
                            var toprow = parameterslist.Where(m => m.InsertedOn >= St && m.InsertedOn <= Et).OrderByDescending(m => m.ParameterID).FirstOrDefault();
                            var lastrow = parameterslist.Where(m => m.InsertedOn >= St && m.InsertedOn <= Et).OrderBy(m => m.ParameterID).FirstOrDefault();
                            int PartsCount = 0, CuttingTime = 0;
                            if (toprow != null && lastrow != null)
                            {
                                PartsCount = Convert.ToInt32(toprow.PartsTotal - lastrow.PartsTotal);
                                CuttingTime = Convert.ToInt32(toprow.CuttingTime) - Convert.ToInt32(lastrow.CuttingTime);
                            }
                            if (idletime == 0)
                                idletime = 1;
                            var parts_cutting = db1.unitworkccs_tblpartscountandcutting.Where(m => m.CorrectedDate == CrctDate.Date && m.Isdeleted == 0 && m.StartTime == St && m.EndTime == Et).FirstOrDefault();
                            if (parts_cutting == null)
                            {
                                DateTime cremod11 = DateTime.Now;
                                string cremod = cremod11.ToString("yyyy-MM-dd HH:MM:ss");
                                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                                {
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
                                    parts.TargetQuantity = idletime;
                                    db.unitworkccs_tblpartscountandcutting.Add(parts);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                                {
                                    var parts = db.unitworkccs_tblpartscountandcutting.Find(parts_cutting.pcid);
                                    parts.PartCount = PartsCount;
                                    parts.CuttingTime = CuttingTime;
                                    //parts.CorrectedDate = CrctDate.Date;
                                    //parts.StartTime = Convert.ToDateTime(sttime);
                                    //parts.EndTime = Convert.ToDateTime(edtime);
                                    //parts.Isdeleted = 0;
                                    //parts.CreatedOn = DateTime.Now;
                                    //parts.CreatedBy = 1;
                                    //parts.TargetQuantity = idletime;
                                    parts.ModifiedOn = DateTime.Now;
                                    parts.ModifiedBy = 1;
                                    db.Entry(parts).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                        }
                        //else
                        //{
                        //    break;
                        //}
                        stime = edtime;
                        edtime = edtime.AddHours(1);
                    }
                }
            }
        }


        public void CalCulatePartsCount()
        {

        }


        #region PreviouCode


        //public string getcorrecteddate()
        //{
        //    DateTime nowDate = DateTime.Now;
        //    string corrdate = DateTime.Now.ToString("yyyy-MM-dd");
        //    if (nowDate.Hour < 7 && nowDate.Hour > 0)
        //    {
        //        corrdate = nowDate.AddDays(-1).ToString("yyyy-MM-dd");
        //    }
        //    return corrdate;
        //}
        //private void MyTimer_Tick(object sender, EventArgs e)
        //{

        //    using (MsqlConnection con1 = new MsqlConnection())
        //    {
        //        try
        //        {

        //            DataTable dt = new DataTable();
        //            using (MsqlConnection con = new MsqlConnection())
        //            {
        //                con.open();
        //                var machDetails = "SELECT MachineID FROM i_facility.unitworkccs_tblmachinedetailss as m WHERE m.IsDeleted = 0 and m.IsNormalWC = 0 ;";
        //                MySqlDataAdapter msda = new MySqlDataAdapter(machDetails, con.msqlConnection);
        //                msda.Fill(dt);
        //                con.close();
        //            }

        //            for (int p = 0; p < dt.Rows.Count; p++)
        //            {
        //                int OperatingTimeV = 0, PowerOffTimeV = 0, BDTimeV = 0, LossTimeV = 0, SetupTimeV = 0, SetupMinorTimeV = 0, PowerOnTime = 0;
        //                DataTable dt1 = new DataTable();

        //                int macid = Convert.ToInt32(dt.Rows[p][0].ToString());
        //                //DateTime nowDate = DateTime.Now;
        //                string corrdate = getcorrecteddate().ToString("yyyy-MM-dd");
        //                //if (nowDate.Hour < 7 && nowDate.Hour > 0)
        //                //{
        //                //    corrdate = nowDate.AddDays(-1).ToString("yyyy-MM-dd");
        //                //}

        //                DataTable opdt = new DataTable();
        //                DataTable pwdt = new DataTable();
        //                DataTable bddt = new DataTable();
        //                DataTable ltdt = new DataTable();
        //                DataTable stdt = new DataTable();
        //                DataTable smdt = new DataTable();
        //                var OperatingTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE, StartTime, EndTime)), 0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + " and ModeType = 'PROD';";
        //                var PowerOffTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE,StartTime,EndTime)),0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeType = 'POWEROFF' and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + ";";
        //                var BDTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE,StartTime,EndTime)),0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeTypeEnd = 1 and ModeType = 'MNT' and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + ";";
        //                var LossTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE,StartTime,EndTime)),0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeType = 'IDLE' and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + ";";
        //                var SetupTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE,StartTime,LossCodeEnteredTime)),0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeType = 'SETUP' and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + ";";
        //                var SetupMinorTime = "SELECT  IFNULL(SUM(timestampdiff(MINUTE, StartTime, EndTime)), 0) from i_facility.tblsetupmaint where IsCompleted = 1 and ModeID in(Select ModeID from i_facility.unitworkccs_tbllivemode where MachineID = " + macid + " and CorrectedDate = '" + corrdate + " ') and MachineID = " + macid + " ; ";
        //                using (MsqlConnection con = new MsqlConnection())
        //                {
        //                    con.open();
        //                    MySqlDataAdapter initdet = new MySqlDataAdapter(OperatingTime, con.msqlConnection);
        //                    initdet.Fill(opdt);
        //                    con.close();
        //                }
        //                using (MsqlConnection con = new MsqlConnection())
        //                {
        //                    con.open();
        //                    OperatingTimeV = Convert.ToInt32(opdt.Rows[0][0].ToString());
        //                    MySqlDataAdapter initdet1 = new MySqlDataAdapter(PowerOffTime, con.msqlConnection);
        //                    initdet1.Fill(pwdt);
        //                    con.close();
        //                }
        //                using (MsqlConnection con = new MsqlConnection())
        //                {
        //                    con.open();
        //                    PowerOffTimeV = Convert.ToInt32(pwdt.Rows[0][0].ToString());
        //                    MySqlDataAdapter initdet2 = new MySqlDataAdapter(BDTime, con.msqlConnection);
        //                    initdet2.Fill(bddt);
        //                    con.close();
        //                }
        //                using (MsqlConnection con = new MsqlConnection())
        //                {
        //                    con.open();
        //                    BDTimeV = Convert.ToInt32(bddt.Rows[0][0].ToString());
        //                    MySqlDataAdapter initdet3 = new MySqlDataAdapter(LossTime, con.msqlConnection);
        //                    initdet3.Fill(ltdt);
        //                    con.close();
        //                }
        //                using (MsqlConnection con = new MsqlConnection())
        //                {
        //                    con.open();
        //                    LossTimeV = Convert.ToInt32(ltdt.Rows[0][0].ToString());
        //                    MySqlDataAdapter initdet4 = new MySqlDataAdapter(SetupTime, con.msqlConnection);
        //                    initdet4.Fill(stdt);
        //                    con.close();
        //                }
        //                using (MsqlConnection con = new MsqlConnection())
        //                {
        //                    con.open();
        //                    SetupTimeV = Convert.ToInt32(stdt.Rows[0][0].ToString());
        //                    MySqlDataAdapter initdet5 = new MySqlDataAdapter(SetupMinorTime, con.msqlConnection);
        //                    initdet5.Fill(smdt);
        //                    con.close();
        //                }
        //                using (MsqlConnection con = new MsqlConnection())
        //                {
        //                    con.open();
        //                    SetupMinorTimeV = Convert.ToInt32(smdt.Rows[0][0].ToString());
        //                    PowerOnTime = OperatingTimeV + LossTimeV + SetupTimeV;
        //                    con.close();
        //                }

        //                using (MsqlConnection con = new MsqlConnection())
        //                {
        //                    con.open();
        //                    var livemodedetails = "SELECT  MacMode from i_facility.unitworkccs_tbllivemode  where (IsCompleted = 0 or (IsCompleted = 1 and ModeTypeEnd = 0)) and CorrectedDate = '" + corrdate + " 'and MachineID = " + macid + " ;";
        //                    MySqlDataAdapter msda1 = new MySqlDataAdapter(livemodedetails, con.msqlConnection);
        //                    msda1.Fill(dt1);
        //                    con.close();
        //                }
        //                for (int i = 0; i < dt1.Rows.Count; i++)
        //                {
        //                    int count = 0;
        //                    string macname = dt1.Rows[i][0].ToString();
        //                    using (MsqlConnection conn1 = new MsqlConnection())
        //                    {
        //                        conn1.open();
        //                        DataTable dt2 = new DataTable();
        //                        var mimicsdetails = "SELECT Count(*) FROM i_facility.unitworkccs_tblmimicss where MachineID = " + macid + "  and CorrectedDate = '" + corrdate + "';";
        //                        MySqlDataAdapter msda2 = new MySqlDataAdapter(mimicsdetails, conn1.msqlConnection);
        //                        msda2.Fill(dt2);
        //                        conn1.close();

        //                        if (Convert.ToInt32(dt2.Rows[0][0].ToString()) > 0)
        //                        {
        //                            if (macname == "IDLE")
        //                            {


        //                                    int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                //var losstime = " SELECT SUM(timestampdiff(MINUTE, StartTime, EndTime)) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and CorrectedDate ='" + corrdate + "' and MachineID = " + macid + " and ModeType = 'IDLE'; ";
        //                                //MySqlDataAdapter lossup = new MySqlDataAdapter(losstime, con.msqlConnection);
        //                                //lossup.Fill(loss);
        //                                //int lossval = LossTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                //DataTable up = new DataTable();
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();
        //                                    var UpdateValue = "select IFNULL(SUM(timestampdiff(MINUTE,StartTime, current_timestamp())),0)  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    sa.close();
        //                                    int? rowval = 0;
        //                                    if (loss.Rows.Count > 0)
        //                                        rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = LossTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = LossTimeV + 0;
        //                                    }
        //                                    if (updateval == 0)
        //                                    {
        //                                        PowerOnTime = PowerOnTime + LossTimeV;
        //                                    }
        //                                    else
        //                                    {
        //                                        PowerOnTime = PowerOnTime + (updateval - LossTimeV);
        //                                    }
        //                                    //using (MsqlConnection sa1 = new MsqlConnection())
        //                                    //{
        //                                    sa.open();
        //                                    var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime =" + OperatingTimeV + ", BreakdownTime =" + BDTimeV + " ,IdleTime=" + updateval + ", SetupTime = " + SetupTimeV + ", MachineOffTime = " + PowerOffTimeV + " where MachineID = " + macid + " and CorrectedDate = '" + corrdate + "'; ";
        //                                    MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa.msqlConnection);
        //                                    msda4.Fill(loss);
        //                                    sa.close();
        //                                    //}
        //                                }
        //                            }
        //                            else if (macname == "POWEROFF")
        //                            {

        //                                    int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                ////var BTime = " SELECT SUM(timestampdiff(MINUTE,StartTime,EndTime)) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeTypeEnd = 1 and CorrectedDate ='" + corrdate + "' and MachineID = " + macid + " and ModeType = 'POWEROFF'; ";
        //                                ////MySqlDataAdapter lossup = new MySqlDataAdapter(BTime, con.msqlConnection);
        //                                ////lossup.Fill(loss);
        //                                //int BDTimeval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                ////DataTable up = new DataTable();
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();
        //                                    var UpdateValue = "select IFNULL(SUM(timestampdiff(MINUTE,StartTime,current_timestamp())),0)  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    sa.close();
        //                                    int? rowval = 0;
        //                                    if (loss.Rows.Count > 0)
        //                                        rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = BDTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = BDTimeV + 0;
        //                                    }

        //                                    updateval = updateval + PowerOffTimeV;
        //                                    //using (MsqlConnection sa1 = new MsqlConnection())
        //                                    {
        //                                        sa.open();
        //                                        var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime = " + OperatingTimeV + ", BreakdownTime =" + BDTimeV + ",IdleTime=" + LossTimeV + ", SetupTime = " + SetupTimeV + ", MachineOffTime = " + updateval + " where MachineID = " + macid + "  and CorrectedDate = '" + corrdate + "';";
        //                                        MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa.msqlConnection);
        //                                        msda4.Fill(loss);
        //                                        sa.close();
        //                                    }
        //                                }
        //                            }
        //                            else if (macname == "SETUP")
        //                            {

        //                                    int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                //var SetupTime = " select SUM(timestampdiff(MINUTE,StartTime,CURDATE()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = Cdate and MachineID = " + macid + " and MacMode='" + macname + "' ";
        //                                //MySqlDataAdapter lossup = new MySqlDataAdapter(BDTime, con.msqlConnection);
        //                                //lossup.Fill(loss);
        //                                //int BDTimeval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                //DataTable up = new DataTable();
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();
        //                                    var UpdateValue = "select IFNULL(SUM(timestampdiff(MINUTE,StartTime,current_timestamp())),0)  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    sa.close();
        //                                    int? rowval = 0;
        //                                    if (loss.Rows.Count > 0)
        //                                        rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = SetupTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = SetupTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    if (updateval == 0)
        //                                    {
        //                                        PowerOnTime = PowerOnTime + SetupTimeV;
        //                                    }
        //                                    else
        //                                    {
        //                                        PowerOnTime = PowerOnTime + (updateval - SetupTimeV);
        //                                    }
        //                                    //using (MsqlConnection sa1 = new MsqlConnection())
        //                                    {
        //                                        sa.open();
        //                                        var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime = " + OperatingTimeV + ", BreakdownTime = " + BDTimeV + ",IdleTime=" + LossTimeV + ", SetupTime =" + updateval + ", MachineOffTime =" + PowerOffTimeV + " where MachineID = " + macid + "  and CorrectedDate = '" + corrdate + "'; ";
        //                                        MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa.msqlConnection);
        //                                        msda4.Fill(loss);
        //                                        sa.close();
        //                                    }
        //                                }
        //                            }
        //                            else if (macname == "PROD")
        //                            {
        //                                int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                //var SetupTime = " select SUM(timestampdiff(MINUTE,StartTime,CURDATE()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = Cdate and MachineID = " + macid + " and MacMode='" + macname + "' ";
        //                                //MySqlDataAdapter lossup = new MySqlDataAdapter(BDTime, con.msqlConnection);
        //                                //lossup.Fill(loss);
        //                                //int BDTimeval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                //DataTable up = new DataTable();
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();
        //                                    var UpdateValue = "select IFNULL(SUM(timestampdiff(MINUTE,StartTime,current_timestamp())),0)  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    sa.close();
        //                                    int? rowval = 0;
        //                                    if (loss.Rows.Count > 0)
        //                                        rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = OperatingTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = OperatingTimeV + 0;
        //                                    }
        //                                    PowerOnTime = PowerOnTime + (updateval - OperatingTimeV);
        //                                    //using (MsqlConnection sa1 = new MsqlConnection())
        //                                    {
        //                                        sa.open();
        //                                        var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime = " + updateval + ", BreakdownTime =  " + BDTimeV + ",IdleTime=" + LossTimeV + ", SetupTime = " + SetupTimeV + ", MachineOffTime = " + PowerOffTimeV + " where MachineID = " + macid + " and CorrectedDate = '" + corrdate + "';";
        //                                        MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa.msqlConnection);
        //                                        msda4.Fill(loss);
        //                                        sa.close();
        //                                    }
        //                                }
        //                            }
        //                            else if (macname == "MNT")
        //                            {

        //                                    int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                //var SetupTime = " select SUM(timestampdiff(MINUTE,StartTime,CURDATE()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = Cdate and MachineID = " + macid + " and MacMode='" + macname + "' ";
        //                                //MySqlDataAdapter lossup = new MySqlDataAdapter(BDTime, con.msqlConnection);
        //                                //lossup.Fill(loss);
        //                                //int BDTimeval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                //DataTable up = new DataTable();
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();
        //                                    var UpdateValue = "select IFNULL(SUM(timestampdiff(MINUTE,StartTime,current_timestamp())),0)  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    sa.close();
        //                                    int? rowval = 0;
        //                                    if (loss.Rows.Count > 0)
        //                                        rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = BDTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = BDTimeV + 0;
        //                                    }
        //                                    //using (MsqlConnection sa1 = new MsqlConnection())
        //                                    {
        //                                        sa.open();
        //                                        var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime = " + OperatingTimeV + ", BreakdownTime = " + updateval + ",IdleTime=" + LossTimeV + ", SetupTime = " + SetupTimeV + ", MachineOffTime =" + PowerOffTimeV + " where MachineID =  " + macid + " and CorrectedDate = '" + corrdate + "';";
        //                                        MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa.msqlConnection);
        //                                        msda4.Fill(loss);
        //                                        sa.close();
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            using (MsqlConnection sa = new MsqlConnection())
        //                            {
        //                                sa.open();
        //                                DataTable insertt = new DataTable();
        //                                var inrt = " INSERT INTO i_facility.unitworkccs_tblmimicss (MachineOnTime,OperatingTime,SetupTime,IdleTime,MachineOffTime,BreakdownTime,MachineID,CorrectedDate) VALUES (0,0,0,0,0,0," + macid + ",'" + corrdate + "');";
        //                                MySqlDataAdapter lossup = new MySqlDataAdapter(inrt, conn1.msqlConnection);
        //                                lossup.Fill(insertt);
        //                                sa.close();
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            IntoFile(ex.ToString());
        //            // IntoFile("Mimics" + ex.ToString());
        //        }
        //        finally
        //        {
        //            con1.close();
        //        }
        //    }
        //}


        #region commented
        //private void MyTimer_Tick(object sender, EventArgs e)
        //{

        //    using (MsqlConnection con = new MsqlConnection())
        //    {
        //        try
        //        {
        //            con.open();
        //            DataTable dt = new DataTable();

        //            var machDetails = "SELECT MachineID FROM i_facility.unitworkccs_tblmachinedetailss as m WHERE m.IsDeleted = 0 and m.IsNormalWC = 0 ;";
        //            MySqlDataAdapter msda = new MySqlDataAdapter(machDetails, con.msqlConnection);
        //            msda.Fill(dt);
        //            con.close();

        //            for (int p = 0; p < dt.Rows.Count; p++)
        //            {
        //                int OperatingTimeV = 0, PowerOffTimeV = 0, BDTimeV = 0, LossTimeV = 0, SetupTimeV = 0, SetupMinorTimeV = 0, PowerOnTime = 0;
        //                DataTable dt1 = new DataTable();

        //                int macid = Convert.ToInt32(dt.Rows[p][0].ToString());
        //                DateTime nowDate = DateTime.Now;
        //                string corrdate = DateTime.Now.ToString("yyyy-MM-dd");
        //                if (nowDate.Hour < 7 && nowDate.Hour > 0)
        //                {
        //                    corrdate = nowDate.AddDays(-1).ToString("yyyy-MM-dd");
        //                }

        //                DataTable opdt = new DataTable();
        //                DataTable bddt = new DataTable();
        //                DataTable ltdt = new DataTable();
        //                DataTable stdt = new DataTable();
        //                DataTable smdt = new DataTable();
        //                var OperatingTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE, StartTime, EndTime)), 0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + " and ModeType = 'PROD';";
        //                var PowerOffTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE,StartTime,EndTime)),0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeType = 'POWEROFF' and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + ";";
        //                var BDTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE,StartTime,EndTime)),0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeTypeEnd = 1 and ModeType = 'MNT' and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + ";";
        //                var LossTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE,StartTime,EndTime)),0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeType = 'IDLE' and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + ";";
        //                var SetupTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE,StartTime,LossCodeEnteredTime)),0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeType = 'SETUP' and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + ";";
        //                var SetupMinorTime = "SELECT  IFNULL(SUM(timestampdiff(MINUTE, StartTime, EndTime)), 0) from i_facility.tblsetupmaint where IsCompleted = 1 and ModeID in(Select ModeID from i_facility.unitworkccs_tbllivemode where MachineID = " + macid + " and CorrectedDate = '" + corrdate + " ') and MachineID = " + macid + " ; ";

        //                con.open();
        //                MySqlDataAdapter initdet = new MySqlDataAdapter(OperatingTime, con.msqlConnection);
        //                initdet.Fill(opdt);
        //                PowerOffTimeV = Convert.ToInt32(opdt.Rows[0][0].ToString());
        //                con.close();

        //                con.open();
        //                MySqlDataAdapter initdet2 = new MySqlDataAdapter(BDTime, con.msqlConnection);
        //                initdet2.Fill(bddt);
        //                BDTimeV = Convert.ToInt32(bddt.Rows[0][0].ToString());
        //                con.close();

        //                con.open();
        //                MySqlDataAdapter initdet3 = new MySqlDataAdapter(LossTime, con.msqlConnection);
        //                initdet3.Fill(ltdt);
        //                LossTimeV = Convert.ToInt32(ltdt.Rows[0][0].ToString());
        //                con.close();

        //                con.open();
        //                MySqlDataAdapter initdet4 = new MySqlDataAdapter(SetupTime, con.msqlConnection);
        //                initdet4.Fill(stdt);
        //                SetupTimeV = Convert.ToInt32(stdt.Rows[0][0].ToString());
        //                con.close();

        //                con.open();
        //                MySqlDataAdapter initdet5 = new MySqlDataAdapter(SetupMinorTime, con.msqlConnection);
        //                initdet5.Fill(smdt);
        //                SetupMinorTimeV = Convert.ToInt32(smdt.Rows[0][0].ToString());
        //                PowerOnTime = OperatingTimeV + LossTimeV + SetupTimeV;
        //                con.close();



        //                con.open();
        //                var livemodedetails = "SELECT  MacMode from i_facility.unitworkccs_tbllivemode  where (IsCompleted = 0 or (IsCompleted = 1 and ModeTypeEnd = 0)) and CorrectedDate = '" + corrdate + " 'and MachineID = " + macid + " ;";
        //                MySqlDataAdapter msda1 = new MySqlDataAdapter(livemodedetails, con.msqlConnection);
        //                msda1.Fill(dt1);
        //                con.close();

        //                for (int i = 0; i < dt1.Rows.Count; i++)
        //                {
        //                    int count = 0;
        //                    string macname = dt1.Rows[i][0].ToString();
        //                    using (MsqlConnection conn1 = new MsqlConnection())
        //                    {
        //                        conn1.open();
        //                        DataTable dt2 = new DataTable();
        //                        var mimicsdetails = "SELECT Count(*) FROM i_facility.unitworkccs_tblmimicss where MachineID = " + macid + "  and CorrectedDate = '" + corrdate + "';";
        //                        MySqlDataAdapter msda2 = new MySqlDataAdapter(mimicsdetails, conn1.msqlConnection);
        //                        msda2.Fill(dt2);
        //                        conn1.close();

        //                        if (Convert.ToInt32(dt2.Rows[0][0].ToString()) > 0)
        //                        {
        //                            if (macname == "IDLE")
        //                            {
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();
        //                                    int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                    //var losstime = " SELECT SUM(timestampdiff(MINUTE, StartTime, EndTime)) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and CorrectedDate ='" + corrdate + "' and MachineID = " + macid + " and ModeType = 'IDLE'; ";
        //                                    //MySqlDataAdapter lossup = new MySqlDataAdapter(losstime, con.msqlConnection);
        //                                    //lossup.Fill(loss);
        //                                    //int lossval = LossTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    //DataTable up = new DataTable();


        //                                    var UpdateValue = "select SUM(timestampdiff(MINUTE,StartTime, current_timestamp()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    sa.close();
        //                                    int rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = LossTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = LossTimeV + 0;
        //                                    }
        //                                    if (updateval == 0)
        //                                    {
        //                                        PowerOnTime = PowerOnTime + LossTimeV;
        //                                    }
        //                                    else
        //                                    {
        //                                        PowerOnTime = PowerOnTime + (updateval - LossTimeV);
        //                                    }
        //                                    //using (MsqlConnection sa1 = new MsqlConnection())
        //                                    //{
        //                                    sa.open();
        //                                    var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime =" + OperatingTimeV + ", BreakdownTime =" + BDTimeV + " ,IdleTime=" + updateval + ", SetupTime = " + SetupTimeV + ", MachineOffTime = " + PowerOffTimeV + " where MachineID = " + macid + " and CorrectedDate = '" + corrdate + "'; ";
        //                                    MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa.msqlConnection);
        //                                    msda4.Fill(loss);
        //                                    sa.close();
        //                                    //}
        //                                }
        //                            }
        //                            else if (macname == "POWEROFF")
        //                            {
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                    ////var BTime = " SELECT SUM(timestampdiff(MINUTE,StartTime,EndTime)) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeTypeEnd = 1 and CorrectedDate ='" + corrdate + "' and MachineID = " + macid + " and ModeType = 'POWEROFF'; ";
        //                                    ////MySqlDataAdapter lossup = new MySqlDataAdapter(BTime, con.msqlConnection);
        //                                    ////lossup.Fill(loss);
        //                                    //int BDTimeval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    ////DataTable up = new DataTable();
        //                                    sa.open();
        //                                    var UpdateValue = "select SUM(timestampdiff(MINUTE,StartTime,current_timestamp()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    sa.close();
        //                                    int rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = BDTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = BDTimeV + 0;
        //                                    }

        //                                    updateval = updateval + PowerOffTimeV;
        //                                    //using (MsqlConnection sa1 = new MsqlConnection())
        //                                    {
        //                                        sa.open();
        //                                        var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime = " + OperatingTimeV + ", BreakdownTime =" + BDTimeV + ",IdleTime=" + LossTimeV + ", SetupTime = " + SetupTimeV + ", MachineOffTime = " + updateval + " where MachineID = " + macid + "  and CorrectedDate = '" + corrdate + "';";
        //                                        MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa.msqlConnection);
        //                                        msda4.Fill(loss);
        //                                        sa.close();
        //                                    }
        //                                }
        //                            }
        //                            else if (macname == "SETUP")
        //                            {
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();
        //                                    int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                    //var SetupTime = " select SUM(timestampdiff(MINUTE,StartTime,CURDATE()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = Cdate and MachineID = " + macid + " and MacMode='" + macname + "' ";
        //                                    //MySqlDataAdapter lossup = new MySqlDataAdapter(BDTime, con.msqlConnection);
        //                                    //lossup.Fill(loss);
        //                                    //int BDTimeval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    //DataTable up = new DataTable();
        //                                    var UpdateValue = "select SUM(timestampdiff(MINUTE,StartTime,current_timestamp()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    sa.close();
        //                                    int rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = SetupTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = SetupTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    if (updateval == 0)
        //                                    {
        //                                        PowerOnTime = PowerOnTime + SetupTimeV;
        //                                    }
        //                                    else
        //                                    {
        //                                        PowerOnTime = PowerOnTime + (updateval - SetupTimeV);
        //                                    }
        //                                    //using (MsqlConnection sa1 = new MsqlConnection())
        //                                    {
        //                                        sa.open();
        //                                        var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime = " + OperatingTimeV + ", BreakdownTime = " + BDTimeV + ",IdleTime=" + LossTimeV + ", SetupTime =" + updateval + ", MachineOffTime =" + PowerOffTimeV + " where MachineID = " + macid + "  and CorrectedDate = '" + corrdate + "'; ";
        //                                        MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa.msqlConnection);
        //                                        msda4.Fill(loss);
        //                                        sa.close();
        //                                    }
        //                                }
        //                            }
        //                            else if (macname == "PROD")
        //                            {
        //                                int updateval = 0;
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();

        //                                    DataTable loss = new DataTable();
        //                                    //var SetupTime = " select SUM(timestampdiff(MINUTE,StartTime,CURDATE()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = Cdate and MachineID = " + macid + " and MacMode='" + macname + "' ";
        //                                    //MySqlDataAdapter lossup = new MySqlDataAdapter(BDTime, con.msqlConnection);
        //                                    //lossup.Fill(loss);
        //                                    //int BDTimeval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    //DataTable up = new DataTable();
        //                                    var UpdateValue = "select SUM(timestampdiff(MINUTE,StartTime,current_timestamp()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    sa.close();
        //                                    int? rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = OperatingTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = OperatingTimeV + 0;
        //                                    }
        //                                    PowerOnTime = PowerOnTime + (updateval - OperatingTimeV);
        //                                    //using (MsqlConnection sa1 = new MsqlConnection())
        //                                    {
        //                                        sa.open();
        //                                        var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime = " + updateval + ", BreakdownTime =  " + BDTimeV + ",IdleTime=" + LossTimeV + ", SetupTime = " + SetupTimeV + ", MachineOffTime = " + PowerOffTimeV + " where MachineID = " + macid + " and CorrectedDate = '" + corrdate + "';";
        //                                        MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa.msqlConnection);
        //                                        msda4.Fill(loss);
        //                                        sa.close();
        //                                    }
        //                                }
        //                            }
        //                            else if (macname == "MNT")
        //                            {
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();
        //                                    int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                    //var SetupTime = " select SUM(timestampdiff(MINUTE,StartTime,CURDATE()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = Cdate and MachineID = " + macid + " and MacMode='" + macname + "' ";
        //                                    //MySqlDataAdapter lossup = new MySqlDataAdapter(BDTime, con.msqlConnection);
        //                                    //lossup.Fill(loss);
        //                                    //int BDTimeval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    //DataTable up = new DataTable();
        //                                    var UpdateValue = "select SUM(timestampdiff(MINUTE,StartTime,current_timestamp()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    sa.close();
        //                                    int rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = BDTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = BDTimeV + 0;
        //                                    }
        //                                    //using (MsqlConnection sa1 = new MsqlConnection())
        //                                    {
        //                                        sa.open();
        //                                        var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime = " + OperatingTimeV + ", BreakdownTime = " + updateval + ",IdleTime=" + LossTimeV + ", SetupTime = " + SetupTimeV + ", MachineOffTime =" + PowerOffTimeV + " where MachineID =  " + macid + " and CorrectedDate = '" + corrdate + "';";
        //                                        MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa.msqlConnection);
        //                                        msda4.Fill(loss);
        //                                        sa.close();
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            //using (MsqlConnection sa = new MsqlConnection())
        //                            {
        //                                conn1.open();
        //                                DataTable insertt = new DataTable();
        //                                var inrt = " INSERT INTO i_facility.unitworkccs_tblmimicss (MachineOnTime,OperatingTime,SetupTime,IdleTime,MachineOffTime,BreakdownTime,MachineID,CorrectedDate) VALUES (0,0,0,0,0,0," + macid + ",'" + corrdate + "');";
        //                                MySqlDataAdapter lossup = new MySqlDataAdapter(inrt, conn1.msqlConnection);
        //                                lossup.Fill(insertt);
        //                                conn1.close();
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            IntoFile(ex.ToString());
        //            // IntoFile("Mimics" + ex.ToString());
        //        }
        //        finally
        //        {
        //            con.close();
        //        }
        //    }
        //}
        #endregion
        #region Previous Code
        //private void MyTimer_Tick(object sender, EventArgs e)
        //{

        //    using (MsqlConnection con = new MsqlConnection())
        //    {
        //        try
        //        {
        //            con.open();
        //            DataTable dt = new DataTable();

        //            var machDetails = "SELECT MachineID FROM i_facility.unitworkccs_tblmachinedetailss as m WHERE m.IsDeleted = 0 and m.IsNormalWC = 0 ;";
        //            MySqlDataAdapter msda = new MySqlDataAdapter(machDetails, con.msqlConnection);
        //            msda.Fill(dt);

        //            for (int p = 0; p < dt.Rows.Count; p++)
        //            {
        //                int OperatingTimeV = 0, PowerOffTimeV = 0, BDTimeV = 0, LossTimeV = 0, SetupTimeV = 0, SetupMinorTimeV = 0, PowerOnTime = 0;
        //                DataTable dt1 = new DataTable();

        //                int macid = Convert.ToInt32(dt.Rows[p][0].ToString());
        //                DateTime nowDate = DateTime.Now;
        //                string corrdate = DateTime.Now.ToString("yyyy-MM-10");
        //                if (nowDate.Hour < 7 && nowDate.Hour > 0)
        //                {
        //                    corrdate = nowDate.AddDays(-1).ToString("yyyy-MM-dd");
        //                }
        //                using (MsqlConnection sa = new MsqlConnection())
        //                {
        //                    sa.open();
        //                    DataTable ini = new DataTable();
        //                    var OperatingTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE, StartTime, EndTime)), 0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + " and ModeType = 'PROD';";
        //                    var PowerOffTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE,StartTime,EndTime)),0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeType = 'POWEROFF' and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + ";";
        //                    var BDTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE,StartTime,EndTime)),0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeTypeEnd = 1 and ModeType = 'MNT' and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + ";";
        //                    var LossTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE,StartTime,EndTime)),0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeType = 'IDLE' and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + ";";
        //                    var SetupTime = " SELECT IFNULL(SUM(timestampdiff(MINUTE,StartTime,LossCodeEnteredTime)),0) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeType = 'SETUP' and CorrectedDate = '" + corrdate + " ' and MachineID = " + macid + ";";
        //                    var SetupMinorTime = "SELECT  IFNULL(SUM(timestampdiff(MINUTE, StartTime, EndTime)), 0) from i_facility.tblsetupmaint where IsCompleted = 1 and ModeID in(Select ModeID from i_facility.unitworkccs_tbllivemode where MachineID = " + macid + " and CorrectedDate = '" + corrdate + " ') and MachineID = " + macid + " ; ";



        //                    MySqlDataAdapter initdet = new MySqlDataAdapter(OperatingTime, sa.msqlConnection);
        //                    initdet.Fill(ini);


        //                    PowerOffTimeV = Convert.ToInt32(ini.Rows[1][1].ToString());
        //                    MySqlDataAdapter initdet2 = new MySqlDataAdapter(BDTime, sa.msqlConnection);
        //                    initdet2.Fill(ini);
        //                    BDTimeV = Convert.ToInt32(ini.Rows[2][1].ToString());
        //                    MySqlDataAdapter initdet3 = new MySqlDataAdapter(LossTime, sa.msqlConnection);
        //                    initdet3.Fill(ini);
        //                    LossTimeV = Convert.ToInt32(ini.Rows[3][1].ToString());
        //                    MySqlDataAdapter initdet4 = new MySqlDataAdapter(SetupTime, sa.msqlConnection);
        //                    initdet4.Fill(ini);
        //                    SetupTimeV = Convert.ToInt32(ini.Rows[4][2].ToString());
        //                    MySqlDataAdapter initdet5 = new MySqlDataAdapter(SetupMinorTime, sa.msqlConnection);
        //                    initdet5.Fill(ini);
        //                    SetupMinorTimeV = Convert.ToInt32(ini.Rows[5][0].ToString());
        //                    PowerOnTime = OperatingTimeV + LossTimeV + SetupTimeV;
        //                }
        //                using (MsqlConnection sa = new MsqlConnection())
        //                {
        //                    sa.open();
        //                    var livemodedetails = "SELECT  MacMode from i_facility.unitworkccs_tbllivemode  where (IsCompleted = 0 or (IsCompleted = 1 and ModeTypeEnd = 0)) and CorrectedDate = '" + corrdate + " 'and MachineID = " + macid + " ;";
        //                    MySqlDataAdapter msda1 = new MySqlDataAdapter(livemodedetails, sa.msqlConnection);
        //                    msda1.Fill(dt1);
        //                }
        //                for (int i = 0; i < dt1.Rows.Count; i++)
        //                {
        //                    int count = 0;
        //                    string macname = dt1.Rows[i][0].ToString();
        //                    using (MsqlConnection conn1 = new MsqlConnection())
        //                    {
        //                        conn1.open();
        //                        DataTable dt2 = new DataTable();
        //                        var mimicsdetails = "SELECT Count(*) FROM i_facility.unitworkccs_tblmimicss where MachineID = " + macid + "  and CorrectedDate = '" + corrdate + "';";
        //                        MySqlDataAdapter msda2 = new MySqlDataAdapter(mimicsdetails, conn1.msqlConnection);
        //                        msda2.Fill(dt2);

        //                        if (Convert.ToInt32(dt2.Rows[0][0].ToString()) > 0)
        //                        {
        //                            if (macname == "IDLE")
        //                            {
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();
        //                                    int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                    //var losstime = " SELECT SUM(timestampdiff(MINUTE, StartTime, EndTime)) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and CorrectedDate ='" + corrdate + "' and MachineID = " + macid + " and ModeType = 'IDLE'; ";
        //                                    //MySqlDataAdapter lossup = new MySqlDataAdapter(losstime, con.msqlConnection);
        //                                    //lossup.Fill(loss);
        //                                    //int lossval = LossTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    //DataTable up = new DataTable();


        //                                    var UpdateValue = "select SUM(timestampdiff(MINUTE,StartTime, current_timestamp()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    int rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = LossTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = LossTimeV + 0;
        //                                    }
        //                                    if (updateval == 0)
        //                                    {
        //                                        PowerOnTime = PowerOnTime + LossTimeV;
        //                                    }
        //                                    else
        //                                    {
        //                                        PowerOnTime = PowerOnTime + (updateval - LossTimeV);
        //                                    }
        //                                    using (MsqlConnection sa1 = new MsqlConnection())
        //                                    {
        //                                        sa1.open();
        //                                        var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime =" + OperatingTimeV + ", BreakdownTime =" + BDTimeV + " ,IdleTime=" + updateval + ", SetupTime = " + SetupTimeV + ", MachineOffTime = " + PowerOffTimeV + " where MachineID = " + macid + " and CorrectedDate = '" + corrdate + "'; ";
        //                                        MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa1.msqlConnection);
        //                                        msda4.Fill(loss);
        //                                    }
        //                                }
        //                            }
        //                            else if (macname == "POWEROFF")
        //                            {
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();
        //                                    int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                    ////var BTime = " SELECT SUM(timestampdiff(MINUTE,StartTime,EndTime)) from i_facility.unitworkccs_tbllivemode where IsCompleted = 1 and ModeTypeEnd = 1 and CorrectedDate ='" + corrdate + "' and MachineID = " + macid + " and ModeType = 'POWEROFF'; ";
        //                                    ////MySqlDataAdapter lossup = new MySqlDataAdapter(BTime, con.msqlConnection);
        //                                    ////lossup.Fill(loss);
        //                                    //int BDTimeval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    ////DataTable up = new DataTable();

        //                                    var UpdateValue = "select SUM(timestampdiff(MINUTE,StartTime,current_timestamp()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    int rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = BDTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = BDTimeV + 0;
        //                                    }

        //                                    updateval = updateval + PowerOffTimeV;
        //                                    using (MsqlConnection sa1 = new MsqlConnection())
        //                                    {
        //                                        sa1.open();
        //                                        var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime = " + OperatingTimeV + ", BreakdownTime =" + BDTimeV + ",IdleTime=" + LossTimeV + ", SetupTime = " + SetupTimeV + ", MachineOffTime = " + updateval + " where MachineID = " + macid + "  and CorrectedDate = '" + corrdate + "';";
        //                                        MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa1.msqlConnection);
        //                                        msda4.Fill(loss);
        //                                    }
        //                                }
        //                            }
        //                            else if (macname == "SETUP")
        //                            {
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();
        //                                    int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                    //var SetupTime = " select SUM(timestampdiff(MINUTE,StartTime,CURDATE()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = Cdate and MachineID = " + macid + " and MacMode='" + macname + "' ";
        //                                    //MySqlDataAdapter lossup = new MySqlDataAdapter(BDTime, con.msqlConnection);
        //                                    //lossup.Fill(loss);
        //                                    //int BDTimeval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    //DataTable up = new DataTable();
        //                                    var UpdateValue = "select SUM(timestampdiff(MINUTE,StartTime,current_timestamp()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    int rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = SetupTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = SetupTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    if (updateval == 0)
        //                                    {
        //                                        PowerOnTime = PowerOnTime + SetupTimeV;
        //                                    }
        //                                    else
        //                                    {
        //                                        PowerOnTime = PowerOnTime + (updateval - SetupTimeV);
        //                                    }
        //                                    using (MsqlConnection sa1 = new MsqlConnection())
        //                                    {
        //                                        sa1.open();
        //                                        var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime = " + OperatingTimeV + ", BreakdownTime = " + BDTimeV + ",IdleTime=" + LossTimeV + ", SetupTime =" + updateval + ", MachineOffTime =" + PowerOffTimeV + " where MachineID = " + macid + "  and CorrectedDate = '" + corrdate + "'; ";
        //                                        MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa1.msqlConnection);
        //                                        msda4.Fill(loss);
        //                                    }
        //                                }
        //                            }
        //                            else if (macname == "PROD")
        //                            {
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();
        //                                    int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                    //var SetupTime = " select SUM(timestampdiff(MINUTE,StartTime,CURDATE()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = Cdate and MachineID = " + macid + " and MacMode='" + macname + "' ";
        //                                    //MySqlDataAdapter lossup = new MySqlDataAdapter(BDTime, con.msqlConnection);
        //                                    //lossup.Fill(loss);
        //                                    //int BDTimeval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    //DataTable up = new DataTable();
        //                                    var UpdateValue = "select SUM(timestampdiff(MINUTE,StartTime,current_timestamp()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    int? rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = OperatingTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = OperatingTimeV + 0;
        //                                    }
        //                                    PowerOnTime = PowerOnTime + (updateval - OperatingTimeV);
        //                                    using (MsqlConnection sa1 = new MsqlConnection())
        //                                    {
        //                                        sa1.open();
        //                                        var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime = " + updateval + ", BreakdownTime =  " + BDTimeV + ",IdleTime=" + LossTimeV + ", SetupTime = " + SetupTimeV + ", MachineOffTime = " + PowerOffTimeV + " where MachineID = " + macid + " and CorrectedDate = '" + corrdate + "';";
        //                                        MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa1.msqlConnection);
        //                                        msda4.Fill(loss);
        //                                    }
        //                                }
        //                            }
        //                            else if (macname == "MNT")
        //                            {
        //                                using (MsqlConnection sa = new MsqlConnection())
        //                                {
        //                                    sa.open();
        //                                    int updateval = 0;
        //                                    DataTable loss = new DataTable();
        //                                    //var SetupTime = " select SUM(timestampdiff(MINUTE,StartTime,CURDATE()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = Cdate and MachineID = " + macid + " and MacMode='" + macname + "' ";
        //                                    //MySqlDataAdapter lossup = new MySqlDataAdapter(BDTime, con.msqlConnection);
        //                                    //lossup.Fill(loss);
        //                                    //int BDTimeval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    //DataTable up = new DataTable();
        //                                    var UpdateValue = "select SUM(timestampdiff(MINUTE,StartTime,current_timestamp()))  from i_facility.unitworkccs_tbllivemode where IsCompleted = 0 and CorrectedDate = '" + corrdate + "' and MachineID = " + macid + " and MacMode = '" + macname + "';";
        //                                    MySqlDataAdapter msda3 = new MySqlDataAdapter(UpdateValue, sa.msqlConnection);
        //                                    msda3.Fill(loss);
        //                                    int rowval = Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    if (rowval != null)
        //                                    {
        //                                        updateval = BDTimeV + Convert.ToInt32(loss.Rows[0][0].ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        updateval = BDTimeV + 0;
        //                                    }
        //                                    using (MsqlConnection sa1 = new MsqlConnection())
        //                                    {
        //                                        sa1.open();
        //                                        var upmimics = "UPDATE i_facility.unitworkccs_tblmimicss SET OperatingTime = " + OperatingTimeV + ", BreakdownTime = " + updateval + ",IdleTime=" + LossTimeV + ", SetupTime = " + SetupTimeV + ", MachineOffTime =" + PowerOffTimeV + " where MachineID =  " + macid + " and CorrectedDate = '" + corrdate + "';";
        //                                        MySqlDataAdapter msda4 = new MySqlDataAdapter(upmimics, sa1.msqlConnection);
        //                                        msda4.Fill(loss);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            using (MsqlConnection sa = new MsqlConnection())
        //                            {
        //                                sa.open();
        //                                DataTable insertt = new DataTable();
        //                                var inrt = " INSERT INTO i_facility.unitworkccs_tblmimicss (MachineOnTime,OperatingTime,SetupTime,IdleTime,MachineOffTime,BreakdownTime,MachineID,CorrectedDate) VALUES (0,0,0,0,0,0," + macid + ",'" + corrdate + "');";
        //                                MySqlDataAdapter lossup = new MySqlDataAdapter(inrt, sa.msqlConnection);
        //                                lossup.Fill(insertt);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            IntoFile(ex.ToString());
        //            // IntoFile("Mimics" + ex.ToString());
        //        }
        //        finally
        //        {
        //            con.close();
        //        }
        //    }
        //}
        #endregion
        //private void MyTimer_Tick1(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        //MessageBox.Show("Getpartcuntcuttingtime In Timer");
        //        Getpartcuntcuttingtime();

        //    }
        //    catch (Exception ex)
        //    {
        //        //IntoFile("Getpartcuntcuttingtime" + ex.ToString());
        //    }


        //}
        //public void Parametermastercount(string correcteddate, int machineid, double ideltime)
        //{
        //    using (MsqlConnection con = new MsqlConnection())
        //    {
        //        //con.open();
        //        //DataTable dt = new DataTable();
        //        //var paramdetails = "SELECT * FROM i_facility.unitworkccs_parameters_master where CorrectedDate='" + correcteddate + "' and MachineId=" + machineid + " order by InsertedOn asc  ; ";
        //        //MySqlDataAdapter myd = new MySqlDataAdapter(paramdetails, con.msqlConnection);

        //        DateTime starttime = Convert.ToDateTime(correcteddate + " " + "06:00:00");
        //        DateTime endtime = starttime.AddDays(1);
        //        DateTime temp = Convert.ToDateTime(correcteddate + " " + "06:00:00");
        //        temp = temp.AddDays(1);
        //        if (endtime <= temp)
        //        {
        //            DateTime stime = Convert.ToDateTime(correcteddate + " " + "06:00:00");
        //            DateTime edtime = stime.AddHours(1);
        //            DataTable dt = new DataTable();
        //            con.open();
        //            //MessageBox.Show("before unitworkccs_parameters_master day wise");
        //            var paramdetails = "SELECT * FROM i_facility.unitworkccs_parameters_master where CorrectedDate='" + correcteddate + "' and InsertedOn>='" + starttime.ToString("yyyy-MM-dd HH:mm:ss") + "' and InsertedOn<='" + endtime.ToString("yyyy-MM-dd HH:mm:ss") + "' and MachineId=" + machineid + " order by InsertedOn asc  ; ";
        //            MySqlDataAdapter pam = new MySqlDataAdapter(paramdetails, con.msqlConnection);
        //            pam.Fill(dt);
        //            con.close();
        //            for (int f = 0; f < dt.Rows.Count; f++)
        //            {
        //                using (MsqlConnection sa = new MsqlConnection())
        //                {
        //                    sa.open();
        //                    DataTable dtp = new DataTable();
        //                    //MessageBox.Show("before unitworkccs_parameters_master hour wise");
        //                    var paramdetailsasc = "SELECT PartsTotal,CuttingTime,InsertedOn FROM i_facility.unitworkccs_parameters_master where CorrectedDate='" + correcteddate + "' and InsertedOn>='" + stime.ToString("yyyy-MM-dd HH:mm:ss") + "' and InsertedOn<='" + edtime.ToString("yyyy-MM-dd HH:mm:ss") + "' and MachineId=" + machineid + " order by InsertedOn asc; ";
        //                    pam = new MySqlDataAdapter(paramdetailsasc, sa.msqlConnection);
        //                    pam.Fill(dtp);
        //                    sa.close();
        //                    if (edtime <= temp)
        //                    {
        //                        for (int i = 0; i < dtp.Rows.Count; i++)
        //                        {
        //                            //using (MsqlConnection sa1 = new MsqlConnection())
        //                            {

        //                                double p = Convert.ToDouble(dtp.Rows[dtp.Rows.Count - 1][0].ToString());
        //                                double p1 = Convert.ToDouble(dtp.Rows[0][0].ToString());
        //                                double c = Convert.ToDouble(dtp.Rows[dtp.Rows.Count - 1][1].ToString());
        //                                double c1 = Convert.ToDouble(dtp.Rows[0][1].ToString());
        //                                double partsc = p - p1;
        //                                double cuttsc = c - c1;
        //                                //double duration = Convert.ToDateTime(DateTime.Now).Subtract(Convert.ToDateTime(stime)).Minutes;
        //                                if (ideltime == 0 || ideltime == null)
        //                                {
        //                                    ideltime = 1;
        //                                }
        //                                //double idle = Convert.ToUInt32(duration / ideltime);
        //                                double idle = ideltime;
        //                                DataTable dt3 = new DataTable();
        //                                sa.open();
        //                                var countpartscountandcutting = "SELECT count(*) from unitworkccs_tblpartsscountandcutting where MachineID=" + machineid + " and StartTime>='" + stime.ToString("yyyy-MM-dd HH:mm:ss") + "' and EndTime<='" + edtime.ToString("yyyy-MM-dd HH:mm:ss") + "' and Isdeleted=0 and  CorrectedDate='" + correcteddate + "';";
        //                                MySqlDataAdapter msc = new MySqlDataAdapter(countpartscountandcutting, sa.msqlConnection);
        //                                msc.Fill(dt3);
        //                                sa.close();
        //                                //MessageBox.Show("afterr count check");
        //                                if (Convert.ToInt32(dt3.Rows[0][0].ToString()) == 0)
        //                                {
        //                                    sa.open();
        //                                    DataTable dt1 = new DataTable();
        //                                    var parts = "SELECT Endtime FROM unitworkccs_tblpartsscountandcutting where CorrectedDate='" + correcteddate + "' and isdeleted=0 order by starttime desc;";
        //                                    MySqlDataAdapter ps = new MySqlDataAdapter(parts, sa.msqlConnection);
        //                                    ps.Fill(dt1);
        //                                    sa.close();
        //                                    //DateTime sttime = Convert.ToDateTime(dt1.Rows[0][0].ToString());
        //                                    //DateTime etime = sttime.AddHours(1);
        //                                    string sttime = stime.ToString("yyyy-MM-dd HH:mm:ss");
        //                                    string etime = edtime.ToString("yyyy-MM-dd HH:mm:ss");
        //                                    DateTime cremod11 = DateTime.Now;
        //                                    string cremod = cremod11.ToString("yyyy-MM-dd HH:MM:ss");
        //                                    //using (MsqlConnection con1 = new MsqlConnection())
        //                                    {
        //                                        sa.open();
        //                                        var partscountandcutting = "insert into i_facility.unitworkccs_tblpartsscountandcutting(MachineID, PartCount, CuttingTime,CorrectedDate,StartTime,EndTime, Isdeleted, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy,TargetQuantity) values(" + machineid + "," + partsc + "," + cuttsc + ",'" + correcteddate + "','" + sttime + "','" + etime + "',0,'" + cremod + "',1,'" + cremod + "',1," + idle + ");";
        //                                        // MessageBox.Show(partscountandcutting);
        //                                        MySqlDataAdapter ms = new MySqlDataAdapter(partscountandcutting, sa.msqlConnection);
        //                                        ms.Fill(dt1);
        //                                        sa.close();
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    using (MsqlConnection sa2 = new MsqlConnection())
        //                                    {
        //                                        sa2.open();
        //                                        DataTable dt4 = new DataTable();
        //                                        //MessageBox.Show("update");
        //                                        var updatepresent = "UPDATE i_facility.ssunitworkccs_tblpartsscountandcutting set PartCount=" + partsc + ", CuttingTime=" + cuttsc + ",TargetQuantity=" + idle + " where MachineID=" + machineid + " and StartTime>='" + stime + "' and EndTime<='" + edtime + "' ;";
        //                                        MySqlDataAdapter upc = new MySqlDataAdapter(updatepresent, sa2.msqlConnection);
        //                                        upc.Fill(dt4);
        //                                        sa2.close();
        //                                    }
        //                                }
        //                                //else
        //                                //{
        //                                //    string sttime = stime.ToString("yyyy-MM-dd HH:MM:ss");
        //                                //    string etime = edtime.ToString("yyyy-MM-dd HH:MM:ss");
        //                                //    DateTime cremod = DateTime.Now;
        //                                //    DataTable dt1 = new DataTable();
        //                                //    var partscountandcutting = "insert into unitworkccs_tblpartsscountandcutting(MachineID, PartCount, CuttingTime,CorrectedDate,StartTime,EndTime, Isdeleted, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy,TargetQuantity) values(" + machineid + "," + partsc + "," + cuttsc + ",'" + correcteddate + "','" + sttime + "','" + etime + "',0,'" + cremod + "',1,'" + cremod + "',1," + idle + ");";
        //                                //    MySqlDataAdapter ms = new MySqlDataAdapter(partscountandcutting, con.msqlConnection);
        //                                //    ms.Fill(dt1);
        //                                //}
        //                                //if (Convert.ToInt32(dt3.Rows[0][0].ToString()) > 0)
        //                                //{
        //                                //    DataTable dt4 = new DataTable();
        //                                //    var updatepresent = "UPDATE unitworkccs_tblpartsscountandcutting set MachineID=" + machineid + ", PartCount=" + partsc + ", CuttingTime=" + cuttsc + ",TargetQuantity=" + idle + " where MachineID=" + machineid + " ;";
        //                                //    MySqlDataAdapter upc = new MySqlDataAdapter(updatepresent, con.msqlConnection);
        //                                //    upc.Fill(dt4);
        //                                //}
        //                                //else
        //                                //{
        //                                //DataTable dt1 = new DataTable();
        //                                //var partscountandcutting = "insert into unitworkccs_tblpartsscountandcutting(MachineID, PartCount, CuttingTime,CorrectedDate,StartTime,EndTime, Isdeleted, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy,TargetQuantity) values(" + machineid + "," + partsc + "," + cuttsc + ",'" + correcteddate + "','" + correcteddate + " " + "06:00:00" + "','" + correcteddate + " " + "06:00:00" + "',0,'" + correcteddate + " " + "06:00:00" + "',1,'" + correcteddate + " " + "06:00:00" + "',1," + idle + ");";
        //                                //MySqlDataAdapter ms = new MySqlDataAdapter(partscountandcutting, con.msqlConnection);
        //                                //ms.Fill(dt1);
        //                                //}
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        break;
        //                    }
        //                    stime = edtime;
        //                    edtime = edtime.AddHours(1);
        //                }
        //            }
        //            con.close();
        //        }
        //    }
        //}

        //public void Getpartcuntcuttingtime()
        //{
        //    using (MsqlConnection con = new MsqlConnection())
        //    {
        //        //MessageBox.Show("Getpartcuntcuttingtime");
        //        DateTime corrti = DateTime.Now;
        //        //string correcteddate = "2019-01-29";
        //        string correcteddate = getcorrecteddate().ToString();
        //        //MessageBox.Show(correcteddate);
        //        con.open();
        //        DataTable dt = new DataTable();
        //        var cellDetails = "select cellid from tblcell where isdeleted=0;";
        //        MySqlDataAdapter csda = new MySqlDataAdapter(cellDetails, con.msqlConnection);
        //        csda.Fill(dt);

        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            using (MsqlConnection sa = new MsqlConnection())
        //            {
        //                sa.open();
        //                DataTable dt1 = new DataTable();
        //                int celliddet = Convert.ToInt32(dt.Rows[i][0].ToString());
        //                var psrtsdet = "SELECT MachineID,OperationNumber FROM i_facility.unitworkccs_tblmachinedetailss where cellid=" + celliddet + " and isdeleted=0 ;";
        //                MySqlDataAdapter msda = new MySqlDataAdapter(psrtsdet, sa.msqlConnection);
        //                msda.Fill(dt1);
        //                for (int j = 0; j < dt1.Rows.Count; j++)
        //                {
        //                    //DataTable dt2 = new DataTable();
        //                    int machid = Convert.ToInt32(dt1.Rows[j][0].ToString());
        //                    int operationdet = Convert.ToInt32(dt1.Rows[j][1].ToString());
        //                    // MessageBox.Show("Before IdleTime");
        //                    double idletime = Getidletime(correcteddate, machid, operationdet);
        //                    //MessageBox.Show(idletime.ToString());
        //                    con.close();
        //                    // MessageBox.Show("Before Parametermastercount");
        //                    Parametermastercount(correcteddate, machid, idletime);
        //                    //var wrkdetails = "SELECT  FGCode from i_facility.tblworkorderentry where OperationNo=" + operationdet + " and MachineId=" + machid + ";";
        //                    //MySqlDataAdapter wsda = new MySqlDataAdapter(wrkdetails, con.msqlConnection);
        //                    //wsda.Fill(dt2);
        //                    //for (int k = 0; k < dt2.Rows.Count; k++)
        //                    //{
        //                    //    var fgcodedet = dt2.Rows[k][0].ToString();
        //                    //    DataTable dt3 = new DataTable();
        //                    //    var partsdetails = "SELECT (60/IdealCycleTime) as IdealCycleTime from i_facility.unitworkccs_tblpartss where isdeleted=0 and FGCode=" + fgcodedet + " ; ";
        //                    //    MySqlDataAdapter psda = new MySqlDataAdapter(partsdetails, con.msqlConnection);
        //                    //    psda.Fill(dt3);
        //                    //    for (int l = 0; l < dt3.Rows.Count; l++)
        //                    //    {
        //                    //        double idletimedet = Convert.ToDouble(dt3.Rows[l][0]);
        //                    //        parametermastercount(correcteddate, machid, idletimedet);
        //                    //    }
        //                    //}

        //                }
        //            }
        //        }

        //    }
        //}

        //public double Getidletime(string correcteddate, int machid, int operationdet)
        //{
        //    double ideltime = 0;
        //    using (MsqlConnection con = new MsqlConnection())
        //    {
        //        DataTable dt2 = new DataTable();
        //        con.open();
        //        var wrkdetails = "SELECT  FGCode from i_facility.tblworkorderentry where OperationNo=" + operationdet + " and MachineId=" + machid + ";";
        //        MySqlDataAdapter wsda = new MySqlDataAdapter(wrkdetails, con.msqlConnection);
        //        wsda.Fill(dt2);
        //        for (int k = 0; k < dt2.Rows.Count; k++)
        //        {
        //            var fgcodedet = dt2.Rows[k][0].ToString();
        //            DataTable dt3 = new DataTable();
        //            var partsdetails = "SELECT (60/IdealCycleTime) as IdealCycleTime from i_facility.unitworkccs_tblpartss where isdeleted=0 and FGCode=" + fgcodedet + " ; ";
        //            MySqlDataAdapter psda = new MySqlDataAdapter(partsdetails, con.msqlConnection);
        //            psda.Fill(dt3);
        //            for (int l = 0; l < dt3.Rows.Count; l++)
        //            {
        //                double idletimedet = Convert.ToDouble(dt3.Rows[l][0]);
        //                ideltime = ideltime + idletimedet;
        //                //parametermastercount(correcteddate, machid, idletimedet);
        //            }
        //        }
        //    }
        //    return ideltime;
        //}

        #endregion

        public void IntoFile(string Msg)
        {
            string path1 = AppDomain.CurrentDomain.BaseDirectory;
            string appPath = Application.StartupPath + @"\MimicsLog_"+DateTime.Now.ToString("yyyy-MM-dd")+".txt";
            using (StreamWriter writer = new StreamWriter(appPath, true)) //true => Append Text
            {
                writer.WriteLine(System.DateTime.Now + ":  " + Msg);
            }
        }
    }
}
