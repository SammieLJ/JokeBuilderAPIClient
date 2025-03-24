// See https://aka.ms/new-console-template for more information

using System;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Concurrent;

public class JokeBuilder
{
    public required string id { get; set; }
    public string type { get; set; }
    public string setup { get; set; }
    public string punchline { get; set; }
}	

    // needed by CLI menu
    public class Option
    {
        public string Name { get; }
        public Action Selected { get; }

        public Option(string name, Action selected)
        {
            Name = name;
            Selected = selected;
        }
    }

public class Class1
{
    private static string? urlParameters;
    public static List<Option> options; // needed by CLI menu

    public static async Task Main(string[] args)
    {
        string url = "https://official-joke-api.appspot.com/random_joke";

        // id of object is type of JokeBuilder and value is JSON string (JokeBuilder object)
        ConcurrentDictionary<int, string> scores = new ConcurrentDictionary<int, string>();

        using var client = new HttpClient();
        client.BaseAddress = new Uri(url);

        // Add an Accept header for JSON format.
        client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));

        // Get data response
        var response = client.GetAsync(urlParameters).Result; 

        /* ======================== CLI menu ========================
        Users should be able to:
		1.) Fetch jokes (specify the number).
		2.) View all stored jokes.
		3.) Search jokes by type.
		4.) Process multiple jokes (remove N jokes at once).
		5.) Exit.
        =========================== CLI menu =========================*/ 

        // ======================== Begin CLI menu ========================
        // Create options that you want your menu to have
        options = new List<Option>
        {
            new Option("1.) Fetch jokes (specify the number)", () => FetchAndWriteJokes(ref scores, ref response, client, urlParameters)),
            new Option("2.) View all stored jokes.", () =>  ViewAllStoredJokes(ref scores)),
            new Option("3.) Search jokes by type.", () =>  SearchJokesByType(ref scores)),
            new Option("4.) Process multiple jokes (remove N jokes at once).", () =>  Remove_N_JokesAtOnce(ref scores)),
            new Option("5.) Exit.", () => Environment.Exit(0)),
        };

        // Set the default index of the selected item to be the first
        int index = 0;

        // Write the menu out
        WriteMenu(options, options[index]);

        // Store key info in here
        ConsoleKeyInfo keyinfo;
        do
        {
            keyinfo = Console.ReadKey();

            // Handle each key input (down arrow will write the menu again with a different selected item)
            if (keyinfo.Key == ConsoleKey.DownArrow)
            {
                if (index + 1 < options.Count)
                {
                    index++;
                    WriteMenu(options, options[index]);
                }
            }
            if (keyinfo.Key == ConsoleKey.UpArrow)
            {
                if (index - 1 >= 0)
                {
                    index--;
                    WriteMenu(options, options[index]);
                }
            }
            // Handle different action for the option
            if (keyinfo.Key == ConsoleKey.Enter)
            {
                options[index].Selected.Invoke();
                index = 0;
            }
        }
        while (keyinfo.Key != ConsoleKey.X);

        Console.ReadKey();
        // ======================== End CLI menu ========================
    }

    // Default action of all the options. You can create more methods
    static void FetchAndWriteJokes(ref ConcurrentDictionary<int, string> scores, ref System.Net.Http.HttpResponseMessage response, HttpClient client, string urlParameters)
    {
        Console.Clear();
        
        // Step 1: Fetch jokes (specify the number).
        Console.WriteLine("Enter number of jokes you want to fetch: ");
        int numberOfJokes = Convert.ToInt32(Console.ReadLine());

        for (int i = 0; i < numberOfJokes; i++)
        {
            // Get data response
            response = client.GetAsync(urlParameters).Result; 

            if (response.IsSuccessStatusCode)
            {
                //Dobiš JSON
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                var responseString = response.Content.ReadAsStringAsync().Result;
                JokeBuilder? result = JsonConvert.DeserializeObject<JokeBuilder>(responseString);

                //preveri, če je razred JokerBuilder napolnjen
                if (result != null)
                {
                    Console.WriteLine("Joke ID: {0}", result.id);
                    Console.WriteLine("Joke type: {0}", result.type);
                    Console.WriteLine("Joke setup: {0}", result.setup);
                    Console.WriteLine("Joke punchline: {0}", result.punchline);

                    // Serialize the object<JokeBuilder> to JSON
                    scores.TryAdd(int.Parse(result.id), JsonConvert.SerializeObject(result));
                }

            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode,
                            response.ReasonPhrase);
            }
        }

        Thread.Sleep(3000);
        WriteMenu(options, options.First());
    }


    static void ViewAllStoredJokes(ref ConcurrentDictionary<int, string> scores)
    {
        Console.Clear();
        
        // Step 2: View all stored jokes.
        Console.WriteLine("View all stored jokes: "); 
        foreach (var item in scores)
        {
            JokeBuilder? jokeBuilderItem = JsonConvert.DeserializeObject<JokeBuilder>(item.Value.ToString());
            Console.WriteLine("Joke ID: {0}", item.Key);
            if (jokeBuilderItem != null)
            {
                Console.WriteLine("Joke type: {0}", jokeBuilderItem.type);
                Console.WriteLine("Joke setup: {0}", jokeBuilderItem.setup);
                Console.WriteLine("Joke punchline: {0}", jokeBuilderItem.punchline);
            }
            else
            {
                Console.WriteLine("JokeBuilder item is null.");
            }
        }

        Thread.Sleep(3000);
        WriteMenu(options, options.First());
    }

    static void SearchJokesByType(ref ConcurrentDictionary<int, string> scores)
    {
        Console.Clear();
        
        // Step 3: Search jokes by type.
        Console.WriteLine("Search jokes by type: ");
        Console.WriteLine("Enter joke type: ");
        string? input = Console.ReadLine();
        string jokeType = input ?? string.Empty;
        foreach (var item in scores)
        {
            JokeBuilder? jokeBuilderItem = JsonConvert.DeserializeObject<JokeBuilder>(item.Value.ToString());
            if (jokeBuilderItem != null && jokeBuilderItem.type == jokeType)
            {
                Console.WriteLine("Joke ID: {0}", item.Key);
                Console.WriteLine("Joke type: {0}", jokeBuilderItem.type);
                Console.WriteLine("Joke setup: {0}", jokeBuilderItem.setup);
                Console.WriteLine("Joke punchline: {0}", jokeBuilderItem.punchline);
            }
        }

        Thread.Sleep(3000);
        WriteMenu(options, options.First());
    }
    static void Remove_N_JokesAtOnce(ref ConcurrentDictionary<int, string> scores)
    {
        Console.Clear();
        
        // Step 4: Process multiple jokes (remove N jokes at once).
        Console.WriteLine("Process multiple jokes (remove N jokes at once): ");
        Console.WriteLine("Enter number of jokes you want to remove: ");
        int numberOfJokesToRemove = Convert.ToInt32(Console.ReadLine());
        if (numberOfJokesToRemove > scores.Count)
        {
            Console.WriteLine("Number of jokes to remove is greater than number of jokes stored.");
            return;
        }
        if (numberOfJokesToRemove == 0)
        {
            Console.WriteLine("Number of jokes to remove is 0.");
            return;
        }
        for (int i = 0; i < numberOfJokesToRemove; i++)
        {
            Console.WriteLine("Enter joke ID you want to remove: ");
            String? input = Console.ReadLine();
            if (input == null  || input == string.Empty)
            {
                Console.WriteLine("Input is empty.");
                return;
            } else {
                int jokeID = Convert.ToInt32(input);
                var keyToRemove = scores.FirstOrDefault(x => x.Key == jokeID).Key;
                scores.TryRemove(keyToRemove, out _);
            }
        }

        Thread.Sleep(3000);
        WriteMenu(options, options.First());
    }

    static void WriteMenu(List<Option> options, Option selectedOption)
    {
        Console.Clear();

        foreach (Option option in options)
        {
            if (option == selectedOption)
            {
                Console.Write("> ");
            }
            else
            {
                Console.Write(" ");
            }

            Console.WriteLine(option.Name);
        }
    }
    
}
