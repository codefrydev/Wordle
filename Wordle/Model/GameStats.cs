namespace Wordle.Model;

public class GameStats
{
    public int Played { get; set; }
    public int Wins { get; set; }
    public int Streak { get; set; }
    public int MaxStreak { get; set; }
    public int[] Distribution { get; set; } = [0, 0, 0, 0, 0, 0];
    public string? LastPlayedDate { get; set; }
}
