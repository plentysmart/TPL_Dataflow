using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace DataflowPlayground.Blocks
{
    public class ExceptionHandling
    {
        [Fact]
        public void BasicExample()
        {
            BasicExampleAsyncWrapper().Wait();
        }

        private static async Task BasicExampleAsyncWrapper()
        {
            var processedValues = new List<int>();
            var divideBlock = new TransformBlock<int, int>((x) =>
            {
                if (x%2 != 0)
                    throw new ArgumentException("This block can process only even numbers");
                processedValues.Add(x);
                return x/2;
            });
            Assert.True(divideBlock.Post(2));
            Assert.True(divideBlock.Post(3));
            Assert.True(divideBlock.Post(6));
            divideBlock.Complete();

            while (await divideBlock.OutputAvailableAsync())
            {
              
                    var value = divideBlock.Receive();
                    Debug.WriteLine(value);
               
            }
            Assert.Equal(1, processedValues.Count); // Only first message processed
            Assert.True(divideBlock.Completion.IsFaulted);
        }

        [Fact]
        public void Keep_posting_to_failed_block()
        {
            KeepPostingAsyncWrapper().Wait();
        }

        private static async Task KeepPostingAsyncWrapper(ExecutionDataflowBlockOptions options = null)
        {
            var processedValues = new List<int>();
            Func<int, int> transform = (x) =>
            {
                if (x%2 != 0)
                    throw new ArgumentException("This block can process only even numbers");
                processedValues.Add(x);
                return x/2;
            };

            var divideBlock = options != null
                ? new TransformBlock<int, int>(transform, options)
                : new TransformBlock<int, int>(transform);
            Assert.True(divideBlock.Post(2));
            Assert.True(divideBlock.Post(3));
            while (await divideBlock.OutputAvailableAsync())
            {
                    var value = divideBlock.Receive();
                    Debug.WriteLine(value);  
            }
            Assert.False(divideBlock.Post(2));
        }

        [Fact]
        public void ExceptionHandlig_FirstApproach()
        {
            ExceptionHandlig_FirstApproachSyncWrapper().Wait();
        }

        private static async Task ExceptionHandlig_FirstApproachSyncWrapper()
        {
            var processedValues = new List<int>();
            Func<int, MessageOut<int, int>> transform = (x) =>
            {
                if (x%2 != 0)
                    return new MessageOut<int, int>(x, new ArgumentException("This block can process only even numbers"));
                processedValues.Add(x);
                return new MessageOut<int, int>(x, x/2);
            };

            var divideBlock = new TransformBlock<int, MessageOut<int, int>>(transform);
            divideBlock.Post(2);
            divideBlock.Post(3);
            divideBlock.Post(4);
            divideBlock.Post(6);
            divideBlock.Complete();
            var sucessfull = 0;
            var failed = 0;
            while (await divideBlock.OutputAvailableAsync())
            {
                    var value = divideBlock.Receive();
                    if (value.IsFaulted) failed++;
                    else
                    {
                        sucessfull++;
                        Debug.WriteLine(value.Output);
                    }
            }

            Assert.Equal(3, sucessfull);
            Assert.Equal(1, failed);
        }

        public class MessageOut<TIn, TOut>
        {
            private readonly Exception _exception;
            private TOut _output;
            public TIn Input { get; private set; }

            public TOut Output
            {
                get
                {
                    if (IsFaulted)
                        throw _exception ?? new ArgumentException("Message is faulted. Detailed exception not available");
                    return _output;
                }
                private set { _output = value; }
            }


            public MessageOut(TIn input,TOut output)
            {
                Input = input;
                Output = output;
            }
            public MessageOut(TIn input, Exception exception)
            {
                _exception = exception;
                Input = input;
                IsFaulted = true;
            }

            public bool IsFaulted { get; private set; }
        }
    }
}
