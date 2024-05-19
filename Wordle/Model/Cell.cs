using Wordle.Shared;

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
            if (PlaceHolder == ' ' || Config.CurrentIndex ==Row) return "white";
            if (Config.WordToFind.Contains(PlaceHolder))
            {
                for (var i = 0; i < Config.WordToFind.Length; i++)
                { 
                    if (Config.WordToFind[i] == PlaceHolder && i==Col) return "green";
                }
                return "orange";
            }
            return "grey";
        }
        public string GetTextColor()
        {
            if (Config.CurrentIndex == Row) return "black"; 
            return "white";
        }
    }
}
