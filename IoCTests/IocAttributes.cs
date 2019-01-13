using System;
using CodeCraft.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoCTests
{
    [TestClass]
    public class IocAttributes
    {
        public interface ITest
        {
            string TestString();
        }

        public class ATest : ITest
        {
            public string TestString() => "ATest Instances";
        }

        public class BTest : ITest
        {
            public string TestString() => "BTest Instances";
        }

        public class Main : IMain
        {
#pragma warning disable 0649
            [Injection("A")]
            private ITest Tester;
#pragma warning restore 0649

            [Injection("A", InjectionType.Singleton)]
            public ITest TesterSingleton { get; private set; }

            [Injection("A", InjectionType.NewInstance)]
            public ITest TesterNonSingleton { get; private set; }
            public Main()
            {

            }
            public Main(int t)
            { }


            public override string ToString() => Tester.TestString();
        }
        public interface IMain
        {
            ITest TesterSingleton { get; }
            ITest TesterNonSingleton { get; }
        }

        [TestMethod]
        public void TestSingletonInjection()
        {
            var container = IoC.Instance;
            container.RegisterType<ITest, ATest>("A");
            container.RegisterType<IMain, Main>("TestImplementation");

            var main1 = IoC.Instance.ResolveNewInstance<IMain>("TestImplementation");
            var main2 = IoC.Instance.ResolveNewInstance<IMain>("TestImplementation");

            Assert.AreSame(main1.TesterSingleton, main2.TesterSingleton);
        }

        [TestMethod]
        public void TestNOSingletonInjection()
        {
            var container = IoC.Instance;
            container.RegisterType<ITest, ATest>("A");
            container.RegisterType<IMain, Main>("TestImplementation");

            var main1 = IoC.Instance.ResolveNewInstance<IMain>("TestImplementation");
            var main2 = IoC.Instance.ResolveNewInstance<IMain>("TestImplementation");

            Assert.AreNotSame(main1.TesterNonSingleton, main2.TesterNonSingleton);
        }

        [TestMethod]
        public void SwitchImplementation()
        {
            var container = IoC.Instance;

            container.RegisterType<ITest, ATest>("A");
            container.RegisterType<IMain, Main>("TestImplementation");
            var main = IoC.Instance.ResolveNewInstance<IMain>("TestImplementation");
            Assert.IsNotNull(main);
            Assert.AreEqual("ATest Instances", main.ToString());

            container.RegisterType<ITest, BTest>("A");
            main = IoC.Instance.ResolveNewInstance<IMain>("TestImplementation");
            Assert.IsNotNull(main);
            Assert.AreEqual("BTest Instances", main.ToString());

        }
    }
}
