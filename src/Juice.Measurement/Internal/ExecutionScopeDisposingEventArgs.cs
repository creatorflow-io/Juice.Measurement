namespace Juice.Measurement.Internal
{
    internal class ExecutionScopeDisposingEventArgs
    {
        public string ScopeName { get; }
        public string OriginalScopeName { get; }

        public ExecutionScopeDisposingEventArgs(string scopeName, string originalScopeName)
        {
            ScopeName = scopeName;
            OriginalScopeName = originalScopeName;
        }
    }
}
