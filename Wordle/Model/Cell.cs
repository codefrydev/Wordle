namespace Wordle.Model
{
    public class Cell
    {
        public char PlaceHolder { get; set; } = ' ';
        public int Row { get; set; }
        public int Col { get; set; }

        public string PlaceHolderToString()=> PlaceHolder.ToString().ToUpper();
        public string GetBackGroundColor()
        {
            var dark = Config.Theme == "dark";
            if (PlaceHolder == ' ' || Config.CurrentIndex == Row) return dark ? "#1a1915" : "#f5f0e6";
            if (Config.WordToFind.Contains(PlaceHolder))
            {
                for (var i = 0; i < Config.WordToFind.Length; i++)
                {
                    if (Config.WordToFind[i] == PlaceHolder && i == Col) return "#4a7c59";
                }
                return "#c9a227";
            }
            return dark ? "#3d3832" : "#5c5346";
        }
        public string GetTextColor()
        {
            if (Config.CurrentIndex == Row) return Config.Theme == "dark" ? "#e8e0d0" : "#2c2420";
            return "#ffffff";
        }

        /// <summary>Returns CSS class for revealed tile state so palette variables apply; empty when empty or current row.</summary>
        public string GetStateClass()
        {
            if (PlaceHolder == ' ' || Config.CurrentIndex == Row) return "";
            var word = Config.WordToFind;
            if (word[Col] == PlaceHolder) return "wordle-tile-state-correct";
            if (word.Contains(PlaceHolder)) return "wordle-tile-state-present";
            return "wordle-tile-state-absent";
        }
    }
}
