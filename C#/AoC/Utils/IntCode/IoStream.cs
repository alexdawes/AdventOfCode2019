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
                    await Task.Run(async () => await waitTask, ct);
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
    }
}
