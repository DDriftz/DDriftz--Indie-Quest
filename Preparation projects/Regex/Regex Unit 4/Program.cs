using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks; // Required for async Main

namespace Regex_4
{
    internal class Program
    {
        // Changed Main to be async to allow await for HttpClient calls
        static async Task Main(string[] args)
        {
            // HttpClient is used to send HTTP requests and receive HTTP responses.
            // It's good practice to create one instance and reuse it.
            var httpClient = new HttpClient();

            // Array of Steam game page URLs to process.
            // These URLs have been updated as per your request.
            string[] gamePageUrls = [
                @"https://store.steampowered.com/app/2183900/Warhammer_40000_Space_Marine_2/",
                @"https://store.steampowered.com/app/3017860/DOOM_The_Dark_Ages/",
                @"https://store.steampowered.com/app/2623190/The_Elder_Scrolls_IV_Oblivion_Remastered/", // Fictional/Placeholder URL
                @"https://store.steampowered.com/app/2622380/ELDEN_RING_NIGHTREIGN/", // Fictional/Placeholder URL
                @"https://store.steampowered.com/app/311210/Call_of_Duty_Black_Ops_III/"
            ];

            // Regex pattern to extract the game name.
            // It looks for a meta tag with property "og:title".
            // Example: <meta property="og:title" content="Return of the Obra Dinn on Steam">
            // Group 1 captures the game name. "(?:\s+on\s+Steam)?" makes " on Steam" optional.
            string gameNamePattern = @"<meta\s+property=""og:title""\s+content=""(.*?)(?:\s+on\s+Steam)?"">";

            // Regex pattern to extract review sections (handles "Recent Reviews" and "All Reviews").
            // This pattern looks for a structure like:
            // <div class="subtitle">Recent Reviews:</div> ... <span class="game_review_summary ...">Rating Text</span>
            // Group 1 captures "Recent Reviews:" or "All Reviews:".
            // Group 2 captures the rating text (e.g., "Mostly Positive").
            string reviewSectionPattern = @"<div class=""subtitle(?: column(?: all| recent))?"">\s*(Recent Reviews:|All Reviews:)\s*</div>[\s\S]*?<span class=""game_review_summary[^""]*""(?: itemprop=""description"")?>(.*?)</span>";
            
            // Fallback regex pattern for a single overall rating, often marked with itemprop="description".
            // This is useful if the more structured reviewSectionPattern doesn't find matches,
            // or if a page has a simpler layout for reviews.
            // Group 1 captures the rating text.
            string singleOverallRatingPattern = @"<span class=""game_review_summary\s*[^""]*""\s*itemprop=""description"">(.*?)</span>";

            // Loop through each URL in the gamePageUrls array.
            foreach (string url in gamePageUrls)
            {
                Console.WriteLine($"Fetching data for: {url}...");
                string htmlCode = "";
                try
                {
                    // Asynchronously get the HTML content of the page.
                    htmlCode = await httpClient.GetStringAsync(url);
                }
                catch (HttpRequestException e)
                {
                    // Handle potential errors during the HTTP request.
                    // This is especially important for potentially non-existent or future game URLs.
                    Console.WriteLine($"Error fetching {url}: {e.Message}");
                    Console.WriteLine("This game may not be released or the page might be unavailable.\n");
                    continue; // Skip to the next URL if an error occurs.
                }

                // Extract game name using the gameNamePattern.
                Match nameMatch = Regex.Match(htmlCode, gameNamePattern, RegexOptions.IgnoreCase);
                string gameName = "Unknown Game"; // Default name if not found.

                if (nameMatch.Success && nameMatch.Groups.Count > 1)
                {
                    gameName = nameMatch.Groups[1].Value.Trim();
                }
                else // Fallback: try to get the name from the <title> tag if meta tag fails.
                {
                    Match alternativeNameMatch = Regex.Match(htmlCode, @"<title>(.*?)<\/title>", RegexOptions.IgnoreCase);
                    if (alternativeNameMatch.Success && alternativeNameMatch.Groups.Count > 1)
                    {
                        gameName = alternativeNameMatch.Groups[1].Value.Trim();
                        // Clean up " on Steam" or similar suffixes if present in the title.
                        string[] suffixesToRemove = [" on Steam", " - Steam", " on Steam Games"];
                        foreach(string suffix in suffixesToRemove)
                        {
                            if (gameName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                            {
                                gameName = gameName.Substring(0, gameName.Length - suffix.Length).Trim();
                                break; 
                            }
                        }
                    }
                }
                Console.WriteLine(gameName.ToUpper());

                // Extract all review sections (Recent and All) using reviewSectionPattern.
                MatchCollection reviewMatches = Regex.Matches(htmlCode, reviewSectionPattern);

                bool foundReviewsStructured = false;
                if (reviewMatches.Count > 0)
                {
                    foreach (Match reviewMatch in reviewMatches)
                    {
                        // Ensure the match has the expected groups.
                        if (reviewMatch.Groups.Count >= 3)
                        {
                            string reviewType = reviewMatch.Groups[1].Value.Trim(); // e.g., "Recent Reviews:"
                            string ratingText = reviewMatch.Groups[2].Value.Trim(); // e.g., "Mostly Positive"
                            
                            // To match the example output's casing ("Recent reviews:", "All reviews:")
                            if (reviewType.Equals("Recent Reviews:", StringComparison.OrdinalIgnoreCase))
                            {
                                reviewType = "Recent reviews:";
                            }
                            else if (reviewType.Equals("All Reviews:", StringComparison.OrdinalIgnoreCase))
                            {
                                reviewType = "All reviews:";
                            }
                            Console.WriteLine($"{reviewType} {ratingText}");
                            foundReviewsStructured = true;
                        }
                    }
                }
                
                // If no structured reviews were found using reviewSectionPattern,
                // try the fallback singleOverallRatingPattern.
                if (!foundReviewsStructured)
                {
                    Match overallRatingMatch = Regex.Match(htmlCode, singleOverallRatingPattern);
                    if (overallRatingMatch.Success && overallRatingMatch.Groups.Count > 1)
                    {
                        string ratingText = overallRatingMatch.Groups[1].Value.Trim();
                        // Assume this fallback is for "All Reviews" as per common usage.
                        Console.WriteLine($"All reviews: {ratingText}");
                        foundReviewsStructured = true; // Mark as found for the final check.
                    }
                }

                // If no reviews were found by any method.
                if (!foundReviewsStructured)
                {
                    // For new/unreleased games, it's common to not have reviews.
                    if (gameName.Contains("Space Marine 2", StringComparison.OrdinalIgnoreCase) ||
                        gameName.Contains("DOOM The Dark Ages", StringComparison.OrdinalIgnoreCase) ||
                        gameName.Contains("Oblivion Remastered", StringComparison.OrdinalIgnoreCase) || // Placeholder
                        gameName.Contains("ELDEN RING NIGHTREIGN", StringComparison.OrdinalIgnoreCase) // Placeholder
                        ) 
                    {
                        Console.WriteLine("No review information found (game may be unreleased or page is a placeholder).");
                    }
                    else
                    {
                        Console.WriteLine("No review information found.");
                    }
                }
                Console.WriteLine(); // Add a blank line for better readability between games.
            }
        }
    }
}
