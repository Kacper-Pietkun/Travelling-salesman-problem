using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TspAlgorithms
{
    public class TspFileReader
    {
        public List<(int, PointF)> ReadFile(string path)
        {
            List<(int, PointF)> result = new List<(int, PointF)>();
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] splits = line.Split(' ', 3);
                try
                {
                    int index = Int32.Parse(splits[0]);
                    float x = float.Parse(splits[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(splits[2], CultureInfo.InvariantCulture);
                    result.Add((index, new PointF(x, y)));
                }
                catch { }
            }
            return result;
        }
    }
}
