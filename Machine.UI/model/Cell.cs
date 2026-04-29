using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.model
{
    public class Cell
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Index { get; set; }

        public bool IsValid { get; set; } = true; // có slot hay không

        public string Result { get; set; } = ""; // OK / NG / EMPTY
    }
}
