using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRKSDAQFanucToolLife
{
   public class AxisDetails2
    {
        public int AD2ID { get; set; }
        public int MachineID { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string FeedRate { get; set; }
        public string SpindleLoad { get; set; }
        public string SpindleSpeed { get; set; }
        public int IsDeleted { get; set; }
        public string InsertedOn { get; set; }      
        public string CorrectedDate { get; set; }
        public int SpindleTemperature { get; set; }
        public int AxisNo { get; set; }
        public string FeedRateUnit { get; set; }
    }
}
