using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.model
{
    public class SummaryResults
    {
                public string Model { get; set; }
                public int Total { get; set; }
                public int Ok { get; set; }
                public int Ng { get; set; }
                public int None { get; set; }
                 public double OkRate { get; set; } // %
    }
}
