using System.Linq;
using System.Text;
using Juice.Measurement.Internal;
using Juice.Utils;

namespace Juice.Measurement
{
    /// <summary>
    /// Scoped service to track time.
    /// </summary>
	public class TimeTracker : ITimeTracker
    {
        private Stack<string> _scopes = new();
        public List<ExecutionRecord> Records { get; } = new();

        /// <inheritdoc />
        public IDisposable NewExecutionScope(string name)
        {

            _scopes.Push(name);
            var scope = new ExecutionScope(GetScopeName(), name);
            scope.OnDispose += (sender, args) =>
            {
                Records.Add(new ExecutionRecord(args.OriginalScopeName, args.ScopeName, _scopes.Count, args.ElapsedTime));
                _scopes.Pop();
            };
            return scope;
        }

        public string ToString(bool displayMillisecond)
        {
            // Create a table to display the execution records.
            var table = new ConsoleTable(new string[][] { new string[] { "Scope", "Depth", "Elapsed Time" } },
                Records.Select(r => new string[] { r.ScopeName, r.Depth.ToString(),
                    displayMillisecond ? r.ElapsedTime.TotalMilliseconds + " ms" : r.ElapsedTime.ToString() }).ToArray());
            var maxLen = Records.Max(r => r.ScopeName.Length);
            table.Cols = new int[] { maxLen + 2, 10, 20 };
            table.ColsAlign = new[] { ColAlign.left, ColAlign.center, ColAlign.right };
            return table.PrintTable();
        }

        override public string ToString()
        {
            return ToString(false);
        }

        private string GetScopeName()
        {
            return string.Join(" -> ", _scopes.Reverse());
        }

        private bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _scopes.Clear();
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
