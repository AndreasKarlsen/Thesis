public class LexicalNesting
{
    private atomic static int X = 0;
    private atomic static int Y = 0;
    private atomic static int Z = 0;

    public void Main(string[] args)
    {
        if (args.Length != 2)
            return;

        int tmpx;
        int tmpy;
        if (int.TryParse(args[0],out tmpx) 
            || !int.TryParse(args[1],out tmpy))
            return;

        atomic
        {
            atomic
            {
                X = tmpx;
                Y = tmpy;
            }

            Z = X * Y;
        }

        System.Console.WriteLine(Z);
    }
}