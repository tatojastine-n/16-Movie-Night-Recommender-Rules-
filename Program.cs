using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Movie
{
    public string Title { get; }
    public string Rating { get; }
    public int DurationMinutes { get; }
    public HashSet<string> Tags { get; }
    public double Score { get; } // Optional rating score for tie-breaking

    private static readonly HashSet<string> ValidRatings = new HashSet<string>
    {
        "G", "PG", "PG-13", "R", "NC-17", "NR", "TV-MA", "TV-14", "TV-Y", "TV-Y7"
    };

    public Movie(string title, string rating, int durationMinutes, IEnumerable<string> tags, double score = 0)
    {
        if (!ValidRatings.Contains(rating))
            throw new ArgumentException($"Invalid rating: {rating}");

        Title = title;
        Rating = rating;
        DurationMinutes = durationMinutes;
        Tags = new HashSet<string>(tags.Select(t => t.ToLower()));
        Score = score;
    }

    public bool MatchesPreferences(int? maxDuration, HashSet<string> allowedRatings, string mood)
    {
        if (maxDuration.HasValue && DurationMinutes > maxDuration.Value)
            return false;

        if (allowedRatings != null && allowedRatings.Count > 0 && !allowedRatings.Contains(Rating))
            return false;

        if (!string.IsNullOrWhiteSpace(mood) && !Tags.Contains(mood.ToLower()))
            return false;

        return true;
    }
}

public class MovieRecommender
{
    public List<Movie> GetRecommendations(
        List<Movie> catalog,
        HashSet<string> allowedRatings,
        int? maxDuration,
        string mood,
        int maxResults = 5)
    {
        // First pass: filter by hard constraints
        var filtered = catalog.Where(movie =>
            movie.MatchesPreferences(maxDuration, allowedRatings, mood))
            .ToList();

        // Then sort by tie-breaker criteria (score descending, then duration ascending)
        return filtered.OrderByDescending(m => m.Score)
                     .ThenBy(m => m.DurationMinutes)
                     .Take(maxResults)
                     .ToList();
    }
}
namespace Movie_Night_Recommender__Rules_
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var catalog = new List<Movie>
        {
            new Movie("The Avengers", "PG-13", 143, new[] {"action", "adventure", "exciting"}, 8.0),
            new Movie("Toy Story", "G", 81, new[] {"family", "funny", "happy"}, 8.3),
            new Movie("Inception", "PG-13", 148, new[] {"mind-bending", "exciting", "suspenseful"}, 8.8),
            new Movie("Finding Nemo", "G", 100, new[] {"family", "happy", "emotional"}, 8.1)
        };

            var recommender = new MovieRecommender();

            Console.WriteLine("Movie Recommendation System");
            Console.WriteLine("Enter your preferences:");

            Console.Write("Allowed ratings (comma separated, e.g. G,PG,PG-13): ");
            var allowedRatings = new HashSet<string>(
                Console.ReadLine().Split(',').Select(r => r.Trim().ToUpper()));

            Console.Write("Maximum duration in minutes (or leave blank): ");
            int? maxDuration = int.TryParse(Console.ReadLine(), out int duration) ? duration : (int?)null;

            Console.Write("Mood tag (e.g. exciting, funny, emotional): ");
            string mood = Console.ReadLine().ToLower();

            var recommendations = recommender.GetRecommendations(
                catalog,
                allowedRatings,
                maxDuration,
                mood);

            Console.WriteLine("\nRecommended Movies:");
            if (recommendations.Count == 0)
            {
                Console.WriteLine("No movies match your criteria.");
            }
            else
            {
                foreach (var movie in recommendations)
                {
                    Console.WriteLine($"{movie.Title} ({movie.Rating}, {movie.DurationMinutes} min, " +
                                    $"Rating Score: {movie.Score}, Tags: {string.Join(", ", movie.Tags)})");
                }
            }
        }
    }
}
