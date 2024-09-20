namespace Juice.Measurement.Internal
{
    internal class ExecutionScopeDisposingEventArgs
    {
        public string ScopeName { get; }
        public string OriginalScopeName { get; }
        public TimeSpan ElapsedTime { get; }

        public ExecutionScopeDisposingEventArgs(string scopeName, string originalScopeName, TimeSpan elapsedTime)
        {
            ScopeName = scopeName;
            OriginalScopeName = originalScopeName;
            ElapsedTime = elapsedTime;
        }
    }
}
