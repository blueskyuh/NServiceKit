using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using NServiceKit.Common.Web;
using NServiceKit.Logging;
using NServiceKit.Text;
using NServiceKit.WebHost.Endpoints.Support;

namespace NServiceKit.WebHost.Endpoints
{
    /// <summary>An application host HTTP listener long running base.</summary>
    public abstract class AppHostHttpListenerLongRunningBase : AppHostHttpListenerBase
    {
        private class ThreadPoolManager : IDisposable
        {
            private readonly object syncRoot = new object();
            private volatile bool isDisposing;
            private readonly AutoResetEvent autoResetEvent;
            private int avalaibleThreadCount = 0;

            /// <summary>Initializes a new instance of the NServiceKit.WebHost.Endpoints.AppHostHttpListenerLongRunningBase.ThreadPoolManager class.</summary>
            ///
            /// <param name="poolSize">Size of the pool.</param>
            public ThreadPoolManager(int poolSize)
            {
                autoResetEvent = new AutoResetEvent(false);
                avalaibleThreadCount = poolSize;
            }

            /// <summary>Returns the top-of-stack object without removing it.</summary>
            ///
            /// <param name="threadStart">The thread start to peek.</param>
            ///
            /// <returns>The current top-of-stack object.</returns>
            public Thread Peek(ThreadStart threadStart)
            {
                while (!isDisposing && avalaibleThreadCount == 0)
                    autoResetEvent.WaitOne();

                lock (syncRoot)
                {
                    if (isDisposing)
                        return null;

                    if (Interlocked.Decrement(ref avalaibleThreadCount) < 0)
                        return Peek(threadStart);
                }

                return new Thread(threadStart);
            }

            /// <summary>Frees this object.</summary>
            public void Free()
            {
                Interlocked.Increment(ref avalaibleThreadCount);
                autoResetEvent.Set();
            }

            /// <summary>
            /// Exécute les tâches définies par l'application associées à la libération ou à la redéfinition des ressources non managées.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
                lock (this)
                {
                    if (isDisposing)
                        return;

                    isDisposing = true;
                }
            }
        }

        private readonly AutoResetEvent listenForNextRequest = new AutoResetEvent(false);
        private readonly ThreadPoolManager threadPoolManager;
        private readonly ILog log = LogManager.GetLogger(typeof(HttpListenerBase));

        /// <summary>Initializes a new instance of the NServiceKit.WebHost.Endpoints.AppHostHttpListenerLongRunningBase class.</summary>
        ///
        /// <param name="poolSize">Size of the pool.</param>
        protected AppHostHttpListenerLongRunningBase(int poolSize = 500) { threadPoolManager = new ThreadPoolManager(poolSize); }

        /// <summary>Initializes a new instance of the NServiceKit.WebHost.Endpoints.AppHostHttpListenerLongRunningBase class.</summary>
        ///
        /// <param name="serviceName">           Name of the service.</param>
        /// <param name="assembliesWithServices">A variable-length parameters list containing assemblies with services.</param>
        protected AppHostHttpListenerLongRunningBase(string serviceName, params Assembly[] assembliesWithServices)
            : this(serviceName, 500, assembliesWithServices) { }

        /// <summary>Initializes a new instance of the NServiceKit.WebHost.Endpoints.AppHostHttpListenerLongRunningBase class.</summary>
        ///
        /// <param name="serviceName">           Name of the service.</param>
        /// <param name="poolSize">              Size of the pool.</param>
        /// <param name="assembliesWithServices">A variable-length parameters list containing assemblies with services.</param>
        protected AppHostHttpListenerLongRunningBase(string serviceName, int poolSize, params Assembly[] assembliesWithServices)
            : base(serviceName, assembliesWithServices) { threadPoolManager = new ThreadPoolManager(poolSize); }

        /// <summary>Initializes a new instance of the NServiceKit.WebHost.Endpoints.AppHostHttpListenerLongRunningBase class.</summary>
        ///
        /// <param name="serviceName">           Name of the service.</param>
        /// <param name="handlerPath">           Full pathname of the handler file.</param>
        /// <param name="assembliesWithServices">A variable-length parameters list containing assemblies with services.</param>
        protected AppHostHttpListenerLongRunningBase(string serviceName, string handlerPath, params Assembly[] assembliesWithServices)
            : this(serviceName, handlerPath, 500, assembliesWithServices) { }

        /// <summary>Initializes a new instance of the NServiceKit.WebHost.Endpoints.AppHostHttpListenerLongRunningBase class.</summary>
        ///
        /// <param name="serviceName">           Name of the service.</param>
        /// <param name="handlerPath">           Full pathname of the handler file.</param>
        /// <param name="poolSize">              Size of the pool.</param>
        /// <param name="assembliesWithServices">A variable-length parameters list containing assemblies with services.</param>
        protected AppHostHttpListenerLongRunningBase(string serviceName, string handlerPath, int poolSize, params Assembly[] assembliesWithServices)
            : base(serviceName, handlerPath, assembliesWithServices) { threadPoolManager = new ThreadPoolManager(poolSize); }


        private bool disposed = false;

        /// <summary>Releases the unmanaged resources used by the NServiceKit.WebHost.Endpoints.AppHostHttpListenerLongRunningBase and optionally releases the managed resources.</summary>
        ///
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposed) return;

            lock (this)
            {
                if (disposed) return;

                if (disposing)
                {
                    threadPoolManager.Dispose();
                }

                // new shared cleanup logic
                disposed = true;

                base.Dispose(disposing);
            }
        }

        /// <summary>Starts.</summary>
        ///
        /// <param name="urlBase">The URL base.</param>
        public override void Start(string urlBase)
        {
            Start(urlBase, Listen);
        }

        private bool IsListening
        {
            get { return this.IsStarted && this.Listener != null && this.Listener.IsListening; }
        }

        // Loop here to begin processing of new requests.
        private void Listen(object state)
        {
            while (IsListening)
            {
                if (Listener == null) return;

                try
                {
                    Listener.BeginGetContext(ListenerCallback, Listener);
                    listenForNextRequest.WaitOne();
                }
                catch (Exception ex)
                {
                    log.Error("Listen()", ex);
                    return;
                }
                if (Listener == null) return;
            }
        }

        // Handle the processing of a request in here.
        private void ListenerCallback(IAsyncResult asyncResult)
        {
            var listener = asyncResult.AsyncState as HttpListener;
            HttpListenerContext context;

            if (listener == null) return;
            var isListening = listener.IsListening;

            try
            {
                if (!isListening)
                {
                    log.DebugFormat("Ignoring ListenerCallback() as HttpListener is no longer listening");
                    return;
                }
                // The EndGetContext() method, as with all Begin/End asynchronous methods in the .NET Framework,
                // blocks until there is a request to be processed or some type of data is available.
                context = listener.EndGetContext(asyncResult);
            }
            catch (Exception ex)
            {
                // You will get an exception when httpListener.Stop() is called
                // because there will be a thread stopped waiting on the .EndGetContext()
                // method, and again, that is just the way most Begin/End asynchronous
                // methods of the .NET Framework work.
                string errMsg = ex + ": " + isListening;
                log.Warn(errMsg);
                return;
            }
            finally
            {
                // Once we know we have a request (or exception), we signal the other thread
                // so that it calls the BeginGetContext() (or possibly exits if we're not
                // listening any more) method to start handling the next incoming request
                // while we continue to process this request on a different thread.
                listenForNextRequest.Set();
            }

            log.InfoFormat("{0} Request : {1}", context.Request.UserHostAddress, context.Request.RawUrl);

            RaiseReceiveWebRequest(context);


            threadPoolManager.Peek(() =>
            {
                try
                {
                    ProcessRequest(context);
                }
                catch (Exception ex)
                {
                    string error = string.Format("Error this.ProcessRequest(context): [{0}]: {1}", ex.GetType().Name, ex.Message);
                    log.ErrorFormat(error);

                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("{");
                        sb.AppendLine("\"ResponseStatus\":{");
                        sb.AppendFormat(" \"ErrorCode\":{0},\n", ex.GetType().Name.EncodeJson());
                        sb.AppendFormat(" \"Message\":{0},\n", ex.Message.EncodeJson());
                        sb.AppendFormat(" \"StackTrace\":{0}\n", ex.StackTrace.EncodeJson());
                        sb.AppendLine("}");
                        sb.AppendLine("}");

                        context.Response.StatusCode = 500;
                        context.Response.ContentType = ContentType.Json;
                        byte[] sbBytes = sb.ToString().ToUtf8Bytes();
                        context.Response.OutputStream.Write(sbBytes, 0, sbBytes.Length);
                        context.Response.Close();
                    }
                    catch (Exception errorEx)
                    {
                        error = string.Format("Error this.ProcessRequest(context)(Exception while writing error to the response): [{0}]: {1}",
                                              errorEx.GetType().Name, errorEx.Message);
                        log.ErrorFormat(error);
                    }
                }

                threadPoolManager.Free();
            }).Start();
        }
    }
}
