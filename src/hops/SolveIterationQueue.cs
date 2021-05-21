using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Hops
{
    static class SolveIterationQueue
    {
        static Task _solveTask;
        static ConcurrentStack<SolveDataList> _stack = new ConcurrentStack<SolveDataList>();
        static int _maxConcurrentRequests;
        static bool _idleSet = false;
        static ConcurrentDictionary<Guid, Action> _componentCallbacks = new ConcurrentDictionary<Guid, Action>();

        static public void Add(SolveDataList datalist)
        {
            if (!_idleSet && !datalist.Synchronous)
            {
                _idleSet = true;
                Rhino.RhinoApp.Idle += RhinoApp_Idle;
            }
            _maxConcurrentRequests = HopsAppSettings.MaxConcurrentRequests;

            if (datalist.Synchronous)
            {
                var stack = new ConcurrentStack<SolveDataList>();
                stack.Push(datalist);
                var task = Task.Run(() => ProcessStack(stack));
                task.Wait();
                return;
            }

            _stack.Push(datalist);
            if (_solveTask == null || _solveTask.IsCompleted)
                _solveTask = Task.Run(() => ProcessStack(_stack));
        }

        private static void RhinoApp_Idle(object sender, System.EventArgs e)
        {
            if (_stack.Count > 0 && (_solveTask == null || _solveTask.IsCompleted))
                _solveTask = Task.Run(() => ProcessStack(_stack));

            foreach (var callback in _componentCallbacks.Values)
                callback();
            _componentCallbacks.Clear();
        }

        public static void AddIdleCallback(Guid componentId, System.Action callback)
        {
            _componentCallbacks[componentId] = callback;
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
                        if (childTasks.Count >= _maxConcurrentRequests)
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
