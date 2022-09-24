using System;
using System.Threading;

namespace BlendInteractive.ActionQueue.Tests
{
    public class TestActionQueue : AbstractActionQueue<TestAction>
    {
        public TestActionQueue(CancellationToken token) : base(token) { }

        public Action<Exception> OnException { get; set; }

        protected override void LogException(Exception ex) => OnException?.Invoke(ex);

        protected override void ProcessItem(TestAction item) => item.Execute();

        public TestAction QueueEmptyTestMessage() => this.QueueAction(EmptyTestMessage());

        public TestAction QueueTestMessage(Action action) => this.QueueAction(new TestAction(action));

        public static TestAction EmptyTestMessage() => new TestAction(() => { });
    }


}
