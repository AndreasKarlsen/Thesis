
namespace AtomicExamples
{
    //Before
    public class AtomicExample
    {
        public atomic AtomicExample aField;

        public AtomicExample ExampleMethod(atomic int i)
        {
            atomic int k = 0;
            k = i;

            return aField.aField;
        }
    }

    //After
    public class AtomicExample
    {
        public TMVar<AtomicExample> aField = new TMVar<AtomicExample>();

        public AtomicExample ExampleMethod(TMInt i)
        {
            TMInt k = new TMInt(0);
            k.Value = i.Value;

            return aField.Value.aField.Value;
        }
    }
}