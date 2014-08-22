using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RollingChecksums
{
    /// <summary>
    /// Rolling Checksum algorithm Adler32 based on Fletcher checksum algorithm.
    /// <see cref="!:http://en.wikipedia.org/wiki/Adler-32"/>
    /// </summary>
    /// <remarks>
    /// It should be faster than CRC32 algorithm (according to wikipedia).
    /// </remarks>
    public class Adler32 : IRollingChecksum
    {
        /// <summary>
        /// Highest prime that is lower than 65535 (2^16).
        /// </summary>
        const int PRIME = 65521;

        public uint Checksum { get; protected set; }
        public int Window { get; protected set; }
        public int ChecksumWindowSize { get; set; }

        /// <summary>
        /// Cyclic buffer that has Window length.
        /// </summary>
        int[] buffer;
        int bufferptr;

        bool wasFilled;

        int nextChecksumCountdown;

        public Adler32()
        {
            Checksum = 0;
        }

        public uint Fill(byte[] data, int windowSize = -1)
        {
            Window = windowSize > 0 ? windowSize : data.Length;

            buffer = new int[Window];

            uint a = 1;
            uint b = 0;

            for (int i = 0; i < data.Length; i++)
            {
                a = (a + data[i]) % PRIME;
                b = (b + a) % PRIME;

                buffer[bufferptr++] = data[i];
                bufferptr %= buffer.Length;
            }
            composeChecksum(a, b);

            wasFilled = true;
            resetWindowCountdown();

            return Checksum;
        }

        public IEnumerable<uint> Roll(byte[] data, int checksumWindowSize = -1)
        {
            ChecksumWindowSize = checksumWindowSize > 0 ? checksumWindowSize : Window;

            for (int i = 0; i < data.Length; i++)
            {
                Roll(data[i]);
                
                if (nextChecksumCountdown == 0)
                {
                    resetWindowCountdown();
                    yield return Checksum;
                }
            }
        }

        public uint Roll(int data)
        {
            if (!wasFilled)
                throw new InvalidOperationException("Please use Fill method before Rolling checksum any further");

            uint a = Checksum & 0xFFFF;
            uint b = (Checksum >> 16) & 0xFFFF;

            int oldest = getOldestValue();

            a = (uint)(a + PRIME - oldest + data) % PRIME;
            b = (uint)(b + PRIME - (oldest * Window) % PRIME + a - 1) % PRIME;

            composeChecksum(a, b);

            buffer[bufferptr++] = data;
            bufferptr %= buffer.Length;

            nextChecksumCountdown--;

            return Checksum;
        }

        private int getOldestValue()
        {
            return buffer[(bufferptr + Window) % Window];
        }

        private void resetWindowCountdown(){
            nextChecksumCountdown = ChecksumWindowSize;
        }

        /// <summary>
        /// Composes checksum from both parts.
        /// </summary>
        /// <param name="a">Value for the lower 16 bits of the checksum.</param>
        /// <param name="b">Value for the higer 16 bits of the checksum.</param>
        /// <returns>Composed checksum.</returns>
        private uint composeChecksum(uint a, uint b) {
            return Checksum = (b * 65536) | a;
        }
    }
}
