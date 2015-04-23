
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
    public class AtomicRefExample
    {
        public static void TestMethodAtomic(ref STM.Implementation.Lockbased.TMInt i)
        {
            i.Value = 12;
        }

        public static void TestMethod(ref int i)
        {
            i = 12;
        }

        public static void Main()
        {
            int i = 10;
            STM.Implementation.Lockbased.TMInt iAtomic = new STM.Implementation.Lockbased.TMInt(10);

            STM.Implementation.Lockbased.TMInt _9cbf811d39af4934ae5aebae2963d63f = new STM.Implementation.Lockbased.TMInt(i);
            TestMethodAtomic(ref _9cbf811d39af4934ae5aebae2963d63f);
            i = _9cbf811d39af4934ae5aebae2963d63f.Value;

            STM.Implementation.Lockbased.TMInt _f3a7fd49d20c41c68ec2843c58e65f19 = new STM.Implementation.Lockbased.TMInt(iAtomic.Value);
            TestMethodAtomic(ref _f3a7fd49d20c41c68ec2843c58e65f19);
            iAtomic.Value = _f3a7fd49d20c41c68ec2843c58e65f19.Value;

            int _b44e0eeb5aee49669db8188cd5b500b3 = iAtomic.Value;
            TestMethod(ref _b44e0eeb5aee49669db8188cd5b500b3);
            iAtomic.Value = _b44e0eeb5aee49669db8188cd5b500b3;
        }
    }

}