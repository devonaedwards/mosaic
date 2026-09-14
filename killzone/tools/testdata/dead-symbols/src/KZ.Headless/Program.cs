namespace KZ.Headless
{
    using KZ.Sim;

    static class Program
    {
        static void Main()
        {
            World w = new World();
            w.Spawn(0, 0);
            w.Step();
        }
    }
}
