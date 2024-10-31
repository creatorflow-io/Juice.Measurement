### Measurement

This service is useful for measure the execution time of a method by create the execution scopes and checkpoints in your code, and then print the details of execution time step by step to determine where is the slowly code.
You can use it with new transient instance to meansure inside a class, method, or use dependency injection pattern to meansure cross services.

```csharp
    ITimeTracker timeTracker = new TimeTracker();
    using (timeTracker.BeginScope("Test"))
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
| › Start              |    0     | › 334.2 µs    | + 367.3 µs   |
| « Test               |    0     | › 602.5 µs    | + 234.3 µs   |
|  › Checkpoint 0.0    |    1     | › 635.3 µs    |  + 83.1 µs   |
|  › Checkpoint 0.1    |    1     | › 5.974 ms    |  + 5.3 ms    |
|  « Inner Test        |    1     | › 5.991 ms    |  + 5.4 ms    |
|   › Checkpoint 1.0   |    2     | › 5.992 ms    |   + 1.5 µs   |
|   « Inner Inner Test |    2     | › 20.637 ms   |   + 14.6 ms  |
|    › Checkpoint 1.1  |    3     | › 20.637 ms   |    + 1.3 µs  |
|    › Checkpoint 1.2  |    3     | › 52.464 ms   |    + 31.8 ms |
|   » Inner Inner Test |    2     | › 52.931 ms   |   32.1 ms    |
|  » Inner Test        |    1     | › 53.001 ms   |  47.0 ms     |
|  › Checkpoint 1      |    1     | › 53.002 ms   |  + 47.0 ms   |
|  « Inner Test 2      |    1     | › 53.020 ms   |  + 52.4 ms   |
|   › Checkpoint 1.3.0 |    2     | › 53.021 ms   |   + 7.3 µs   |
|   › Checkpoint 1.3.1 |    2     | › 68.046 ms   |   + 15.0 ms  |
|   › Checkpoint 1.4   |    2     | › 68.051 ms   |   + 4.5 µs   |
|  » Inner Test 2      |    1     | › 68.054 ms   |  15.0 ms     |
| » Test               |    0     | › 68.059 ms   | 67.5 ms      |
| › End                |    0     | › 68.077 ms   | + 18.9 µs    |
|----------------------------------------------------------------|
| Total                |          |               | 68.9 ms      |
'----------------------------------------------------------------'
```

Inside the **Elapsed Time** column:
- '›' symbol means the time elapsed from begin of the measurement
- '+' symbol means the time elapsed from begin of the scope or the last checkpoint