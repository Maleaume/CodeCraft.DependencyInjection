using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeCraft.DependencyInjection;
using System;

namespace IoCTests
{
    [TestClass]
    public class SimpleIoCRetrieves
    {
        public interface ITest
        {
            int Counter { get; set; }
            string TestString();
        }

        public class ATest : ITest
        {
            public int Counter { get; set; } = 0;
            public string TestString() => "ATest Instances";
        }

        public class BTest : ITest
        {
            public int Counter { get; set; } = 0;
            public string TestString() => "BTest Instances";
        }

        public class Main : IMain
        {
            [Injection("A", InjectionType.NewInstance)]
            private readonly ITest Tester;
            public Main()
            {
           
            }
            public override string ToString() => Tester.TestString();
        }
        public interface IMain
        { }
        [TestMethod]
        public void ResolveDifferentInstance()
        {
            var container = IoC.Instance;
            container.RegisterType<ITest, ATest>("A"); 
            var A1 = container.ResolveNewInstance<ITest>("A");
            A1.Counter++;
            var A2 = container.ResolveNewInstance<ITest>("A");
            Assert.AreNotEqual(A1.Counter, A2.Counter);
            Assert.AreNotSame(A2, A1);
        }

        [TestMethod]
        public void ResolveSameInstance()
        {
            var container = IoC.Instance;
            container.RegisterType<ITest, ATest>("A");  
            var A1 = container.Resolve<ITest>("A");
            A1.Counter++;
            var A2 = container.Resolve<ITest>("A");
            Assert.AreEqual(A1.Counter, A2.Counter);
            Assert.AreSame(A2, A1);
        }

        [TestMethod]
        [TestCategory("Register Instance")]
        public void SimpleCase_CreateRegisterRetrieve()
        {
            var A = new ATest { Counter = 2 };
            IoC.Instance.RegisterInstance<ITest>(A, "A");
            A.Counter++;

            var A1 = IoC.Instance.Resolve<ITest>("A");
            Assert.AreSame(A1, A); 
        }

        [TestMethod]
        [TestCategory("Register Instance")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleCase_TryRegisterNullInstance()
        {
            IoC.Instance.RegisterInstance<ITest>(null, "A");
        }



        [TestMethod]
        public void TestMethod1()
        {
            var container = IoC.Instance;

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

            var aMain = container.Resolve<IMain>("A");
            Assert.IsNotNull(aMain);
            Assert.AreEqual("ATest Instances", aMain.ToString());
            var bMain = container.Resolve<IMain>("B");
            Assert.IsNotNull(bMain);
            Assert.AreEqual("ATest Instances", bMain.ToString());
        }


        [TestMethod]
        public void ResolveImplementation()
        {
            var container = IoC.Instance;
            container.RegisterType<ITest, ATest>("A");
            container.RegisterType<ITest, BTest>("B");

            var mainImpl = IoC.Instance.Resolve<Main>();
            Assert.AreEqual("ATest Instances", mainImpl.ToString());
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
            var container = IoC.Instance;

            container.RegisterType<IUnit<int>, Unit<int>>("B");
            container.RegisterType<IUnit<ATest>, DUnit>("A");


            var dUnit = container.Resolve<IUnit<ATest>>("A");
            Assert.IsNotNull(dUnit);

            Assert.AreEqual("Dunit",  dUnit.ToString());
        }
    }
}
