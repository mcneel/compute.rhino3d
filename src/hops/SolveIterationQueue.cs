using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Hops
{
    static class SolveIterationQueue
    {
        static Task solveTask;
        static ConcurrentStack<SolveDataList> stack = new ConcurrentStack<SolveDataList>();
        static int maxConcurrentRequests;
        static bool idleSet = false;
        static ConcurrentDictionary<Guid, Action> componentCallbacks = new ConcurrentDictionary<Guid, Action>();

        static public void Add(SolveDataList datalist)
        {
            if (!idleSet && !datalist.Synchronous)
            {
                idleSet = true;
                Rhino.RhinoApp.Idle += RhinoApp_Idle;
            }
            maxConcurrentRequests = HopsAppSettings.MaxConcurrentRequests;

            if (datalist.Synchronous)
            {
                var stack = new ConcurrentStack<SolveDataList>();
                stack.Push(datalist);
                var task = Task.Run(() => ProcessStack(stack));
                task.Wait();
                return;
            }

            stack.Push(datalist);
            if (solveTask == null || solveTask.IsCompleted)
                solveTask = Task.Run(() => ProcessStack(stack));
        }

        private static void RhinoApp_Idle(object sender, System.EventArgs e)
        {
            if (stack.Count > 0 && (solveTask == null || solveTask.IsCompleted))
                solveTask = Task.Run(() => ProcessStack(stack));

            foreach (var callback in componentCallbacks.Values)
                callback();
            componentCallbacks.Clear();
        }

        public static void AddIdleCallback(Guid componentId, System.Action callback)
        {
            componentCallbacks[componentId] = callback;
        }

        static void ProcessStack(ConcurrentStack<SolveDataList> stack)
        {
            List<Task> childTasks = new List<Task>();

            while (stack.TryPop(out SolveDataList datalist))
            {
                for (int i = 0; i < datalist.Count; i++)
                {
                    Task t = datalist.Solve(i);
                    if (t != null)
                    {
                        childTasks.Add(t);
                        if (childTasks.Count >= maxConcurrentRequests)
                        {
                            var taskArray = childTasks.ToArray();
                            Task.WaitAny(childTasks.ToArray());
                            childTasks.Clear();
                            foreach (var task in taskArray)
                            {
                                if (!task.IsCompleted)
                                    childTasks.Add(task);
                            }
                        }
                    }
                }
            }

            var remainingTasks = childTasks.ToArray();
            if (remainingTasks.Length > 0)
            {
                Task.WaitAll(remainingTasks);
            }
        }

    }
}
