using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Core
{
    public abstract class Actor
    {
        private readonly Dictionary<Type, Action<object>> _handlers = new Dictionary<Type, Action<object>>();

        private ActionBlock<Message> executor;

        protected Actor()
        {
            executor = new ActionBlock<Message>(
                (message) => _handlers[message.GetType()](message));
        }

        public void Post<T>(T message) where T : Message
        {
            executor.Post(message);
        }

        protected void On<T>(Action<T> @do) where T:Message
        {
            _handlers.Add(typeof(T), o => @do((T)o));
        }
    }
}
