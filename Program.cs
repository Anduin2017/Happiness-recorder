using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace JAI
{
    class Program
    {
        const string database = "records.json";
        static async Task Main(string[] args)
        {
            if (!File.Exists(database))
            {
                await File.WriteAllTextAsync(database, JsonSerializer.Serialize(Array.Empty<DateTime>()));
            }
            var recordsText = await File.ReadAllTextAsync(database);
            var recordsObjects = JsonSerializer.Deserialize<List<DateTime>>(recordsText);

            var totalPoints = 100.0;
            var lastCalculatingPoint = DateTime.MinValue;
            foreach (var record in recordsObjects.OrderBy(t => t))
            {
                // Convert the past time as points. Add those.
                var elapsed = record - lastCalculatingPoint;
                var points = GetPointsFromWaitingTime(elapsed);
                totalPoints = AddScore(totalPoints, points);

                // Since this is a triggered event. Count it!
                var pointsLater = WastePoint(totalPoints);
                Console.WriteLine($"Then with {totalPoints:N1} points, you recorded at {record:MM-dd}, feels {GetStatus(totalPoints)}. You have {(totalPoints / 2.0 - 5):N1} points left and then feels {GetStatus(pointsLater)}.");
                totalPoints = pointsLater;
                Console.WriteLine($"...");

                // Record last point.
                lastCalculatingPoint = record;
            }

            var finalElapsed = DateTime.Now - lastCalculatingPoint;
            var pointsLast = GetPointsFromWaitingTime(finalElapsed);
            totalPoints = AddScore(totalPoints, pointsLast);
            Console.WriteLine($"------------------------------------------------");
            Console.WriteLine($"\n");
            var feels = GetStatus(totalPoints);
            Console.WriteLine($"Please enter your PAI: [If you don't know, press enter]");
            int pai = 100;
            var hasPai = int.TryParse(Console.ReadLine(), out pai);
            Console.WriteLine($"------------------------------------------------\n\n\n");
            Console.WriteLine($"Your current point is:    {totalPoints:N1}.    Feels {feels}..");
            Console.WriteLine($"Suggestion: {GetSuggestions(totalPoints)}\n");
            if(hasPai && pai > 0)
            {
                var pieArg = GetPAIArg(pai);
                var bodyScore = pieArg * totalPoints;
                Console.WriteLine(GetBodySuggestions(bodyScore));
                Console.WriteLine($"------------------------------------------------");
            }

            Console.WriteLine($"Press [A] to add a record... Press [N] to quit.");
            var key = Console.ReadKey().Key.ToString().ToLower().Trim();
            if (key == "a")
            {
                var newRecordTime = DateTime.Now;
                recordsObjects.Add(newRecordTime);

                var elapsed = newRecordTime - lastCalculatingPoint;
                var points = GetPointsFromWaitingTime(elapsed);
                totalPoints = AddScore(totalPoints, points);

                // Since this is a triggered event. Count it!
                var pointsLater = WastePoint(totalPoints);
                Console.WriteLine($"With {totalPoints:N1} points, you recorded at {newRecordTime:MM-dd}, feels {GetStatus(totalPoints)}. You have {(totalPoints / 2.0 - 5):N1} points left and then feels {GetStatus(pointsLater)}.");
                totalPoints = pointsLater;
                Console.WriteLine($"...");

            }

            var newList = recordsObjects.OrderBy(t => t).ToList();
            var newJson = JsonSerializer.Serialize(newList, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(database, newJson);
        }

        static double GetPointsFromWaitingTime(TimeSpan input)
        {
            var x = input.TotalDays;
            var points = 0.0;
            if (x > 9)
            {
                points = 100;
            }
            else
            {
                points = -0.5 * x * x + 15.5 * x;
            }
            Console.WriteLine($"{x:N1} days passed. And you got {points:N1} points from that.");
            return points;
        }

        static double WastePoint(double sourceWastedBefore)
        {
            return sourceWastedBefore / 2.0 - 5;
        }

        /// <summary>
        /// PAI stands for Personal Activity Intelligence. You earn PAI points by elevating your heart rate. Anything that gets your body moving and your heart pumping counts (not just exercise and steps).  
        /// Your personal PAI score is based on your heart rate data and your personal profile, including age, sex, and fitness level. PAI uses this data to give you one simple, personalized score.  
        /// PAI's algorithm is based on data collected from the HUNT study, which took place over a 25-year period involving 45,000 participants. It was conducted by the Faculty of Medicine at the Norwegian University of Science and Technology and developed by Professor Ulrik Wisløff, one of the world’s leading scientists in Exercise in Medicine.
        /// From this study, we were able to prove that you can live a longer, healthier life by maintaining a PAI score of 100 over a 7-day rolling period.
        /// </summary>
        static double GetPAIArg(int pie)
        {
            var y = Math.Sqrt(pie) / 10;
            return y;
        }

        static double AddScore(double sourceScore, double scoreToAdd)
        {
            return Math.Min(100, sourceScore + scoreToAdd);
        }

        static string GetStatus(double score)
        {
            if (score >= 90)
                return "ideal";
            else if (score >= 80)
                return "excellent";
            else if (score >= 70)
                return "awesome";
            else if (score >= 60)
                return "good";
            else if (score >= 50)
                return "acceptable";
            else if (score >= 40)
                return "tired";
            else if (score >= 30)
                return "very hard to accept";
            else if (score >= 20)
                return "bad";
            else if (score >= 10)
                return "shit";
            else if (score >= 0)
                return "about to die...";
            else
                throw new InvalidOperationException();
        }

        static string GetSuggestions(double currentScore)
        {
            if (currentScore >= 90)
                return "You should have some happiness.";
            else if (currentScore >= 70)
                return "You can have some happiness.";
            else if (currentScore >= 50)
                return "You'd better not. Experience is not so good.";
            else if (currentScore >= 30)
                return "Please don't!! Your body is in a real bad status!";
            else if (currentScore >= 0)
                return "Please DO NOT HURT YOUR BODY!!! You WILL get injured and feel extreamly unconfortable if you insists.";
            else
                throw new InvalidOperationException();
        }

        static string GetBodySuggestions(double currentBodyScore)
        {
            if (currentBodyScore >= 85)
                return "Your body is ideal!!! Do anything you like!";
            else if (currentBodyScore >= 70)
                return "Your body is good!! Try to have fun!";
            else if (currentBodyScore >= 47)
                return "Your body is not good! Please try to do some basic exercies.";
            else if (currentBodyScore >= 0)
                return "Your body is very very bad... Please consider recovery.";
            else
                throw new InvalidOperationException();
        }
    }
}
