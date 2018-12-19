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
            [FieldInjection("A")]
            private ITest Tester;
            public Main()
            {
              
            }

            public void Toto([FieldInjection] ITest tester)
            {

            }
        
            public override string ToString() => Tester.TestString();
        }
        public interface IMain
        { }



        [TestMethod]
        public void TestMethod1()
        {
            var container = IoC.Instance;

            container.RegisterType<ITest, ATest>("A");
            container.RegisterType<ITest, BTest>("B");
            container.RegisterType<IMain, Main>("A");
            container.RegisterType<IMain, Main>("B");
            var main = IoC.Instance.ResolveNewInstance<IMain>("A");

            Assert.IsNotNull(main);  
            Assert.AreEqual("ATest Instances", main.ToString());

        }
    }
}
