using System.Threading;
using Xunit;

namespace Blend.ActionQueue.Tests.Examples
{
    public class ExampleTests
    {
        [Fact]
        public void TestQueueWorks()
        {
            var exampleQueue = new ExampleQueue(CancellationToken.None);

            int totalExecutions = 0;

            exampleQueue.QueueAction(new ExampleAction(() => totalExecutions += 2));
            exampleQueue.QueueAction(new ExampleAction(() => totalExecutions += 4));
            exampleQueue.QueueAction(new ExampleAction(() => totalExecutions += 8));

            // Hopefully adding 3 numbers doesn't take longer than 100ms
            Thread.Sleep(100);
            Assert.Equal(14, totalExecutions);
        }

    }
}
