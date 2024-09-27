using System.Diagnostics;

namespace Juice.Measurement.Internal
{
    internal class ExecutionScope : IDisposable
    {
        public EventHandler<ExecutionScopeDisposingEventArgs>? OnDispose;
        private Stopwatch? _stopwatch;
        private Stopwatch? _checkpoint;
        private string _scopeName;
        private string _originalScopeName;
        public TimeSpan ElapsedTime => _stopwatch?.Elapsed ?? TimeSpan.Zero;
        public TimeSpan CheckpointTime
        {
            get
            {
                var checkpointTime = _checkpoint?.Elapsed ?? TimeSpan.Zero;
                _checkpoint?.Restart();
                return checkpointTime;
            }
        }

        public ExecutionScope(string scopeName, string originalScopeName)
        {
            _stopwatch = Stopwatch.StartNew();
            _checkpoint = Stopwatch.StartNew();
            _scopeName = scopeName;
            _originalScopeName = originalScopeName;
        }

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _stopwatch?.Stop();
                    _checkpoint?.Stop();
                    OnDispose?.Invoke(this, new ExecutionScopeDisposingEventArgs(_scopeName, _originalScopeName));
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                OnDispose = null;
                _stopwatch = null;
                _checkpoint = null;
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
