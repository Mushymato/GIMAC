using System.Runtime.CompilerServices;

namespace GingerIslandMainlandAdjustments.AtraStuff;

/// <summary>
/// Utilities for generating good randoms.
/// </summary>
public static class RandomUtils
{
    /// <summary>
    /// Returns true with a probability of <paramref name="chance"/>.
    /// </summary>
    /// <param name="random">Random to use.</param>
    /// <param name="chance">Probability.</param>
    /// <returns>true with the probability of <paramref name="chance"/>, false otherwise.</returns>
    [MethodImpl(TKConstants.Hot)]
    public static bool OfChance(this Random random, double chance)
    {
        if (chance <= 0)
        {
            return false;
        }
        else if (chance >= 1)
        {
            return true;
        }
        else
        {
            return random.NextDouble() < chance;
        }
    }

    /// <summary>
    /// Prewarms a random.
    /// </summary>
    /// <param name="random">The random to prewarm.</param>
    /// <returns>The random.</returns>
    public static Random PreWarm(this Random random)
    {
        int prewarm = random.Next(64);
        for (int i = 0; i < prewarm; i++)
        {
            random.NextDouble();
        }

        prewarm = random.Next(64);
        for (int i = 0; i < prewarm; i++)
        {
            random.NextDouble();
        }

        return random;
    }

    /// <summary>
    /// Gets a random seeded by the days played, the unique ID, and another initial factor.
    /// </summary>
    /// <param name="dayFactor">seeding factor.</param>
    /// <param name="initial">seeding factor but a string.</param>
    /// <remarks>Comes prewarmed.</remarks>
    /// <returns>A seeded random.</returns>
    public static Random GetSeededRandom(int dayFactor, string initial) =>
        GetSeededRandom(dayFactor, Game1.hash.GetDeterministicHashCode(initial));

    /// <summary>
    /// Gets a random seeded by the days played, the unique ID, and another initial factor.
    /// </summary>
    /// <param name="dayFactor">seeding factor.</param>
    /// <param name="initial">another seeding factor.</param>
    /// <remarks>Comes prewarmed.</remarks>
    /// <returns>A seeded random.</returns>
    public static Random GetSeededRandom(int dayFactor, int initial)
    {
        unchecked
        {
            Random random =
                new((int)(Game1.uniqueIDForThisGame + (ulong)(dayFactor * Game1.stats.DaysPlayed) ^ (ulong)initial));
            random.PreWarm();
            return random;
        }
    }
}
