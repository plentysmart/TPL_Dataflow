using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tests
{
    public interface IFoo:IActor
    {
        int SomeMethod(int input);
    }

    public class Foo : IFoo
    {
        public int SomeMethod(int input)
        {
            return input * 10;
        }
    }
}
