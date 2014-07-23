using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace DataflowPlayground.Blocks
{
    public class TransformBlock
    {
        [Fact]
        public void Basic()
        {
            var transformationBlock = new TransformBlock<int, int>((x) => x*x);
            transformationBlock.Post(5);
            transformationBlock.Post(10);
            transformationBlock.Post(20);
           
            Assert.Equal(25, transformationBlock.Receive());
            Assert.Equal(100, transformationBlock.Receive());
            Assert.Equal(400, transformationBlock.Receive());
        }

        [Fact]
        public void Basic_Async()
        {
            var transformationBlock = new TransformBlock<int, int>((x) => Task.FromResult(x*x));
            transformationBlock.Post(5);
            transformationBlock.Post(10);
            transformationBlock.Post(20);

            Assert.Equal(25, transformationBlock.Receive());
            Assert.Equal(100, transformationBlock.Receive());
            Assert.Equal(400, transformationBlock.Receive());
        }

        [Fact]
        public async Task Basic_TryReceive()
        {
            var transformationBlock = new TransformBlock<int, int>((x) => Task.FromResult(x * x));
            transformationBlock.Post(5);
            transformationBlock.Post(10);
            transformationBlock.Post(20);
            transformationBlock.Complete();
            var itemsReceived = 0;
            while (await transformationBlock.OutputAvailableAsync())
            {
                int item;

                while (transformationBlock.TryReceive(out item))
                {
                    Debug.WriteLine(item);
                    itemsReceived++;
                }
            }
            Assert.Equal(3, itemsReceived);
        }
    }
}
