using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowExceptionHandling
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            RunTest();

            Console.ReadLine();
        }

        private static async void RunTest()
        {
            try
            {
                BasicExample().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("First call: exception thrown");
            }

            try
            {
                BasicExample().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Second call: exception thrown");
            }

        }

        public static async Task BasicExample()
        {
            var processedValues = new List<int>();
            var divideBlock = new TransformBlock<int, int>((x) =>
            {
                if (x%2 != 0)
                    throw new ArgumentException("This block can process only even numbers");
                processedValues.Add(x);
                return x/2;
            });
            divideBlock.Post(2);
            divideBlock.Post(3);
            divideBlock.Post(6);
            divideBlock.Complete();

            while (divideBlock.OutputAvailableAsync().GetAwaiter().GetResult())
            {
                try
                {
                    var value = divideBlock.Receive();
                    Console.WriteLine(value);

                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(" divideBlock.Receive(): exception thrown");
                    throw;
                }
            }
            try
            {
                divideBlock.Completion.Wait();
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("divideBlock.Completion.Wait(): exception thrown");
            }

        }
    }
}
