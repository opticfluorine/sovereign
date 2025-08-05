// Sovereign Engine
// Copyright (c) 2025 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Security.Cryptography;

namespace Sovereign.EngineUtil.Numerics;

/// <summary>
///     Random number generators.
/// </summary>
/// <remarks>
///     The Rng class provides two random number generation facilities: a cryptographically secure
///     pseudorandom number generator (CSPRNG) and a fast pseudorandom number generator(PRNG). The
///     CSPRNG is implemented using System.Security.Cryptography.RandomNumberGenerator, while the
///     PRNG is implemented using System.Random. The underlying implementations may vary between platforms
///     and .NET runtime versions.
///     <br />
///     The fast PRNG is not cryptographically secure; that is, it is vulnerable to adversarial attacks where
///     the internal state of the PRNG may be fully determined by collecting a number of samples from the fast
///     PRNG (e.g. by applying lattice reduction methods). It is suitable only for non-sensitive random samples,
///     e.g. selecting a random direction of movement for an NPC. It should not be used for situations where a
///     player could gain an unfair advantage by predicting past or future outputs of the PRNG (e.g. loot drops).
///     The tradeoff is that the fast PRNG is fast; that is, it can supply a high throughput of random numbers
///     with low latency, and so is more scalable than the CSPRNG.
///     <br />
///     The CSPRNG is cryptographically secure; it is resistant to adversarial attacks such as lattice reduction.
///     It is very difficult for a player to predict past or future outputs from the CSPRNG. Accordingly, it is
///     suitable for situations where a player could gain an unfair advantage by predicting these values (e.g.
///     loot drops). The tradeoff is that the CSPRNG may be significantly slower than the fast PRNG depending on
///     hardware (in my microbenchmark, ~50x slower with a CPU with the RDRAND instruction, or ~120x slower on a
///     CPU without RDRAND - this is probably driven by limited entropy in the non-RDRAND case, so average use
///     may be closer to the 50x case).
///     <br />
///     As of the time of writing (when Sovereign used .NET 9), the fast PRNG is backed by the xoshiro256**
///     algorithm as implemented in System.Random. There is a limitation with System.Random where the xoshiro256**
///     cannot use a custom seed; providing a custom seed causes System.Random to fall back on a legacy algorithm
///     (a "modified" version of a subtractive algorithm from Knuth). We create a per-thread unique Random instance
///     here rather than using the System.Random.Shared "instance" (actually a set of ThreadStatic instances as of
///     .NET 9) - this gives us the future option to supply a custom seed to every instance if we want to pin
///     the seed for debug purposes.
/// </remarks>
public static class Rng
{
    [ThreadStatic] private static RandomNumberGenerator? _csPrng;
    [ThreadStatic] private static Random? _fastPrng;

    private static bool useCustomSeed;
    private static int customSeed;

    /// <summary>
    ///     Sets a custom seed to be used by the fast PRNG for all future per-thread instances. This has no effect
    ///     on threads where the fast PRNG has already been used prior to calling this method.
    /// </summary>
    /// <param name="seed"></param>
    public static void SetCustomSeed(int seed)
    {
        useCustomSeed = true;
        customSeed = seed;
    }

    /// <summary>
    ///     Gets a non-negative 32-bit integer from the fast PRNG.
    /// </summary>
    /// <returns>Non-negative 32-bit integer.</returns>
    public static int NextInt32()
    {
        _fastPrng ??= useCustomSeed ? new Random(customSeed) : new Random();
        return _fastPrng.Next();
    }

    /// <summary>
    ///     Gets a 32-bit integer from the cryptographically secure pseudorandom number generator (CSPRNG).
    /// </summary>
    /// <returns>Random 32-bit integer.</returns>
    public static int NextInt32Secure()
    {
        Span<byte> span = stackalloc byte[4];
        _csPrng ??= RandomNumberGenerator.Create();
        _csPrng.GetBytes(span);
        return BitConverter.ToInt32(span);
    }

    /// <summary>
    ///     Gets a 64-bit integer from the CSPRNG.
    /// </summary>
    /// <returns>Random 64-bit integer.</returns>
    public static long NextInt64Secure()
    {
        Span<byte> span = stackalloc byte[8];
        _csPrng ??= RandomNumberGenerator.Create();
        _csPrng.GetBytes(span);
        return BitConverter.ToInt64(span);
    }

    /// <summary>
    ///     Gets a pair of 64-bit integers from the CSPRNG.
    /// </summary>
    /// <returns>Pair of random 64-bit integers.</returns>
    public static ValueTuple<long, long> NextInt64PairSecure()
    {
        Span<byte> span = stackalloc byte[16];
        _csPrng ??= RandomNumberGenerator.Create();
        _csPrng.GetBytes(span);
        return (BitConverter.ToInt64(span.Slice(0, 8)), BitConverter.ToInt64(span.Slice(8)));
    }
}