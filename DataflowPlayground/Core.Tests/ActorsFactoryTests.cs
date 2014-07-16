using Xunit;

namespace Core.Tests
{
    public class ActorsFactoryTests
    {
        [Fact]
        public void when_creating_actor_dynamic_proxy_created()
        {
            IFoo foo = ActorsFactory.Create<IFoo>();
            Assert.NotNull(foo);
        }
        
    }
}