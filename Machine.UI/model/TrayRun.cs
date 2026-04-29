using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.model
{
    public class TrayRun
    {
        public int Id { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string TrayName { get; set; }
        public List<VisionData> VisionDatas { get; set; }
    }
}
