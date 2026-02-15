using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Wordle.Model;
using Wordle.Components;

namespace Wordle.Pages
{
    public partial class Index
    {
        bool win = false;
        List<List<Cell>> list = [];
        readonly Random random = new();
        const string wordleListPath = "https://raw.githubusercontent.com/PrashantUnity/SomeCoolScripts/main/wordlewords.txt";
        private HashSet<string> hashOfWordData = [];
        private List<string> listOfWordData = [];
        Stack<char> currentword = new();
        private readonly List<List<char>> alphabet =
        [
            [.. "qwertyuiop".ToCharArray()],
            [.. "asdfghjkl".ToCharArray()],
            [.. "zxcvbnm".ToCharArray()]
        ];
        Dictionary<char, int> hashOfCharacter = new();
        private bool showLostModal;
        private bool showHowToPlayModal;
        private bool showStatsModal;
        private bool showWinModal;
        private int? winGuessCount;
        private int? lastSubmittedRow;
        private int? triggerShakeRow;
        private bool isDailyMode = false;
        private GameStats? stats;
        private string? shareMessage;
        private bool shareCopied;
        private string theme = "light";

        [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;

        private string BorderColor => theme == "dark" ? "#e8e0d0" : "#2c2420";
        private string KeyStyleDefault => theme == "dark" ? "background-color:#2d2a26;color:#e8e0d0" : "background-color:#e8e0d0;color:#2c2420";

        private void Reset()
        {
            hashOfCharacter = new();
            currentword = new();
            win = false;
            lastSubmittedRow = null;
            triggerShakeRow = null;
            winGuessCount = null;
            shareCopied = false;
            Config.Reset();
            list = Config.List;
            GetRandomWords();
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("blazorKeyPressed", DotNetObjectReference.Create(this));
            }
        }

        private async Task ToggleThemeAsync()
        {
            var next = theme == "dark" ? "light" : "dark";
            await JSRuntime.InvokeVoidAsync("setWordleTheme", next);
            theme = next;
            Config.Theme = theme;
            StateHasChanged();
        }

        void Restart()
        {
            _ = RecordLossAsync();
            showLostModal = true;
            Reset();
            StateHasChanged();
        }

        void CloseLostModal()
        {
            showLostModal = false;
            StateHasChanged();
        }

        void HelpDialog()
        {
            showHowToPlayModal = true;
            StateHasChanged();
        }

        private async Task OpenStatsModalAsync()
        {
            await LoadStatsAsync();
            showStatsModal = true;
            StateHasChanged();
        }

        private void CloseStatsModal()
        {
            showStatsModal = false;
            StateHasChanged();
        }

        private async Task LoadStatsAsync()
        {
            try
            {
                var result = await JSRuntime.InvokeAsync<GameStatsDto>("getWordleStats");
                if (result != null)
                    stats = new GameStats
                    {
                        Played = result.Played,
                        Wins = result.Wins,
                        Streak = result.Streak,
                        MaxStreak = result.MaxStreak,
                        Distribution = result.Distribution ?? new int[6],
                        LastPlayedDate = result.LastPlayedDate
                    };
                else
                    stats = new GameStats();
            }
            catch
            {
                stats = new GameStats();
            }
        }

        private async Task RecordWinAsync(int guessCount)
        {
            try
            {
                var s = await GetStatsFromJsAsync();
                if (s == null) return;
                s.Played++;
                s.Wins++;
                var today = await GetDateKeyAsync();
                if (s.LastPlayedDate != today)
                {
                    var yesterdayOfToday = YesterdayOf(today);
                    if (s.LastPlayedDate != null && s.LastPlayedDate != yesterdayOfToday)
                        s.Streak = 0;
                    s.Streak++;
                    s.LastPlayedDate = today;
                }
                if (s.Streak > s.MaxStreak) s.MaxStreak = s.Streak;
                if (guessCount >= 1 && guessCount <= 6 && s.Distribution != null && s.Distribution.Length >= guessCount)
                    s.Distribution[guessCount - 1]++;
                await SetStatsToJsAsync(s);
            }
            catch { }
        }

        private async Task RecordLossAsync()
        {
            try
            {
                var s = await GetStatsFromJsAsync();
                if (s == null) return;
                s.Played++;
                s.Streak = 0;
                s.LastPlayedDate = await GetDateKeyAsync();
                await SetStatsToJsAsync(s);
            }
            catch { }
        }

        private static string YesterdayOf(string dateKey)
        {
            if (string.IsNullOrEmpty(dateKey) || dateKey.Length < 10) return dateKey;
            if (!DateTime.TryParse(dateKey, out var d)) return dateKey;
            return d.AddDays(-1).ToString("yyyy-MM-dd");
        }

        private async Task<GameStatsDto?> GetStatsFromJsAsync()
        {
            try
            {
                return await JSRuntime.InvokeAsync<GameStatsDto>("getWordleStats");
            }
            catch { return null; }
        }

        private async Task SetStatsToJsAsync(GameStatsDto dto)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("setWordleStats", dto);
            }
            catch { }
        }

        private async Task<string> GetDateKeyAsync()
        {
            try
            {
                return await JSRuntime.InvokeAsync<string>("getWordleDateKey") ?? "";
            }
            catch { return ""; }
        }

        private async Task CopyShareAsync()
        {
            if (string.IsNullOrEmpty(shareMessage)) return;
            try
            {
                await JSRuntime.InvokeVoidAsync("wordleCopyShare", shareMessage);
                shareCopied = true;
                StateHasChanged();
            }
            catch { }
        }

        private async Task FireConfettiAsync()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("wordleConfetti");
            }
            catch { }
        }

        [JSInvokable]
        public void OnArrowKeyPressed(string key)
        {
            if (win) return;
            if (Config.CurrentIndex >= list.Count)
            {
                Restart();
                return;
            }

            var stackToList = currentword.Reverse().ToList();
            if (key == "Enter")
            {
                if (currentword.Count >= 5 && ValidWord(string.Concat(stackToList)))
                {
                    var submittedRow = Config.CurrentIndex;
                    if (string.Concat(stackToList) == Config.WordToFind)
                    {
                        win = true;
                        winGuessCount = submittedRow + 1;
                        shareMessage = BuildShareMessage(submittedRow + 1);
                        _ = RecordWinAsync(winGuessCount.Value);
                        _ = FireConfettiAsync();
                        showWinModal = true;
                    }
                    Config.CurrentIndex++;
                    var word = Config.WordToFind;
                    for (var col = 0; col < stackToList.Count; col++)
                    {
                        var letter = stackToList[col];
                        int state = word[col] == letter ? 2 : (word.Contains(letter) ? 1 : 0);
                        if (!hashOfCharacter.TryGetValue(letter, out var existing) || state > existing)
                            hashOfCharacter[letter] = state;
                    }
                    currentword = new();
                    lastSubmittedRow = submittedRow;
                    StateHasChanged();
                }
                else if (currentword.Count >= 5)
                {
                    triggerShakeRow = Config.CurrentIndex;
                    StateHasChanged();
                    _ = Task.Delay(450).ContinueWith(_ => InvokeAsync(() => { triggerShakeRow = null; StateHasChanged(); }));
                }
                if (Config.CurrentIndex >= list.Count && !win)
                {
                    Restart();
                }
                StateHasChanged();
                return;
            }
            if (key == "Backspace")
            {
                if (currentword.Count > 0)
                {
                    list[Config.CurrentIndex][currentword.Count - 1].PlaceHolder = ' ';
                    currentword.Pop();
                }
                StateHasChanged();
                return;
            }
            if (key.Length == 1 && currentword.Count < 5)
            {
                var c = key.ToLower()[0];
                if (alphabet[0].Contains(c) || alphabet[1].Contains(c) || alphabet[2].Contains(c))
                {
                    currentword.Push(c);
                    list[Config.CurrentIndex][currentword.Count - 1].PlaceHolder = c;
                }
            }
            StateHasChanged();
        }

        private string BuildShareMessage(int rowsUsed)
        {
            var lines = new List<string>();
            lines.Add("Wordle (1940s) " + (isDailyMode ? DateTime.UtcNow.ToString("yyyy-MM-dd") : "") + " " + rowsUsed + "/6");
            var word = Config.WordToFind;
            for (var r = 0; r < rowsUsed && r < list.Count; r++)
            {
                var row = list[r];
                var line = "";
                for (var c = 0; c < 5; c++)
                {
                    var ch = row[c].PlaceHolder;
                    if (word[c] == ch) line += "🟩";
                    else if (word.Contains(ch)) line += "🟨";
                    else line += "⬜";
                }
                lines.Add(line);
            }
            return string.Join("\n", lines);
        }

        protected override async Task OnInitializedAsync()
        {
            Config.CurrentIndex = 0;
            win = false;
            Config.Reset();
            list = Config.List;
            currentword = new();
            try
            {
                theme = await JSRuntime.InvokeAsync<string>("getWordleTheme");
                Config.Theme = theme;
            }
            catch { }
            using var client = new HttpClient();
            var content = await client.GetAsync(wordleListPath);
            if (content.IsSuccessStatusCode)
            {
                var word = await content.Content.ReadAsStringAsync();
                listOfWordData = new(word.Split(','));
                hashOfWordData = new(listOfWordData);
                GetRandomWords();
            }
        }

        private bool ValidWord(string word) => hashOfWordData.Contains(word);

        private void GetRandomWords()
        {
            if (listOfWordData.Count == 0) return;
            if (isDailyMode)
            {
                var seed = (int)(DateTime.UtcNow.Date.Ticks % int.MaxValue);
                var seeded = new Random(seed);
                Config.WordToFind = listOfWordData[seeded.Next(listOfWordData.Count)];
            }
            else
            {
                Config.WordToFind = listOfWordData[random.Next(listOfWordData.Count)];
            }
        }

        private void ToggleDailyMode(ChangeEventArgs e)
        {
            isDailyMode = e.Value as bool? ?? false;
            Reset();
        }

        private string RowCssClass(int rowIndex)
        {
            var c = "wordle-row";
            if (triggerShakeRow == rowIndex) c += " wordle-row-shake";
            return c;
        }

        private string TileCssClass(int rowIndex, int colIndex)
        {
            var c = "wordle-tile";
            if (lastSubmittedRow == rowIndex) c += " wordle-tile-flip";
            return c;
        }
    }

    public class GameStatsDto
    {
        public int Played { get; set; }
        public int Wins { get; set; }
        public int Streak { get; set; }
        public int MaxStreak { get; set; }
        public int[]? Distribution { get; set; }
        public string? LastPlayedDate { get; set; }
    }
}
