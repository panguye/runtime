// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace System.Tests
{
    public partial class HalfTests
    {
        // binary32 (float) has a machine epsilon of 2^-10 (approx. 9.77e-04). However, this
        // is slightly too accurate when writing tests meant to run against libm implementations
        // for various platforms. 2^-8 (approx. 3.91e-03) seems to be as accurate as we can get.
        //
        // The tests themselves will take CrossPlatformMachineEpsilon and adjust it according to the expected result
        // so that the delta used for comparison will compare the most significant digits and ignore
        // any digits that are outside the half precision range (3-4 digits).
        //
        // For example, a test with an expect result in the format of 0.xxxxxxxxx will use
        // CrossPlatformMachineEpsilon for the variance, while an expected result in the format of 0.0xxxxxxxxx
        // will use CrossPlatformMachineEpsilon / 10 and expected result in the format of x.xxxxxx will
        // use CrossPlatformMachineEpsilon * 10.
        private static Half CrossPlatformMachineEpsilon => (Half)3.90625e-03f;

        [Fact]
        public static void Epsilon()
        {
            Assert.Equal(0x0001u, BitConverter.HalfToUInt16Bits(Half.Epsilon));
        }

        [Fact]
        public static void PositiveInfinity()
        {
            Assert.Equal(0x7C00u, BitConverter.HalfToUInt16Bits(Half.PositiveInfinity));
        }

        [Fact]
        public static void NegativeInfinity()
        {
            Assert.Equal(0xFC00u, BitConverter.HalfToUInt16Bits(Half.NegativeInfinity));
        }

        [Fact]
        public static void NaN()
        {
            Assert.Equal(0xFE00u, BitConverter.HalfToUInt16Bits(Half.NaN));
        }

        [Fact]
        public static void MinValue()
        {
            Assert.Equal(0xFBFFu, BitConverter.HalfToUInt16Bits(Half.MinValue));
        }

        [Fact]
        public static void MaxValue()
        {
            Assert.Equal(0x7BFFu, BitConverter.HalfToUInt16Bits(Half.MaxValue));
        }

        [Fact]
        public static void Ctor_Empty()
        {
            var value = new Half();
            Assert.Equal(0x0000, BitConverter.HalfToUInt16Bits(value));
        }

        public static IEnumerable<object[]> IsFinite_TestData()
        {
            yield return new object[] { Half.NegativeInfinity, false };                  // Negative Infinity
            yield return new object[] { Half.MinValue, true };                           // Min Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8400), true };   // Max Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x83FF), true };   // Min Negative Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8001), true };   // Max Negative Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8000), true };   // Negative Zero
            yield return new object[] { Half.NaN, false };                               // NaN
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0000), true };   // Positive Zero
            yield return new object[] { Half.Epsilon, true };                            // Min Positive Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x03FF), true };   // Max Positive Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0400), true };   // Min Positive Normal
            yield return new object[] { Half.MaxValue, true };                           // Max Positive Normal
            yield return new object[] { Half.PositiveInfinity, false };                  // Positive Infinity
        }

        [Theory]
        [MemberData(nameof(IsFinite_TestData))]
        public static void IsFinite(Half value, bool expected)
        {
            Assert.Equal(expected, Half.IsFinite(value));
        }

        public static IEnumerable<object[]> IsInfinity_TestData()
        {
            yield return new object[] { Half.NegativeInfinity, true };                   // Negative Infinity
            yield return new object[] { Half.MinValue, false };                          // Min Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8400), false };  // Max Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x83FF), false };  // Min Negative Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8001), false };  // Max Negative Subnormal (Negative Epsilon)
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8000), false };  // Negative Zero
            yield return new object[] { Half.NaN, false };                               // NaN
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0000), false };  // Positive Zero
            yield return new object[] { Half.Epsilon, false };                           // Min Positive Subnormal (Positive Epsilon)
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x03FF), false };  // Max Positive Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0400), false };  // Min Positive Normal
            yield return new object[] { Half.MaxValue, false };                          // Max Positive Normal
            yield return new object[] { Half.PositiveInfinity, true };                   // Positive Infinity
        }

        [Theory]
        [MemberData(nameof(IsInfinity_TestData))]
        public static void IsInfinity(Half value, bool expected)
        {
            Assert.Equal(expected, Half.IsInfinity(value));
        }

        public static IEnumerable<object[]> IsNaN_TestData()
        {
            yield return new object[] { Half.NegativeInfinity, false };                  // Negative Infinity
            yield return new object[] { Half.MinValue, false };                          // Min Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8400), false };  // Max Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x83FF), false };  // Min Negative Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8001), false };  // Max Negative Subnormal (Negative Epsilon)
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8000), false };  // Negative Zero
            yield return new object[] { Half.NaN, true };                                // NaN
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0000), false };  // Positive Zero
            yield return new object[] { Half.Epsilon, false };                           // Min Positive Subnormal (Positive Epsilon)
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x03FF), false };  // Max Positive Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0400), false };  // Min Positive Normal
            yield return new object[] { Half.MaxValue, false };                          // Max Positive Normal
            yield return new object[] { Half.PositiveInfinity, false };                  // Positive Infinity
        }

        [Theory]
        [MemberData(nameof(IsNaN_TestData))]
        public static void IsNaN(Half value, bool expected)
        {
            Assert.Equal(expected, Half.IsNaN(value));
        }

        public static IEnumerable<object[]> IsNegative_TestData()
        {
            yield return new object[] { Half.NegativeInfinity, true };                   // Negative Infinity
            yield return new object[] { Half.MinValue, true };                           // Min Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8400), true };   // Max Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x83FF), true };   // Min Negative Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8001), true };   // Max Negative Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8000), true };   // Negative Zero
            yield return new object[] { Half.NaN, true };                                // NaN
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0000), false };  // Positive Zero
            yield return new object[] { Half.Epsilon, false };                           // Min Positive Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x03FF), false };  // Max Positive Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0400), false };  // Min Positive Normal
            yield return new object[] { Half.MaxValue, false };                          // Max Positive Normal
            yield return new object[] { Half.PositiveInfinity, false };                  // Positive Infinity
        }

        [Theory]
        [MemberData(nameof(IsNegative_TestData))]
        public static void IsNegative(Half value, bool expected)
        {
            Assert.Equal(expected, Half.IsNegative(value));
        }

        public static IEnumerable<object[]> IsNegativeInfinity_TestData()
        {
            yield return new object[] { Half.NegativeInfinity, true };                   // Negative Infinity
            yield return new object[] { Half.MinValue, false };                          // Min Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8400), false };  // Max Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x83FF), false };  // Min Negative Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8001), false };  // Max Negative Subnormal (Negative Epsilon)
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8000), false };  // Negative Zero
            yield return new object[] { Half.NaN, false };                               // NaN
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0000), false };  // Positive Zero
            yield return new object[] { Half.Epsilon, false };                           // Min Positive Subnormal (Positive Epsilon)
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x03FF), false };  // Max Positive Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0400), false };  // Min Positive Normal
            yield return new object[] { Half.MaxValue, false };                          // Max Positive Normal
            yield return new object[] { Half.PositiveInfinity, false };                  // Positive Infinity
        }

        [Theory]
        [MemberData(nameof(IsNegativeInfinity_TestData))]
        public static void IsNegativeInfinity(Half value, bool expected)
        {
            Assert.Equal(expected, Half.IsNegativeInfinity(value));
        }

        public static IEnumerable<object[]> IsNormal_TestData()
        {
            yield return new object[] { Half.NegativeInfinity, false };                  // Negative Infinity
            yield return new object[] { Half.MinValue, true };                           // Min Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8400), true };   // Max Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x83FF), false };  // Min Negative Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8001), false };  // Max Negative Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8000), false };  // Negative Zero
            yield return new object[] { Half.NaN, false };                               // NaN
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0000), false };  // Positive Zero
            yield return new object[] { Half.Epsilon, false };                           // Min Positive Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x03FF), false };  // Max Positive Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0400), true };   // Min Positive Normal
            yield return new object[] { Half.MaxValue, true };                           // Max Positive Normal
            yield return new object[] { Half.PositiveInfinity, false };                  // Positive Infinity
        }

        [Theory]
        [MemberData(nameof(IsNormal_TestData))]
        public static void IsNormal(Half value, bool expected)
        {
            Assert.Equal(expected, Half.IsNormal(value));
        }

        public static IEnumerable<object[]> IsPositiveInfinity_TestData()
        {
            yield return new object[] { Half.NegativeInfinity, false };                  // Negative Infinity
            yield return new object[] { Half.MinValue, false };                          // Min Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8400), false };  // Max Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x83FF), false };  // Min Negative Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8001), false };  // Max Negative Subnormal (Negative Epsilon)
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8000), false };  // Negative Zero
            yield return new object[] { Half.NaN, false };                               // NaN
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0000), false };  // Positive Zero
            yield return new object[] { Half.Epsilon, false };                           // Min Positive Subnormal (Positive Epsilon)
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x03FF), false };  // Max Positive Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0400), false };  // Min Positive Normal
            yield return new object[] { Half.MaxValue, false };                          // Max Positive Normal
            yield return new object[] { Half.PositiveInfinity, true };                   // Positive Infinity
        }

        [Theory]
        [MemberData(nameof(IsPositiveInfinity_TestData))]
        public static void IsPositiveInfinity(Half value, bool expected)
        {
            Assert.Equal(expected, Half.IsPositiveInfinity(value));
        }

        public static IEnumerable<object[]> IsSubnormal_TestData()
        {
            yield return new object[] { Half.NegativeInfinity, false };                  // Negative Infinity
            yield return new object[] { Half.MinValue, false };                          // Min Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8400), false };  // Max Negative Normal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x83FF), true };   // Min Negative Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8001), true };   // Max Negative Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8000), false };  // Negative Zero
            yield return new object[] { Half.NaN, false };                               // NaN
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0000), false };  // Positive Zero
            yield return new object[] { Half.Epsilon, true };                            // Min Positive Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x03FF), true };   // Max Positive Subnormal
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x0400), false };  // Min Positive Normal
            yield return new object[] { Half.MaxValue, false };                          // Max Positive Normal
            yield return new object[] { Half.PositiveInfinity, false };                  // Positive Infinity
        }

        [Theory]
        [MemberData(nameof(IsSubnormal_TestData))]
        public static void IsSubnormal(Half value, bool expected)
        {
            Assert.Equal(expected, Half.IsSubnormal(value));
        }

        public static IEnumerable<object[]> CompareTo_ThrowsArgumentException_TestData()
        {
            yield return new object[] { "a" };
            yield return new object[] { 234.0 };
        }

        [Theory]
        [MemberData(nameof(CompareTo_ThrowsArgumentException_TestData))]
        public static void CompareTo_ThrowsArgumentException(object obj)
        {
            Assert.Throws<ArgumentException>(() => Half.MaxValue.CompareTo(obj));
        }

        public static IEnumerable<object[]> CompareTo_TestData()
        {
            yield return new object[] { Half.MaxValue, Half.MaxValue, 0 };
            yield return new object[] { Half.MaxValue, Half.MinValue, 1 };
            yield return new object[] { Half.Epsilon, BitConverter.UInt16BitsToHalf(0x8001), 1 };
            yield return new object[] { Half.MaxValue, BitConverter.UInt16BitsToHalf(0x0000), 1 };
            yield return new object[] { Half.MaxValue, Half.Epsilon, 1 };
            yield return new object[] { Half.MaxValue, Half.PositiveInfinity, -1 };
            yield return new object[] { Half.MinValue, Half.MaxValue, -1 };
            yield return new object[] { Half.MaxValue, Half.NaN, 1 };
            yield return new object[] { Half.NaN, Half.NaN, 0 };
            yield return new object[] { Half.NaN, BitConverter.UInt16BitsToHalf(0x0000), -1 };
            yield return new object[] { Half.MaxValue, null, 1 };
            yield return new object[] { Half.MinValue, Half.NegativeInfinity, 1 };
            yield return new object[] { Half.NegativeInfinity, Half.MinValue, -1 };
            yield return new object[] { BitConverter.UInt16BitsToHalf(0x8000), Half.NegativeInfinity, 1 }; // Negative zero
            yield return new object[] { Half.NegativeInfinity, BitConverter.UInt16BitsToHalf(0x8000), -1 }; // Negative zero
            yield return new object[] { Half.NegativeInfinity, Half.NegativeInfinity, 0};
            yield return new object[] { Half.PositiveInfinity, Half.PositiveInfinity, 0};
            yield return new object[] { (Half)(-180f), (Half)(-180f), 0};
            yield return new object[] { (Half)(180f), (Half)(180f), 0};
            yield return new object[] { (Half)(-180f), (Half)(180f), -1};
            yield return new object[] { (Half)(180f), (Half)(-180f), 1};
            yield return new object[] { (Half)(-65535), (object)null, 1};
        }

        [Theory]
        [MemberData(nameof(CompareTo_TestData))]
        public static void CompareTo(Half value, object obj, int expected)
        {
            if (obj is Half other)
            {
                Assert.Equal(expected, Math.Sign(value.CompareTo(other)));

                if (Half.IsNaN(value) || Half.IsNaN(other))
                {
                    Assert.False(value >= other);
                    Assert.False(value > other);
                    Assert.False(value <= other);
                    Assert.False(value < other);
                }
                else
                {
                    if (expected >= 0)
                    {
                        Assert.True(value >= other);
                        Assert.False(value < other);
                    }
                    if (expected > 0)
                    {
                        Assert.True(value > other);
                        Assert.False(value <= other);
                    }
                    if (expected <= 0)
                    {
                        Assert.True(value <= other);
                        Assert.False(value > other);
                    }
                    if (expected < 0)
                    {
                        Assert.True(value < other);
                        Assert.False(value >= other);
                    }
                }
            }

            Assert.Equal(expected, Math.Sign(value.CompareTo(obj)));
        }

        public static IEnumerable<object[]> Equals_TestData()
        {
            yield return new object[] { Half.MaxValue, Half.MaxValue, true };
            yield return new object[] { Half.MaxValue, Half.MinValue, false };
            yield return new object[] { Half.MaxValue, BitConverter.UInt16BitsToHalf(0x0000), false };
            yield return new object[] { Half.NaN, Half.NaN, true };
            yield return new object[] { Half.MaxValue, 789.0f, false };
            yield return new object[] { Half.MaxValue, "789", false };
        }

        [Theory]
        [MemberData(nameof(Equals_TestData))]
        public static void EqualsTest(Half value, object obj, bool expected)
        {
            Assert.Equal(expected, value.Equals(obj));
        }

        public static IEnumerable<object[]> ExplicitConversion_ToSingle_TestData()
        {
            (Half Original, float Expected)[] data = // Fraction is shifted left by 42, Exponent is -15 then +127 = +112
            {
                (BitConverter.UInt16BitsToHalf(0b0_01111_0000000000), 1f), // 1
                (BitConverter.UInt16BitsToHalf(0b1_01111_0000000000), -1f), // -1
                (Half.MaxValue, 65504f), // 65500
                (Half.MinValue, -65504f), // -65500
                (BitConverter.UInt16BitsToHalf(0b0_01011_1001100110), 0.0999755859375f), // 0.1ish
                (BitConverter.UInt16BitsToHalf(0b1_01011_1001100110), -0.0999755859375f), // -0.1ish
                (BitConverter.UInt16BitsToHalf(0b0_10100_0101000000), 42f), // 42
                (BitConverter.UInt16BitsToHalf(0b1_10100_0101000000), -42f), // -42
                (Half.PositiveInfinity, float.PositiveInfinity), // PosInfinity
                (Half.NegativeInfinity, float.NegativeInfinity), // NegInfinity
                (BitConverter.UInt16BitsToHalf(0b0_11111_1000000000), BitConverter.Int32BitsToSingle(0x7FC00000)), // Positive Quiet NaN
                (Half.NaN, float.NaN), // Negative Quiet NaN
                (BitConverter.UInt16BitsToHalf(0b0_11111_1010101010), BitConverter.Int32BitsToSingle(0x7FD54000)), // Positive Signalling NaN - Should preserve payload
                (BitConverter.UInt16BitsToHalf(0b1_11111_1010101010), BitConverter.Int32BitsToSingle(unchecked((int)0xFFD54000))), // Negative Signalling NaN - Should preserve payload
                (Half.Epsilon, 1/16777216f), // PosEpsilon = 0.000000059605...
                (BitConverter.UInt16BitsToHalf(0), 0), // 0
                (BitConverter.UInt16BitsToHalf(0b1_00000_0000000000), -0f), // -0
                (BitConverter.UInt16BitsToHalf(0b0_10000_1001001000), 3.140625f), // 3.140625
                (BitConverter.UInt16BitsToHalf(0b1_10000_1001001000), -3.140625f), // -3.140625
                (BitConverter.UInt16BitsToHalf(0b0_10000_0101110000), 2.71875f), // 2.71875
                (BitConverter.UInt16BitsToHalf(0b1_10000_0101110000), -2.71875f), // -2.71875
                (BitConverter.UInt16BitsToHalf(0b0_01111_1000000000), 1.5f), // 1.5
                (BitConverter.UInt16BitsToHalf(0b1_01111_1000000000), -1.5f), // -1.5
                (BitConverter.UInt16BitsToHalf(0b0_01111_1000000001), 1.5009765625f), // 1.5009765625
                (BitConverter.UInt16BitsToHalf(0b1_01111_1000000001), -1.5009765625f), // -1.5009765625
                (BitConverter.UInt16BitsToHalf(0b0_00001_0000000000), BitConverter.Int32BitsToSingle(0x38800000)), // smallest normal
                (BitConverter.UInt16BitsToHalf(0b0_00000_1111111111), BitConverter.Int32BitsToSingle(0x387FC000)), // largest subnormal
                (BitConverter.UInt16BitsToHalf(0b0_00000_1000000000), BitConverter.Int32BitsToSingle(0x38000000)), // middle subnormal
                (BitConverter.UInt16BitsToHalf(0b0_00000_0111111111), BitConverter.Int32BitsToSingle(0x37FF8000)), // just below middle subnormal
                (BitConverter.UInt16BitsToHalf(0b0_00000_0000000001), BitConverter.Int32BitsToSingle(0x33800000)), // smallest subnormal
                (BitConverter.UInt16BitsToHalf(0b1_00000_0000000001), BitConverter.Int32BitsToSingle(unchecked((int)0xB3800000))), // highest negative subnormal
                (BitConverter.UInt16BitsToHalf(0b1_00000_0111111111), BitConverter.Int32BitsToSingle(unchecked((int)0xB7FF8000))), // just above negative middle subnormal
                (BitConverter.UInt16BitsToHalf(0b1_00000_1000000000), BitConverter.Int32BitsToSingle(unchecked((int)0xB8000000))), // negative middle subnormal
                (BitConverter.UInt16BitsToHalf(0b1_00000_1111111111), BitConverter.Int32BitsToSingle(unchecked((int)0xB87FC000))), // lowest negative subnormal
                (BitConverter.UInt16BitsToHalf(0b1_00001_0000000000), BitConverter.Int32BitsToSingle(unchecked((int)0xB8800000))) // highest negative normal
            };

            foreach ((Half original, float expected) in data)
            {
                yield return new object[] { original, expected };
            }
        }

        [MemberData(nameof(ExplicitConversion_ToSingle_TestData))]
        [Theory]
        public static void ExplicitConversion_ToSingle(Half value, float expected) // Check the underlying bits for verifying NaNs
        {
            float f = (float)value;
            Assert.Equal(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(f));
        }

        public static IEnumerable<object[]> ExplicitConversion_ToDouble_TestData()
        {
            (Half Original, double Expected)[] data = // Fraction is shifted left by 42, Exponent is -15 then +127 = +112
            {
                (BitConverter.UInt16BitsToHalf(0b0_01111_0000000000), 1d), // 1
                (BitConverter.UInt16BitsToHalf(0b1_01111_0000000000), -1d), // -1
                (Half.MaxValue, 65504d), // 65500
                (Half.MinValue, -65504d), // -65500
                (BitConverter.UInt16BitsToHalf(0b0_01011_1001100110), 0.0999755859375d), // 0.1ish
                (BitConverter.UInt16BitsToHalf(0b1_01011_1001100110), -0.0999755859375d), // -0.1ish
                (BitConverter.UInt16BitsToHalf(0b0_10100_0101000000), 42d), // 42
                (BitConverter.UInt16BitsToHalf(0b1_10100_0101000000), -42d), // -42
                (Half.PositiveInfinity, double.PositiveInfinity), // PosInfinity
                (Half.NegativeInfinity, double.NegativeInfinity), // NegInfinity
                (BitConverter.UInt16BitsToHalf(0b0_11111_1000000000), BitConverter.Int64BitsToDouble(0x7FF80000_00000000)), // Positive Quiet NaN
                (Half.NaN, double.NaN), // Negative Quiet NaN
                (BitConverter.UInt16BitsToHalf(0b0_11111_1010101010), BitConverter.Int64BitsToDouble(0x7FFAA800_00000000)), // Positive Signalling NaN - Should preserve payload
                (BitConverter.UInt16BitsToHalf(0b1_11111_1010101010), BitConverter.Int64BitsToDouble(unchecked((long)0xFFFAA800_00000000))), // Negative Signalling NaN - Should preserve payload
                (Half.Epsilon, 1/16777216d), // PosEpsilon = 0.000000059605...
                (BitConverter.UInt16BitsToHalf(0), 0d), // 0
                (BitConverter.UInt16BitsToHalf(0b1_00000_0000000000), -0d), // -0
                (BitConverter.UInt16BitsToHalf(0b0_10000_1001001000), 3.140625d), // 3.140625
                (BitConverter.UInt16BitsToHalf(0b1_10000_1001001000), -3.140625d), // -3.140625
                (BitConverter.UInt16BitsToHalf(0b0_10000_0101110000), 2.71875d), // 2.71875
                (BitConverter.UInt16BitsToHalf(0b1_10000_0101110000), -2.71875d), // -2.71875
                (BitConverter.UInt16BitsToHalf(0b0_01111_1000000000), 1.5d), // 1.5
                (BitConverter.UInt16BitsToHalf(0b1_01111_1000000000), -1.5d), // -1.5
                (BitConverter.UInt16BitsToHalf(0b0_01111_1000000001), 1.5009765625d), // 1.5009765625
                (BitConverter.UInt16BitsToHalf(0b1_01111_1000000001), -1.5009765625d), // -1.5009765625
                (BitConverter.UInt16BitsToHalf(0b0_00001_0000000000), BitConverter.Int64BitsToDouble(0x3F10000000000000)), // smallest normal
                (BitConverter.UInt16BitsToHalf(0b0_00000_1111111111), BitConverter.Int64BitsToDouble(0x3F0FF80000000000)), // largest subnormal
                (BitConverter.UInt16BitsToHalf(0b0_00000_1000000000), BitConverter.Int64BitsToDouble(0x3F00000000000000)), // middle subnormal
                (BitConverter.UInt16BitsToHalf(0b0_00000_0111111111), BitConverter.Int64BitsToDouble(0x3EFFF00000000000)), // just below middle subnormal
                (BitConverter.UInt16BitsToHalf(0b0_00000_0000000001), BitConverter.Int64BitsToDouble(0x3E70000000000000)), // smallest subnormal
                (BitConverter.UInt16BitsToHalf(0b1_00000_0000000001), BitConverter.Int64BitsToDouble(unchecked((long)0xBE70000000000000))), // highest negative subnormal
                (BitConverter.UInt16BitsToHalf(0b1_00000_0111111111), BitConverter.Int64BitsToDouble(unchecked((long)0xBEFFF00000000000))), // just above negative middle subnormal
                (BitConverter.UInt16BitsToHalf(0b1_00000_1000000000), BitConverter.Int64BitsToDouble(unchecked((long)0xBF00000000000000))), // negative middle subnormal
                (BitConverter.UInt16BitsToHalf(0b1_00000_1111111111), BitConverter.Int64BitsToDouble(unchecked((long)0xBF0FF80000000000))), // lowest negative subnormal
                (BitConverter.UInt16BitsToHalf(0b1_00001_0000000000), BitConverter.Int64BitsToDouble(unchecked((long)0xBF10000000000000))) // highest negative normal
            };

            foreach ((Half original, double expected) in data)
            {
                yield return new object[] { original, expected };
            }
        }

        [MemberData(nameof(ExplicitConversion_ToDouble_TestData))]
        [Theory]
        public static void ExplicitConversion_ToDouble(Half value, double expected) // Check the underlying bits for verifying NaNs
        {
            double d = (double)value;
            Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(d));
        }

        // ---------- Start of To-half conversion tests ----------
        public static IEnumerable<object[]> ExplicitConversion_FromSingle_TestData()
        {
            (float, Half)[] data =
            {
                (MathF.PI, BitConverter.UInt16BitsToHalf(0b0_10000_1001001000)), // 3.140625
                (MathF.E, BitConverter.UInt16BitsToHalf(0b0_10000_0101110000)), // 2.71875
                (-MathF.PI, BitConverter.UInt16BitsToHalf(0b1_10000_1001001000)), // -3.140625
                (-MathF.E, BitConverter.UInt16BitsToHalf(0b1_10000_0101110000)), // -2.71875
                (float.MaxValue, Half.PositiveInfinity), // Overflow
                (float.MinValue, Half.NegativeInfinity), // Overflow
                (float.PositiveInfinity, Half.PositiveInfinity), // Overflow
                (float.NegativeInfinity, Half.NegativeInfinity), // Overflow
                (float.NaN, Half.NaN), // Quiet Negative NaN
                (BitConverter.Int32BitsToSingle(0x7FC00000), BitConverter.UInt16BitsToHalf(0b0_11111_1000000000)), // Quiet Positive NaN
                (BitConverter.Int32BitsToSingle(unchecked((int)0xFFD55555)),
                    BitConverter.UInt16BitsToHalf(0b1_11111_1010101010)), // Signalling Negative NaN
                (BitConverter.Int32BitsToSingle(0x7FD55555), BitConverter.UInt16BitsToHalf(0b0_11111_1010101010)), // Signalling Positive NaN
                (float.Epsilon, BitConverter.UInt16BitsToHalf(0)), // Underflow
                (-float.Epsilon, BitConverter.UInt16BitsToHalf(0b1_00000_0000000000)), // Underflow
                (1f, BitConverter.UInt16BitsToHalf(0b0_01111_0000000000)), // 1
                (-1f, BitConverter.UInt16BitsToHalf(0b1_01111_0000000000)), // -1
                (0f, BitConverter.UInt16BitsToHalf(0)), // 0
                (-0f, BitConverter.UInt16BitsToHalf(0b1_00000_0000000000)), // -0
                (42f, BitConverter.UInt16BitsToHalf(0b0_10100_0101000000)), // 42
                (-42f, BitConverter.UInt16BitsToHalf(0b1_10100_0101000000)), // -42
                (0.1f, BitConverter.UInt16BitsToHalf(0b0_01011_1001100110)), // 0.0999755859375
                (-0.1f, BitConverter.UInt16BitsToHalf(0b1_01011_1001100110)), // -0.0999755859375
                (1.5f, BitConverter.UInt16BitsToHalf(0b0_01111_1000000000)), // 1.5
                (-1.5f, BitConverter.UInt16BitsToHalf(0b1_01111_1000000000)), // -1.5
                (1.5009765625f, BitConverter.UInt16BitsToHalf(0b0_01111_1000000001)), // 1.5009765625
                (-1.5009765625f, BitConverter.UInt16BitsToHalf(0b1_01111_1000000001)), // -1.5009765625
                (BitConverter.Int32BitsToSingle(0x38800000), BitConverter.UInt16BitsToHalf(0b0_00001_0000000000)), // smallest normal
                (BitConverter.Int32BitsToSingle(0x387FC000), BitConverter.UInt16BitsToHalf(0b0_00000_1111111111)), // largest subnormal
                (BitConverter.Int32BitsToSingle(0x38000000), BitConverter.UInt16BitsToHalf(0b0_00000_1000000000)), // middle subnormal
                (BitConverter.Int32BitsToSingle(0x37FF8000), BitConverter.UInt16BitsToHalf(0b0_00000_0111111111)), // just below middle subnormal
                (BitConverter.Int32BitsToSingle(0x33800000), BitConverter.UInt16BitsToHalf(0b0_00000_0000000001)), // smallest subnormal
                (BitConverter.Int32BitsToSingle(unchecked((int)0xB3800000)),
                    BitConverter.UInt16BitsToHalf(0b1_00000_0000000001)), // highest negative subnormal
                (BitConverter.Int32BitsToSingle(unchecked((int)0xB7FF8000)),
                    BitConverter.UInt16BitsToHalf(0b1_00000_0111111111)), // just above negative middle subnormal
                (BitConverter.Int32BitsToSingle(unchecked((int)0xB8000000)),
                    BitConverter.UInt16BitsToHalf(0b1_00000_1000000000)), // negative middle subnormal
                (BitConverter.Int32BitsToSingle(unchecked((int)0xB87FC000)),
                    BitConverter.UInt16BitsToHalf(0b1_00000_1111111111)), // lowest negative subnormal
                (BitConverter.Int32BitsToSingle(unchecked((int)0xB8800000)),
                    BitConverter.UInt16BitsToHalf(0b1_00001_0000000000)), // highest negative normal
                (BitConverter.Int32BitsToSingle(0b0_10001001_00000000111000000000001),
                                  BitConverter.UInt16BitsToHalf(0b0_11001_0000000100)), // 1027.5+ULP rounds up
                (BitConverter.Int32BitsToSingle(0b0_10001001_00000000111000000000000),
                                  BitConverter.UInt16BitsToHalf(0b0_11001_0000000100)), // 1027.5 rounds to even
                (BitConverter.Int32BitsToSingle(0b0_10001001_00000000110111111111111),
                                  BitConverter.UInt16BitsToHalf(0b0_11001_0000000011)), // 1027.5-ULP rounds down
                (BitConverter.Int32BitsToSingle(unchecked((int)0b1_10001001_00000000110111111111111)),
                                                 BitConverter.UInt16BitsToHalf(0b1_11001_0000000011)), // -1027.5+ULP rounds towards zero
                (BitConverter.Int32BitsToSingle(unchecked((int)0b1_10001001_00000000111000000000000)),
                                                 BitConverter.UInt16BitsToHalf(0b1_11001_0000000100)), // -1027.5 rounds to even
                (BitConverter.Int32BitsToSingle(unchecked((int)0b1_10001001_00000000111000000000001)),
                                                 BitConverter.UInt16BitsToHalf(0b1_11001_0000000100)), // -1027.5-ULP rounds away from zero
                (BitConverter.Int32BitsToSingle(0b0_01110000_00000001110000000000001),
                                 BitConverter.UInt16BitsToHalf(0b0_00000_1000000100)), // subnormal + ULP rounds up
                (BitConverter.Int32BitsToSingle(0b0_01110000_00000001110000000000000),
                                 BitConverter.UInt16BitsToHalf(0b0_00000_1000000100)), // subnormal rounds to even
                (BitConverter.Int32BitsToSingle(0b0_01110000_00000001101111111111111),
                                 BitConverter.UInt16BitsToHalf(0b0_00000_1000000011)), // subnormal - ULP rounds down
                (BitConverter.Int32BitsToSingle(unchecked((int)0b1_01110000_00000001101111111111111)),
                                                BitConverter.UInt16BitsToHalf(0b1_00000_1000000011)), // neg subnormal + ULP rounds higher
                (BitConverter.Int32BitsToSingle(unchecked((int)0b1_01110000_00000001110000000000000)),
                                                BitConverter.UInt16BitsToHalf(0b1_00000_1000000100)), // neg subnormal rounds to even
                (BitConverter.Int32BitsToSingle(unchecked((int)0b1_01110000_00000001101111111111111)),
                                                BitConverter.UInt16BitsToHalf(0b1_00000_1000000011)), // neg subnormal - ULP rounds lower,
                (BitConverter.Int32BitsToSingle(0x33000000), BitConverter.UInt16BitsToHalf(0b0_00000_000000000)), // (half-precision minimum subnormal / 2) should underflow to zero
            };

            foreach ((float original, Half expected) in data)
            {
                yield return new object[] { original, expected };
            }
        }

        [MemberData(nameof(ExplicitConversion_FromSingle_TestData))]
        [Theory]
        public static void ExplicitConversion_FromSingle(float f, Half expected) // Check the underlying bits for verifying NaNs
        {
            Half h = (Half)f;
            Assert.Equal(BitConverter.HalfToUInt16Bits(expected), BitConverter.HalfToUInt16Bits(h));
        }

        public static IEnumerable<object[]> ExplicitConversion_FromDouble_TestData()
        {
            (double, Half)[] data =
            {
                (Math.PI, BitConverter.UInt16BitsToHalf(0b0_10000_1001001000)), // 3.140625
                (Math.E, BitConverter.UInt16BitsToHalf(0b0_10000_0101110000)), // 2.71875
                (-Math.PI, BitConverter.UInt16BitsToHalf(0b1_10000_1001001000)), // -3.140625
                (-Math.E, BitConverter.UInt16BitsToHalf(0b1_10000_0101110000)), // -2.71875
                (double.MaxValue, Half.PositiveInfinity), // Overflow
                (double.MinValue, Half.NegativeInfinity), // Overflow
                (double.PositiveInfinity, Half.PositiveInfinity), // Overflow
                (double.NegativeInfinity, Half.NegativeInfinity), // Overflow
                (double.NaN, Half.NaN), // Quiet Negative NaN
                (BitConverter.Int64BitsToDouble(0x7FF80000_00000000),
                    BitConverter.UInt16BitsToHalf(0b0_11111_1000000000)), // Quiet Positive NaN
                (BitConverter.Int64BitsToDouble(unchecked((long)0xFFFAAAAA_AAAAAAAA)),
                    BitConverter.UInt16BitsToHalf(0b1_11111_1010101010)), // Signalling Negative NaN
                (BitConverter.Int64BitsToDouble(0x7FFAAAAA_AAAAAAAA),
                    BitConverter.UInt16BitsToHalf(0b0_11111_1010101010)), // Signalling Positive NaN
                (double.Epsilon, BitConverter.UInt16BitsToHalf(0)), // Underflow
                (-double.Epsilon, BitConverter.UInt16BitsToHalf(0b1_00000_0000000000)), // Underflow
                (1d, BitConverter.UInt16BitsToHalf(0b0_01111_0000000000)), // 1
                (-1d, BitConverter.UInt16BitsToHalf(0b1_01111_0000000000)), // -1
                (0d, BitConverter.UInt16BitsToHalf(0)), // 0
                (-0d, BitConverter.UInt16BitsToHalf(0b1_00000_0000000000)), // -0
                (42d, BitConverter.UInt16BitsToHalf(0b0_10100_0101000000)), // 42
                (-42d, BitConverter.UInt16BitsToHalf(0b1_10100_0101000000)), // -42
                (0.1d, BitConverter.UInt16BitsToHalf(0b0_01011_1001100110)), // 0.0999755859375
                (-0.1d, BitConverter.UInt16BitsToHalf(0b1_01011_1001100110)), // -0.0999755859375
                (1.5d, BitConverter.UInt16BitsToHalf(0b0_01111_1000000000)), // 1.5
                (-1.5d, BitConverter.UInt16BitsToHalf(0b1_01111_1000000000)), // -1.5
                (1.5009765625d, BitConverter.UInt16BitsToHalf(0b0_01111_1000000001)), // 1.5009765625
                (-1.5009765625d, BitConverter.UInt16BitsToHalf(0b1_01111_1000000001)), // -1.5009765625
                (BitConverter.Int64BitsToDouble(0x3F10000000000000),
                    BitConverter.UInt16BitsToHalf(0b0_00001_0000000000)), // smallest normal
                (BitConverter.Int64BitsToDouble(0x3F0FF80000000000),
                    BitConverter.UInt16BitsToHalf(0b0_00000_1111111111)), // largest subnormal
                (BitConverter.Int64BitsToDouble(0x3f00000000000000),
                    BitConverter.UInt16BitsToHalf(0b0_00000_1000000000)), // middle subnormal
                (BitConverter.Int64BitsToDouble(0x3EFFF00000000000),
                    BitConverter.UInt16BitsToHalf(0b0_00000_0111111111)), // just below middle subnormal
                (BitConverter.Int64BitsToDouble(0x3E70000000000000),
                    BitConverter.UInt16BitsToHalf(0b0_00000_0000000001)), // smallest subnormal
                (BitConverter.Int64BitsToDouble(unchecked((long)0xBE70000000000000)),
                    BitConverter.UInt16BitsToHalf(0b1_00000_0000000001)), // highest negative subnormal
                (BitConverter.Int64BitsToDouble(unchecked((long)0xBEFFF00000000000)),
                    BitConverter.UInt16BitsToHalf(0b1_00000_0111111111)), // just above negative middle subnormal
                (BitConverter.Int64BitsToDouble(unchecked((long)0xBF00000000000000)),
                    BitConverter.UInt16BitsToHalf(0b1_00000_1000000000)), // negative middle subnormal
                (BitConverter.Int64BitsToDouble(unchecked((long)0xBF0FF80000000000)),
                    BitConverter.UInt16BitsToHalf(0b1_00000_1111111111)), // lowest negative subnormal
                (BitConverter.Int64BitsToDouble(unchecked((long)0xBF10000000000000)),
                    BitConverter.UInt16BitsToHalf(0b1_00001_0000000000)), // highest negative normal
                (BitConverter.Int64BitsToDouble(0x40900E0000000001),
                    BitConverter.UInt16BitsToHalf(0b0_11001_0000000100)), // 1027.5+ULP rounds up
                (BitConverter.Int64BitsToDouble(0x40900E0000000000),
                    BitConverter.UInt16BitsToHalf(0b0_11001_0000000100)), // 1027.5 rounds to even
                (BitConverter.Int64BitsToDouble(0x40900DFFFFFFFFFF),
                    BitConverter.UInt16BitsToHalf(0b0_11001_0000000011)), // 1027.5-ULP rounds down
                (BitConverter.Int64BitsToDouble(unchecked((long)0xC0900DFFFFFFFFFF)),
                    BitConverter.UInt16BitsToHalf(0b1_11001_0000000011)), // -1027.5+ULP rounds towards zero
                (BitConverter.Int64BitsToDouble(unchecked((long)0xC0900E0000000000)),
                    BitConverter.UInt16BitsToHalf(0b1_11001_0000000100)), // -1027.5 rounds to even
                (BitConverter.Int64BitsToDouble(unchecked((long)0xC0900E0000000001)),
                    BitConverter.UInt16BitsToHalf(0b1_11001_0000000100)), // -1027.5-ULP rounds away from zero
                (BitConverter.Int64BitsToDouble(0x3F001C0000000001),
                    BitConverter.UInt16BitsToHalf(0b0_00000_1000000100)), // subnormal + ULP rounds up
                (BitConverter.Int64BitsToDouble(0x3F001C0000000001),
                    BitConverter.UInt16BitsToHalf(0b0_00000_1000000100)), // subnormal rounds to even
                (BitConverter.Int64BitsToDouble(0x3F001BFFFFFFFFFF),
                    BitConverter.UInt16BitsToHalf(0b0_00000_1000000011)), // subnormal - ULP rounds down
                (BitConverter.Int64BitsToDouble(unchecked((long)0xBF001BFFFFFFFFFF)),
                    BitConverter.UInt16BitsToHalf(0b1_00000_1000000011)), // neg subnormal + ULP rounds higher
                (BitConverter.Int64BitsToDouble(unchecked((long)0xBF001C0000000000)),
                    BitConverter.UInt16BitsToHalf(0b1_00000_1000000100)), // neg subnormal rounds to even
                (BitConverter.Int64BitsToDouble(unchecked((long)0xBF001C0000000001)),
                    BitConverter.UInt16BitsToHalf(0b1_00000_1000000100)), // neg subnormal - ULP rounds lower
                (BitConverter.Int64BitsToDouble(0x3E60000000000000), BitConverter.UInt16BitsToHalf(0b0_00000_000000000)), // (half-precision minimum subnormal / 2) should underflow to zero
            };

            foreach ((double original, Half expected) in data)
            {
                yield return new object[] { original, expected };
            }
        }

        [MemberData(nameof(ExplicitConversion_FromDouble_TestData))]
        [Theory]
        public static void ExplicitConversion_FromDouble(double d, Half expected) // Check the underlying bits for verifying NaNs
        {
            Half h = (Half)d;
            Assert.Equal(BitConverter.HalfToUInt16Bits(expected), BitConverter.HalfToUInt16Bits(h));
        }

        public static IEnumerable<object[]> Parse_Valid_TestData()
        {
            NumberStyles defaultStyle = NumberStyles.Float | NumberStyles.AllowThousands;

            NumberFormatInfo emptyFormat = NumberFormatInfo.CurrentInfo;

            var dollarSignCommaSeparatorFormat = new NumberFormatInfo()
            {
                CurrencySymbol = "$",
                CurrencyGroupSeparator = ","
            };

            var decimalSeparatorFormat = new NumberFormatInfo()
            {
                NumberDecimalSeparator = "."
            };

            NumberFormatInfo invariantFormat = NumberFormatInfo.InvariantInfo;

            yield return new object[] { "-123", defaultStyle, null, -123.0f };
            yield return new object[] { "0", defaultStyle, null, 0.0f };
            yield return new object[] { "123", defaultStyle, null, 123.0f };
            yield return new object[] { "  123  ", defaultStyle, null, 123.0f };
            yield return new object[] { (567.89f).ToString(), defaultStyle, null, 567.89f };
            yield return new object[] { (-567.89f).ToString(), defaultStyle, null, -567.89f };
            yield return new object[] { "1E23", defaultStyle, null, 1E23f };

            yield return new object[] { emptyFormat.NumberDecimalSeparator + "234", defaultStyle, null, 0.234f };
            yield return new object[] { "234" + emptyFormat.NumberDecimalSeparator, defaultStyle, null, 234.0f };
            yield return new object[] { new string('0', 13) + "65504" + emptyFormat.NumberDecimalSeparator, defaultStyle, null, 65504f };
            yield return new object[] { new string('0', 14) + "65504" + emptyFormat.NumberDecimalSeparator, defaultStyle, null, 65504f };

            // 2^11 + 1. Not exactly representable
            yield return new object[] { "2049.0", defaultStyle, invariantFormat, 2048.0f };
            yield return new object[] { "2049.000000000000001", defaultStyle, invariantFormat, 2050.0f };
            yield return new object[] { "2049.0000000000000001", defaultStyle, invariantFormat, 2050.0f };
            yield return new object[] { "2049.00000000000000001", defaultStyle, invariantFormat, 2050.0f };
            yield return new object[] { "5.000000000000000004", defaultStyle, invariantFormat, 5.0f };
            yield return new object[] { "5.0000000000000000004", defaultStyle, invariantFormat, 5.0f };
            yield return new object[] { "5.004", defaultStyle, invariantFormat, 5.004f };
            yield return new object[] { "5.004000000000000000", defaultStyle, invariantFormat, 5.004f };
            yield return new object[] { "5.0040000000000000000", defaultStyle, invariantFormat, 5.004f };
            yield return new object[] { "5.040", defaultStyle, invariantFormat, 5.04f };

            yield return new object[] { "5004.000000000000000", defaultStyle, invariantFormat, 5004.0f };
            yield return new object[] { "50040.0", defaultStyle, invariantFormat, 50040.0f };
            yield return new object[] { "5004", defaultStyle, invariantFormat, 5004.0f };
            yield return new object[] { "050040", defaultStyle, invariantFormat, 50040.0f };
            yield return new object[] { "0.000000000000000000", defaultStyle, invariantFormat, 0.0f };
            yield return new object[] { "0.005", defaultStyle, invariantFormat, 0.005f };
            yield return new object[] { "0.0400", defaultStyle, invariantFormat, 0.04f };
            yield return new object[] { "1200e0", defaultStyle, invariantFormat, 1200.0f };
            yield return new object[] { "120100e-4", defaultStyle, invariantFormat, 12.01f };
            yield return new object[] { "12010.00e-4", defaultStyle, invariantFormat, 1.201f };
            yield return new object[] { "12000e-4", defaultStyle, invariantFormat, 1.2f };
            yield return new object[] { "1200", defaultStyle, invariantFormat, 1200.0f };

            yield return new object[] { (123.1f).ToString(), NumberStyles.AllowDecimalPoint, null, 123.1f };
            yield return new object[] { (1000.0f).ToString("N0"), NumberStyles.AllowThousands, null, 1000.0f };

            yield return new object[] { "123", NumberStyles.Any, emptyFormat, 123.0f };
            yield return new object[] { (123.567f).ToString(), NumberStyles.Any, emptyFormat, 123.567f };
            yield return new object[] { "123", NumberStyles.Float, emptyFormat, 123.0f };
            yield return new object[] { "$1,000", NumberStyles.Currency, dollarSignCommaSeparatorFormat, 1000.0f };
            yield return new object[] { "$1000", NumberStyles.Currency, dollarSignCommaSeparatorFormat, 1000.0f };
            yield return new object[] { "123.123", NumberStyles.Float, decimalSeparatorFormat, 123.123f };
            yield return new object[] { "(123)", NumberStyles.AllowParentheses, decimalSeparatorFormat, -123.0f };

            yield return new object[] { "NaN", NumberStyles.Any, invariantFormat, float.NaN };
            yield return new object[] { "Infinity", NumberStyles.Any, invariantFormat, float.PositiveInfinity };
            yield return new object[] { "-Infinity", NumberStyles.Any, invariantFormat, float.NegativeInfinity };
        }

        [Theory]
        [MemberData(nameof(Parse_Valid_TestData))]
        public static void Parse(string value, NumberStyles style, IFormatProvider provider, float expectedFloat)
        {
            bool isDefaultProvider = provider == null || provider == NumberFormatInfo.CurrentInfo;
            Half result;
            Half expected = (Half)expectedFloat;
            if ((style & ~(NumberStyles.Float | NumberStyles.AllowThousands)) == 0 && style != NumberStyles.None)
            {
                // Use Parse(string) or Parse(string, IFormatProvider)
                if (isDefaultProvider)
                {
                    Assert.True(Half.TryParse(value, out result));
                    Assert.True(expected.Equals(result));

                    Assert.Equal(expected, Half.Parse(value));
                }

                Assert.True(expected.Equals(Half.Parse(value, provider: provider)));
            }

            // Use Parse(string, NumberStyles, IFormatProvider)
            Assert.True(Half.TryParse(value, style, provider, out result));
            Assert.True(expected.Equals(result) || (Half.IsNaN(expected) && Half.IsNaN(result)));

            Assert.True(expected.Equals(Half.Parse(value, style, provider)) || (Half.IsNaN(expected) && Half.IsNaN(result)));

            if (isDefaultProvider)
            {
                // Use Parse(string, NumberStyles) or Parse(string, NumberStyles, IFormatProvider)
                Assert.True(Half.TryParse(value, style, NumberFormatInfo.CurrentInfo, out result));
                Assert.True(expected.Equals(result));

                Assert.True(expected.Equals(Half.Parse(value, style)));
                Assert.True(expected.Equals(Half.Parse(value, style, NumberFormatInfo.CurrentInfo)));
            }
        }

        public static IEnumerable<object[]> Parse_Invalid_TestData()
        {
            NumberStyles defaultStyle = NumberStyles.Float;

            var dollarSignDecimalSeparatorFormat = new NumberFormatInfo();
            dollarSignDecimalSeparatorFormat.CurrencySymbol = "$";
            dollarSignDecimalSeparatorFormat.NumberDecimalSeparator = ".";

            yield return new object[] { null, defaultStyle, null, typeof(ArgumentNullException) };
            yield return new object[] { "", defaultStyle, null, typeof(FormatException) };
            yield return new object[] { " ", defaultStyle, null, typeof(FormatException) };
            yield return new object[] { "Garbage", defaultStyle, null, typeof(FormatException) };

            yield return new object[] { "ab", defaultStyle, null, typeof(FormatException) }; // Hex value
            yield return new object[] { "(123)", defaultStyle, null, typeof(FormatException) }; // Parentheses
            yield return new object[] { (100.0f).ToString("C0"), defaultStyle, null, typeof(FormatException) }; // Currency

            yield return new object[] { (123.456f).ToString(), NumberStyles.Integer, null, typeof(FormatException) }; // Decimal
            yield return new object[] { "  " + (123.456f).ToString(), NumberStyles.None, null, typeof(FormatException) }; // Leading space
            yield return new object[] { (123.456f).ToString() + "   ", NumberStyles.None, null, typeof(FormatException) }; // Leading space
            yield return new object[] { "1E23", NumberStyles.None, null, typeof(FormatException) }; // Exponent

            yield return new object[] { "ab", NumberStyles.None, null, typeof(FormatException) }; // Negative hex value
            yield return new object[] { "  123  ", NumberStyles.None, null, typeof(FormatException) }; // Trailing and leading whitespace
        }

        [Theory]
        [MemberData(nameof(Parse_Invalid_TestData))]
        public static void Parse_Invalid(string value, NumberStyles style, IFormatProvider provider, Type exceptionType)
        {
            bool isDefaultProvider = provider == null || provider == NumberFormatInfo.CurrentInfo;
            Half result;
            if ((style & ~(NumberStyles.Float | NumberStyles.AllowThousands)) == 0 && style != NumberStyles.None && (style & NumberStyles.AllowLeadingWhite) == (style & NumberStyles.AllowTrailingWhite))
            {
                // Use Parse(string) or Parse(string, IFormatProvider)
                if (isDefaultProvider)
                {
                    Assert.False(Half.TryParse(value, out result));
                    Assert.Equal(default(Half), result);

                    Assert.Throws(exceptionType, () => Half.Parse(value));
                }

                Assert.Throws(exceptionType, () => Half.Parse(value, provider: provider));
            }

            // Use Parse(string, NumberStyles, IFormatProvider)
            Assert.False(Half.TryParse(value, style, provider, out result));
            Assert.Equal(default(Half), result);

            Assert.Throws(exceptionType, () => Half.Parse(value, style, provider));

            if (isDefaultProvider)
            {
                // Use Parse(string, NumberStyles) or Parse(string, NumberStyles, IFormatProvider)
                Assert.False(Half.TryParse(value, style, NumberFormatInfo.CurrentInfo, out result));
                Assert.Equal(default(Half), result);

                Assert.Throws(exceptionType, () => Half.Parse(value, style));
                Assert.Throws(exceptionType, () => Half.Parse(value, style, NumberFormatInfo.CurrentInfo));
            }
        }

        public static IEnumerable<object[]> Parse_ValidWithOffsetCount_TestData()
        {
            foreach (object[] inputs in Parse_Valid_TestData())
            {
                yield return new object[] { inputs[0], 0, ((string)inputs[0]).Length, inputs[1], inputs[2], inputs[3] };
            }

            const NumberStyles DefaultStyle = NumberStyles.Float | NumberStyles.AllowThousands;

            yield return new object[] { "-123", 1, 3, DefaultStyle, null, (float)123 };
            yield return new object[] { "-123", 0, 3, DefaultStyle, null, (float)-12 };
            yield return new object[] { "1E23", 0, 3, DefaultStyle, null, (float)1E2 };
            yield return new object[] { "123", 0, 2, NumberStyles.Float, new NumberFormatInfo(), (float)12 };
            yield return new object[] { "$1,000", 1, 3, NumberStyles.Currency, new NumberFormatInfo() { CurrencySymbol = "$", CurrencyGroupSeparator = "," }, (float)10 };
            yield return new object[] { "(123)", 1, 3, NumberStyles.AllowParentheses, new NumberFormatInfo() { NumberDecimalSeparator = "." }, (float)123 };
            yield return new object[] { "-Infinity", 1, 8, NumberStyles.Any, NumberFormatInfo.InvariantInfo, float.PositiveInfinity };
        }

        [Theory]
        [MemberData(nameof(Parse_ValidWithOffsetCount_TestData))]
        public static void Parse_Span_Valid(string value, int offset, int count, NumberStyles style, IFormatProvider provider, float expectedFloat)
        {
            bool isDefaultProvider = provider == null || provider == NumberFormatInfo.CurrentInfo;
            Half result;
            Half expected = (Half)expectedFloat;
            if ((style & ~(NumberStyles.Float | NumberStyles.AllowThousands)) == 0 && style != NumberStyles.None)
            {
                // Use Parse(string) or Parse(string, IFormatProvider)
                if (isDefaultProvider)
                {
                    Assert.True(Half.TryParse(value.AsSpan(offset, count), out result));
                    Assert.Equal(expected, result);

                    Assert.Equal(expected, Half.Parse(value.AsSpan(offset, count)));
                }

                Assert.Equal(expected, Half.Parse(value.AsSpan(offset, count), provider: provider));
            }

            Assert.True(expected.Equals(Half.Parse(value.AsSpan(offset, count), style, provider)) || (Half.IsNaN(expected) && Half.IsNaN(Half.Parse(value.AsSpan(offset, count), style, provider))));

            Assert.True(Half.TryParse(value.AsSpan(offset, count), style, provider, out result));
            Assert.True(expected.Equals(result) || (Half.IsNaN(expected) && Half.IsNaN(result)));
        }

        [Theory]
        [MemberData(nameof(Parse_Invalid_TestData))]
        public static void Parse_Span_Invalid(string value, NumberStyles style, IFormatProvider provider, Type exceptionType)
        {
            if (value != null)
            {
                Assert.Throws(exceptionType, () => float.Parse(value.AsSpan(), style, provider));

                Assert.False(float.TryParse(value.AsSpan(), style, provider, out float result));
                Assert.Equal(0, result);
            }
        }

        public static IEnumerable<object[]> ToString_TestData()
        {
            yield return new object[] { -4570.0f, "G", null, "-4570" };
            yield return new object[] { 0.0f, "G", null, "0" };
            yield return new object[] { 4570.0f, "G", null, "4570" };

            yield return new object[] { float.NaN, "G", null, "NaN" };

            yield return new object[] { 2468.0f, "N", null, "2,468.00" };

            // Changing the negative pattern doesn't do anything without also passing in a format string
            var customNegativePattern = new NumberFormatInfo() { NumberNegativePattern = 0 };
            yield return new object[] { -6310.0f, "G", customNegativePattern, "-6310" };

            var customNegativeSignDecimalGroupSeparator = new NumberFormatInfo()
            {
                NegativeSign = "#",
                NumberDecimalSeparator = "~",
                NumberGroupSeparator = "*"
            };
            yield return new object[] { -2468.0f, "N", customNegativeSignDecimalGroupSeparator, "#2*468~00" };
            yield return new object[] { 2468.0f, "N", customNegativeSignDecimalGroupSeparator, "2*468~00" };

            var customNegativeSignGroupSeparatorNegativePattern = new NumberFormatInfo()
            {
                NegativeSign = "xx", // Set to trash to make sure it doesn't show up
                NumberGroupSeparator = "*",
                NumberNegativePattern = 0
            };
            yield return new object[] { -2468.0f, "N", customNegativeSignGroupSeparatorNegativePattern, "(2*468.00)" };

            NumberFormatInfo invariantFormat = NumberFormatInfo.InvariantInfo;
            yield return new object[] { float.NaN, "G", invariantFormat, "NaN" };
            yield return new object[] { float.PositiveInfinity, "G", invariantFormat, "Infinity" };
            yield return new object[] { float.NegativeInfinity, "G", invariantFormat, "-Infinity" };
        }

        public static IEnumerable<object[]> ToString_TestData_NotNetFramework()
        {
            foreach (var testData in ToString_TestData())
            {
                yield return testData;
            }

            yield return new object[] { Half.MinValue, "G", null, "-65500" };
            yield return new object[] { Half.MaxValue, "G", null, "65500" };

            yield return new object[] { Half.Epsilon, "G", null, "6E-08" };

            NumberFormatInfo invariantFormat = NumberFormatInfo.InvariantInfo;
            yield return new object[] { Half.Epsilon, "G", invariantFormat, "6E-08" };

            yield return new object[] { 32.5f, "C100", invariantFormat, "¤32.5000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" };
            yield return new object[] { 32.5f, "P100", invariantFormat, "3,250.0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %" };
            yield return new object[] { 32.5f, "E100", invariantFormat, "3.2500000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000E+001" };
            yield return new object[] { 32.5f, "F100", invariantFormat, "32.5000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" };
            yield return new object[] { 32.5f, "N100", invariantFormat, "32.5000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" };
        }

        [Fact]
        public static void Test_ToString_NotNetFramework()
        {
            using (new ThreadCultureChange(CultureInfo.InvariantCulture))
            {
                foreach (object[] testdata in ToString_TestData_NotNetFramework())
                {
                    ToStringTest(testdata[0] is float floatData ? (Half)floatData : (Half)testdata[0], (string)testdata[1], (IFormatProvider)testdata[2], (string)testdata[3]);
                }
            }
        }

        private static void ToStringTest(Half f, string format, IFormatProvider provider, string expected)
        {
            bool isDefaultProvider = provider == null;
            if (string.IsNullOrEmpty(format) || format.ToUpperInvariant() == "G")
            {
                if (isDefaultProvider)
                {
                    Assert.Equal(expected, f.ToString());
                    Assert.Equal(expected, f.ToString((IFormatProvider)null));
                }
                Assert.Equal(expected, f.ToString(provider));
            }
            if (isDefaultProvider)
            {
                Assert.Equal(expected.Replace('e', 'E'), f.ToString(format.ToUpperInvariant())); // If format is upper case, then exponents are printed in upper case
                Assert.Equal(expected.Replace('E', 'e'), f.ToString(format.ToLowerInvariant())); // If format is lower case, then exponents are printed in lower case
                Assert.Equal(expected.Replace('e', 'E'), f.ToString(format.ToUpperInvariant(), null));
                Assert.Equal(expected.Replace('E', 'e'), f.ToString(format.ToLowerInvariant(), null));
            }
            Assert.Equal(expected.Replace('e', 'E'), f.ToString(format.ToUpperInvariant(), provider));
            Assert.Equal(expected.Replace('E', 'e'), f.ToString(format.ToLowerInvariant(), provider));
        }

        [Fact]
        public static void ToString_InvalidFormat_ThrowsFormatException()
        {
            Half f = (Half)123.0f;
            Assert.Throws<FormatException>(() => f.ToString("Y")); // Invalid format
            Assert.Throws<FormatException>(() => f.ToString("Y", null)); // Invalid format
            long intMaxPlus1 = (long)int.MaxValue + 1;
            string intMaxPlus1String = intMaxPlus1.ToString();
            Assert.Throws<FormatException>(() => f.ToString("E" + intMaxPlus1String));
        }

        [Fact]
        public static void TryFormat()
        {
            using (new ThreadCultureChange(CultureInfo.InvariantCulture))
            {
                foreach (object[] testdata in ToString_TestData())
                {
                    float localI = (float)testdata[0];
                    string localFormat = (string)testdata[1];
                    IFormatProvider localProvider = (IFormatProvider)testdata[2];
                    string localExpected = (string)testdata[3];

                    try
                    {
                        char[] actual;
                        int charsWritten;

                        // Just right
                        actual = new char[localExpected.Length];
                        Assert.True(localI.TryFormat(actual.AsSpan(), out charsWritten, localFormat, localProvider));
                        Assert.Equal(localExpected.Length, charsWritten);
                        Assert.Equal(localExpected, new string(actual));

                        // Longer than needed
                        actual = new char[localExpected.Length + 1];
                        Assert.True(localI.TryFormat(actual.AsSpan(), out charsWritten, localFormat, localProvider));
                        Assert.Equal(localExpected.Length, charsWritten);
                        Assert.Equal(localExpected, new string(actual, 0, charsWritten));

                        // Too short
                        if (localExpected.Length > 0)
                        {
                            actual = new char[localExpected.Length - 1];
                            Assert.False(localI.TryFormat(actual.AsSpan(), out charsWritten, localFormat, localProvider));
                            Assert.Equal(0, charsWritten);
                        }
                    }
                    catch (Exception exc)
                    {
                        throw new Exception($"Failed on `{localI}`, `{localFormat}`, `{localProvider}`, `{localExpected}`. {exc}");
                    }
                }
            }
        }

        public static IEnumerable<object[]> ToStringRoundtrip_TestData()
        {
            yield return new object[] { Half.NegativeInfinity };
            yield return new object[] { Half.MinValue };
            yield return new object[] { -MathF.PI };
            yield return new object[] { -MathF.E };
            yield return new object[] { -0.845512408f };
            yield return new object[] { -0.0f };
            yield return new object[] { Half.NaN };
            yield return new object[] { 0.0f };
            yield return new object[] { 0.845512408f };
            yield return new object[] { Half.Epsilon };
            yield return new object[] { MathF.E };
            yield return new object[] { MathF.PI };
            yield return new object[] { Half.MaxValue };
            yield return new object[] { Half.PositiveInfinity };

            yield return new object[] { (BitConverter.UInt16BitsToHalf(0b0_00001_0000000000)) }; // smallest normal
            yield return new object[] { (BitConverter.UInt16BitsToHalf(0b0_00000_1111111111)) }; // largest subnormal
            yield return new object[] { (BitConverter.UInt16BitsToHalf(0b0_00000_1000000000)) }; // middle subnormal
            yield return new object[] { (BitConverter.UInt16BitsToHalf(0b0_00000_0111111111)) }; // just below middle subnormal
            yield return new object[] { (BitConverter.UInt16BitsToHalf(0b0_00000_0000000001)) }; // smallest subnormal
            yield return new object[] { (BitConverter.UInt16BitsToHalf(0b1_00000_0000000001)) }; // highest negative subnormal
            yield return new object[] { (BitConverter.UInt16BitsToHalf(0b1_00000_0111111111)) }; // just above negative middle subnormal
            yield return new object[] { (BitConverter.UInt16BitsToHalf(0b1_00000_1000000000)) }; // negative middle subnormal
            yield return new object[] { (BitConverter.UInt16BitsToHalf(0b1_00000_1111111111)) }; // lowest negative subnormal
            yield return new object[] { (BitConverter.UInt16BitsToHalf(0b1_00001_0000000000)) }; // highest negative normal
        }

        [Theory]
        [MemberData(nameof(ToStringRoundtrip_TestData))]
        public static void ToStringRoundtrip(object o_value)
        {
            float value = o_value is float floatValue ? floatValue : (float)(Half)o_value;
            Half result = Half.Parse(value.ToString());
            Assert.Equal(BitConverter.HalfToUInt16Bits((Half)value), BitConverter.HalfToUInt16Bits(result));
        }

        [Theory]
        [MemberData(nameof(ToStringRoundtrip_TestData))]
        public static void ToStringRoundtrip_R(object o_value)
        {
            float value = o_value is float floatValue ? floatValue : (float)(Half)o_value;
            Half result = Half.Parse(value.ToString("R"));
            Assert.Equal(BitConverter.HalfToUInt16Bits((Half)value), BitConverter.HalfToUInt16Bits(result));
        }

        public static IEnumerable<object[]> RoundTripFloat_CornerCases()
        {
            // Magnitude smaller than 2^-24 maps to 0
            yield return new object[] { (Half)(5.2e-20f), 0 };
            yield return new object[] { (Half)(-5.2e-20f), 0 };
            // Magnitude smaller than 2^(map to subnormals
            yield return new object[] { (Half)(1.52e-5f), 1.52e-5f };
            yield return new object[] { (Half)(-1.52e-5f), -1.52e-5f };
            // Normal numbers
            yield return new object[] { (Half)(55.77f), 55.75f };
            yield return new object[] { (Half)(-55.77f), -55.75f };
            // Magnitude smaller than 2^(map to infinity
            yield return new object[] { (Half)(1.7e38f), float.PositiveInfinity };
            yield return new object[] { (Half)(-1.7e38f), float.NegativeInfinity };
            // Infinity and NaN map to infinity and Nan
            yield return new object[] { Half.PositiveInfinity, float.PositiveInfinity };
            yield return new object[] { Half.NegativeInfinity, float.NegativeInfinity };
            yield return new object[] { Half.NaN, float.NaN };
        }

        [Theory]
        [MemberData(nameof(RoundTripFloat_CornerCases))]
        public static void ToSingle(Half half, float verify)
        {
            float f = (float)half;
            Assert.Equal(f, verify, precision: 1);
        }

        [Fact]
        public static void EqualityMethodAndOperator()
        {
            Assert.True(Half.NaN.Equals(Half.NaN));
            Assert.False(Half.NaN == Half.NaN);
            Assert.Equal(Half.NaN, Half.NaN);
        }

        public static IEnumerable<object[]> MaxMagnitudeNumber_TestData()
        {
            yield return new object[] { Half.NegativeInfinity, Half.PositiveInfinity, Half.PositiveInfinity };
            yield return new object[] { Half.PositiveInfinity, Half.NegativeInfinity, Half.PositiveInfinity };
            yield return new object[] { Half.MinValue, Half.MaxValue, Half.MaxValue };
            yield return new object[] { Half.MaxValue, Half.MinValue, Half.MaxValue };
            yield return new object[] { Half.NaN, Half.NaN, Half.NaN };
            yield return new object[] { Half.NaN, (Half)1.0f, (Half)1.0f };
            yield return new object[] { (Half)1.0f, Half.NaN, (Half)1.0f };
            yield return new object[] { Half.PositiveInfinity, Half.NaN, Half.PositiveInfinity };
            yield return new object[] { Half.NegativeInfinity, Half.NaN, Half.NegativeInfinity };
            yield return new object[] { Half.NaN, Half.PositiveInfinity, Half.PositiveInfinity };
            yield return new object[] { Half.NaN, Half.NegativeInfinity, Half.NegativeInfinity };
            yield return new object[] { (Half)(-0.0f), (Half)0.0f, (Half)0.0f };
            yield return new object[] { (Half)0.0f, (Half)(-0.0f), (Half)0.0f };
            yield return new object[] { (Half)2.0f, (Half)(-3.0f), (Half)(-3.0f) };
            yield return new object[] { (Half)(-3.0f), (Half)2.0f, (Half)(-3.0f) };
            yield return new object[] { (Half)3.0f, (Half)(-2.0f), (Half)3.0f };
            yield return new object[] { (Half)(-2.0f), (Half)3.0f, (Half)3.0f };
        }

        [Theory]
        [MemberData(nameof(MaxMagnitudeNumber_TestData))]
        public static void MaxMagnitudeNumberTest(Half x, Half y, Half expectedResult)
        {
            AssertExtensions.Equal(expectedResult, Half.MaxMagnitudeNumber(x, y), (Half)0.0f);
        }

        public static IEnumerable<object[]> MaxNumber_TestData()
        {
            yield return new object[] { Half.NegativeInfinity, Half.PositiveInfinity, Half.PositiveInfinity };
            yield return new object[] { Half.PositiveInfinity, Half.NegativeInfinity, Half.PositiveInfinity };
            yield return new object[] { Half.MinValue, Half.MaxValue, Half.MaxValue };
            yield return new object[] { Half.MaxValue, Half.MinValue, Half.MaxValue };
            yield return new object[] { Half.NaN, Half.NaN, Half.NaN };
            yield return new object[] { Half.NaN, (Half)1.0f, (Half)1.0f };
            yield return new object[] { (Half)1.0f, Half.NaN, (Half)1.0f };
            yield return new object[] { Half.PositiveInfinity, Half.NaN, Half.PositiveInfinity };
            yield return new object[] { Half.NegativeInfinity, Half.NaN, Half.NegativeInfinity };
            yield return new object[] { Half.NaN, Half.PositiveInfinity, Half.PositiveInfinity };
            yield return new object[] { Half.NaN, Half.NegativeInfinity, Half.NegativeInfinity };
            yield return new object[] { (Half)(-0.0f), (Half)0.0f, (Half)0.0f };
            yield return new object[] { (Half)0.0f, (Half)(-0.0f), (Half)0.0f };
            yield return new object[] { (Half)2.0f, (Half)(-3.0f), (Half)2.0f };
            yield return new object[] { (Half)(-3.0f), (Half)2.0f, (Half)2.0f };
            yield return new object[] { (Half)3.0f, (Half)(-2.0f), (Half)3.0f };
            yield return new object[] { (Half)(-2.0f), (Half)3.0f, (Half)3.0f };
        }

        [Theory]
        [MemberData(nameof(MaxNumber_TestData))]
        public static void MaxNumberTest(Half x, Half y, Half expectedResult)
        {
            AssertExtensions.Equal(expectedResult, Half.MaxNumber(x, y), (Half)0.0f);
        }

        public static IEnumerable<object[]> MinMagnitudeNumber_TestData()
        {
            yield return new object[] { Half.NegativeInfinity, Half.PositiveInfinity, Half.NegativeInfinity };
            yield return new object[] { Half.PositiveInfinity, Half.NegativeInfinity, Half.NegativeInfinity };
            yield return new object[] { Half.MinValue, Half.MaxValue, Half.MinValue };
            yield return new object[] { Half.MaxValue, Half.MinValue, Half.MinValue };
            yield return new object[] { Half.NaN, Half.NaN, Half.NaN };
            yield return new object[] { Half.NaN, (Half)1.0f, (Half)1.0f };
            yield return new object[] { (Half)1.0f, Half.NaN, (Half)1.0f };
            yield return new object[] { Half.PositiveInfinity, Half.NaN, Half.PositiveInfinity };
            yield return new object[] { Half.NegativeInfinity, Half.NaN, Half.NegativeInfinity };
            yield return new object[] { Half.NaN, Half.PositiveInfinity, Half.PositiveInfinity };
            yield return new object[] { Half.NaN, Half.NegativeInfinity, Half.NegativeInfinity };
            yield return new object[] { (Half)(-0.0f), (Half)0.0f, (Half)(-0.0f) };
            yield return new object[] { (Half)0.0f, (Half)(-0.0f), (Half)(-0.0f) };
            yield return new object[] { (Half)2.0f, (Half)(-3.0f), (Half)2.0f };
            yield return new object[] { (Half)(-3.0f), (Half)2.0f, (Half)2.0f };
            yield return new object[] { (Half)3.0f, (Half)(-2.0f), (Half)(-2.0f) };
            yield return new object[] { (Half)(-2.0f), (Half)3.0f, (Half)(-2.0f) };
        }

        [Theory]
        [MemberData(nameof(MinMagnitudeNumber_TestData))]
        public static void MinMagnitudeNumberTest(Half x, Half y, Half expectedResult)
        {
            AssertExtensions.Equal(expectedResult, Half.MinMagnitudeNumber(x, y), (Half)0.0f);
        }

        public static IEnumerable<object[]> MinNumber_TestData()
        {
            yield return new object[] { Half.NegativeInfinity, Half.PositiveInfinity, Half.NegativeInfinity };
            yield return new object[] { Half.PositiveInfinity, Half.NegativeInfinity, Half.NegativeInfinity };
            yield return new object[] { Half.MinValue, Half.MaxValue, Half.MinValue };
            yield return new object[] { Half.MaxValue, Half.MinValue, Half.MinValue };
            yield return new object[] { Half.NaN, Half.NaN, Half.NaN };
            yield return new object[] { Half.NaN, (Half)1.0f, (Half)1.0f };
            yield return new object[] { (Half)1.0f, Half.NaN, (Half)1.0f };
            yield return new object[] { Half.PositiveInfinity, Half.NaN, Half.PositiveInfinity };
            yield return new object[] { Half.NegativeInfinity, Half.NaN, Half.NegativeInfinity };
            yield return new object[] { Half.NaN, Half.PositiveInfinity, Half.PositiveInfinity };
            yield return new object[] { Half.NaN, Half.NegativeInfinity, Half.NegativeInfinity };
            yield return new object[] { (Half)(-0.0f), (Half)0.0f, (Half)(-0.0f) };
            yield return new object[] { (Half)0.0f, (Half)(-0.0f), (Half)(-0.0f) };
            yield return new object[] { (Half)2.0f, (Half)(-3.0f), (Half)(-3.0f) };
            yield return new object[] { (Half)(-3.0f), (Half)2.0f, (Half)(-3.0f) };
            yield return new object[] { (Half)3.0f, (Half)(-2.0f), (Half)(-2.0f) };
            yield return new object[] { (Half)(-2.0f), (Half)3.0f, (Half)(-2.0f) };
        }

        [Theory]
        [MemberData(nameof(MinNumber_TestData))]
        public static void MinNumberTest(Half x, Half y, Half expectedResult)
        {
            AssertExtensions.Equal(expectedResult, Half.MinNumber(x, y), (Half)0.0f);
        }

        public static IEnumerable<object[]> ExpM1_TestData()
        {
            yield return new object[] {  Half.NegativeInfinity, (Half)(-1.0f),                  CrossPlatformMachineEpsilon * (Half)10 };
            yield return new object[] { (Half)(-3.14159265f),   (Half)(-0.956786082f),          CrossPlatformMachineEpsilon };             // value: -(pi)
            yield return new object[] { (Half)(-2.71828183f),   (Half)(-0.934011964f),          CrossPlatformMachineEpsilon };             // value: -(e)
            yield return new object[] { (Half)(-2.30258509f),   (Half)(-0.9f),                  CrossPlatformMachineEpsilon };             // value: -(ln(10))
            yield return new object[] { (Half)(-1.57079633f),   (Half)(-0.792120424f),          CrossPlatformMachineEpsilon };             // value: -(pi / 2)
            yield return new object[] { (Half)(-1.44269504f),   (Half)(-0.763709912f),          CrossPlatformMachineEpsilon };             // value: -(log2(e))
            yield return new object[] { (Half)(-1.41421356f),   (Half)(-0.756883266f),          CrossPlatformMachineEpsilon };             // value: -(sqrt(2))
            yield return new object[] { (Half)(-1.12837917f),   (Half)(-0.676442736f),          CrossPlatformMachineEpsilon };             // value: -(2 / sqrt(pi))
            yield return new object[] { (Half)(-1.0f),          (Half)(-0.632120559f),          CrossPlatformMachineEpsilon };
            yield return new object[] { (Half)(-0.785398163f),  (Half)(-0.544061872f),          CrossPlatformMachineEpsilon };             // value: -(pi / 4)
            yield return new object[] { (Half)(-0.707106781f),  (Half)(-0.506931309f),          CrossPlatformMachineEpsilon };             // value: -(1 / sqrt(2))
            yield return new object[] { (Half)(-0.693147181f),  (Half)(-0.5f),                  CrossPlatformMachineEpsilon };             // value: -(ln(2))
            yield return new object[] { (Half)(-0.636619772f),  (Half)(-0.470922192f),          CrossPlatformMachineEpsilon };             // value: -(2 / pi)
            yield return new object[] { (Half)(-0.434294482f),  (Half)(-0.352278515f),          CrossPlatformMachineEpsilon };             // value: -(log10(e))
            yield return new object[] { (Half)(-0.318309886f),  (Half)(-0.272622651f),          CrossPlatformMachineEpsilon };             // value: -(1 / pi)
            yield return new object[] { (Half)(-0.0f),          (Half)( 0.0f),                  (Half)0.0f };
            yield return new object[] {  Half.NaN,               Half.NaN,                      (Half)0.0f };
            yield return new object[] { (Half)( 0.0f),          (Half)( 0.0f),                  (Half)0.0f };
            yield return new object[] { (Half)( 0.318309886f),  (Half)( 0.374802227f),          CrossPlatformMachineEpsilon };             // value:  (1 / pi)
            yield return new object[] { (Half)( 0.434294482f),  (Half)( 0.543873444f),          CrossPlatformMachineEpsilon };             // value:  (log10(e))
            yield return new object[] { (Half)( 0.636619772f),  (Half)( 0.890081165f),          CrossPlatformMachineEpsilon };             // value:  (2 / pi)
            yield return new object[] { (Half)( 0.693147181f),  (Half)( 1.0f),                  CrossPlatformMachineEpsilon * (Half)10 };  // value:  (ln(2))
            yield return new object[] { (Half)( 0.707106781f),  (Half)( 1.02811498f),           CrossPlatformMachineEpsilon * (Half)10 };  // value:  (1 / sqrt(2))
            yield return new object[] { (Half)( 0.785398163f),  (Half)( 1.19328005f),           CrossPlatformMachineEpsilon * (Half)10 };  // value:  (pi / 4)
            yield return new object[] { (Half)( 1.0f),          (Half)( 1.71828183f),           CrossPlatformMachineEpsilon * (Half)10 };
            yield return new object[] { (Half)( 1.12837917f),   (Half)( 2.09064302f),           CrossPlatformMachineEpsilon * (Half)10 };  // value:  (2 / sqrt(pi))
            yield return new object[] { (Half)( 1.41421356f),   (Half)( 3.11325038f),           CrossPlatformMachineEpsilon * (Half)10 };  // value:  (sqrt(2))
            yield return new object[] { (Half)( 1.44269504f),   (Half)( 3.23208611f),           CrossPlatformMachineEpsilon * (Half)10 };  // value:  (log2(e))
            yield return new object[] { (Half)( 1.57079633f),   (Half)( 3.81047738f),           CrossPlatformMachineEpsilon * (Half)10 };  // value:  (pi / 2)
            yield return new object[] { (Half)( 2.30258509f),   (Half)( 9.0f),                  CrossPlatformMachineEpsilon * (Half)10 };  // value:  (ln(10))
            yield return new object[] { (Half)( 2.71828183f),   (Half)( 14.1542622f),           CrossPlatformMachineEpsilon * (Half)100 }; // value:  (e)
            yield return new object[] { (Half)( 3.14159265f),   (Half)( 22.1406926f),           CrossPlatformMachineEpsilon * (Half)100 }; // value:  (pi)
            yield return new object[] {  Half.PositiveInfinity,  Half.PositiveInfinity, 0.0 };
        }

        [Theory]
        [MemberData(nameof(ExpM1_TestData))]
        public static void ExpM1Test(Half value, Half expectedResult, Half allowedVariance)
        {
            AssertExtensions.Equal(expectedResult, Half.ExpM1(value), allowedVariance);
        }

        public static IEnumerable<object[]> Exp2_TestData()
        {
            yield return new object[] {  Half.NegativeInfinity, (Half)(0.0f),           (Half)0.0f };
            yield return new object[] { (Half)(-3.14159265f),   (Half)(0.113314732f),   CrossPlatformMachineEpsilon };        // value: -(pi)
            yield return new object[] { (Half)(-2.71828183f),   (Half)(0.151955223f),   CrossPlatformMachineEpsilon };        // value: -(e)
            yield return new object[] { (Half)(-2.30258509f),   (Half)(0.202699566f),   CrossPlatformMachineEpsilon };        // value: -(ln(10))
            yield return new object[] { (Half)(-1.57079633f),   (Half)(0.336622537f),   CrossPlatformMachineEpsilon };        // value: -(pi / 2)
            yield return new object[] { (Half)(-1.44269504f),   (Half)(0.367879441f),   CrossPlatformMachineEpsilon };        // value: -(log2(e))
            yield return new object[] { (Half)(-1.41421356f),   (Half)(0.375214227f),   CrossPlatformMachineEpsilon };        // value: -(sqrt(2))
            yield return new object[] { (Half)(-1.12837917f),   (Half)(0.457429347f),   CrossPlatformMachineEpsilon };        // value: -(2 / sqrt(pi))
            yield return new object[] { (Half)(-1.0f),          (Half)(0.5f),           CrossPlatformMachineEpsilon };
            yield return new object[] { (Half)(-0.785398163f),  (Half)(0.580191810f),   CrossPlatformMachineEpsilon };        // value: -(pi / 4)
            yield return new object[] { (Half)(-0.707106781f),  (Half)(0.612547327f),   CrossPlatformMachineEpsilon };        // value: -(1 / sqrt(2))
            yield return new object[] { (Half)(-0.693147181f),  (Half)(0.618503138f),   CrossPlatformMachineEpsilon };        // value: -(ln(2))
            yield return new object[] { (Half)(-0.636619772f),  (Half)(0.643218242f),   CrossPlatformMachineEpsilon };        // value: -(2 / pi)
            yield return new object[] { (Half)(-0.434294482f),  (Half)(0.740055574f),   CrossPlatformMachineEpsilon };        // value: -(log10(e))
            yield return new object[] { (Half)(-0.318309886f),  (Half)(0.802008879f),   CrossPlatformMachineEpsilon };        // value: -(1 / pi)
            yield return new object[] { (Half)(-0.0f),          (Half)(1.0f),           (Half)0.0f };
            yield return new object[] {  Half.NaN,               Half.NaN,              (Half)0.0f };
            yield return new object[] { (Half)( 0.0f),          (Half)(1.0f),           (Half)0.0f };
            yield return new object[] { (Half)( 0.318309886f),  (Half)(1.24686899f),    CrossPlatformMachineEpsilon * (Half)10 };   // value:  (1 / pi)
            yield return new object[] { (Half)( 0.434294482f),  (Half)(1.35124987f),    CrossPlatformMachineEpsilon * (Half)10 };   // value:  (log10(e))
            yield return new object[] { (Half)( 0.636619772f),  (Half)(1.55468228f),    CrossPlatformMachineEpsilon * (Half)10 };   // value:  (2 / pi)
            yield return new object[] { (Half)( 0.693147181f),  (Half)(1.61680667f),    CrossPlatformMachineEpsilon * (Half)10 };   // value:  (ln(2))
            yield return new object[] { (Half)( 0.707106781f),  (Half)(1.63252692f),    CrossPlatformMachineEpsilon * (Half)10 };   // value:  (1 / sqrt(2))
            yield return new object[] { (Half)( 0.785398163f),  (Half)(1.72356793f),    CrossPlatformMachineEpsilon * (Half)10 };   // value:  (pi / 4)
            yield return new object[] { (Half)( 1.0f),          (Half)(2.0f),           CrossPlatformMachineEpsilon * (Half)10 };
            yield return new object[] { (Half)( 1.12837917f),   (Half)(2.18612996f),    CrossPlatformMachineEpsilon * (Half)10 };   // value:  (2 / sqrt(pi))
            yield return new object[] { (Half)( 1.41421356f),   (Half)(2.66514414f),    CrossPlatformMachineEpsilon * (Half)10 };   // value:  (sqrt(2))
            yield return new object[] { (Half)( 1.44269504f),   (Half)(2.71828183f),    CrossPlatformMachineEpsilon * (Half)10 };   // value:  (log2(e))
            yield return new object[] { (Half)( 1.57079633f),   (Half)(2.97068642f),    CrossPlatformMachineEpsilon * (Half)10 };   // value:  (pi / 2)
            yield return new object[] { (Half)( 2.30258509f),   (Half)(4.93340967f),    CrossPlatformMachineEpsilon * (Half)10 };   // value:  (ln(10))
            yield return new object[] { (Half)( 2.71828183f),   (Half)(6.58088599f),    CrossPlatformMachineEpsilon * (Half)10 };   // value:  (e)
            yield return new object[] { (Half)( 3.14159265f),   (Half)(8.82497783f),    CrossPlatformMachineEpsilon * (Half)10 };   // value:  (pi)
            yield return new object[] {  Half.PositiveInfinity,  Half.PositiveInfinity, 0.0f };
        }

        [Theory]
        [MemberData(nameof(Exp2_TestData))]
        public static void Exp2Test(Half value, Half expectedResult, Half allowedVariance)
        {
            AssertExtensions.Equal(expectedResult, Half.Exp2(value), allowedVariance);
        }

        public static IEnumerable<object[]> Exp2M1_TestData()
        {
            yield return new object[] {  Half.NegativeInfinity, (Half)(-1.0f),          (Half)0.0f };
            yield return new object[] { (Half)(-3.14159265f),   (Half)(-0.886685268f),  CrossPlatformMachineEpsilon };            // value: -(pi)
            yield return new object[] { (Half)(-2.71828183f),   (Half)(-0.848044777f),  CrossPlatformMachineEpsilon };            // value: -(e)
            yield return new object[] { (Half)(-2.30258509f),   (Half)(-0.797300434f),  CrossPlatformMachineEpsilon };            // value: -(ln(10))
            yield return new object[] { (Half)(-1.57079633f),   (Half)(-0.663377463f),  CrossPlatformMachineEpsilon };            // value: -(pi / 2)
            yield return new object[] { (Half)(-1.44269504f),   (Half)(-0.632120559f),  CrossPlatformMachineEpsilon };            // value: -(log2(e))
            yield return new object[] { (Half)(-1.41421356f),   (Half)(-0.624785773f),  CrossPlatformMachineEpsilon };            // value: -(sqrt(2))
            yield return new object[] { (Half)(-1.12837917f),   (Half)(-0.542570653f),  CrossPlatformMachineEpsilon };            // value: -(2 / sqrt(pi))
            yield return new object[] { (Half)(-1.0f),          (Half)(-0.5f),          CrossPlatformMachineEpsilon };
            yield return new object[] { (Half)(-0.785398163f),  (Half)(-0.419808190f),  CrossPlatformMachineEpsilon };            // value: -(pi / 4)
            yield return new object[] { (Half)(-0.707106781f),  (Half)(-0.387452673f),  CrossPlatformMachineEpsilon };            // value: -(1 / sqrt(2))
            yield return new object[] { (Half)(-0.693147181f),  (Half)(-0.381496862f),  CrossPlatformMachineEpsilon };            // value: -(ln(2))
            yield return new object[] { (Half)(-0.636619772f),  (Half)(-0.356781758f),  CrossPlatformMachineEpsilon };            // value: -(2 / pi)
            yield return new object[] { (Half)(-0.434294482f),  (Half)(-0.259944426f),  CrossPlatformMachineEpsilon };            // value: -(log10(e))
            yield return new object[] { (Half)(-0.318309886f),  (Half)(-0.197991121f),  CrossPlatformMachineEpsilon };            // value: -(1 / pi)
            yield return new object[] { (Half)(-0.0f),          (Half)( 0.0f),          (Half)0.0f };
            yield return new object[] {  Half.NaN,               Half.NaN,              (Half)0.0f };
            yield return new object[] { (Half)( 0.0f),          (Half)( 0.0f),          (Half)0.0f };
            yield return new object[] { (Half)( 0.318309886f),  (Half)( 0.246868989f),  CrossPlatformMachineEpsilon };            // value:  (1 / pi)
            yield return new object[] { (Half)( 0.434294482f),  (Half)( 0.351249873f),  CrossPlatformMachineEpsilon };            // value:  (log10(e))
            yield return new object[] { (Half)( 0.636619772f),  (Half)( 0.554682275f),  CrossPlatformMachineEpsilon };            // value:  (2 / pi)
            yield return new object[] { (Half)( 0.693147181f),  (Half)( 0.616806672f),  CrossPlatformMachineEpsilon };            // value:  (ln(2))
            yield return new object[] { (Half)( 0.707106781f),  (Half)( 0.632526919f),  CrossPlatformMachineEpsilon };            // value:  (1 / sqrt(2))
            yield return new object[] { (Half)( 0.785398163f),  (Half)( 0.723567934f),  CrossPlatformMachineEpsilon };            // value:  (pi / 4)
            yield return new object[] { (Half)( 1.0f),          (Half)( 1.0f),          CrossPlatformMachineEpsilon * (Half)10 };
            yield return new object[] { (Half)( 1.12837917f),   (Half)( 1.18612996f),   CrossPlatformMachineEpsilon * (Half)10 }; // value:  (2 / sqrt(pi))
            yield return new object[] { (Half)( 1.41421356f),   (Half)( 1.66514414f),   CrossPlatformMachineEpsilon * (Half)10 }; // value:  (sqrt(2))
            yield return new object[] { (Half)( 1.44269504f),   (Half)( 1.71828183f),   CrossPlatformMachineEpsilon * (Half)10 }; // value:  (log2(e))
            yield return new object[] { (Half)( 1.57079633f),   (Half)( 1.97068642f),   CrossPlatformMachineEpsilon * (Half)10 }; // value:  (pi / 2)
            yield return new object[] { (Half)( 2.30258509f),   (Half)( 3.93340967f),   CrossPlatformMachineEpsilon * (Half)10 }; // value:  (ln(10))
            yield return new object[] { (Half)( 2.71828183f),   (Half)( 5.58088599f),   CrossPlatformMachineEpsilon * (Half)10 }; // value:  (e)
            yield return new object[] { (Half)( 3.14159265f),   (Half)( 7.82497783f),   CrossPlatformMachineEpsilon * (Half)10 }; // value:  (pi)
            yield return new object[] {  Half.PositiveInfinity,  Half.PositiveInfinity, (Half)0.0f };
        }

        [Theory]
        [MemberData(nameof(Exp2M1_TestData))]
        public static void Exp2M1Test(Half value, Half expectedResult, Half allowedVariance)
        {
            AssertExtensions.Equal(expectedResult, Half.Exp2M1(value), allowedVariance);
        }

        public static IEnumerable<object[]> Exp10_TestData()
        {
            yield return new object[] {  Half.NegativeInfinity, (Half)0.0f,             (Half)0.0f };
            yield return new object[] { (Half)(-3.14159265f),   (Half)0.000721784159f,  CrossPlatformMachineEpsilon / (Half)1000 };  // value: -(pi)
            yield return new object[] { (Half)(-2.71828183f),   (Half)0.00191301410f,   CrossPlatformMachineEpsilon / (Half)100 };   // value: -(e)
            yield return new object[] { (Half)(-2.30258509f),   (Half)0.00498212830f,   CrossPlatformMachineEpsilon / (Half)100 };   // value: -(ln(10))
            yield return new object[] { (Half)(-1.57079633f),   (Half)0.0268660410f,    CrossPlatformMachineEpsilon / (Half)10 };    // value: -(pi / 2)
            yield return new object[] { (Half)(-1.44269504f),   (Half)0.0360831928f,    CrossPlatformMachineEpsilon / (Half)10 };    // value: -(log2(e))
            yield return new object[] { (Half)(-1.41421356f),   (Half)0.0385288847f,    CrossPlatformMachineEpsilon / (Half)10 };    // value: -(sqrt(2))
            yield return new object[] { (Half)(-1.12837917f),   (Half)0.0744082059f,    CrossPlatformMachineEpsilon / (Half)10 };    // value: -(2 / sqrt(pi))
            yield return new object[] { (Half)(-1.0f),          (Half)0.1f,             CrossPlatformMachineEpsilon };
            yield return new object[] { (Half)(-0.785398163f),  (Half)0.163908636f,     CrossPlatformMachineEpsilon };         // value: -(pi / 4)
            yield return new object[] { (Half)(-0.707106781f),  (Half)0.196287760f,     CrossPlatformMachineEpsilon };         // value: -(1 / sqrt(2))
            yield return new object[] { (Half)(-0.693147181f),  (Half)0.202699566f,     CrossPlatformMachineEpsilon };         // value: -(ln(2))
            yield return new object[] { (Half)(-0.636619772f),  (Half)0.230876765f,     CrossPlatformMachineEpsilon };         // value: -(2 / pi)
            yield return new object[] { (Half)(-0.434294482f),  (Half)0.367879441f,     CrossPlatformMachineEpsilon };         // value: -(log10(e))
            yield return new object[] { (Half)(-0.318309886f),  (Half)0.480496373f,     CrossPlatformMachineEpsilon };         // value: -(1 / pi)
            yield return new object[] { (Half)(-0.0f),          (Half)1.0f,             (Half)0.0f };
            yield return new object[] {  Half.NaN,               Half.NaN,              (Half)0.0f };
            yield return new object[] { (Half)( 0.0f),          (Half)1.0f,             (Half)0.0f };
            yield return new object[] { (Half)( 0.318309886f),  (Half)2.08118116f,      CrossPlatformMachineEpsilon * (Half)10 };    // value:  (1 / pi)
            yield return new object[] { (Half)( 0.434294482f),  (Half)2.71828183f,      CrossPlatformMachineEpsilon * (Half)10 };    // value:  (log10(e))
            yield return new object[] { (Half)( 0.636619772f),  (Half)4.33131503f,      CrossPlatformMachineEpsilon * (Half)10 };    // value:  (2 / pi)
            yield return new object[] { (Half)( 0.693147181f),  (Half)4.93340967f,      CrossPlatformMachineEpsilon * (Half)10 };    // value:  (ln(2))
            yield return new object[] { (Half)( 0.707106781f),  (Half)5.09456117f,      CrossPlatformMachineEpsilon * (Half)10 };    // value:  (1 / sqrt(2))
            yield return new object[] { (Half)( 0.785398163f),  (Half)6.10095980f,      CrossPlatformMachineEpsilon * (Half)10 };    // value:  (pi / 4)
            yield return new object[] { (Half)( 1.0f),          (Half)10.0f,            CrossPlatformMachineEpsilon * (Half)100 };
            yield return new object[] { (Half)( 1.12837917f),   (Half)13.4393779f,      CrossPlatformMachineEpsilon * (Half)100 };   // value:  (2 / sqrt(pi))
            yield return new object[] { (Half)( 1.41421356f),   (Half)25.9545535f,      CrossPlatformMachineEpsilon * (Half)100 };   // value:  (sqrt(2))
            yield return new object[] { (Half)( 1.44269504f),   (Half)27.7137338f,      CrossPlatformMachineEpsilon * (Half)100 };   // value:  (log2(e))
            yield return new object[] { (Half)( 1.57079633f),   (Half)37.2217105f,      CrossPlatformMachineEpsilon * (Half)100 };   // value:  (pi / 2)
            yield return new object[] { (Half)( 2.30258509f),   (Half)200.717432f,      CrossPlatformMachineEpsilon * (Half)1000 };  // value:  (ln(10))
            yield return new object[] { (Half)( 2.71828183f),   (Half)522.735300f,      CrossPlatformMachineEpsilon * (Half)1000 };  // value:  (e)
            yield return new object[] { (Half)( 3.14159265f),   (Half)1385.45573f,      CrossPlatformMachineEpsilon * (Half)10000 }; // value:  (pi)
            yield return new object[] {  Half.PositiveInfinity,  Half.PositiveInfinity, (Half)0.0f };
        }

        [Theory]
        [MemberData(nameof(Exp10_TestData))]
        public static void Exp10Test(Half value, Half expectedResult, Half allowedVariance)
        {
            AssertExtensions.Equal(expectedResult, Half.Exp10(value), allowedVariance);
        }

        public static IEnumerable<object[]> Exp10M1_TestData()
        {
            yield return new object[] {  Half.NegativeInfinity, (Half)(-1.0f),          (Half)0.0f };
            yield return new object[] { (Half)(-3.14159265f),   (Half)(-0.999278216f),  CrossPlatformMachineEpsilon };               // value: -(pi)
            yield return new object[] { (Half)(-2.71828183f),   (Half)(-0.998086986f),  CrossPlatformMachineEpsilon };               // value: -(e)
            yield return new object[] { (Half)(-2.30258509f),   (Half)(-0.995017872f),  CrossPlatformMachineEpsilon };               // value: -(ln(10))
            yield return new object[] { (Half)(-1.57079633f),   (Half)(-0.973133959f),  CrossPlatformMachineEpsilon };               // value: -(pi / 2)
            yield return new object[] { (Half)(-1.44269504f),   (Half)(-0.963916807f),  CrossPlatformMachineEpsilon };               // value: -(log2(e))
            yield return new object[] { (Half)(-1.41421356f),   (Half)(-0.961471115f),  CrossPlatformMachineEpsilon };               // value: -(sqrt(2))
            yield return new object[] { (Half)(-1.12837917f),   (Half)(-0.925591794f),  CrossPlatformMachineEpsilon };               // value: -(2 / sqrt(pi))
            yield return new object[] { (Half)(-1.0f),          (Half)(-0.9f),          CrossPlatformMachineEpsilon };
            yield return new object[] { (Half)(-0.785398163f),  (Half)(-0.836091364f),  CrossPlatformMachineEpsilon };               // value: -(pi / 4)
            yield return new object[] { (Half)(-0.707106781f),  (Half)(-0.803712240f),  CrossPlatformMachineEpsilon };               // value: -(1 / sqrt(2))
            yield return new object[] { (Half)(-0.693147181f),  (Half)(-0.797300434f),  CrossPlatformMachineEpsilon };               // value: -(ln(2))
            yield return new object[] { (Half)(-0.636619772f),  (Half)(-0.769123235f),  CrossPlatformMachineEpsilon };               // value: -(2 / pi)
            yield return new object[] { (Half)(-0.434294482f),  (Half)(-0.632120559f),  CrossPlatformMachineEpsilon };               // value: -(log10(e))
            yield return new object[] { (Half)(-0.318309886f),  (Half)(-0.519503627f),  CrossPlatformMachineEpsilon };               // value: -(1 / pi)
            yield return new object[] { (Half)(-0.0f),          (Half)( 0.0f),          (Half)0.0f };
            yield return new object[] {  Half.NaN,               Half.NaN,              (Half)0.0f };
            yield return new object[] { (Half)( 0.0f),          (Half)( 0.0f),          (Half)0.0f };
            yield return new object[] { (Half)( 0.318309886f),  (Half)( 1.08118116f),   CrossPlatformMachineEpsilon * (Half)10 };    // value:  (1 / pi)
            yield return new object[] { (Half)( 0.434294482f),  (Half)( 1.71828183f),   CrossPlatformMachineEpsilon * (Half)10 };    // value:  (log10(e))
            yield return new object[] { (Half)( 0.636619772f),  (Half)( 3.33131503f),   CrossPlatformMachineEpsilon * (Half)10 };    // value:  (2 / pi)
            yield return new object[] { (Half)( 0.693147181f),  (Half)( 3.93340967f),   CrossPlatformMachineEpsilon * (Half)10 };    // value:  (ln(2))
            yield return new object[] { (Half)( 0.707106781f),  (Half)( 4.09456117f),   CrossPlatformMachineEpsilon * (Half)10 };    // value:  (1 / sqrt(2))
            yield return new object[] { (Half)( 0.785398163f),  (Half)( 5.10095980f),   CrossPlatformMachineEpsilon * (Half)10 };    // value:  (pi / 4)
            yield return new object[] { (Half)( 1.0f),          (Half)( 9.0f),          CrossPlatformMachineEpsilon * (Half)10 };
            yield return new object[] { (Half)( 1.12837917f),   (Half)( 12.4393779f),   CrossPlatformMachineEpsilon * (Half)100 };   // value:  (2 / sqrt(pi))
            yield return new object[] { (Half)( 1.41421356f),   (Half)( 24.9545535f),   CrossPlatformMachineEpsilon * (Half)100 };   // value:  (sqrt(2))
            yield return new object[] { (Half)( 1.44269504f),   (Half)( 26.7137338f),   CrossPlatformMachineEpsilon * (Half)100 };   // value:  (log2(e))
            yield return new object[] { (Half)( 1.57079633f),   (Half)( 36.2217105f),   CrossPlatformMachineEpsilon * (Half)100 };   // value:  (pi / 2)
            yield return new object[] { (Half)( 2.30258509f),   (Half)( 199.717432f),   CrossPlatformMachineEpsilon * (Half)1000 };  // value:  (ln(10))
            yield return new object[] { (Half)( 2.71828183f),   (Half)( 521.735300f),   CrossPlatformMachineEpsilon * (Half)1000 };  // value:  (e)
            yield return new object[] { (Half)( 3.14159265f),   (Half)( 1384.45573f),   CrossPlatformMachineEpsilon * (Half)10000 }; // value:  (pi)
            yield return new object[] {  Half.PositiveInfinity,  Half.PositiveInfinity, (Half)0.0f };
        }

        [Theory]
        [MemberData(nameof(Exp10M1_TestData))]
        public static void Exp10M1Test(Half value, Half expectedResult, Half allowedVariance)
        {
            AssertExtensions.Equal(expectedResult, Half.Exp10M1(value), allowedVariance);
        }

        public static IEnumerable<object[]> LogP1_TestData()
        {
            yield return new object[] {  Half.NegativeInfinity,  Half.NaN,              (Half)0.0f };
            yield return new object[] { (Half)(-3.14159265f),    Half.NaN,              (Half)0.0f };                              //                              value: -(pi)
            yield return new object[] { (Half)(-2.71828183f),    Half.NaN,              (Half)0.0f };                              //                              value: -(e)
            yield return new object[] { (Half)(-1.41421356f),    Half.NaN,              (Half)0.0f };                              //                              value: -(sqrt(2))
            yield return new object[] {  Half.NaN,               Half.NaN,              (Half)0.0f };
            yield return new object[] { (Half)(-1.0f),           Half.NegativeInfinity, (Half)0.0f };
            yield return new object[] { (Half)(-0.956786082f),  (Half)(-3.14159265f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(pi)
            yield return new object[] { (Half)(-0.934011964f),  (Half)(-2.71828183f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(e)
            yield return new object[] { (Half)(-0.9f),          (Half)(-2.30258509f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(ln(10))
            yield return new object[] { (Half)(-0.792120424f),  (Half)(-1.57079633f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(pi / 2)
            yield return new object[] { (Half)(-0.763709912f),  (Half)(-1.44269504f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(log2(e))
            yield return new object[] { (Half)(-0.756883266f),  (Half)(-1.41421356f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(sqrt(2))
            yield return new object[] { (Half)(-0.676442736f),  (Half)(-1.12837917f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(2 / sqrt(pi))
            yield return new object[] { (Half)(-0.632120559f),  (Half)(-1.0f),          CrossPlatformMachineEpsilon * (Half)10 };
            yield return new object[] { (Half)(-0.544061872f),  (Half)(-0.785398163f),  CrossPlatformMachineEpsilon };             // expected: -(pi / 4)
            yield return new object[] { (Half)(-0.506931309f),  (Half)(-0.707106781f),  CrossPlatformMachineEpsilon };             // expected: -(1 / sqrt(2))
            yield return new object[] { (Half)(-0.5f),          (Half)(-0.693147181f),  CrossPlatformMachineEpsilon };             // expected: -(ln(2))
            yield return new object[] { (Half)(-0.470922192f),  (Half)(-0.636619772f),  CrossPlatformMachineEpsilon };             // expected: -(2 / pi)
            yield return new object[] { (Half)(-0.0f),          (Half)( 0.0f),          0.0f };
            yield return new object[] { (Half)( 0.0f),          (Half)( 0.0f),          0.0f };
            yield return new object[] { (Half)( 0.374802227f),  (Half)( 0.318309886f),  CrossPlatformMachineEpsilon };             // expected:  (1 / pi)
            yield return new object[] { (Half)( 0.543873444f),  (Half)( 0.434294482f),  CrossPlatformMachineEpsilon };             // expected:  (log10(e))
            yield return new object[] { (Half)( 0.890081165f),  (Half)( 0.636619772f),  CrossPlatformMachineEpsilon };             // expected:  (2 / pi)
            yield return new object[] { (Half)( 1.0f),          (Half)( 0.693147181f),  CrossPlatformMachineEpsilon };             // expected:  (ln(2))
            yield return new object[] { (Half)( 1.02811498f),   (Half)( 0.707106781f),  CrossPlatformMachineEpsilon };             // expected:  (1 / sqrt(2))
            yield return new object[] { (Half)( 1.19328005f),   (Half)( 0.785398163f),  CrossPlatformMachineEpsilon };             // expected:  (pi / 4)
            yield return new object[] { (Half)( 1.71828183f),   (Half)( 1.0f),          CrossPlatformMachineEpsilon * (Half)10 };
            yield return new object[] { (Half)( 2.09064302f),   (Half)( 1.12837917f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (2 / sqrt(pi))
            yield return new object[] { (Half)( 3.11325038f),   (Half)( 1.41421356f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (sqrt(2))
            yield return new object[] { (Half)( 3.23208611f),   (Half)( 1.44269504f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (log2(e))
            yield return new object[] { (Half)( 3.81047738f),   (Half)( 1.57079633f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (pi / 2)
            yield return new object[] { (Half)( 9.0f),          (Half)( 2.30258509f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (ln(10))
            yield return new object[] { (Half)( 14.1542622f),   (Half)( 2.71828183f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (e)
            yield return new object[] { (Half)( 22.1406926f),   (Half)( 3.14159265f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (pi)
            yield return new object[] {  Half.PositiveInfinity,  Half.PositiveInfinity, (Half)0.0f };
        }

        [Theory]
        [MemberData(nameof(LogP1_TestData))]
        public static void LogP1Test(Half value, Half expectedResult, Half allowedVariance)
        {
            AssertExtensions.Equal(expectedResult, Half.LogP1(value), allowedVariance);
        }

        public static IEnumerable<object[]> Log2P1_TestData()
        {
            yield return new object[] {  Half.NegativeInfinity,  Half.NaN,              (Half)0.0f };
            yield return new object[] {  Half.NaN,               Half.NaN,              (Half)0.0f };
            yield return new object[] { (Half)(-1.0f),           Half.NegativeInfinity, (Half)0.0f };
            yield return new object[] { (Half)(-0.886685268f),  (Half)(-3.14159265f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(pi)
            yield return new object[] { (Half)(-0.848044777f),  (Half)(-2.71828183f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(e)
            yield return new object[] { (Half)(-0.797300434f),  (Half)(-2.30258509f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(ln(10))
            yield return new object[] { (Half)(-0.663377463f),  (Half)(-1.57079633f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(pi / 2)
            yield return new object[] { (Half)(-0.632120559f),  (Half)(-1.44269504f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(log2(e))
            yield return new object[] { (Half)(-0.624785773f),  (Half)(-1.41421356f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(sqrt(2))
            yield return new object[] { (Half)(-0.542570653f),  (Half)(-1.12837917f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(2 / sqrt(pi))
            yield return new object[] { (Half)(-0.5f),          (Half)(-1.0f),          CrossPlatformMachineEpsilon * (Half)10 };
            yield return new object[] { (Half)(-0.419808190f),  (Half)(-0.785398163f),  CrossPlatformMachineEpsilon };             // expected: -(pi / 4)
            yield return new object[] { (Half)(-0.387452673f),  (Half)(-0.707106781f),  CrossPlatformMachineEpsilon };             // expected: -(1 / sqrt(2))
            yield return new object[] { (Half)(-0.381496862f),  (Half)(-0.693147181f),  CrossPlatformMachineEpsilon };             // expected: -(ln(2))
            yield return new object[] { (Half)(-0.356781758f),  (Half)(-0.636619772f),  CrossPlatformMachineEpsilon };             // expected: -(2 / pi)
            yield return new object[] { (Half)(-0.259944426f),  (Half)(-0.434294482f),  CrossPlatformMachineEpsilon };             // expected: -(log10(e))
            yield return new object[] { (Half)(-0.197991121f),  (Half)(-0.318309886f),  CrossPlatformMachineEpsilon };             // expected: -(1 / pi)
            yield return new object[] { (Half)(-0.0f),          (Half)( 0.0f),          (Half)0.0f };
            yield return new object[] { (Half)( 0.0f),          (Half)( 0.0f),          (Half)0.0f };
            yield return new object[] { (Half)( 0.246868989f),  (Half)( 0.318309886f),  CrossPlatformMachineEpsilon };             // expected:  (1 / pi)
            yield return new object[] { (Half)( 0.351249873f),  (Half)( 0.434294482f),  CrossPlatformMachineEpsilon };             // expected:  (log10(e))
            yield return new object[] { (Half)( 0.554682275f),  (Half)( 0.636619772f),  CrossPlatformMachineEpsilon };             // expected:  (2 / pi)
            yield return new object[] { (Half)( 0.616806672f),  (Half)( 0.693147181f),  CrossPlatformMachineEpsilon };             // expected:  (ln(2))
            yield return new object[] { (Half)( 0.632526919f),  (Half)( 0.707106781f),  CrossPlatformMachineEpsilon };             // expected:  (1 / sqrt(2))
            yield return new object[] { (Half)( 0.723567934f),  (Half)( 0.785398163f),  CrossPlatformMachineEpsilon };             // expected:  (pi / 4)
            yield return new object[] { (Half)( 1.0f),          (Half)( 1.0f),          CrossPlatformMachineEpsilon * (Half)10 };
            yield return new object[] { (Half)( 1.18612996f),   (Half)( 1.12837917f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (2 / sqrt(pi))
            yield return new object[] { (Half)( 1.66514414f),   (Half)( 1.41421356f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (sqrt(2))
            yield return new object[] { (Half)( 1.71828183f),   (Half)( 1.44269504f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (log2(e))
            yield return new object[] { (Half)( 1.97068642f),   (Half)( 1.57079633f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (pi / 2)
            yield return new object[] { (Half)( 3.93340967f),   (Half)( 2.30258509f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (ln(10))
            yield return new object[] { (Half)( 5.58088599f),   (Half)( 2.71828183f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (e)
            yield return new object[] { (Half)( 7.82497783f),   (Half)( 3.14159265f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (pi)
            yield return new object[] {  Half.PositiveInfinity,  Half.PositiveInfinity, (Half)0.0f };
        }

        [Theory]
        [MemberData(nameof(Log2P1_TestData))]
        public static void Log2P1Test(Half value, Half expectedResult, Half allowedVariance)
        {
            AssertExtensions.Equal(expectedResult, Half.Log2P1(value), allowedVariance);
        }

        public static IEnumerable<object[]> Log10P1_TestData()
        {
            yield return new object[] {  Half.NegativeInfinity,  Half.NaN,              (Half)0.0f };
            yield return new object[] { (Half)(-3.14159265f),    Half.NaN,              (Half)0.0f };                              //                              value: -(pi)
            yield return new object[] { (Half)(-2.71828183f),    Half.NaN,              (Half)0.0f };                              //                              value: -(e)
            yield return new object[] { (Half)(-1.41421356f),    Half.NaN,              (Half)0.0f };                              //                              value: -(sqrt(2))
            yield return new object[] {  Half.NaN,               Half.NaN,              (Half)0.0f };
            yield return new object[] { (Half)(-1.0f),           Half.NegativeInfinity, (Half)0.0f };
            yield return new object[] { (Half)(-0.998086986f),  (Half)(-2.71828183f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(e)
            yield return new object[] { (Half)(-0.995017872f),  (Half)(-2.30258509f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(ln(10))
            yield return new object[] { (Half)(-0.973133959f),  (Half)(-1.57079633f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(pi / 2)
            yield return new object[] { (Half)(-0.963916807f),  (Half)(-1.44269504f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(log2(e))
            yield return new object[] { (Half)(-0.961471115f),  (Half)(-1.41421356f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(sqrt(2))
            yield return new object[] { (Half)(-0.925591794f),  (Half)(-1.12837917f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected: -(2 / sqrt(pi))
            yield return new object[] { (Half)(-0.9f),           (Half)(-1.0f),          CrossPlatformMachineEpsilon * (Half)10 };
            yield return new object[] { (Half)(-0.836091364f),  (Half)(-0.785398163f),  CrossPlatformMachineEpsilon };             // expected: -(pi / 4)
            yield return new object[] { (Half)(-0.803712240f),  (Half)(-0.707106781f),  CrossPlatformMachineEpsilon };             // expected: -(1 / sqrt(2))
            yield return new object[] { (Half)(-0.797300434f),  (Half)(-0.693147181f),  CrossPlatformMachineEpsilon };             // expected: -(ln(2))
            yield return new object[] { (Half)(-0.769123235f),  (Half)(-0.636619772f),  CrossPlatformMachineEpsilon };             // expected: -(2 / pi)
            yield return new object[] { (Half)(-0.632120559f),  (Half)(-0.434294482f),  CrossPlatformMachineEpsilon };             // expected: -(log10(e))
            yield return new object[] { (Half)(-0.519503627f),  (Half)(-0.318309886f),  CrossPlatformMachineEpsilon };             // expected: -(1 / pi)
            yield return new object[] { (Half)(-0.0f),          (Half)( 0.0f),          (Half)0.0f };
            yield return new object[] { (Half)( 0.0f),          (Half)( 0.0f),          (Half)0.0f };
            yield return new object[] { (Half)( 1.08118116f),   (Half)( 0.318309886f),  CrossPlatformMachineEpsilon };             // expected:  (1 / pi)
            yield return new object[] { (Half)( 1.71828183f),   (Half)( 0.434294482f),  CrossPlatformMachineEpsilon };             // expected:  (log10(e))        value: (e)
            yield return new object[] { (Half)( 3.33131503f),   (Half)( 0.636619772f),  CrossPlatformMachineEpsilon };             // expected:  (2 / pi)
            yield return new object[] { (Half)( 3.93340967f),   (Half)( 0.693147181f),  CrossPlatformMachineEpsilon };             // expected:  (ln(2))
            yield return new object[] { (Half)( 4.09456117f),   (Half)( 0.707106781f),  CrossPlatformMachineEpsilon };             // expected:  (1 / sqrt(2))
            yield return new object[] { (Half)( 5.10095980f),   (Half)( 0.785398163f),  CrossPlatformMachineEpsilon };             // expected:  (pi / 4)
            yield return new object[] { (Half)( 9.0f),          (Half)( 1.0f),          CrossPlatformMachineEpsilon * (Half)10 };
            yield return new object[] { (Half)( 12.4393779f),   (Half)( 1.12837917f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (2 / sqrt(pi))
            yield return new object[] { (Half)( 24.9545535f),   (Half)( 1.41421356f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (sqrt(2))
            yield return new object[] { (Half)( 26.7137338f),   (Half)( 1.44269504f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (log2(e))
            yield return new object[] { (Half)( 36.2217105f),   (Half)( 1.57079633f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (pi / 2)
            yield return new object[] { (Half)( 199.717432f),   (Half)( 2.30258509f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (ln(10))
            yield return new object[] { (Half)( 521.735300f),   (Half)( 2.71828183f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (e)
            yield return new object[] { (Half)( 1384.45573f),   (Half)( 3.14159265f),   CrossPlatformMachineEpsilon * (Half)10 };  // expected:  (pi)
            yield return new object[] {  Half.PositiveInfinity,  Half.PositiveInfinity, (Half)0.0f };
        }

        [Theory]
        [MemberData(nameof(Log10P1_TestData))]
        public static void Log10P1Test(Half value, Half expectedResult, Half allowedVariance)
        {
            AssertExtensions.Equal(expectedResult, Half.Log10P1(value), allowedVariance);
        }
    }
}
