using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Core.Tests
{
    public class ActorTests
    {
        [Fact]
        public void when_message_posted_and_there_is_handler_registered_execute_handler()
        {
            var testActor = new ActorTest();
            testActor.Post(new ActorTest.TestMessage());
            Assert.True(testActor.Executed);
        }

        [Fact]
        public void messages_should_be_executed_sequentionaly_one_after_another()
        {
            var testActor = new ActorTest();
            var tasks =  new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                var taskId = i;
                tasks.Add(new Task(() => testActor.Post(new ActorTest.MultiThreadedMessage{Value = taskId})));
            }
            foreach (var task in tasks)
            {
                task.Start();
            }
            Task.WaitAll(tasks.ToArray());
            // Needs better way to figure out if message has been processed;
            Thread.Sleep(10 * 1000);
            Assert.False(testActor.Error);
            Assert.Equal(10,testActor.HandledValues.Distinct().Count());
        }


        private class ActorTest : Actor
        {
            public bool Executed = false;

            public bool Error = false;

            public List<int>  HandledValues= new List<int>();

            private int internalValue;

            public ActorTest()
            {
                this.On<TestMessage>((message) =>
                {
                    this.Executed = true;
                });
                this.On<MultiThreadedMessage>((message) =>
                {
                    internalValue = message.Value;
                    for (int i = 0; i < 3; i++)
                    {
                       Thread.Sleep(300);
                        if (internalValue != message.Value)
                        {
                            Error = true;
                            return;
                        }
                    }
                    HandledValues.Add(internalValue);

                });
            }

            public class TestMessage : Message
            {
           
            }

            public class MultiThreadedMessage : Message
            {
                public int Value { get; set; }
            }
        }
    }
}
