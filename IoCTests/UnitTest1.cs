using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeCraft.DependencyInjection;

namespace IoCTests
{
    [TestClass]
    public class UnitTest1
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
            ITest Tester;
            public Main(ITest tester)
            {
                Tester = tester;
            }
            public override string ToString() => Tester.TestString();
        }
        public interface IMain
        { }
        [TestMethod]
        public void TestMethod1()
        {
            IoC container = IoC.Instance;

            container.RegisterType<ITest, ATest>("A");
            container.RegisterType<ITest, BTest>("B");
            container.RegisterType<IMain, Main>("A");
            container.RegisterType<IMain, Main>("B");
            var Aimpl = container.Resolve<ITest>("A");
            Assert.IsNotNull(Aimpl);
            Assert.AreEqual("ATest Instances", Aimpl.TestString());
            var Bimpl = container.Resolve<ITest>("B");
            Assert.IsNotNull(Bimpl);
            Assert.AreEqual("BTest Instances", Bimpl.TestString());

            IMain aMain = container.Resolve<IMain>("A");
            Assert.IsNotNull(aMain);
            Assert.AreEqual("ATest Instances", aMain.ToString());
            IMain bMain = container.Resolve<IMain>("B");
            Assert.IsNotNull(bMain);
            Assert.AreEqual("BTest Instances", bMain.ToString());
        }


        public  class Unit<T> : IUnit<T>
        {
            public override string ToString() => typeof(T).ToString();
        }
        public class DUnit : Unit<ATest>
        {
            public override string ToString() => "Dunit";
        }

        public class IntUnit : Unit<BTest>
        {
        }

        public interface IUnit<T>
        {
            string ToString();
        }

        [TestMethod]
        public void TestMethod()
        {
            IoC container = IoC.Instance;

            container.RegisterType<IUnit<int>, Unit<int>>("B");
            container.RegisterType<IUnit<ATest>, DUnit>("A");
            

            IUnit<ATest> dUnit = container.Resolve<IUnit<ATest>>("A");
            Assert.IsNotNull(dUnit);

            Assert.AreEqual("Dunit",  dUnit.ToString());
        }
    }
}
