namespace KZ.Sim
{
    public sealed class Territory
    {
        readonly byte[] owner = new byte[64];

        /// <summary>TEST-ONLY: only a test ever draws a border.</summary>
        public void SetVerticalBorder(int x, byte left) { owner[x] = left; }

        public byte OwnerAt(int x) { return owner[x]; }
    }
}
