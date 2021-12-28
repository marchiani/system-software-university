using System.Collections;

namespace SystemSoftwareUniversity.Extensions
{
    public static class BitArrayExtensions
    {
        public static short GetIndexOfFirst(this BitArray bitArray, bool element)
        {
            for (short i = 0; i < bitArray.Count; i++)
            {
                if (bitArray[i] == element)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}