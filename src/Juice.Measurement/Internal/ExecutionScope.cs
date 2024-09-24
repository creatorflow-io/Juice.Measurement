using System.Diagnostics;

namespace Juice.Measurement.Internal
{
    internal class ExecutionScope : IDisposable
    {
        public EventHandler<ExecutionScopeDisposingEventArgs>? OnDispose;
        private Stopwatch? _stopwatch;
        private Stopwatch? _checkpointStopwatch;
        private string _scopeName;
        private string _originalScopeName;
        private bool _disposedValue;
        private List<Checkpoint> _checkpoints = new();

        public Checkpoint[] Checkpoints => _checkpoints.ToArray();
        public TimeSpan ElapsedTime => _stopwatch?.Elapsed ?? TimeSpan.Zero;

        public ExecutionScope(string scopeName, string originalScopeName)
        {
            _stopwatch = Stopwatch.StartNew();
            _checkpointStopwatch = Stopwatch.StartNew();
            _scopeName = scopeName;
            _originalScopeName = originalScopeName;
        }

        public void Checkpoint(string name, long? elapsedMs = default)
        {
            _checkpoints.Add(new Checkpoint(name, elapsedMs ?? _checkpointStopwatch?.ElapsedMilliseconds??0));
            _checkpointStopwatch?.Restart();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _stopwatch?.Stop();
                    OnDispose?.Invoke(this, new ExecutionScopeDisposingEventArgs(_scopeName, _originalScopeName));
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                OnDispose = null;
                _stopwatch = null;
                _checkpointStopwatch = null;
                _checkpoints.Clear();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
