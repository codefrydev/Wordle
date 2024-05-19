
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Wordle.Model;

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
        [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("blazorKeyPressed", DotNetObjectReference.Create(this));
            }
        }
        [JSInvokable]
        public void OnArrowKeyPressed(string key)
        {
            if (win) return;
            var stackToList = currentword.Reverse().ToList(); 
            if (key == "Enter")
            { 
                if (currentword.Count >= 5 && ValidWord(string.Concat(stackToList)))
                {
                    if(string.Concat(stackToList)==Config.WordToFind)
                    {
                        Console.WriteLine("you win");
                        OpenDialog();
                        win = true;
                    }
                    Config.CurrentIndex++;
                    currentword = new();
                } 
                StateHasChanged();
                return;
            }
            if (stackToList.Count > 0)
            { 
            }
            if (Config.CurrentIndex >= list.Count)
            { 
                return;
            }
            if (key == "Backspace")
            { 
                if (currentword.Count > 0)
                {
                    list[Config.CurrentIndex][currentword.Count - 1].PlaceHolder = ' ';
                    currentword.Pop();
                }
                Console.WriteLine(string.Concat(currentword.Reverse().ToList())); 
                StateHasChanged();
            }
            
            if (key.Length == 1 && currentword.Count < 5)
            { 
                var c = key.ToLower()[0];
                if (alphabet[0].Contains(c) || alphabet[1].Contains(c) || alphabet[2].Contains(c))
                { 
                    currentword.Push(c);
                    list[Config.CurrentIndex][currentword.Count-1].PlaceHolder = c; 
                }
            } 
            StateHasChanged();
        } 
        protected override async Task OnInitializedAsync()
        {
            Config.CurrentIndex = 0;
            win = false;
            Config.Reset();
            list = Config.List;
            currentword = new();  
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
        public bool CheckWord(string word) => word == Config.WordToFind;
        private void GetRandomWords()
        {
            Config.WordToFind = listOfWordData[random.Next(listOfWordData.Count)];
        }
        private void Reset()
        {
            win = false;
            Config.Reset();
            list = Config.List;
            currentword = new(); 
            GetRandomWords();
        }
    }
}
