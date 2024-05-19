namespace Wordle.Model
{
    public class Config
    {
        public static int MaxAttempt { get; set; } = 6;
        public static int CurrentIndex { get; set; } = 0;
        public static int CurrentAttempt { get; set; } = 6;
        public static string WordToFind { get; set; } = string.Empty;
        public static List<List<Cell>> List { get; private set; } = [];

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
        public static void Reset()
        {
            List = [];
            for (int i = 0; i < MaxAttempt; i++)
            {
                var ls = new List<Cell>();
                for (var j = 0; j < 5; j++)
                {
                    ls.Add(new Cell
                    {
                        Col = j,
                        Row = i,
                        PlaceHolder = ' '
                    });
                }
                List.Add(ls);
            }
        }
    }
}
