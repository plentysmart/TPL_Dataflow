using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace DataflowPlayground.Pipes
{
    public class BasicExamples
    {
        [Fact]
        public void Basic_LinkTo()
        {
            var resultList = new List<string>();
            var bufferBlock = new BufferBlock<string>();
            var greetingBlock = new TransformBlock<string, string>(s =>
            {
                var greeting = "Hello " + s;
                Debug.WriteLine(greeting);
                return greeting;
            });
            var reverseBlock = new TransformBlock<string, string>(s =>
            {
                var reversed = new string(s.Reverse().ToArray());
                Debug.WriteLine(reversed);
                return reversed;
            });
            var addToListBlock = new ActionBlock<string>(s =>
            {
                resultList.Add(s);
            });
            bufferBlock.LinkTo(greetingBlock);
            greetingBlock.LinkTo(reverseBlock);
            reverseBlock.LinkTo(addToListBlock);
            bufferBlock.Post("first");
            bufferBlock.Post("second");
            bufferBlock.Post("third");
            bufferBlock.Complete();
            addToListBlock.Completion.Wait(1000);
            Assert.NotEmpty(resultList);
            Assert.Equal(3, resultList.Count);
        }
    }
}
