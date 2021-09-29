using System;
using System.Collections.Generic;
using System.Windows.Media;

public  class MyColors
{
    private readonly Random Random = new Random();

    public  List<Color> Cell { get; private set; }
    public  Color Red { get; internal set; }

    public  void InitializeCellColors(int count)
    {
        Cell = new List<Color>();

        while (Cell.Count < count)
        {
            byte r = Convert.ToByte(Random.Next(256));
            byte g = Convert.ToByte(Random.Next(256));
            byte b = Convert.ToByte(Random.Next(256));
            Color color = Color.FromRgb(r, g, b);

            if (!Cell.Contains(color))
            {
                Cell.Add(color);
            }
        }
    }
}

