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
        public List<ITrackRecord> Records { get; } = new();

        /// <inheritdoc />
        public IDisposable BeginScope(string name)
        {
            _scopesName.Push(name);
            var scope = new ExecutionScope(GetScopeName(), name);
            scope.OnDispose += (sender, args) =>
            {
                if (sender is ExecutionScope scope)
                {
                    Records.Add(new ScopeEnd(args.OriginalScopeName, args.ScopeName, _scopesName.Count, scope.ElapsedTime));
                }
                _scopesName.Pop();
                _scopes.Pop();
                _currentScope = _scopes.Count > 0 ? _scopes.Peek() : null;
            };
            _scopes.Push(scope);
            _currentScope = scope;
            Records.Add(new ScopeStart(name, _scopes.Count, _stopwatch.Elapsed));
            return scope;
        }

        /// <inheritdoc />
        public void Checkpoint(string name)
        {
            if (_currentScope != null)
            {
                Records.Add(new Checkpoint(name, _scopes.Count, _currentScope.CheckpointTime));
            }
        }

        private static readonly string[] _header = ["Scope", "Depth", "Elapsed Time"];
        private static readonly ColAlign[] _colsAlign = [ColAlign.left, ColAlign.center, ColAlign.right];

        public string ToString(bool humanReadable, int? maxDepth = default)
        {
            // Create a table to display the execution records.
            var table = new ConsoleTable([_header],
                Records.Where(r => !maxDepth.HasValue || r.Depth <= maxDepth)
                .SelectMany(r => new string[][]
                    {
                        ([NamePrefix(r) + r.Name, r.Depth.ToString(), ElapsedTimeString(r, humanReadable)])
                    })
                .Concat([[], ["Total", "", ElapsedTimeToString(_stopwatch.Elapsed, humanReadable)]]).ToArray());

            var maxLen = Records.Max(r => r.Name.Length + r.Depth);
            table.Cols = [maxLen + 2, 10, 20];
            table.ColsAlign = _colsAlign;
            return table.PrintTable();
        }

        private static string NamePrefix(ITrackRecord r)
        {
            return r switch
            {
                Internal.ScopeStart => new string(' ', r.Depth - 1) + "» ",
                Internal.Checkpoint => new string(' ', r.Depth) + "✓ ",
                _ => new string(' ', r.Depth - 1)
            };
        }
        private static string ElapsedTimeString(ITrackRecord r, bool humanReadable)
        {
            return r switch
            {
                Internal.ScopeStart => ElapsedTimeToString(r.ElapsedTime, humanReadable, "▻  "),
                Internal.Checkpoint => ElapsedTimeToString(r.ElapsedTime, humanReadable, "+ "),
                _ => ElapsedTimeToString(r.ElapsedTime, humanReadable)
            };
        }
        private static string ElapsedTimeToString(TimeSpan elapsed, bool humanReadable, string prefix = "")
        {
            return prefix + (humanReadable ? (elapsed.TotalMilliseconds >= 1 ? string.Format("{0:F2} ms", elapsed.TotalMilliseconds)
                : string.Format("{0:F2} µs", elapsed.TotalMicroseconds)) : elapsed.ToString());
        }

        override public string ToString()
        {
            return ToString(true);
        }

        private string GetScopeName()
        {
            return string.Join(" > ", _scopesName.Reverse());
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
