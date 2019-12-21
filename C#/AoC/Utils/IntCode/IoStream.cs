using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AoC.Utils.IntCode
{
    public sealed class IoStream : IEnumerable<long>
    {
        private readonly Queue<long> _queue;
        private TaskCompletionSource<int> _tcs;

        public IoStream(IEnumerable<long> items)
        {
            _queue = new Queue<long>(items);
            _tcs = new TaskCompletionSource<int>();
        }

        public IoStream() : this(new long[] { })
        {
        }

        public void Add(long item)
        {
            _queue.Enqueue(item);
            var tcs = _tcs;
            _tcs = new TaskCompletionSource<int>();
            tcs.SetResult(0);
        }

        public async Task Write(long item)
        {
            await Task.Yield();
            Add(item);
        }

        public async Task<long> Read(CancellationToken ct = default(CancellationToken))
        {
            var waitTask = _tcs.Task;
            while (true)
            {
                if (_queue.TryDequeue(out long result))
                {
                    return result;
                }
                else
                {
                    var cts = new CancellationTokenSource();
                    var ctsJoin = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);
                    var cancellationTask = Task.Delay(-1, ctsJoin.Token);
                    var r = await Task.WhenAny(waitTask, cancellationTask);
                    if (r == cancellationTask)
                    {
                        throw new TaskCanceledException();
                    }
                    cts.Cancel();
                    waitTask = _tcs.Task;
                }
            }
        }


        public async Task<long> Peek(CancellationToken ct = default(CancellationToken))
        {
            var waitTask = _tcs.Task;
            while (true)
            {
                if (_queue.TryPeek(out long result))
                {
                    return result;
                }
                else
                {
                    var cts = new CancellationTokenSource();
                    var ctsJoin = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);
                    var cancellationTask = Task.Delay(-1, ctsJoin.Token);
                    var r = await Task.WhenAny(waitTask, cancellationTask);
                    if (r == cancellationTask)
                    {
                        throw new TaskCanceledException();
                    }
                    cts.Cancel();
                    waitTask = _tcs.Task;
                }
            }
        }


        public IEnumerable<long> AsEnumerable() => _queue.ToList().AsEnumerable();

        public IEnumerator<long> GetEnumerator()
        {
            return AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public async Task WriteString(string input)
        {
            foreach (var character in input)
            {
                await WriteChar(character);
            }
        }

        public async Task WriteChar(char character)
        {
            var value = (long)character;
            await Write(value);
        }

        public async Task<char> ReadCharacter(CancellationToken ct = default(CancellationToken))
        {
            var result = await Read(ct);
            return (char)result;
        }

        public async Task<string> ReadLine(CancellationToken ct = default(CancellationToken))
        {
            await Task.Yield();
            var s = "";
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var next = await Peek(ct);
                    if (next > 127 || (char)next == '\n')
                    {
                        return s;
                    }

                    s += (char)await Read(ct);
                }
            }
            catch (TaskCanceledException)
            {
            }

            return s;
        }

        public async Task<string> ReadString(CancellationToken ct = default(CancellationToken))
        {
            await Task.Yield();
            var s = "";
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var next = await Peek(ct);
                    if (next > 127)
                    {
                        return s;
                    }

                    s += (char)await Read(ct);
                }
            }
            catch (TaskCanceledException)
            {
            }

            return s;
        }
    }
}
