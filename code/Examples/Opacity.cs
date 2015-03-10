using System;
using System.Threading;

public class Opacity
{
    private static int X = 10;
    private static int Y = 10;

    public static int Main(string[] args)
    {
        var t1 = new Thread(() =>
        {
            X = 20;
            Y = 20;
        });

        var t2 = new Thread(() =>
        {
            int tmpx = X;
            int tmpy = Y;
            while (tmpx != tmpy)
            { }
        });

        t1.Start();
        t2.Start();
    }
}