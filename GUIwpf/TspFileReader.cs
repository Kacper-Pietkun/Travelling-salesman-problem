using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIwpf
{
    public class TspFileReader
    {
        public List<PointF> ReadFile(string path)
        {
            List<PointF> result = new List<PointF>();
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] splits = line.Split(' ', 3);
                try
                {
                    int index = Int32.Parse(splits[0]);
                    float x = float.Parse(splits[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(splits[2], CultureInfo.InvariantCulture);
                    result.Add(new PointF(x, y));
                }
                catch { }
            }
            return result;
        }
    }
}
