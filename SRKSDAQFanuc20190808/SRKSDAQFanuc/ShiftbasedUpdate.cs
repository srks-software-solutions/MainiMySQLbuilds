using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRKSDAQFanuc
{
   public class ShiftbasedUpdate
    {
        public int MachineID { get; set; }
        public int ISFirstShift { get; set; }
        public int ISSecondShift { get; set; }
        public int ISThirdShift { get; set; }
    }
}
