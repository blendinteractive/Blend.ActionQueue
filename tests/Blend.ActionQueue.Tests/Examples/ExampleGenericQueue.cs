using System;

namespace Blend.ActionQueue.Tests.Examples
{
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
}
