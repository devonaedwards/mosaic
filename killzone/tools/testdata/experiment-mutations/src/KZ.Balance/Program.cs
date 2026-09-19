// The self-test's fixture balance source. Not compiled by anything - it exists
// so the claim reader and the dispatch reader have a file of the right shape to
// read, including an experiment that declares nothing.

namespace KZ.Balance
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            string which = args.Length > 0 ? args[0] : "all";
            if (which == "all" || which == "one") OneExperiment();
            if (which == "all" || which == "two") TwoExperiment();
            if (which == "all" || which == "three") ThreeExperiment();
            return 0;
        }

        // MEASURES one: alpha
        static void OneExperiment() { }

        // MEASURES two: alpha
        static void TwoExperiment() { }

        // No MEASURES line, deliberately.
        static void ThreeExperiment() { }
    }
}
