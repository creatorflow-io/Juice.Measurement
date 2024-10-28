using System.Diagnostics;

namespace Juice.Measurement.Internal
{
    internal class ExecutionScope : IDisposable
    {
        public EventHandler? OnDispose;
        private Stopwatch? _stopwatch;
        private Stopwatch? _checkpoint;
        private string _scopeName;
        private string _scopeFullName;

        public string Name => _scopeName;
        public string FullName => _scopeFullName;
        public TimeSpan ElapsedTime => _stopwatch?.Elapsed ?? TimeSpan.Zero;
        private TimeSpan CheckpointTime
        {
            get
            {
                var checkpointTime = _checkpoint?.Elapsed ?? TimeSpan.Zero;
                _checkpoint?.Restart();
                return checkpointTime;
            }
        }

        public ExecutionScope(string scopeName, string scopeFullName)
        {
            _stopwatch = Stopwatch.StartNew();
            _checkpoint = Stopwatch.StartNew();
            _scopeName = scopeName;
            _scopeFullName = scopeFullName;
        }

        public Checkpoint Checkpoint(string name, int depth, TimeSpan start) => new(name, _scopeFullName, depth, start, CheckpointTime);

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
                    OnDispose?.Invoke(this, EventArgs.Empty);
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
