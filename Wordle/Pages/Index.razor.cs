
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Wordle.Shared;

namespace Wordle.Pages
{
    public partial class Index
    {
        List<List<char>> list = [];
        readonly Random random = new();
        readonly string wordleListPath = "https://raw.githubusercontent.com/PrashantUnity/SomeCoolScripts/main/wordlewords.txt";
        private HashSet<string> hashOfWordData = [];
        private List<string> listOfWordData = [];
        Stack<char> currentword = new();
        int index = 0;
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
        public async Task OnArrowKeyPressed(string key)
        {
            var stackToList = currentword.Reverse().ToList();
            if (stackToList.Count > 0)
            {
                Console.WriteLine("{0}", string.Concat(stackToList));
            }
            if (index >= list.Count)
            {
                Console.WriteLine("Game Over");
            }
            if (key == "Backspace")
            {
                Console.WriteLine("Backspace");
                if (currentword.Count > 0)
                {
                    currentword.Pop();
                }
                MapStackToListWithEmpty(stackToList);
            }
            else if (key == "Enter")
            {
                Console.WriteLine("Enter");
                if (currentword.Count >= 5 && ValidWord(string.Concat(stackToList)))
                {
                    index++;
                    currentword = new();
                }
            }
            else if (key.Length == 1 && currentword.Count != 5)
            {
                Console.WriteLine("currentword.Count != 5");
                var c = key.ToLower()[0];
                if (alphabet[0].Contains(c) || alphabet[1].Contains(c) || alphabet[2].Contains(c))
                {
                    Console.WriteLine($"{c} is pushed Into Stack");
                    currentword.Push(c);
                    MapStackToList(stackToList);
                }
            }
            await Task.Delay(10);
            StateHasChanged();
        }
        void MapStackToList(List<char> stackToList)
        {
            for (var i = 0; i < stackToList.Count; i++)
            {
                list[index][i] = stackToList[i];
            }
            StateHasChanged();
        }
        void MapStackToListWithEmpty(List<char> stackToList)
        {
            for (var i = 0; i < list[index].Count; i++)
            {
                if (i >= stackToList.Count)
                {
                    list[index][i] = ' ';
                }
                else
                {
                    list[index][i] = stackToList[i];
                }
            }
            StateHasChanged();
        }
        protected override async Task OnInitializedAsync()
        {
            index = 0;
            list = [];
            currentword = new();
            for (int i = 0; i < Config.MaxAttempt; i++)
            {
                var ls = new List<char>();
                for (var j = 0; j < 5; j++)
                {
                    ls.Add(' ');
                }
                list.Add(ls);
            }

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
            index = 0;
            list = [];
            for (int i = 0; i < Config.MaxAttempt; i++)
            {
                var ls = new List<char>();
                for (var j = 0; j < 5; j++)
                {
                    ls.Add(' ');
                }
                list.Add(ls);
            }
            GetRandomWords();
        }
    }
}
