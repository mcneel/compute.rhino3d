using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace compute.geometry
{
    // Divides this process's CPU (including processes it starts) between requests. Each time a request
    // starts, ends, becomes billable, or starts or stops waiting for a lock, the CPU used since the last
    // such moment goes to the one billable request running, is shared evenly when several are running,
    // or is overhead: the non-billable requests running, else idle.
    static class CpuLedger
    {
        public sealed class Entry
        {
            internal bool Waiting;
            internal bool Finished;
            internal double OverheadCpuSeconds;

            public bool Billable { get; internal set; }
            // All CPU billed to this request, including SharedCpuSeconds.
            public double CpuSeconds { get; internal set; }
            // The part of CpuSeconds that is an even share of CPU used while other billable requests were running.
            public double SharedCpuSeconds { get; internal set; }
            public double WaitSeconds { get; internal set; }

            internal bool Running => Billable && !Waiting && !Finished;
        }

        public sealed class OverheadTotal
        {
            public int Count;
            public double CpuSeconds;
        }

        static readonly object ledgerLock = new object();
        static readonly List<Entry> inFlight = new List<Entry>();
        static readonly AsyncLocal<Entry> current = new AsyncLocal<Entry>();
        static TimeSpan lastCpu;
        static double idleCpuSeconds;
        static Dictionary<string, OverheadTotal> overhead = new Dictionary<string, OverheadTotal>();

        public static void Start()
        {
            lock (ledgerLock)
                lastCpu = ProcessTreeCpu.Total();
        }

        public static Entry Begin()
        {
            var entry = new Entry();
            current.Value = entry;
            lock (ledgerLock)
            {
                Advance();
                inFlight.Add(entry);
            }
            return entry;
        }

        public static void End(Entry entry, string overheadLabel)
        {
            lock (ledgerLock)
            {
                Advance();
                inFlight.Remove(entry);
                if (entry.Billable)
                    return;
                if (!overhead.TryGetValue(overheadLabel, out var total))
                    overhead[overheadLabel] = total = new OverheadTotal();
                total.Count++;
                total.CpuSeconds += entry.OverheadCpuSeconds;
            }
        }

        // Marks the current request billable; it stops counting as running when the scope is disposed.
        public static BillableScope EnterBillable()
        {
            var entry = current.Value;
            if (entry != null)
            {
                lock (ledgerLock)
                {
                    Advance();
                    entry.Billable = true;
                }
            }
            return new BillableScope(entry);
        }

        public readonly struct BillableScope : IDisposable
        {
            readonly Entry entry;

            public BillableScope(Entry entry)
            {
                this.entry = entry;
            }

            public void Dispose()
            {
                if (entry == null)
                    return;
                lock (ledgerLock)
                {
                    Advance();
                    entry.Finished = true;
                }
            }
        }

        // Takes lockObject; the current request doesn't count as running while it waits for it.
        public static LockScope Lock(object lockObject)
        {
            if (Monitor.TryEnter(lockObject))
                return new LockScope(lockObject);

            var entry = current.Value;
            if (entry == null)
            {
                Monitor.Enter(lockObject);
                return new LockScope(lockObject);
            }
            long start = Stopwatch.GetTimestamp();
            SetWaiting(entry, true);
            Monitor.Enter(lockObject);
            SetWaiting(entry, false);
            lock (ledgerLock)
                entry.WaitSeconds += Stopwatch.GetElapsedTime(start).TotalSeconds;
            return new LockScope(lockObject);
        }

        public readonly struct LockScope : IDisposable
        {
            readonly object lockObject;

            public LockScope(object lockObject)
            {
                this.lockObject = lockObject;
            }

            public void Dispose() => Monitor.Exit(lockObject);
        }

        // Idle CPU and non-billable requests by label since the last call.
        public static (double IdleCpuSeconds, Dictionary<string, OverheadTotal> Requests) TakeOverhead()
        {
            lock (ledgerLock)
            {
                Advance();
                var result = (idleCpuSeconds, overhead);
                idleCpuSeconds = 0;
                overhead = new Dictionary<string, OverheadTotal>();
                return result;
            }
        }

        static void SetWaiting(Entry entry, bool waiting)
        {
            lock (ledgerLock)
            {
                Advance();
                entry.Waiting = waiting;
            }
        }

        // Called under ledgerLock.
        static void Advance()
        {
            var now = ProcessTreeCpu.Total();
            double delta = (now - lastCpu).TotalSeconds;
            lastCpu = now;
            if (delta <= 0)
                return;

            int running = 0;
            Entry only = null;
            foreach (var entry in inFlight)
            {
                if (entry.Running)
                {
                    running++;
                    only = entry;
                }
            }
            if (running == 1)
            {
                only.CpuSeconds += delta;
                return;
            }
            if (running > 1)
            {
                foreach (var entry in inFlight)
                {
                    if (entry.Running)
                    {
                        entry.CpuSeconds += delta / running;
                        entry.SharedCpuSeconds += delta / running;
                    }
                }
                return;
            }

            int others = 0;
            foreach (var entry in inFlight)
            {
                if (!entry.Billable && !entry.Waiting)
                    others++;
            }
            if (others == 0)
            {
                idleCpuSeconds += delta;
                return;
            }
            foreach (var entry in inFlight)
            {
                if (!entry.Billable && !entry.Waiting)
                    entry.OverheadCpuSeconds += delta / others;
            }
        }
    }
}
