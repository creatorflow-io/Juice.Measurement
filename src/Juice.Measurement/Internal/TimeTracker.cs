using System.Diagnostics;
using Juice.Utils;
using static System.Formats.Asn1.AsnWriter;

namespace Juice.Measurement.Internal
{
    /// <summary>
    /// Scoped service to track time.
    /// </summary>
	public class TimeTracker : ITimeTracker
    {
        private Stopwatch _stopwatch = Stopwatch.StartNew();
        private Stack<ExecutionScope> _scopes = new();
        private ExecutionScope? _currentScope;

        private Stack<string> _scopesName = new();
        public List<ExecutionRecord> Records { get; } = new();

        /// <inheritdoc />
        public IDisposable BeginScope(string name)
        {
            _scopesName.Push(name);
            var scope = new ExecutionScope(GetScopeName(), name);
            scope.OnDispose += (sender, args) =>
            {
                if (sender is ExecutionScope scope)
                {
                    Records.Add(new ExecutionRecord(args.OriginalScopeName, args.ScopeName, _scopesName.Count, scope.ElapsedTime, scope.Checkpoints));
                }
                _scopesName.Pop();
                _scopes.Pop();
                _currentScope = _scopes.Count > 0 ? _scopes.Peek() : null;
            };
            _scopes.Push(scope);
            _currentScope = scope;
            _currentScope?.Checkpoint("Begin", _stopwatch.ElapsedMilliseconds);
            return scope;
        }

        /// <inheritdoc />
        public void Checkpoint(string name)
        {
            _currentScope?.Checkpoint(name);
        }

        private static readonly string[] _header = ["Scope", "Depth", "Elapsed Time"];
        private static readonly ColAlign[] _colsAlign = [ColAlign.left, ColAlign.center, ColAlign.right];

        public string ToString(bool displayMillisecond, int? maxDepth = default)
        {
            // Create a table to display the execution records.
            var table = new ConsoleTable([_header],
                Records.Where(r => !maxDepth.HasValue || r.Depth <= maxDepth)
                .SelectMany(r =>
                {
                    var scope = new List<string[]>
                    {
                    };
                    if (r.Checkpoints.Length > 0)
                    {
                        scope.Add([]);
                        scope.AddRange(r.Checkpoints.Select(c => new string[] { "> " + c.Name, r.Depth.ToString(), "+" + c.ElapsedMs + " ms" }));
                    }
                    scope.Add([r.ScopeName, r.Depth.ToString(), displayMillisecond ? r.ElapsedTime.TotalMilliseconds + " ms" : r.ElapsedTime.ToString()]);
                    return scope;
                })
                .Concat([[], ["Total", "", displayMillisecond ? _stopwatch.ElapsedMilliseconds + " ms" : _stopwatch.Elapsed.ToString()]]).ToArray());

            var maxLen = Records.Max(r => r.ScopeName.Length);
            table.Cols = [maxLen + 2, 10, 20];
            table.ColsAlign = _colsAlign;
            return table.PrintTable();
        }

        override public string ToString()
        {
            return ToString(true);
        }

        private string GetScopeName()
        {
            return string.Join(" -> ", _scopesName.Reverse());
        }

        private bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _scopesName.Clear();
                    _scopes.Clear();
                    _currentScope = null;
                    _stopwatch.Stop();
                    Records.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
