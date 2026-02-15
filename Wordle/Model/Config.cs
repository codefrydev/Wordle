namespace Wordle.Model
{
    public class Config
    {
        public static int MaxAttempt { get; set; } = 6;
        public static int CurrentIndex { get; set; } = 0;
        public static string WordToFind { get; set; } = string.Empty;
        public static List<List<Cell>> List { get; private set; } = [];
        public static string Theme { get; set; } = "light";

        public static string GetKeyBoardColor(int i)
        {
            var dark = Theme == "dark";
            return i switch
            {
                0 => "background-color:#5c5346;color:#ffffff",   /* absent - same both */
                1 => "background-color:#c9a227;color:#ffffff",  /* present, wrong spot */
                2 => "background-color:#4a7c59;color:#ffffff",  /* correct */
                _ => dark ? "background-color:#2d2a26;color:#e8e0d0" : "background-color:#e8e0d0;color:#2c2420"   /* default */
            };
        }

        /// <summary>Returns CSS class for key state so palette variables apply; empty for default.</summary>
        public static string GetKeyBoardClass(int i)
        {
            return i switch { 0 => "wordle-key-absent", 1 => "wordle-key-present", 2 => "wordle-key-correct", _ => "" };
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
            CurrentIndex = 0;
        }
    }
}
