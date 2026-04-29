using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.model
{
    public class TrayModel
    {
        public string Name { get; set; }
        public int Rows { get; set; }
        public int Cols { get; set; }


        public List<Cell> Cells { get; set; } = new List<Cell>();

        // chỉ lấy cell hợp lệ (quan trọng)
        public List<Cell> ValidCells => Cells.Where(c => c.IsValid).ToList();
    }
}
