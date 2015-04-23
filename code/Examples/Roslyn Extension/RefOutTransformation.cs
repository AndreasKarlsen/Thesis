
namespace AtomicExamples
{
    //Before
    public class AtomicRefExample
    {
        public static void TestMethodAtomic(atomic ref int i)
        {
            i = 12;
        }

        public static void TestMethod(ref int i)
        {
            i = 12;
        }

        public static void Main()
        {
            int i = 10;
            atomic int iAtomic = 10;

            TestMethodAtomic(ref i);
            TestMethodAtomic(ref iAtomic);
            TestMethod(ref iAtomic);
        }
    }

    //After

}