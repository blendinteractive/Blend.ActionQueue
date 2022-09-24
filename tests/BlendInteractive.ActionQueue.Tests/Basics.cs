using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace BlendInteractive.ActionQueue.Tests
{
    public class Basics
    {
        [Fact]
        public void CanQueueAndExecuteItem()
        {
            var tokenSource = new System.Threading.CancellationTokenSource();
            var queue = new TestActionQueue(tokenSource.Token);

            var message = queue.QueueEmptyTestMessage();

            message.Wait();
            Assert.True(message.MessageExecuted, "Message has executed");

            tokenSource.Cancel();
        }

        [Fact]
        public void CanQueueMultipleItems()
        {
            var tokenSource = new CancellationTokenSource();
            var queue = new TestActionQueue(tokenSource.Token);

            var one = queue.QueueEmptyTestMessage();
            one.Wait();
            Assert.True(one.MessageExecuted, "Message one has executed");

            var two = queue.QueueEmptyTestMessage();
            two.Wait();
            Assert.True(two.MessageExecuted, "Message two has executed");

            tokenSource.Cancel();
        }

        [Fact]
        public void ItemsExecuteInOrder()
        {
            var tokenSource = new CancellationTokenSource();
            var queue = new TestActionQueue(tokenSource.Token);

            var order = new List<int>();

            var one = queue.QueueTestMessage(() => { order.Add(1); Thread.Sleep(100); });
            var two = queue.QueueTestMessage(() => { order.Add(2); Thread.Sleep(100); });
            var three = queue.QueueTestMessage(() => { order.Add(3); Thread.Sleep(100); });

            WaitHandle.WaitAll(new WaitHandle[] { one.DoneSignal, two.DoneSignal, three.DoneSignal }, new TimeSpan(0, 0, 5), false);

            Assert.Equal(3, order.Count);
            Assert.Equal(1, order[0]);
            Assert.Equal(2, order[1]);
            Assert.Equal(3, order[2]);

            tokenSource.Cancel();
        }

        [Fact]
        public void ExceptionsCanBeCaughtAndDontStopQueueProcessing()
        {
            var tokenSource = new CancellationTokenSource();
            var queue = new TestActionQueue(tokenSource.Token);
            Exception caughtException = null;
            queue.OnException = (ex) => caughtException = ex;

            queue.QueueTestMessage(() => { Thread.Sleep(100); throw new InvalidOperationException("Testing exception"); });
            var followUp = queue.QueueEmptyTestMessage();

            followUp.Wait();

            Assert.NotNull(caughtException);
            Assert.Equal("Testing exception", caughtException.Message);
            Assert.IsType<InvalidOperationException>(caughtException);

            Assert.True(followUp.MessageExecuted);

            tokenSource.Cancel();
        }
    }
}
