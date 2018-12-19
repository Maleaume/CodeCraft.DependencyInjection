using System;
using System.Diagnostics;
using System.IO;
using CodeCraft.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoCTests
{

    public interface IMyDisposable : IDisposable
    { 
    }
    public abstract class MyDisplosable : IMyDisposable
    {
    
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private StreamReader Stream = new StreamReader(new MemoryStream());
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Stream.Dispose();
                    Debug.WriteLine("Dispose");
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DisposableClass() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    } 

    public class MyDisposableA : MyDisplosable 
    {


    }
    public class MyDisposableB : MyDisplosable 
    {


    }
    [TestClass]
    public class DisposableClass
    { 
        
        [TestMethod]
        public void DisposeTest()
        {
            IoC.Instance.RegisterType<IMyDisposable, MyDisposableA>();
            var t = IoC.Instance.Resolve<IMyDisposable>();
            IoC.Instance.RegisterType<IMyDisposable, MyDisposableB>();
            t = IoC.Instance.Resolve<IMyDisposable>();
            t.Dispose();
        }


    }
}
