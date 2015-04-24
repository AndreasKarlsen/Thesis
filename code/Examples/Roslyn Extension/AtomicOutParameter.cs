
namespace AtomicExamples
{
    //Before
    public class AtomicOutExample
    {
        public static void TestMethodAtomic(atomic out int i, int j)
        {
            i = 12;
            j = 12;
        }
    }

    //After
    public class AtomicOutExample
    {
        public static void TestMethodAtomic(out STM.Implementation.Lockbased.TMInt i, int j)
        {
            i = new STM.Implementation.Lockbased.TMInt();
            i.Value = 12;
            j = 12;
        }
    }

}