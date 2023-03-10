using Funq;
using NUnit.Framework;

namespace NServiceKit.Common.Tests
{
    /// <summary>A funq tests.</summary>
    [TestFixture]
    public class FunqTests
    {
        interface IBar { }
        class Bar : IBar { }
        class TestFoo { public IBar Bar { get; set; } }

        /// <summary>Tests 1.</summary>
        [Test]
        public void Test1()
        {
            var container = new Container();
            var m = new TestFoo();
            container.Register<IBar>(new Bar());
            Assert.NotNull(container.Resolve<IBar>(), "Resolve");
            container.AutoWire(m);
            Assert.NotNull(m.Bar, "Autowire");
        }

        /// <summary>Tests 2.</summary>
        [Test]
        public void Test2()
        {
            var container = new Container();
            var m = new TestFoo();
            container.AutoWire(m);
            Assert.Throws<ResolutionException>(() => container.Resolve<IBar>());
            Assert.IsNull(m.Bar); // FAILS HERE
        }         
    }
}