using System.Text.RegularExpressions;

struct GetGameRating
{
    public string? Recent;
    public string? AllTime;
}


/// <summary>
/// Parsing HTML with Regex, the way God intended.
/// 
/// https://stackoverflow.com/questions/1732348/regex-match-open-tags-except-xhtml-self-contained-tags
/// </summary>
public class Regex4
{
    string gnorpApologue = "https://store.steampowered.com/app/1473350/the_Gnorp_Apologue/";

    /// <summary>
    /// A few sample games
    /// </summary>
    (string, string)[] games =
    [
        ("Gnorp Apologue", "https://store.steampowered.com/app/1473350/the_Gnorp_Apologue/"),
        ("STAR WARS The Clone Wars Republic Heroes", "https://store.steampowered.com/app/32420/STAR_WARS_The_Clone_Wars__Republic_Heroes"),
        ("World Basketball Manager 2010", "https://store.steampowered.com/app/46740/World_Basketball_Manager_2010"),
        ("Race To Mars", "https://store.steampowered.com/app/257930/Race_To_Mars"),
        ("Portal", "https://store.steampowered.com/app/400/Portal"),
        ("IDUN Prologue - Frontline Survival", "https://store.steampowered.com/app/3116470/IDUN_Prologue__Frontline_Survival"),
        ("Wheelie King 7 - Motorbike simulator 3D", "https://store.steampowered.com/app/3330620/Wheelie_King_7__Motorbike_simulator_3D")
    ];

    /// <summary>
    /// Runs the two parts
    /// </summary>
    public void Run()
    {
        // Part 1
        Console.WriteLine("Part 1");
        PrintGameRating("Gnorp Apologue", GetGameRating(gnorpApologue));

        // Part 2
        Console.WriteLine("Part 2");
        foreach (var game in games)
        {
            PrintGameRating(game.Item1, GetGameRating(game.Item2));
        }
    }

    /// <summary>
    /// Gets the game rating from the game URL by scraping the HTML
    /// </summary>
    private GetGameRating GetGameRating(string gameUrl)
    {
        var httpClient = new HttpClient();
        string htmlCode = httpClient.GetStringAsync(gameUrl).Result;

        var ratingRegex = new Regex("class=\"game_review_summary[^\"]*\"[^>]*>([^<]+)<");
        var ratingMatches = ratingRegex.Matches(htmlCode);

        if (ratingMatches.Count == 0)
        {
            return new GetGameRating
            {
                Recent = null,
                AllTime = null
            };
        }
        else if (ratingMatches.Count == 1)
        {
            // Some games only have all-time reviews
            return new GetGameRating
            {
                Recent = null,
                AllTime = ratingMatches[0].Groups[1].Value
            };
        }

        return new GetGameRating
        {
            Recent = ratingMatches[0].Groups[1].Value,
            AllTime = ratingMatches[1].Groups[1].Value
        };
    }

    /// <summary>
    /// Pretty-prints the game rating
    /// </summary>
    private void PrintGameRating(string name, GetGameRating rating)
    {
        Console.WriteLine(name.ToUpper());
        if (rating.Recent != null)
        {
            Console.WriteLine($"Recent reviews: {rating.Recent}");
        }
        if (rating.AllTime != null)
        {
            Console.WriteLine($"All reviews: {rating.AllTime}");
        }
        Console.WriteLine();
    }
}