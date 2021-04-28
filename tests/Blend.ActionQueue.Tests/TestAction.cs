using System;
using System.Threading;

namespace Blend.ActionQueue.Tests
{
    public class TestAction
    {
        private readonly Action actionToTake;

        public TestAction(Action actionToTake)
        {
            this.actionToTake = actionToTake;
        }

        public TimeSpan ElapsedExecutionTime { get; private set; }
        public bool MessageExecuted { get; private set; }
        public ManualResetEvent DoneSignal { get; } = new ManualResetEvent(false);

        public void Execute()
        {
            DoneSignal.Reset();

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            actionToTake?.Invoke();

            stopwatch.Stop();
            this.ElapsedExecutionTime = stopwatch.Elapsed;
            MessageExecuted = true;
            DoneSignal.Set();
        }

        public void Wait() => DoneSignal.WaitOne();
    }
}
