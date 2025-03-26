using System;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace JokeService;

public class Joke
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Setup { get; set; }
    public string Punchline { get; set; }
}

public class MenuOption
{
    public string Name { get; }
    public Func<Task> Action { get; }

    public MenuOption(string name, Func<Task> action)
    {
        Name = name;
        Action = action;
    }
}

// preimenuj JokeProcessor u JokeService
// preimenuj FetchJokes u FetchJokesAsync
public class JokeService
{
    internal static HttpClient _httpClient = new HttpClient();
    private static readonly ConcurrentDictionary<int, Joke> _jokeCache = new ConcurrentDictionary<int, Joke>();
    private static List<MenuOption> _menuOptions;
    public static List<MenuOption> options { get; set; } = new List<MenuOption>();

    public static async Task Main(string[] args)
    {
        /*if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_TEST") == "true")
        {
            return;
        }*/
        
        InitializeHttpClient();
        InitializeMenu();

        await RunMenu();
    }

    private static void InitializeHttpClient()
    {
        _httpClient.BaseAddress = new Uri("https://official-joke-api.appspot.com/");
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private static void InitializeMenu()
    {
        _menuOptions = new List<MenuOption>
        {
            new MenuOption("Fetch jokes (specify number)", FetchJokes),
            new MenuOption("View all stored jokes", ViewAllJokes),
            new MenuOption("Search jokes by type", SearchJokesByType),
            new MenuOption("Remove multiple jokes", RemoveMultipleJokes),
            new MenuOption("Exit", static () => { Environment.Exit(0); return Task.CompletedTask; })
        };
    }

    private static async Task RunMenu()
    {
        int selectedIndex = 0;
        
        while (true)
        {
            RenderMenu(selectedIndex);
            
            var key = Console.ReadKey(true).Key;
            
            switch (key)
            {
                case ConsoleKey.DownArrow:
                    selectedIndex = Math.Min(selectedIndex + 1, _menuOptions.Count - 1);
                    break;
                case ConsoleKey.UpArrow:
                    selectedIndex = Math.Max(selectedIndex - 1, 0);
                    break;
                case ConsoleKey.Enter:
                    Console.Clear();
                    await _menuOptions[selectedIndex].Action();
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private static void RenderMenu(int selectedIndex)
    {
        Console.Clear();
        Console.WriteLine("=== Joke Processor ===");
        
        for (int i = 0; i < _menuOptions.Count; i++)
        {
            Console.Write(i == selectedIndex ? "> " : "  ");
            Console.WriteLine(_menuOptions[i].Name);
        }
    }

    private static async Task FetchJokes()
    {
        try
        {
            Console.Write("Enter number of jokes to fetch: ");
            if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
            {
                Console.WriteLine("Invalid input. Please enter a positive number.");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var response = await _httpClient.GetAsync("random_joke");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error fetching joke: {response.StatusCode}");
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync();
                var joke = JsonConvert.DeserializeObject<Joke>(content);

                if (joke != null && int.TryParse(joke.Id, out int id))
                {
                    _jokeCache[id] = joke;
                    DisplayJoke(joke);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static Task ViewAllJokes()
    {
        if (_jokeCache.IsEmpty)
        {
            Console.WriteLine("No jokes stored.");
            return Task.CompletedTask;
        }

        foreach (var (id, joke) in _jokeCache)
        {
            Console.WriteLine($"--- Joke {id} ---");
            DisplayJoke(joke);
        }

        return Task.CompletedTask;
    }

    private static Task SearchJokesByType()
    {
        Console.Write("Enter joke type to search: ");
        var type = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(type))
        {
            Console.WriteLine("Invalid type.");
            return Task.CompletedTask;
        }

        var found = _jokeCache.Values.Where(j => j.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

        if (!found.Any())
        {
            Console.WriteLine("No jokes found for this type.");
            return Task.CompletedTask;
        }

        foreach (var joke in found)
        {
            DisplayJoke(joke);
        }

        return Task.CompletedTask;
    }

    private static Task RemoveMultipleJokes()
    {
        Console.Write("Enter number of jokes to remove: ");
        if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
        {
            Console.WriteLine("Invalid input.");
            return Task.CompletedTask;
        }

        for (int i = 0; i < count; i++)
        {
            Console.Write($"Enter joke ID to remove ({i + 1}/{count}): ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                continue;
            }

            if (_jokeCache.TryRemove(id, out _))
            {
                Console.WriteLine($"Removed joke {id}");
            }
            else
            {
                Console.WriteLine($"Joke {id} not found");
            }
        }

        return Task.CompletedTask;
    }

    private static void DisplayJoke(Joke joke)
    {
        Console.WriteLine($"Type: {joke.Type}");
        Console.WriteLine($"Setup: {joke.Setup}");
        Console.WriteLine($"Punchline: {joke.Punchline}\n");
    }

    // Extract the core logic into testable methods
    public static bool TryAddJokeFromResponse(HttpResponseMessage response, ref ConcurrentDictionary<int, string> scores)
    {
        if (!response.IsSuccessStatusCode) return false;

        var responseString = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<Joke>(responseString);

        if (result != null && int.TryParse(result.Id, out int id))
        {
            return scores.TryAdd(id, JsonConvert.SerializeObject(result));
        }
        return false;
    }

    public static List<string> GetJokeDisplayStrings(ConcurrentDictionary<int, string> scores)
    {
        var displayStrings = new List<string>();
        foreach (var item in scores)
        {
            var joke = JsonConvert.DeserializeObject<Joke>(item.Value);
            if (joke != null)
            {
                displayStrings.Add($"Joke ID: {item.Key}");
                displayStrings.Add($"Type: {joke.Type}");
                displayStrings.Add($"Setup: {joke.Setup}");
                displayStrings.Add($"Punchline: {joke.Punchline}");
            }
        }
        return displayStrings;
    }
}