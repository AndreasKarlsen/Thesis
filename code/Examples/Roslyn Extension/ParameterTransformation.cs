
namespace AtomicExamples
{
    //Before
    public class AtomicParameterExample
    {
        public void TestMethod(atomic int x, atomic ref string s)
        {
            //Body
        }
    }

    //After
    public class AtomicParameterExample
    {
        public void TestMethod(STM.Implementation.Lockbased.TMInt x, ref STM.Implementation.Lockbased.TMVar<string> s)
        {
            //Body
        }
    }

}