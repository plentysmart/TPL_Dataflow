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
            Assert.True( divideBlock.Post(2));
            Assert.True( divideBlock.Post(3));
            Assert.True( divideBlock.Post(6));
            divideBlock.Complete();

            while (await divideBlock.OutputAvailableAsync())
            {
                Assert.DoesNotThrow(() =>
                {
                    var value = divideBlock.Receive();
                    Debug.WriteLine(value);
                });
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
                Assert.DoesNotThrow(() =>
                {
                    var value = divideBlock.Receive();
                    Debug.WriteLine(value);
                });
            }
            Assert.False(divideBlock.Post(2));
        }
    }
}
