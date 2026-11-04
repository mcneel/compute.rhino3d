using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;

namespace compute.geometry
{
    // While the usage log is on: marks each HTTP call this process makes to a server on this machine (a Hops component
    // or a script calling back into Rhino.Compute) with the metadata of the request it was made for, in
    // LocalConnections.CALLER_HEADER, so the nested call is counted for that request even when this process is serving
    // several. The request is known from where the call was made, including threads and tasks the request started.
    // Calls to other machines are left alone, so they carry nothing of the caller's.
    static class NestedCalls
    {
        const string HTTP_LISTENER = "HttpHandlerDiagnosticListener";
        const string REQUEST_OUT = "System.Net.Http.HttpRequestOut";
        const string REQUEST_OUT_START = REQUEST_OUT + ".Start";

        static int started;
        static readonly ConcurrentDictionary<Type, PropertyInfo> requestProperties = new ConcurrentDictionary<Type, PropertyInfo>();

        public static void Start()
        {
            if (Interlocked.Exchange(ref started, 1) == 1)
                return;
            DiagnosticListener.AllListeners.Subscribe(new Observer<DiagnosticListener>(listener =>
            {
                if (listener.Name == HTTP_LISTENER)
                    listener.Subscribe(new Observer<KeyValuePair<string, object>>(OnEvent), (name, _, _) => name.StartsWith(REQUEST_OUT, StringComparison.Ordinal));
            }));
        }

        static void OnEvent(KeyValuePair<string, object> e)
        {
            if (e.Key != REQUEST_OUT_START || e.Value == null)
                return;
            var property = requestProperties.GetOrAdd(e.Value.GetType(), type => type.GetProperty("Request"));
            if (property?.GetValue(e.Value) is not HttpRequestMessage request || !IsThisMachine(request.RequestUri))
                return;
            // Whatever the caller set is replaced, or dropped when the call wasn't made for a billable request.
            request.Headers.Remove(LocalConnections.CALLER_HEADER);
            var entry = CpuLedger.Current;
            if (entry?.Billable == true)
                request.Headers.TryAddWithoutValidation(LocalConnections.CALLER_HEADER, entry.Metadata is JsonElement metadata ? JsonSerializer.Serialize(metadata) : "{}");
        }

        static bool IsThisMachine(Uri uri) =>
            uri != null && uri.IsAbsoluteUri && (uri.IsLoopback || string.Equals(uri.Host, Environment.MachineName, StringComparison.OrdinalIgnoreCase));

        sealed class Observer<T> : IObserver<T>
        {
            readonly Action<T> next;

            public Observer(Action<T> next)
            {
                this.next = next;
            }

            public void OnNext(T value) => next(value);
            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }
    }
}
