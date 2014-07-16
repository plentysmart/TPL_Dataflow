using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ActorsFactory
    {
        public static T Create<T>() where T:IActor
        {
            var implementation = Assembly.GetCallingAssembly().GetTypes().Where(x => x.GetInterfaces().Any(i => i == typeof (T))).Single();
            return (T)new DynamicWrapper<T>( implementation).GetTransparentProxy();
        }

        private class DynamicWrapper<T> : RealProxy
        {
            private dynamic _realInstance;
            
            public DynamicWrapper( Type realType):base(realType)
            {
                _realInstance = Activator.CreateInstance(realType);
            }

            public override IMessage Invoke(IMessage msg)
            {
                throw new NotImplementedException();
            }
        }
    }

    public interface IActor
    {
    }

}
