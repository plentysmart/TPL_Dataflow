using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace DataflowPlayground.Blocks
{
    public class ActionBlockSamples
    {
        /*
         * ActionBlock<T> is target only, does not return any type
         */

        [Fact]
        public void BasicUsage()
        {
            var list = new List<int> {1, 2, 4, 5, 6, 7, 8, 9};

            var removeFromListBlock = new ActionBlock<int>((x) =>
            {
                list.Remove(x);
                Thread.Sleep(100);
            });
            var sp = Stopwatch.StartNew();
            foreach (var element in list.ToArray())
            {
                removeFromListBlock.Post(element);
                Debug.WriteLine("{0} ms- {1} posted", sp.ElapsedMilliseconds, element);
            }
            removeFromListBlock.Complete();
            removeFromListBlock.Completion.Wait();
            Debug.WriteLine("{0} ms - finished", sp.ElapsedMilliseconds);
            Assert.Empty(list);
        }

        [Fact]
        public void BasicUsage_Async()
        {
            var list = new List<int> { 1, 2, 4, 5, 6, 7, 8, 9 };
            /* From documentation (http://msdn.microsoft.com/en-us/library/hh228603(v=vs.110).aspx):
             * When you use an ActionBlock<TInput> object with Action, 
             * processing of each input element is considered completed when the delegate returns.
             * When you use an ActionBlock<TInput> object with System.Func<TInput, Task>, 
             * processing of each input element is considered completed only when the returned Task object is completed
             */
            var removeFromListBlock = new ActionBlock<int>(
                (x) =>
                {
                    var task = new Task(() =>
                    {
                        list.Remove(x);
                         Task.Delay(100).Wait();
                    });
                    task.Start();
                    return task;
                });
            var sp = Stopwatch.StartNew();
            foreach (var element in list.ToArray())
            {
                removeFromListBlock.Post(element);
                Debug.WriteLine("{0} ms- {1} posted", sp.ElapsedMilliseconds, element);
            }
            removeFromListBlock.Complete();
            removeFromListBlock.Completion.Wait();
            Debug.WriteLine("{0} ms - finished", sp.ElapsedMilliseconds);
            Assert.Empty(list);
        }

        [Fact]
        public void ExecutionOptions_BoundedCapacity()
        {
            var options = new ExecutionDataflowBlockOptions()
            {
                BoundedCapacity = 1
            };
            int executions = 0;
            var waitingActcionBlock = new ActionBlock<int>((x) =>
            {
                executions++;
               Thread.Sleep(x);
            }, options);
            Assert.True( waitingActcionBlock.Post(100));
            // Returns false if capacity excided
            Assert.False( waitingActcionBlock.Post(100));
            Assert.False( waitingActcionBlock.Post(100));
            waitingActcionBlock.Complete();
            waitingActcionBlock.Completion.Wait();
            Assert.Equal(1, executions);
        }

        [Fact]
        public void ExecutionOptions_MaxMessagesPerTask()
        {
            var options = new ExecutionDataflowBlockOptions()
            {
                MaxMessagesPerTask = 2
            };
            int executions = 0;
            var waitingActcionBlock = new ActionBlock<int>((x) =>
            {
                /* Normally there would be one task running this block in a loop
                 * Something like ( System.Threading.Tasks.Dataflow.Internal.TargetCore`1.ProcessMessagesLoopCore()):
                 * while(this.Recieve())
                 * {
                 *  @action(x)
                 * }
                 * MaxMessagesPerTask will limit number of messages processed by one task.
                 *
                 * Not really sure how to check if it's running in a new task
                 */
                executions++;
            }, options);

            Assert.True( waitingActcionBlock.Post(100));
            // Returns false if capacity excided
            waitingActcionBlock.Post(100);
            waitingActcionBlock.Complete();
            waitingActcionBlock.Completion.Wait();
            Assert.Equal(2, executions);
        }

        [Fact]
        public void ExecutionOptions_MaxDegreeOfParallelism()
        {
            Assert.True(Environment.ProcessorCount > 1,
                "You really want to see parallel execution on single processor machine?!?!?!");
            var concurrencyObserved = false;
            var options = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
                //MaxDegreeOfParallelism = 1 //uncomment this line to the difference
            };
            var enterFlag = false;
            var concurrentBlock = new ActionBlock<int>((x) =>
            {
                enterFlag = true;
                Thread.Sleep(100);
                if (!enterFlag)
                    concurrencyObserved = true;
                enterFlag = false;

            }, options);
            for (int i = 0; i < 10; i++)
            {
                concurrentBlock.Post(i);
            }
            concurrentBlock.Complete();
            concurrentBlock.Completion.Wait();
            Assert.True(concurrencyObserved);
        }
    }
}
