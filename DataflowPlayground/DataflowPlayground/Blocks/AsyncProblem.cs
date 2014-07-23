using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace DataflowPlayground.Blocks
{
    public class AsyncProblem
    {
        [Fact]
        public void AsyncVsAwaiterProblem()
        {
            var max = 1000;
            var noOfExceptions = 0;
            for (int i = 0; i < max; i++)
            {
                try
                {
                    Await().Wait();
                }
                catch
                {
                    noOfExceptions++;
                }
            }
            Assert.Equal(max,noOfExceptions);
        }

        public async Task Await()
        {
            bool firstPassed = false;
            var divideBlock = new TransformBlock<int, int>((x) =>
            {
                if (firstPassed)
                    throw new ArgumentException("error");
                firstPassed = true;
                return 0;
            });
            divideBlock.Post(2);
            divideBlock.Post(3); // this should cause failure;
            divideBlock.Complete();

            while (await divideBlock.OutputAvailableAsync())
            {
                    var value = divideBlock.Receive(); // this should throw exception on second call

            }
            try
            {
                divideBlock.Completion.Wait();
            }
            catch
            {
            }
        }
    }
}
