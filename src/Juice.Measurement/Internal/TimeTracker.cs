using System.Diagnostics;
using Juice.Utils;

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
        public List<ITrackRecord> Records { get; } = [];

        /// <inheritdoc />
        public IDisposable BeginScope(string name)
        {
            _scopesName.Push(name);
            var scope = new ExecutionScope(name, GetScopeFullName());
            scope.OnDispose += (sender, args) =>
            {
                if (sender is ExecutionScope scope)
                {
                    Records.Add(new ScopeEnd(scope.Name, scope.FullName, _scopesName.Count-1, _stopwatch.Elapsed, scope.ElapsedTime));
                }
                _scopesName.Pop();
                _scopes.Pop();
                _currentScope = _scopes.Count > 0 ? _scopes.Peek() : null;
            };
            _scopes.Push(scope);

            Records.Add(new ScopeStart(name, scope.FullName, _scopes.Count-1, _stopwatch.Elapsed, _currentScope?.ElapsedTime ?? _stopwatch.Elapsed));
            _currentScope = scope;

            return scope;
        }

        /// <inheritdoc />
        public void Checkpoint(string name)
        {
            if (_currentScope != null)
            {
                Records.Add(_currentScope.Checkpoint(name, _scopes.Count, _stopwatch.Elapsed));
            }
            else
            {
                Records.Add(new Checkpoint(name, GetScopeFullName(), _scopes.Count, _stopwatch.Elapsed, _stopwatch.Elapsed));
            }
        }

        private static readonly string[] _header = ["Scope", "Depth", "Time Line", "Elapsed Time"];
        private static readonly ColAlign[] _colsAlign = [ColAlign.left, ColAlign.center, ColAlign.left, ColAlign.left];

        /// <inheritdoc />
        public string ToString(bool humanReadable, int? maxDepth = default, bool checkpoint = true)
        {
            // Create a table to display the execution records.
            var records = Records
                .Where(r => !maxDepth.HasValue || r.Depth <= maxDepth)
                .Where(r => checkpoint || r is not Internal.Checkpoint)
                .SelectMany(r => new string[][]
                    {
                        ([Name(r), r.Depth.ToString(), TimeLine(r, humanReadable), ElapsedTime(r, humanReadable)])
                    });

            var table = new ConsoleTable([_header],
                records
                .Concat([[], ["Total", "", "", ElapsedTimeToString(_stopwatch.Elapsed, humanReadable)]]).ToArray());

            var nameMaxLength = Records.Max(r => r.Name.Length + r.Depth + 2); // 2 for the prefix
            var timeMaxLength = records.Max(r => r[3].Length + 2);
            table.Cols = [nameMaxLength + 2, 10, humanReadable ? 15 : 20, timeMaxLength];
            table.ColsAlign = _colsAlign;
            return table.PrintTable();
        }

        private static string Name(ITrackRecord r)
        {
            return new string(' ', r.Depth) + r switch
            {
                ScopeStart => "« ",
                Checkpoint check => "› ",
                _ =>  "» "
            } + r.Name;
        }

        private static string TimeLine(ITrackRecord r, bool humanReadable) => ElapsedTimeToString(r.RecordTime, humanReadable, "› ", 3);
        private static string ElapsedTime(ITrackRecord r, bool humanReadable)
        {
            return r switch
            {
                Checkpoint check => new string(' ', r.Depth) + ElapsedTimeToString(check.LocalTime, humanReadable, "+ "),
                ScopeStart start => new string(' ', r.Depth) + ElapsedTimeToString(start.LocalTime, humanReadable, "+ "),
                ScopeEnd end => new string(' ', r.Depth) + ElapsedTimeToString(end.ElapsedTime, humanReadable),
                _ => ""
            };
        }
        private static string ElapsedTimeToString(TimeSpan elapsed, bool humanReadable, string prefix = "", int nums=1)
        {
            return prefix + (humanReadable ? (elapsed.TotalMilliseconds >= 1 ? string.Format($"{{0:F{nums}}} ms", elapsed.TotalMilliseconds)
                : string.Format("{0} µs", elapsed.TotalMicroseconds)) : elapsed.ToString());
        }

        override public string ToString()
        {
            return ToString(true);
        }

        private string GetScopeFullName()
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
