// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Numeric
{
    internal class Long
    {
        private readonly int low;
        private readonly int high;
        private readonly bool unsigned;
        private static readonly Dictionary<int, Long> INT_CACHE = new Dictionary<int, Long>();
        private static readonly Dictionary<uint, Long> UINT_CACHE = new Dictionary<uint, Long>();

        public static Long ONE = new Long(1, 0, false);
        public static Long NEG_ONE = new Long(-1, 0, false);
        public static Long UZERO = new Long(0, 0, true);
        public static Long UONE = new Long(1, 0, true);
        public static Long ZERO = new Long(0, 0, false);
        public static Long MIN_VALUE = new Long(0, -2147483648, false);
        public static Long MAX_VALUE = new Long(-1, 2147483647, false);
        public static Long TWO_PWR_24 = new Long(16777216, 0, false);
        public static Long MAX_UNSIGNED_VALUE = new Long(-1, -1, true);
        public static double TWO_PWR_32_DBL = 4294967296;
        public static double TWO_PWR_64_DBL = TWO_PWR_32_DBL * TWO_PWR_32_DBL;
        public static double TWO_PWR_63_DBL = TWO_PWR_64_DBL / 2;
        public static double LN2 = 0.69314718055994529;

        public Long(int low, int high, bool unsigned)
        {
            this.low = low | 0;
            this.high = high | 0;
            this.unsigned = !!unsigned;
        }

        private static uint UnsignedRightShift(int toShift, int shiftBy)
        {
            uint _toShift = (uint)toShift;
            return (_toShift >> shiftBy);
        }

        private static Long FromBits(int lowBits, int highBits, bool unsigned)
        {
            return new Long(lowBits, highBits, unsigned);
        }

        private bool IsZero()
        {
            return this.high == 0 && this.low == 0;
        }

        private Long Not()
        {
            return FromBits(~this.low, ~this.high, this.unsigned);
        }

        private bool Equals(Long other)
        {
            if (this.unsigned != other.unsigned && ((uint)this.high >> 31) == (uint)1 && ((uint)other.high >> 31) == (uint)1)
            {
                return false;
            }
            return this.high == other.high && this.low == other.low;
        }

        private bool IsNegative()
        {
            return !this.unsigned && this.high < 0;
        }

        private bool IsOdd()
        {
            return (this.low & 1) == 1;
        }

        private Long Negate()
        {
            if (!this.unsigned && this.Equals(MIN_VALUE))
            {
                return MIN_VALUE;
            }
            return this.Not().Add(ONE);
        }

        private bool LessThan(Long other)
        {
            return this.Compare(/* validates */ other) < 0;
        }

        private bool GreaterThan(Long other)
        {
            bool res = this.Compare(/* validates */ other) > 0;
            return res;
        }

        private bool GreaterThanOrEqual(Long other)
        {
            bool res = this.Compare(/* validates */ other) >= 0;
            return res;
        }

        private int Compare(Long other)
        {
            if (this.Equals(other))
            {
                return 0;
            }
            bool thisNeg = this.IsNegative(),
              otherNeg = other.IsNegative();
            if (thisNeg && !otherNeg)
            {
                return -1;
            }
            if (!thisNeg && otherNeg)
            {
                return 1;
            }
            // At this point the sign bits are the same
            if (!this.unsigned)
            {
                return this.Subtract(other).IsNegative() ? -1 : 1;
            }
            // Both are positive if at least one is unsigned
            return (UnsignedRightShift(other.high, 0) > UnsignedRightShift(this.high, 0)) || (other.high == this.high && UnsignedRightShift(other.low, 0) > UnsignedRightShift(this.low, 0)) ? -1 : 1;
        }

        private double ToNumber()
        {
            if (this.unsigned)
            {
                return (((uint)this.high >> 0) * TWO_PWR_32_DBL) + ((uint)this.low >> 0);
            }
            return this.high * TWO_PWR_32_DBL + ((uint)this.low >> 0);
        }

        private int ToInt()
        {
            return this.unsigned ? (int)((uint)this.low >> 0) : this.low;
        }

        private Long FromNumber(double value, bool unsigned = false)
        {
            if (double.IsNaN(value))
            {
                return unsigned ? UZERO : ZERO;
            }
            if (unsigned)
            {
                if (value < 0)
                {
                    return UZERO;
                }
                if (value >= TWO_PWR_64_DBL)
                {
                    return MAX_UNSIGNED_VALUE;
                }
            }
            else
            {
                if (value <= -TWO_PWR_63_DBL)
                {
                    return MIN_VALUE;
                }
                if (value + 1 >= TWO_PWR_63_DBL)
                {
                    return MAX_VALUE;
                }
            }
            if (value < 0)
            {
                return FromNumber(-value, unsigned).Negate();
            }
            return FromBits((int)(value % TWO_PWR_32_DBL) | 0, (int)(value / TWO_PWR_32_DBL) | 0, unsigned);
        }

        private Long ShiftRight(int numBits)
        {
            if ((numBits &= 63) == 0)
            {
                return this;
            }
            else if (numBits < 32)
            {
                return FromBits((int)((uint)this.low >> numBits) | (this.high << (32 - numBits)), this.high >> numBits, this.unsigned);
            }
            else
            {
                return FromBits(this.high >> (numBits - 32), this.high >= 0 ? 0 : -1, this.unsigned);
            }
        }

        private Long ShiftLeft(int numBits)
        {
            if ((numBits &= 63) == 0)
            {
                return this;
            }
            else if (numBits < 32)
            {
                return FromBits(this.low << numBits, (this.high << numBits) | (this.low >> (32 - numBits)), this.unsigned);
            }
            else
            {
                return FromBits(0, this.low << (numBits - 32), this.unsigned);
            }
        }

        private Long ShiftRightUnsigned(int numBits)
        {
            if ((numBits &= 63) == 0)
            {
                return this;
            }
            if (numBits < 32)
            {
                return FromBits((this.low >> numBits) | (this.high << (32 - numBits)), this.high >> numBits, this.unsigned);
            }
                
            if ((uint)numBits == 32)
            {
                return FromBits(this.high, 0, this.unsigned);
            }
                  
            return FromBits(this.high >> (numBits - 32), 0, this.unsigned);
        }

        public static Long FromInt(int value, bool unsigned = false)
        {
            Long obj;  
            bool cache;
            if (unsigned)
            {
                uint _value = (uint)value >> 0;
                if (cache = (0 <= value && value < 256))
                {
                    if (UINT_CACHE.ContainsKey(_value))
                    {
                        return UINT_CACHE[_value];
                    }
                }
                obj = FromBits(value, 0, true);
                if (cache)
                {
                    UINT_CACHE[_value] = obj;
                }
                return obj;
            }
            else
            {
                value |= 0;
                if (cache = (-128 <= value && value < 128))
                {
                    if (INT_CACHE.ContainsKey(value))
                    {
                        return INT_CACHE[value];
                    }
                }
                obj = FromBits(value, value < 0 ? -1 : 0, false);
                if (cache)
                {
                    INT_CACHE[value] = obj;
                }
                return obj;
            }
        }

        /// <summary>
        /// Converts this Long to unsigned.
        /// </summary>
        /// <returns>Unsigned long representation of the input signed long.</returns>
        public Long ToUnsigned()
        {
            if (this.unsigned)
            {
                return this;
            }
            return FromBits(this.low, this.high, true);
        }

        public Long Multiply(Long multiplier)
        {
            if (this.IsZero())
            {
                return this;
            }

            if (multiplier.IsZero())
            {
                return this.unsigned ? UZERO : ZERO;
            }
            if (this.Equals(MIN_VALUE))
            {
                return multiplier.IsOdd() ? MIN_VALUE : ZERO;
            }
            if (multiplier.Equals(MIN_VALUE))
            {
                return this.IsOdd() ? MIN_VALUE : ZERO;
            }

            if (this.IsNegative())
            {
                if (multiplier.IsNegative())
                {
                    return this.Negate().Multiply(multiplier.Negate());
                }
                else
                {
                    return this.Negate().Multiply(multiplier).Negate();
                }
            }
            else if (multiplier.IsNegative())
            {
                return this.Multiply(multiplier.Negate()).Negate();
            }

            // If both longs are small, use float multiplication
            if (this.LessThan(TWO_PWR_24) && multiplier.LessThan(TWO_PWR_24))
            {
                return FromNumber(this.ToNumber() * multiplier.ToNumber(), this.unsigned);
            }
            // Divide each long into 4 chunks of 16 bits, and then add up 4x4 products.
            // We can skip products that would overflow.

            uint a48 = (uint)this.high >> 16;
            int a32 = this.high & 0xFFFF;
            uint a16 = (uint)this.low >> 16;
            int a00 = this.low & 0xFFFF;

            uint b48 = (uint)multiplier.high >> 16;
            int b32 = multiplier.high & 0xFFFF;
            uint b16 = (uint)multiplier.low >> 16;
            int b00 = multiplier.low & 0xFFFF;

            uint c48 = 0, c32 = 0, c16 = 0, c00 = 0;
            c00 += (uint)(a00) * (uint)(b00);
            c16 += c00 >> 16;
            c00 &= 0xFFFF;
            c16 += (uint)(a16) * (uint)(b00);
            c32 += c16 >> 16;
            c16 &= 0xFFFF;
            c16 += (uint)(a00) * (uint)(b16);
            c32 += c16 >> 16;
            c16 &= 0xFFFF;
            c32 += (uint)(a32) * (uint)(b00);
            c48 += c32 >> 16;
            c32 &= 0xFFFF;
            c32 += a16 * b16;
            c48 += c32 >> 16;
            c32 &= 0xFFFF;
            c32 += (uint)(a00) * (uint)(b32);
            c48 += c32 >> 16;
            c32 &= 0xFFFF;
            c48 += (uint)(a48 * b00 + a32 * b16 + a16 * b32 + a00 * b48);
            c48 &= 0xFFFF;
            return FromBits((int)((c16 << 16) | c00), (int)((c48 << 16) | c32), this.unsigned);
        }

        public Long Add(Long addend)
        {
            // Divide each number into 4 chunks of 16 bits, and then sum the chunks.

            uint a48 = (uint)this.high >> 16;
            int a32 = this.high & 0xFFFF;
            uint a16 = (uint)this.low >> 16;
            int a00 = this.low & 0xFFFF;

            uint b48 = (uint)addend.high >> 16;
            int b32 = addend.high & 0xFFFF;
            uint b16 = (uint)addend.low >> 16;
            int b00 = addend.low & 0xFFFF;

            uint c48 = 0, c32 = 0, c16 = 0, c00 = 0;
            c00 += (uint)a00 + (uint)b00;
            c16 += c00 >> 16;
            c00 &= 0xFFFF;
            c16 += (uint)a16 + (uint)b16;
            c32 += c16 >> 16;
            c16 &= 0xFFFF;
            c32 += (uint)(a32) + (uint)(b32);
            c48 += c32 >> 16;
            c32 &= 0xFFFF;
            c48 += a48 + b48;
            c48 &= 0xFFFF;
            return FromBits((int)((c16 << 16) | c00), (int)((c48 << 16) | c32), this.unsigned);
        }

        public Long Subtract(Long subtrahend)
        {
            return this.Add(subtrahend.Negate());
        }

        /// <summary>
        /// Returns this Long divided by the specified. The result is signed if this Long is signed or,
        /// unsigned if this Long is unsigned.
        /// </summary>
        /// <param name="divisor"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Long Divide(Long divisor)
        {
            if (divisor.IsZero())
            {
                throw new ArgumentException("'Divide' method in 'Sarif.Numeric.Long.Divide' received an argument to divide by zero.");
            }

            if (this.IsZero())
            {
                return this.unsigned ? UZERO : ZERO;
            }
            Long rem, res;
            if (!this.unsigned)
            {
                // This section is only relevant for signed longs and is derived from the
                // closure library as a whole.
                if (this.Equals(MIN_VALUE))
                {
                    if (divisor.Equals(ONE) || divisor.Equals(NEG_ONE))
                    {
                        return MIN_VALUE;  // recall that -MIN_VALUE == MIN_VALUE
                    }
                    else if (divisor.Equals(MIN_VALUE))
                    {
                        return ONE;
                    }
                    else
                    {
                        // At this point, we have |other| >= 2, so |this/other| < |MIN_VALUE|.
                        Long halfThis = this.ShiftRight(1);
                        Long approx = halfThis.Divide(divisor).ShiftLeft(1);
                        if (approx.Equals(ZERO))
                        {
                            return divisor.IsNegative() ? ONE : NEG_ONE;
                        }
                        else
                        {
                            rem = this.Subtract(divisor.Multiply(approx));
                            res = approx.Add(rem.Divide(divisor));
                            return res;
                        }
                    }
                }
                else if (divisor.Equals(MIN_VALUE))
                {
                    return this.unsigned ? UZERO : ZERO;
                }
                if (this.IsNegative())
                {
                    if (divisor.IsNegative())
                    {
                        return this.Negate().Divide(divisor.Negate());
                    }
                    return this.Negate().Divide(divisor).Negate();
                }
                else if (divisor.IsNegative())
                {
                    return this.Divide(divisor.Negate()).Negate();
                }
                res = ZERO;
            }
            else
            {
                // The algorithm below has not been made for unsigned longs. It's therefore
                // required to take special care of the MSB prior to running it.
                if (!divisor.unsigned)
                {
                    divisor = divisor.ToUnsigned();
                }
                if (divisor.GreaterThan(this))
                {
                    return UZERO; 
                }
                if (divisor.GreaterThan(this.ShiftRightUnsigned(1))) // 15 >>> 1 = 7 ; with divisor = 8 ; true
                {
                    return UONE;
                }
                res = UZERO;
            }

            // Repeat the following until the remainder is less than other:  find a
            // floating-point that approximates remainder / other *from below*, add this
            // into the result, and subtract it from the remainder.  It is critical that
            // the approximate value is less than or equal to the real value so that the
            // remainder never becomes negative.
            rem = this;
            while (rem.GreaterThanOrEqual(divisor))
            {
                // Approximate the result of division. This may be a little greater or
                // smaller than the actual value.
                double approx = Math.Max(1, Math.Floor(rem.ToNumber() / divisor.ToNumber()));

                // We will tweak the approximate result by changing it in the 48-th digit or
                // the smallest non-fractional digit, whichever is larger.
                double log2 = Math.Ceiling(Math.Log(approx) / LN2);
                double delta = (log2 <= 48) ? 1 : Math.Pow(2, log2 - 48);

                  // Decrease the approximation until it is smaller than the remainder.  Note
                  // that if it is too large, the product overflows and is negative.
                Long approxRes = this.FromNumber(approx);
                Long approxRem = approxRes.Multiply(divisor);
                while (approxRem.IsNegative() || approxRem.GreaterThan(rem))
                {
                    approx -= delta;
                    approxRes = FromNumber(approx, this.unsigned);
                    approxRem = approxRes.Multiply(divisor);
                }

                // We know the answer can't be zero... and actually, zero would cause
                // infinite recursion since we would make no progress.
                    if (approxRes.IsZero())
                {
                    approxRes = ONE;
                }

                res = res.Add(approxRes);
                rem = rem.Subtract(approxRem);
            }
            return res;
        }


        /// <summary>
        /// Converts the Long to a string written in the specified radix.
        /// </summary>
        /// <param name="radix">Radix (2-36), defaults to 10</param>
        /// <returns>String representation of the Long object in base 'radix', exception if 'radix' is out of range.</returns>
        public string ToString(int radix)
        {
            if (radix < 2 || 36 < radix)
            {
                throw new Exception("The 'ToString' method in 'Sarif.Numeric.Long' received a radix which is out of range, restrict to (2-36)");
            }
            if (this.IsZero())
            {
                return "0";
            }
            if (this.IsNegative())
            { // Unsigned Longs are never negative
                if (this.Equals(MIN_VALUE))
                {
                    // We need to change the Long value before it can be negated, so we remove
                    // the bottom-most digit in this base and then recurse to do the rest.
                    Long radixLong = FromNumber(radix, false);
                    Long div = this.Divide(radixLong);
                    Long rem1 = div.Multiply(radixLong).Subtract(this);
                    return div.ToString(radix) + Convert.ToString(rem1.ToInt(), radix);
                }
                else
                {
                    return '-' + this.Negate().ToString(radix);
                }
            }

            // Do several (6) digits each time through the loop, so as to
            // minimize the calls to the very expensive emulated div.
            Long radixToPower = FromNumber(Math.Pow(radix, 6), this.unsigned);
            Long rem = this;
            string result = "";
            while (true)
            {
                Long remDiv = rem.Divide(radixToPower);
                int intval = rem.Subtract(remDiv.Multiply(radixToPower)).ToInt() >> 0;
                string digits = Convert.ToString(intval, radix);
                rem = remDiv;
                if (rem.IsZero())
                {
                    return digits + result;
                }
                else
                {
                    while (digits.Length < 6)
                    {
                        digits = '0' + digits;
                    }
                    result = "" + digits + result;
                }
            }
        }
    }
}
