using System.Collections;

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    public static class BitArrayExtensions
    {
        /// <summary>
        ///  Return the number of true values in the BitArray.
        /// </summary>
        /// <param name="array">BitArray to count</param>
        /// <returns>Number of elements set to True</returns>
        public static int CountTrue(this BitArray array)
        {
            int count = 0;

            for (int i = 0; i < array.Count; ++i)
            {
                if (array.Get(i)) { count++; }
            }

            return count;
        }
    }
}
