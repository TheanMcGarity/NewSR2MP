using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Data
{
    public class NetUtility
    {
        /// <summary>
		/// Returns how many bits are necessary to hold a certain number
		/// </summary>
        public static int BitsToHoldUInt(uint value)
        {
            int bits = 1;
            while ((value >>= 1) != 0)
                bits++;
            return bits;
        }

        /// <summary>
        /// Returns how many bits are necessary to hold a certain number
        /// </summary>
        public static int BitsToHoldUInt64(ulong value)
        {
            int bits = 1;
            while ((value >>= 1) != 0)
                bits++;
            return bits;
        }

        /// <summary>
		/// Returns how many bytes are required to hold a certain number of bits
		/// </summary>
		public static int BytesToHoldBits(int numBits)
        {
            return (numBits + 7) / 8;
        }

        // shell sort
        internal static void SortMembersList(System.Reflection.MemberInfo[] list)
        {
            int h;
            int j;
            System.Reflection.MemberInfo tmp;

            h = 1;
            while (h * 3 + 1 <= list.Length)
                h = 3 * h + 1;

            while (h > 0)
            {
                for (int i = h - 1; i < list.Length; i++)
                {
                    tmp = list[i];
                    j = i;
                    while (true)
                    {
                        if (j >= h)
                        {
                            if (string.Compare(list[j - h].Name, tmp.Name, StringComparison.InvariantCulture) > 0)
                            {
                                list[j] = list[j - h];
                                j -= h;
                            }
                            else
                                break;
                        }
                        else
                            break;
                    }

                    list[j] = tmp;
                }
                h /= 3;
            }
        }
    }
}
