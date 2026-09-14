// KILL ZONE - a real-time strategy video game.
// A small test harness. No external dependency, so the simulation can be tested
// anywhere a C# compiler exists, including on a build machine with nothing else
// installed.

using System;
using System.Collections.Generic;

namespace KZ.Tests
{
    public sealed class TestFailure : Exception
    {
        public TestFailure(string message) : base(message) { }
    }

    public static class Assert
    {
        public static void True(bool condition, string what)
        {
            if (!condition) throw new TestFailure("expected true: " + what);
        }

        public static void False(bool condition, string what)
        {
            if (condition) throw new TestFailure("expected false: " + what);
        }

        public static void Equal(long expected, long actual, string what)
        {
            if (expected != actual)
                throw new TestFailure(what + ": expected " + expected + ", got " + actual);
        }

        public static void Equal(string expected, string actual, string what)
        {
            if (expected != actual)
                throw new TestFailure(what + ": expected '" + expected + "', got '" + actual + "'");
        }

        /// <summary>Compare two fixed-point values to within a tolerance.</summary>
        public static void Near(double expected, double actual, double tolerance, string what)
        {
            double d = expected - actual;
            if (d < 0) d = -d;
            if (d > tolerance)
                throw new TestFailure(what + ": expected " + expected.ToString("0.######")
                    + " +/- " + tolerance + ", got " + actual.ToString("0.######"));
        }

        public static void InRange(double low, double high, double actual, string what)
        {
            if (actual < low || actual > high)
                throw new TestFailure(what + ": expected between " + low + " and " + high
                    + ", got " + actual.ToString("0.######"));
        }
    }

    public sealed class TestRunner
    {
        readonly List<string> failures = new List<string>();
        int passed;
        string currentGroup = "";

        public void Group(string name)
        {
            currentGroup = name;
            Console.WriteLine();
            Console.WriteLine("  " + name);
        }

        public void Run(string name, Action test)
        {
            try
            {
                test();
                passed++;
                Console.WriteLine("    pass  " + name);
            }
            catch (TestFailure f)
            {
                failures.Add(currentGroup + " / " + name + ": " + f.Message);
                Console.WriteLine("    FAIL  " + name + "  -  " + f.Message);
            }
            catch (Exception e)
            {
                failures.Add(currentGroup + " / " + name + ": " + e.GetType().Name + " " + e.Message);
                Console.WriteLine("    ERROR " + name + "  -  " + e.GetType().Name + ": " + e.Message);
            }
        }

        public int Report()
        {
            Console.WriteLine();
            Console.WriteLine(new string('-', 60));
            if (failures.Count == 0)
            {
                Console.WriteLine(passed + " passed, 0 failed.");
                return 0;
            }
            Console.WriteLine(passed + " passed, " + failures.Count + " FAILED:");
            for (int i = 0; i < failures.Count; i++) Console.WriteLine("  " + failures[i]);
            return 1;
        }
    }
}
