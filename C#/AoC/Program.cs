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
            Console.Write($"Please specify a day? ({range.Min}-{range.Max}, default={range.Max}): ");
            var dayStr = Console.ReadLine();
            Console.WriteLine();
            var day = int.TryParse(dayStr, out int d) ? d : range.Max;
            if (!solutions.TryGetValue(day, out ISolution solution) || solution == null)
            {
                Console.WriteLine($"Day {day} does not have a solution.");
                return;
            }

            try
            {
                await solution.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine();
                var fc = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ForegroundColor = fc;
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.Read();
            }
        }
    }
}
