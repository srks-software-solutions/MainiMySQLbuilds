using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRKSDAQFanucToolLife
{
   public class AxisDetails1
    {
        public int ADID { get; set; }
        public int MachineID { get; set; }
        public string Axis { get; set; }
        public decimal AbsPos { get; set; }
        public decimal RelPos { get; set; }
        public decimal MacPos { get; set; }
        public decimal DistPos { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int IsDeleted { get; set; }
        public string InsertedOn { get; set; }
        public int Axisdet1Month { get; set; }
        public int Axisdet1Year { get; set; }
        public int Axisdet1WeekNumber { get; set; }
        public int Axisdet1Quarter { get; set; }
        public string CorrectedDate { get; set; }

        

    }
}
