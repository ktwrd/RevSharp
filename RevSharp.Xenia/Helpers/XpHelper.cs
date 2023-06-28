using RevSharp.Xenia.Models;

namespace RevSharp.Xenia.Helpers;

public delegate void ExperienceComparisonDelegate(LevelUserModel model, ExperienceMetadata previous, ExperienceMetadata current);
public class ExperienceMetadata
{
    public ulong UserLevel;
    public ulong UserXp;
    public ulong NextLevelXp;
    public ulong CurrentLevelStart;
    public ulong CurrentLevelEnd;
    public ulong CurrentLevelSize;
    public decimal NextLevelProgress;
}
public static class XpHelper
{
    public const int XpPerLevel = 100;
    /// <summary>
    /// Calculate amount of XP to reach that level (in total)
    /// </summary>
    /// <param name="level">Level to calculate for</param>
    /// <returns>Amount of XP</returns>
    public static ulong XpForLevel(ulong level)
    {
        return level * level * 100;
    }

    public static ExperienceMetadata Generate(LevelUserModel model, string serverId)
    {
        ulong xp = 0;
        if (model.ServerPair.TryGetValue(serverId, out var value))
            xp = value;
        return Generate(model, xp);
    }
    public static ExperienceMetadata Generate(LevelUserModel model, ulong xp)
    {
        var level = (ulong)Math.Floor(0.1 * Math.Sqrt(xp));
        var levelStart = XpForLevel(level);
        var levelEnd = XpForLevel(level + 1);
        var levelSize = levelEnd - levelStart;
        var levelPerc = (xp - levelStart) / (decimal)levelSize;
        var data = new ExperienceMetadata()
        {
            UserLevel = level,
            UserXp = xp,
            NextLevelXp = XpForLevel(level + 1),
            CurrentLevelStart = levelStart,
            CurrentLevelEnd = levelEnd,
            CurrentLevelSize = levelEnd - levelStart,
            NextLevelProgress = Math.Round(levelPerc, 3)
        };
        return data;
    }
}