using System.Collections.Generic;

namespace PCAB_Debugger_ACS
{
    public static class clsSearchList
    {
        /// <summary>
        /// Binary search with the sunday algorithmを行います。検索先 byte 配列の先頭から検索を行います。
        /// </summary>
        /// <param name="target">Target list</param>
        /// <param name="pattern">Search vlue byte array</param>
        /// <returns>Match position (-1 if not found)</returns>
        public static int SearchBytesSunday(List<byte> target, byte[] pattern)
        {
            return SearchBytesSunday(target, pattern, 0);
        }


        /// <summary>
        /// Binary search with the sunday algorithmを行います。検索先 byte 配列の先頭から検索を行います。
        /// </summary>
        /// <param name="target">Target list</param>
        /// <param name="pattern">Search vlue byte array</param>
        /// <param name="index">Start position</param>
        /// <returns>Match position (-1 if not found)</returns>
        public static int SearchBytesSunday(List<byte> target, byte[] pattern, int index)
        {
            int patLen = pattern.Length;
            int txtLen = target.Count;

            if (txtLen < patLen) return -1;
            txtLen -= patLen;

            var bmTable = MakeSundayTable(pattern);

            int patIdx = 0;
            while (index <= txtLen)
            {
                patIdx = patLen; // search position
                for (; patIdx > 0; --patIdx)
                {
                    if (target[index + patIdx - 1] != pattern[patIdx - 1])
                    {
                        break;
                    }
                }

                if (patIdx == 0)
                {   // All matched.
                    return index;
                }

                if (index == txtLen)
                {   // not found.
                    break;
                }

                // テーブルから移動量を求める
                if (bmTable.ContainsKey(target[index + patLen]))
                {
                    index += bmTable[target[index + patLen]];
                }
                else
                {
                    index += patLen + 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Create shift table.
        /// </summary>
        /// <param name="pattern">Search vlue byte array</param>
        /// <returns>A dictionary where keys are bytes and values ​​are shift.</returns>
        private static Dictionary<byte, int> MakeSundayTable(byte[] pattern)
        {
            var result = new Dictionary<byte, int>();
            var counter = pattern.Length;
            var i = 0;
            while (counter > 0)
            {
                result[pattern[i++]] = counter--;
            }
            return result;
        }
    }
}
