using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.model
{
    public class Model1
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public string ProgramVision { get; set; } 
        public int Result { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public Model1() { }
        public Model1(int col, int row, string programVision, int result)
        {
            this.Col = col;
            this.Row = row;
            this.ProgramVision = programVision;
            this.Result = result;
        }
        public Model1(int col, int row)
        {

            this.Col = col;
            this.Row   = row;
            Index = row * col + col;
        }
        public Model1(int col, int row,string name)
        {
            this.Name = name;
            this.Col = col;
            this.Row = row;
            Index = row * col + col;
        }

        public Model1(int col, int row, string name,string programVision)
        {
            this.Name = name;
            this.Col = col;
            this.Row = row;
            this.ProgramVision = programVision;
            Index = row * col + col;
        }


        public TrayModel ToTrayModel()
        {
            var tray = new TrayModel
            {   Name=Name,
                Rows = Row,
                Cols = Col
            };

            for (int r = 0; r < Row; r++)
            {
                for (int c = 0; c < Col; c++)
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
