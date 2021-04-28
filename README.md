# Blend.ActionQueue

This is a simple, non-durable queue for when you need something like a message queue, but with minimal setup, for small, low-volume messages or actions where persistence is not a requirement.

For example, you might use this to invalidate caches across servers without holding up the main UI thread while API calls are executed. Or you might send non-critical notifications via this queue, again freeing up the primary UI thread.

## Usage

To create a queue, implement `AbstractActionQueue<T>`, where `T` is the type of message you'll be queuing. *Note*: each instance of your queue will be a separate queue and thread, so you may want to ensure your queue is a singleton.

## Example Implementation

```csharp
    /// <summary>
    /// This is an example message type where the `Action` to execute is provided in the constructor.
    /// </summary>
    public class ExampleAction
    {
        private readonly Action action;

        public ExampleAction(Action action)
        {
            this.action = action;
        }

        public void Execute() => action?.Invoke();
    }

    /// <summary>
    /// This is an example queue which accepts `ExampleAction` as its "message", and executes the Action.
    /// To log errors, you would set `OnError` to some kind of error handler `Action`.
    /// </summary>
    public class ExampleQueue : AbstractActionQueue<ExampleAction>
    {
        public ExampleQueue(System.Threading.CancellationToken token) : base(token)
        {
        }

        public Action<Exception> OnError { get; set; }

        protected override void LogException(Exception ex) => OnError?.Invoke(ex);

        protected override void ProcessItem(ExampleAction item) => item.Execute();
    }
```

## Example Usage

```csharp
    var exampleQueue = new ExampleQueue(CancellationToken.None);

    int totalExecutions = 0;

    exampleQueue.QueueAction(new ExampleAction(() => totalExecutions += 2));
    exampleQueue.QueueAction(new ExampleAction(() => totalExecutions += 4));
    exampleQueue.QueueAction(new ExampleAction(() => totalExecutions += 8));

    // Hopefully adding 3 numbers doesn't take longer than 100ms
    Thread.Sleep(100);
    Assert.Equal(14, totalExecutions);
```

## Handling Errors

Because `ProcessItem` is being called on a background thread, any errors thrown in the `ProcessItem` will be caught and `LogException` will be called with the exception to pass it through to whatever logging you're using.

## Caveats

The queue is backed by a `BlockingCollection<T>` and items are popped off and executed one at a time. In theory, the queue itself should not have any race conditions or other threading issues, but... mutlithreading is hard.

Keep in mind if using this in an ASP.NET context, you will not be able to rely on things like `HttpContext.Current`, as this is running in a separate thread.

Each instance of a queue is a separate thread and queue. You'll most likely want each queue type to be a singleton. For example:

```csharp
    // WRONG
    new ExampleQueue(CancellationToken.None).QueueAction(new ExampleAction(() => Console.WriteLine("No.")));
    new ExampleQueue(CancellationToken.None).QueueAction(new ExampleAction(() => Console.WriteLine("Don't do this.")));
    new ExampleQueue(CancellationToken.None).QueueAction(new ExampleAction(() => Console.WriteLine("It's wrong.")));
    
    // OK
    private static readonly ExampleQueue queue = new new ExampleQueue(CancellationToken.None);

    queue.QueueAction(new ExampleAction(() => Console.WriteLine("OK.")));
    queue.QueueAction(new ExampleAction(() => Console.WriteLine("Do this.")));
    queue.QueueAction(new ExampleAction(() => Console.WriteLine("It's fine.")));
```

This queue is not durable. If your application shuts down or restarts with items pending in the queue, those items will be lost.
