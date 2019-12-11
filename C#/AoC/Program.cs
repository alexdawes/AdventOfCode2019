using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AoC
{
    public static class Program
    {
        private static IDictionary<int, ISolution> GetSolutions()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(ISolution).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                    .ToDictionary(t => Convert.ToInt32(t.Namespace?.Split(".").Last().Substring(1)), t => (ISolution)Activator.CreateInstance(t));
        }

        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync (string[] args)
        {
            var solutions = GetSolutions();
            var range = (Min: solutions.Keys.Min(), Max: solutions.Keys.Max());
            var first = true;
            while (true)
            {
                int day;
                if (first)
                {
                    first = false;
                    Console.Write($"Please specify a day? ({range.Min}-{range.Max}, default={range.Max}): ");
                    var dayStr = Console.ReadLine();
                    Console.WriteLine();
                    day = int.TryParse(dayStr, out int d) ? d : range.Max;
                }
                else
                {
                    Console.Write($"Please specify a day? ({range.Min}-{range.Max}, default=exit): ");
                    var dayStr = Console.ReadLine();
                    Console.WriteLine();
                    if (int.TryParse(dayStr, out int d))
                    {
                        day = d;
                    }
                    else
                    {
                        return;
                    }
                }
                if (!solutions.TryGetValue(day, out ISolution solution) || solution == null)
                {
                    Console.WriteLine($"Day {day} does not have a solution.");
                    return;
                }

                try
                {
                    await solution.Run();
                    Console.WriteLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine();
                    var fc = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                    Console.ForegroundColor = fc;
                }
            }
        }
    }
}
