using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRKSDAQFanucToolLife
{
    public class MachineStatusModel
    {
        public int MachineID { get; set; }
        public string MachineStatus { get; set; }
        public string MachineEmergency { get; set; }
        public string MachineAlarm { get; set; }
        public string CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public string CorrectedDate { get; set; }
    }
}
