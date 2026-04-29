using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.model
{
    public class VisionData
    {
        public int Id { get; set; }
        public int TrayId { get; set; } // id trayrun
        public int Row { get; set; }
        public int Col { get; set; }
        public int Result { get; set; }
        public DateTime CreatedAt { get; set; }
        public TrayRun TrayRun { get; set; }
    }
}
