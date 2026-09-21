// KILL ZONE - a real-time strategy video game.
// Test entry point.

using System;

namespace KZ.Tests
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("KILL ZONE - simulation tests");
            Console.WriteLine(new string('-', 60));

            TestRunner runner = new TestRunner();
            MathTests.Register(runner);
            SimTests.Register(runner);
            ScenarioTests.Register(runner);
            DeterminismTests.Register(runner);
            return runner.Report();
        }
    }
}
