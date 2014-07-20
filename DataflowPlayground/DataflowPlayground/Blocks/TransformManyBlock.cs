﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace DataflowPlayground.Blocks
{
    public class TransformManyBlock
    {
        [Fact]
        public  void Basic_Async()
        {
            // Xunit runner does not wait for async code;
            Basic_AsyncWrapper().Wait();
        }

        private static async Task Basic_AsyncWrapper()
        {
            var deArrayBlock = new TransformManyBlock<int[], int>(x => x.AsEnumerable());
            deArrayBlock.Post(new[] {1, 2, 3, 4, 5});
            deArrayBlock.Complete();
            var itemsReceived = 0;
            while (await deArrayBlock.OutputAvailableAsync())
            {
                int item = await deArrayBlock.ReceiveAsync();
                Debug.WriteLine(item);
                itemsReceived++;
            }
            Assert.Equal(5, itemsReceived);
            /* Output:
             * 1
             * 2
             * 3
             * 4
             * 5
             */
        }
    }
}
