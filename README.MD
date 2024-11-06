### Measurement

This service is useful for measure the execution time of a method by create the execution scopes and checkpoints in your code, and then print the details of execution time step by step to determine where is the slowly code.
You can use it with new transient instance to meansure inside a class, method, or use dependency injection pattern to meansure cross services.

```csharp
    ITimeTracker timeTracker = new TimeTracker();
    using (timeTracker.BeginScope("Test", "{scopeId}"))
    {
        // Do something
        await Task.Delay(12);
        timeTracker.Checkpoint("Checkpoint 0");

        using (timeTracker.BeginScope("Inner Test"))
        {
            // Do something
            await Task.Delay(15);

            using (timeTracker.BeginScope("Inner Inner Test"))
            {
                // Do something
                timeTracker.Checkpoint("Checkpoint 1.1");
                await Task.Delay(20);
                timeTracker.Checkpoint("Checkpoint 1.2");
            }
        }
        timeTracker.Checkpoint("Checkpoint 1");
        using (timeTracker.BeginScope("Inner Test 2"))
        {
            // Do something
            await Task.Delay(10);
            timeTracker.Checkpoint("Checkpoint 1.3");
            timeTracker.Checkpoint("Checkpoint 1.4");
        }
    }
    Console.WriteLine(timeTracker.ToString());

    // Output:
 ________________________________________________________________
|        Scope         |  Depth   |   Time Line   | Elapsed Time |
|----------------------------------------------------------------|
| › Start              |    0     | › 344.2 µs    | + 382.3 µs   |
| « Test               |    0     | › 633.3 µs    | + 250 µs     |
|  › Checkpoint 0.0    |    1     | › 663.3 µs    |  + 79.7 µs   |
|  › Checkpoint 0.1    |    1     | › 17.416 ms   |  + 16.7 ms   |
|  « Inner Test        |    1     | › 17.445 ms   |  + 16.8 ms   |
|   › Checkpoint 1.0   |    2     | › 17.446 ms   |   + 1.7 µs   |
|   « Inner Inner Test |    2     | › 32.603 ms   |   + 15.2 ms  |
|    › Checkpoint 1.1  |    3     | › 32.604 ms   |    + 1.2 µs  |
|    › Checkpoint 1.2  |    3     | › 63.992 ms   |    + 31.4 ms |
|   » Inner Inner Test |    2     | › 64.294 ms   |   31.5 ms    |
|  » Inner Test        |    1     | › 64.349 ms   |  46.9 ms     |
|  › Checkpoint 1      |    1     | › 64.350 ms   |  + 46.9 ms   |
|  « Inner Test 2      |    1     | › 64.356 ms   |  + 63.7 ms   |
|   › Checkpoint 1.3.0 |    2     | › 64.356 ms   |   + 0.8 µs   |
|   › Checkpoint 1.3.1 |    2     | › 79.517 ms   |   + 15.2 ms  |
|   › Checkpoint 1.4   |    2     | › 79.518 ms   |   + 0.9 µs   |
|  » Inner Test 2      |    1     | › 79.519 ms   |  15.2 ms     |
| » Test               |    0     | › 79.522 ms   | 78.9 ms      |
| › End                |    0     | › 79.530 ms   | + 7.8 µs     |
|----------------------------------------------------------------|
| Total                |          |               | 82.9 ms      |
'----------------------------------------------------------------'
```

Inside the **Elapsed Time** column:
- '›' symbol means the time elapsed from begin of the measurement
- '+' symbol means the time elapsed from begin of the scope or the last checkpoint

### Measurement time store

Store recorded scopes if the `scopeId` and `traceId` have specified, and you can get the details of the scope by the `scopeId` and `traceId`.
It may take more time on the request processing to store the data in the database (often 5 ~ 50 ms), so it is recommended to use it in the background or conditional.

```csharp
// program.cs

    builder.Services.AddExecutionTimeMeasurement();
    builder.Services.AddMeasurementEFStores(builder.Configuration, options =>
    {
        options.DatabaseProvider = "SqlServer";
    });

// middleware
    
    var tracker = context.RequestServices.GetRequiredService<ITimeTracker>();
    var repo = context.RequestServices.GetRequiredService<ITimeRepository>();
    var traceId = context.TraceIdentifier;
    var scopeId = "scopeId";
    var scopeName = "scopeName";
    using var _ = tracker.BeginScope(scopeName, scopeId);
    await _next(context);

    await repo.SaveTrackDataAsync(tracker, traceId, scopeId, scopeName);
```