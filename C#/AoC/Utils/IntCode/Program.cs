using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AoC.Utils.IntCode
{
    public sealed class Program : List<long>
    {
        public Program(List<long> buffer) : base(buffer)
        {
        }

        public Program Clone()
        {
            return new Program(new List<long>(this));
        }

        public static Program Parse(string s)
        {
            return new Program(s.Split(",").Select(long.Parse).ToList());
        }

        public static async Task<Program> Load(string path)
        {
            return Parse(await File.ReadAllTextAsync(path));
        }

        public void Set(long index, long value)
        {
            while (Count <= index)
            {
                Add(0);
            }
            this[(int)index] = value;
        }

        public long Get(long index)
        {
            if (index >= Count)
            {
                return 0;
            }
            return this[(int)index];
        }
    }
}
