using System;
using System.Reflection;
using Serilog;

namespace compute.geometry
{
    // Rhino's usage meter, Rhino.Runtime.UsageMeter (internal to RhinoCommon), found at startup so the same build runs
    // on a Rhino without it. With Rhino's meter on, each billable request is a unit of work in Rhino's ledger, which
    // divides Rhino's CPU between requests and writes a "work" record for each, named by the request id; the request's
    // usage record takes its CPU from there. Without it, or with the meter off, CpuLedger measures requests alone.
    static class RhinoMeter
    {
        delegate long BeginWorkCall(string kind, string id);
        delegate void SetWaitingCall(long work, bool waiting);
        delegate bool EndWorkCall(long work, out double cpuSeconds, out double sharedCpuSeconds);

        static BeginWorkCall beginWork;
        static SetWaitingCall setWaiting;
        static EndWorkCall endWork;

        public static void Start()
        {
            if (beginWork != null)
                return;
            var type = typeof(Rhino.RhinoApp).Assembly.GetType("Rhino.Runtime.UsageMeter");
            if (type == null)
            {
                Log.Information("Usage log: this Rhino has no usage meter, so compute.geometry measures each request's CPU itself");
                return;
            }
            const BindingFlags FLAGS = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                setWaiting = (SetWaitingCall)Delegate.CreateDelegate(typeof(SetWaitingCall), type.GetMethod("SetWaiting", FLAGS));
                endWork = (EndWorkCall)Delegate.CreateDelegate(typeof(EndWorkCall), type.GetMethod("EndWork", FLAGS));
                beginWork = (BeginWorkCall)Delegate.CreateDelegate(typeof(BeginWorkCall), type.GetMethod("BeginWork", FLAGS));
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Usage log: Rhino's usage meter has calls compute.geometry doesn't know, so compute.geometry measures each request's CPU itself");
            }
        }

        // A billable request as a unit of work; 0 when Rhino's meter is off or missing.
        public static long BeginWork(string requestId) => beginWork?.Invoke("request", requestId) ?? 0;

        public static void SetWaiting(long work, bool waiting)
        {
            if (work != 0)
                setWaiting(work, waiting);
        }

        // The request's CPU, and the part that was an even share with other requests running; false for 0.
        public static bool EndWork(long work, out double cpuSeconds, out double sharedCpuSeconds)
        {
            cpuSeconds = 0;
            sharedCpuSeconds = 0;
            return work != 0 && endWork(work, out cpuSeconds, out sharedCpuSeconds);
        }
    }
}
