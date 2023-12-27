using System;

namespace Grondslag
{
    public static class LsRandom
    {
        //public static byte[] seed1 = { 5, 7, 3, 1, 9, 2, 8, 4, 6, 1, 3, 7, 9, 2, 5, 8, 4, 6, 1, 9, 3, 2, 7, 5, 8, 4, 6, 9, 1, 3, 2, 7 }; // 32 numbers, deliberately without 0
        //public static byte[] seed2 = { 8, 3, 2, 5, 7, 1, 4, 9, 6, 2, 7, 4, 5, 1, 8, 6, 3, 9, 2, 4, 7, 1, 8, 6, 3, 5, 9, 2, 4, 1, 7, 8 }; // 32 numbers, deliberately without 0
        //public static byte[] seed3 = { 2, 6, 8, 1, 4, 9, 3, 7, 5, 1, 8, 6, 3, 5, 9, 4, 2, 7, 1, 8, 6, 3, 5, 9, 4, 2, 7, 1, 8, 6, 3, 5 }; // 32 numbers, deliberately without 0

        //private static int _index1 = 0;
        //private static int _index2 = 0;
        //private static int _index3 = 0;

        //public static int GenerateInt(int lowest, int highest)
        //{
        //    int rand = seed1[_index1] * seed2[_index2] % 10;
        //    rand = seed1[_index1];
        //    Debug.WriteLine(rand);

        //    rand = (int)Math.Round(lowest + rand * 0.1f * (highest - lowest));
        //    //int rand = (int)Math.Round(lowest + seed[_index] * 0.1f * (highest - lowest));

        //    //Debug.WriteLine(seed1[_index1] + ", " + seed2[_index2]);


        //    _index1 = (_index1 + seed2[_index2]) % seed1.Length;
        //    _index2 = (_index2 + 1) % seed2.Length; // 13 is used because each time it goes through the array it is offset by -1.
        //    //_index3 = (_index3 - 1 + seed3.Length) % seed3.Length;

        //    return rand;
        //}

        private static int seed = 90724629;
        public static float number = seed; // float for calculation purposes

        public static int q = 73; // first constant
        public static int r = 999999937; // second constant - must be prime

        public static int Int(int lowest, int highest)
        {
            number = (number * q) % r; // After running, 0 <= number < 13

            int value = (int)(lowest + (number / r) * (highest + 1 - lowest));
            
            return value;
        }
    }
}
