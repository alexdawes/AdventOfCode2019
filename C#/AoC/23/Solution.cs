using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IntCode = AoC.Utils.IntCode;

namespace AoC._23
{
    public sealed  class Solution : ISolution
    {
        public sealed class NetworkController
        {
            private readonly Dictionary<long, IntCode.Computer> _computers = new Dictionary<long, IntCode.Computer>();
            private readonly Dictionary<long, Queue<(long X, long Y)>> _queues = new Dictionary<long, Queue<(long X, long Y)>>();
            private readonly Dictionary<long, bool> _idle = new Dictionary<long, bool>();
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();

            private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
            private (long X, long Y)? _natCache;

            public IntCode.IoStream Output { get; } = new IntCode.IoStream();

            public IntCode.IoStream Pokes { get; } = new IntCode.IoStream();

            public void Register(long id, IntCode.Computer computer)
            {
                _computers[id] = computer;
                _queues[id] = new Queue<(long X, long Y)>();
                _idle[id] = false;
            }
            public void Start()
            {
                var _ = Task.WhenAll(_computers.Keys.Select(id => Task.WhenAll(StartListening(id), StartRouting(id))));
                var __ = StartIdleChecking();
            }

            public void Stop()
            {
                _cts.Cancel();
                _cts.Token.WaitHandle.WaitOne();
            }
            
            private async Task StartListening(long id)
            {
                var computer = _computers[id];
                while (!_cts.Token.IsCancellationRequested)
                {
                    var address = await computer.Output.Read();
                    var (x, y) = (await computer.Output.Read(), await computer.Output.Read());
                    await Route(address, x, y);
                }
            }

            private async Task StartRouting(long id)
            {
                var queue = _queues[id];
                var computer = _computers[id];
                while (!_cts.Token.IsCancellationRequested)
                {
                    await computer.WaitUntilInputRequired(_cts.Token);
                    if (queue.Any())
                    {
                        var (x, y) = queue.Dequeue();
                        await computer.Input.Write(x);
                        await computer.Input.Write(y);
                        _idle[id] = false;
                    }
                    else
                    {
                        await computer.Input.Write(-1);
                        _idle[id] = true;
                    }
                }
            }
            
            private async Task StartIdleChecking()
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(100);
                    if (_computers.Keys.All(id => _idle[id]) && _natCache.HasValue)
                    {
                        var (x, y) = _natCache.Value;
                        await Route(0, x, y);
                        await Pokes.Write(x);
                        await Pokes.Write(y);
                    }
                }
            }

            private async Task Route(long address, long x, long y)
            {
                await _semaphore.WaitAsync();
                try
                {
                    await Output.Write(address);
                    await Output.Write(x);
                    await Output.Write(y);
                }
                finally
                {
                    _semaphore.Release();
                }
                if (_queues.ContainsKey(address))
                {
                    _queues[address].Enqueue((x, y));
                }
                else if (address == 255L)
                {
                    _natCache = (x, y);
                }
            }
        }
        
        private async Task<long> Part1()
        {
            var program = await IntCode.Program.Load("23/input");
            var computers = Enumerable.Range(0, 50).Select(i => new IntCode.Computer(program.Clone())).ToList();
            var controller = new NetworkController();
            
            try
            {
                for (var i = 0; i < computers.Count; i++)
                {
                    var computer = computers[i];
                    controller.Register(i, computer);
                    computer.Start();
                    await computer.Input.Write(i);
                }
                controller.Start();

                while (true)
                {
                    var address = await controller.Output.Read();
                    var _ = await controller.Output.Read();
                    var y = await controller.Output.Read();
                    if (address == 255)
                    {
                        return y;
                    }
                }
            }
            finally
            {
                controller.Stop();
                computers.ForEach(computer => computer.Stop());
            }
        }

        private async Task<long> Part2()
        {
            var program = await IntCode.Program.Load("23/input");
            var computers = Enumerable.Range(0, 50).Select(i => new IntCode.Computer(program.Clone())).ToList();
            var controller = new NetworkController();

            try
            {
                for (var i = 0; i < computers.Count; i++)
                {
                    var computer = computers[i];
                    controller.Register(i, computer);
                    computer.Start();
                    await computer.Input.Write(i);
                }
                controller.Start();

                var cache = new HashSet<(long X, long Y)>();
                while (true)
                {
                    var x = await controller.Pokes.Read();
                    var y = await controller.Pokes.Read();
                    if (cache.Contains((x, y)))
                    {
                        return y;
                    }
                    else
                    {
                        cache.Add((x, y));
                    }
                }
            }
            finally
            {
                controller.Stop();
                computers.ForEach(computer => computer.Stop());
            }
        }

        public async Task Run()
        {
            var part1 = await Part1();
            Console.WriteLine($"Part 1: {part1}");

            var part2 = await Part2();
            Console.WriteLine($"Part 2: {part2}");
        }
    }
}
