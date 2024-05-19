namespace Wordle.Shared
{
    public class Config
    {
        public static int MaxAttempt { get; set; } = 6;
        public static int CurrentAttempt { get; set; } = 6;
        public static string WordToFind { get; set; } = string.Empty;

        public static string GetBackGroundColor(char c, List<char> chars)
        {
            if (WordToFind.Contains(c))
            {
                if (WordToFind.IndexOf(c) == chars.IndexOf(c))
                {
                    return "green";
                }
                return "orange";
            }
            return "grey";
        }
        public static string GetTextColor(char c, List<char> chars)
        {
            if (WordToFind.Contains(c))
            {
                if (WordToFind.IndexOf(c) == chars.IndexOf(c))
                {
                    return "white";
                }
                return "black";
            }
            return "blue";
        }
    }
}
