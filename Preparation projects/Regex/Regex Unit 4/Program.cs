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
            string[] gamePageUrls = [
                @"https://store.steampowered.com/app/2183900/Warhammer_40000_Space_Marine_2/",
                @"https://store.steampowered.com/app/3017860/DOOM_The_Dark_Ages/",
                @"https://store.steampowered.com/app/2623190/The_Elder_Scrolls_IV_Oblivion_Remastered/", // Fictional/Placeholder URL
                @"https://store.steampowered.com/app/2622380/ELDEN_RING_NIGHTREIGN/", // Fictional/Placeholder URL
                @"https://store.steampowered.com/app/311210/Call_of_Duty_Black_Ops_III/"
            ];

            // Regex pattern to extract the game name.
            string gameNamePattern = @"<meta\s+property=""og:title""\s+content=""(.*?)(?:\s+on\s+Steam)?"">";

            // Regex pattern to extract review sections (handles "Recent Reviews" and "All Reviews").
            string reviewSectionPattern = @"<div class=""subtitle(?: column(?: all| recent))?"">\s*(Recent Reviews:|All Reviews:)\s*</div>[\s\S]*?<span class=""game_review_summary[^""]*""(?: itemprop=""description"")?>(.*?)</span>";

            // Fallback regex pattern for a single overall rating, often marked with itemprop="description".
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

                // --- MODIFIED REVIEW PROCESSING LOGIC ---
                MatchCollection reviewMatches = Regex.Matches(htmlCode, reviewSectionPattern);

                bool printedRecentReviews = false;
                bool printedAllReviews = false;

                if (reviewMatches.Count > 0)
                {
                    foreach (Match reviewMatch in reviewMatches)
                    {
                        // Ensure the match has the expected groups.
                        if (reviewMatch.Groups.Count >= 3)
                        {
                            string reviewTypeRaw = reviewMatch.Groups[1].Value.Trim(); // e.g., "Recent Reviews:" or "All Reviews:"
                            string ratingText = reviewMatch.Groups[2].Value.Trim(); // e.g., "Mostly Positive"

                            if (reviewTypeRaw.Equals("Recent Reviews:", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!printedRecentReviews)
                                {
                                    Console.WriteLine($"Recent reviews: {ratingText}");
                                    printedRecentReviews = true;
                                }
                            }
                            else if (reviewTypeRaw.Equals("All Reviews:", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!printedAllReviews)
                                {
                                    Console.WriteLine($"All reviews: {ratingText}");
                                    printedAllReviews = true;
                                }
                            }
                        }
                    }
                }

                // Fallback for "All Reviews" if not found by the structured pattern and not already printed.
                if (!printedAllReviews)
                {
                    Match overallRatingMatch = Regex.Match(htmlCode, singleOverallRatingPattern);
                    if (overallRatingMatch.Success && overallRatingMatch.Groups.Count > 1)
                    {
                        string ratingText = overallRatingMatch.Groups[1].Value.Trim();
                        Console.WriteLine($"All reviews: {ratingText}");
                        printedAllReviews = true; // Mark as printed via fallback
                    }
                }

                // If no reviews were found by any method.
                if (!printedRecentReviews && !printedAllReviews)
                {
                    // For new/unreleased games, it's common to not have reviews.
                    // (Using your original list of potentially unreleased games for this check)
                    bool isPotentiallyUnreleased = gameName.Contains("Space Marine 2", StringComparison.OrdinalIgnoreCase) ||
                                                   gameName.Contains("DOOM The Dark Ages", StringComparison.OrdinalIgnoreCase) ||
                                                   gameName.Contains("Oblivion Remastered", StringComparison.OrdinalIgnoreCase) ||
                                                   gameName.Contains("ELDEN RING NIGHTREIGN", StringComparison.OrdinalIgnoreCase);
                    
                    if (isPotentiallyUnreleased) 
                    {
                        Console.WriteLine("No review information found (game may be unreleased or page is a placeholder).");
                    }
                    else
                    {
                        Console.WriteLine("No review information found.");
                    }
                }
                // --- END OF MODIFIED REVIEW PROCESSING LOGIC ---
                Console.WriteLine(); // Add a blank line for better readability between games.
            }
        }
    }
}

