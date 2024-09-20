using System.Diagnostics;

namespace Juice.Measurement.Internal
{
    internal class ExecutionScope : IDisposable
    {
        public EventHandler<ExecutionScopeDisposingEventArgs>? OnDispose;
        private Stopwatch? _stopwatch;
        private string _scopeName;
        private string _originalScopeName;
        private bool _disposedValue;

        public ExecutionScope(string scopeName, string originalScopeName)
        {
            _stopwatch = Stopwatch.StartNew();
            _scopeName = scopeName;
            _originalScopeName = originalScopeName;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _stopwatch?.Stop();
                    OnDispose?.Invoke(this, new ExecutionScopeDisposingEventArgs(_scopeName, _originalScopeName, _stopwatch?.Elapsed ?? TimeSpan.Zero));
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                OnDispose = null;
                _stopwatch = null;
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
