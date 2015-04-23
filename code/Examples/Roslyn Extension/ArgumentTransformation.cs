
namespace AtomicExamples
{
    //Before
    public class AtomicArgumentExample
    {
        public static void TestMethod(atomic int x, int y)
        {
            //Body
        }

        public static void Main()
        {
            TestMethod(1, 2);
        }
    }

    //After
    public class AtomicArgumentExample
    {
        public static void TestMethod(STM.Implementation.Lockbased.TMInt x, int y)
        {
            //Body
        }

        public static void Main()
        {
            TestMethod(newSTM.Implementation.Lockbased.TMInt(1), 2);
        }
    }

}