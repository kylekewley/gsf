/**************************************************************************\
   Copyright � 2009 - Gbtc, James Ritchie Carroll
   All rights reserved.
  
   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:
  
      * Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
       
      * Redistributions in binary form must reproduce the above
        copyright notice, this list of conditions and the following
        disclaimer in the documentation and/or other materials provided
        with the distribution.
  
   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY
   EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
   IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
   PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
   OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
   OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  
\**************************************************************************/

using System.Runtime.Serialization;
using System.Units;

namespace System
{
    /// <summary>
    /// Standard Network Time Protocol Timetag.
    /// </summary>
    /// <remarks>
    /// As recommended by RFC-2030, all NTP timestamps earlier than 3h 14m 08s UTC on 20 January 1968
    /// are reckoned from 6h 28m 16s UTC on 7 February 2036. This gives the <see cref="NtpTimeTag"/>
    /// class a functioning range of 1968-01-20 03:14:08 to 2104-02-26 09:42:23.
    /// </remarks>
    public class NtpTimeTag : TimeTagBase
    {
        // NTP dates are measured as the number of seconds since 1/1/1900, so we calculate this date to
        // get offset in ticks for later conversion.
        private static long NtpDateOffsetTicks = (new DateTime(1900, 1, 1, 0, 0, 0)).Ticks;

        // According to RFC-2030, NTP dates can also be measured as the number of seconds since 2/7/2036
        // at 6h 28m 16s UTC if MSB is set, so we also calculate this date to get offset in ticks for
        // later conversion as well.
        private static long NtpDateOffsetTicksAlt = (new DateTime(2036, 2, 7, 6, 28, 16)).Ticks;

        /// <summary>
        /// Creates a new <see cref="NtpTimeTag"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected NtpTimeTag(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>Creates a new <see cref="NtpTimeTag"/>, given number of seconds since 1/1/1900.</summary>
        /// <param name="seconds">Number of seconds since 1/1/1900.</param>
        public NtpTimeTag(double seconds)
            : base(GetBaseDateOffsetTicks(seconds), seconds)
        {
        }

        /// <summary>Creates a new <see cref="NtpTimeTag"/>, given number of seconds and fractional seconds since 1/1/1900.</summary>
        /// <param name="seconds">Number of seconds since 1/1/1900.</param>
        /// <param name="fraction">Number of fractional seconds, in whole picoseconds.</param>
        [CLSCompliant(false)]
        public NtpTimeTag(uint seconds, uint fraction)
            : base(GetBaseDateOffsetTicks(seconds), seconds + (fraction / (double)uint.MaxValue))
        {
        }

        /// <summary>Creates a new <see cref="NtpTimeTag"/>, given 64-bit NTP timestamp.</summary>
        /// <param name="timestamp">NTP timestamp containing number of seconds since 1/1/1900 in hi-word and fractional seconds in lo-word.</param>
        [CLSCompliant(false)]
        public NtpTimeTag(ulong timestamp)
            : this(HiUDWord(timestamp), LoUDWord(timestamp))
        {
        }

        /// <summary>Creates a new <see cref="NtpTimeTag"/>, given specified <see cref="Ticks"/>.</summary>
        /// <param name="timestamp">Timestamp in <see cref="Ticks"/> to create Unix timetag from (minimum valid date is 1/1/1900).</param>
        public NtpTimeTag(Ticks timestamp)
            : base(GetBaseDateOffsetTicks(timestamp), timestamp)
        {
        }

        /// <summary>
        /// Gets 64-bit NTP timestamp.
        /// </summary>
        [CLSCompliant(false)]
        public ulong Timestamp
        {
            get
            {
                return GetNTPTimestampFromTicks(ToDateTime());
            }
        }

        /// <summary>
        /// Gets proper NTP offset based on <paramref name="seconds"/> value, see RFC-2030.
        /// </summary>
        /// <param name="seconds">Seconds value.</param>
        /// <returns>Proper NTP offset.</returns>
        protected static long GetBaseDateOffsetTicks(double seconds)
        {
            return GetBaseDateOffsetTicks(Ticks.FromSeconds(seconds));
        }

        /// <summary>
        /// Gets proper NTP offset based on <paramref name="timestamp"/> value, see RFC-2030.
        /// </summary>
        /// <param name="timestamp"><see cref="Ticks"/> timestamp value.</param>
        /// <returns>Proper NTP offset.</returns>
        protected static long GetBaseDateOffsetTicks(Ticks timestamp)
        {
            if (timestamp < NtpDateOffsetTicksAlt)
                return NtpDateOffsetTicks;
            else
                return NtpDateOffsetTicksAlt;
        }

        /// <summary>
        /// Gets proper NTP offset based on most significant on <paramref name="timestamp"/> value, see RFC-2030.
        /// </summary>
        /// <param name="seconds">NTP seconds timestamp value.</param>
        /// <returns>Proper NTP offset.</returns>
        [CLSCompliant(false)]
        protected static long GetBaseDateOffsetTicks(uint seconds)
        {
            if ((seconds & 0x80000000) > 0)
                return NtpDateOffsetTicks;
            else
                return NtpDateOffsetTicksAlt;
        }

        /// <summary>
        /// Gets 64-bit NTP timestamp given <paramref name="timestamp"/> in <see cref="Ticks"/>.
        /// </summary>
        /// <param name="timestamp">Timestamp in <see cref="Ticks"/>.</param>
        /// <returns>Seconds in NTP from given <paramref name="timestamp"/>.</returns>
        [CLSCompliant(false)]
        protected static ulong GetNTPTimestampFromTicks(Ticks timestamp)
        {
            timestamp -= GetBaseDateOffsetTicks(timestamp);

            uint seconds = (uint)Math.Truncate(timestamp.ToSeconds());
            uint fraction = (uint)(Ticks.ToSeconds(timestamp.DistanceBeyondSecond()) * uint.MaxValue);

            return MakeUQWord(seconds, fraction);
        }

        // TODO: Replace with proper extension functions...
        private static uint LoUDWord(ulong quadWord)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToUInt32(BitConverter.GetBytes(quadWord), 0);
            else
                return BitConverter.ToUInt32(BitConverter.GetBytes(quadWord), 4);
        }

        private static uint HiUDWord(ulong quadWord)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToUInt32(BitConverter.GetBytes(quadWord), 4);
            else
                return BitConverter.ToUInt32(BitConverter.GetBytes(quadWord), 0);
        }

        private static ulong MakeUQWord(uint high, uint low)
        {
            byte[] bytes = new byte[8];

            if (BitConverter.IsLittleEndian)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(low), 0, bytes, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(high), 0, bytes, 4, 4);
            }
            else
            {
                Buffer.BlockCopy(BitConverter.GetBytes(high), 0, bytes, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(low), 0, bytes, 4, 4);
            }

            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}