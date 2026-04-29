using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.model
{
    public class Model1
    {
        public int col { get; set; }
        public int row { get; set; } 
        public string programVision {  get; set; }
        public int result { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public Model1(int col, int row, string programVision, int result)
        {
            this.col = col;
            this.row = row;
            this.programVision = programVision;
            this.result = result;
        }
        public Model1(int col, int row)
        {

            this.col = col;
            this.row = row;
            Index = row * col + col;
        }
        public Model1(int col, int row,string name)
        {
            this.Name = name;
            this.col = col;
            this.row = row;
            Index = row * col + col;
        }

        public Model1(int col, int row, string name,string programVision)
        {
            this.Name = name;
            this.col = col;
            this.row = row;
            this.programVision = programVision;
            Index = row * col + col;
        }


        public TrayModel ToTrayModel()
        {
            var tray = new TrayModel
            {   Name=Name,
                Rows = row,
                Cols = col
            };

            for (int r = 0; r < row; r++)
            {
                for (int c = 0; c < col; c++)
                {
                    tray.Cells.Add(new Cell
                    {
                        Row = r,
                        Col = c
                    });
                }
            }

            return tray;
        }
    }
}
