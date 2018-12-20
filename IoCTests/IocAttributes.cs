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
            [FieldInjection("A")]
            private ITest Tester;
#pragma warning restore 0649
            public Main()
            {

            }
            public Main(int t)
            { }


            public override string ToString() => Tester.TestString();
        }
        public interface IMain
        { }



        [TestMethod]
        public void SwithImplementation()
        {
            var container = IoC.Instance;

            container.RegisterType<ITest, ATest>("A");
          //  container.RegisterType<ITest, BTest>("B");
            container.RegisterType<IMain, Main>("A");
           // container.RegisterType<IMain, Main>("B");
            var main = IoC.Instance.ResolveNewInstance<IMain>("A");
            Assert.IsNotNull(main);
            Assert.AreEqual("ATest Instances", main.ToString());
            container.RegisterType<ITest, BTest>("A");
            main = IoC.Instance.ResolveNewInstance<IMain>("A");
            Assert.IsNotNull(main);
            Assert.AreEqual("BTest Instances", main.ToString());

        }
    }
}
