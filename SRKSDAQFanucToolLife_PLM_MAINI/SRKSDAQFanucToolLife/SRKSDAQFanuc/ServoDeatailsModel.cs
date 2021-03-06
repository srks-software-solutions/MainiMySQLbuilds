using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRKSDAQFanucToolLife
{
   public class ServoDeatailsModel
    {
        public int MachineID { get; set; }
        public string ServoAxis { get; set; }
        public string ServoLoadMeter { get; set; }
        public string LoadCurrent { get; set; }
        public string LoadCurrentAmp { get; set; }
        public string StartDateTime { get; set; }
        public int IsDeleted { get; set; }
        public string InsertedOn { get; set; }
        public int Insertedby { get; set; }
        public int ServoTemperature { get; set; }
        public int ServoCoolingFanSpeed { get; set; }
        public string CorrectedDate { get; set; }
        public int DCLinkVoltage { get; set; }

    }
}
