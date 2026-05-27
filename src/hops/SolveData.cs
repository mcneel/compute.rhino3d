using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hops
{
    class SolveData
    {
        readonly Resthopper.IO.Schema input;

        public SolveData(Resthopper.IO.Schema input)
        {
            this.input = input;
        }

        public bool HasSolveData
        {
            get
            {
                return Output != null;
            }
        }

        public Resthopper.IO.Schema Output { get; private set; }

        Task workingTask;
        public Task Solve(RemoteDefinition remoteDefinition, bool useMemoryCache, Action completedCallback)
        {
            workingTask = Task.Run(() =>
            {
                Output = remoteDefinition.Solve(input, useMemoryCache);
                completedCallback();
            });
            return workingTask;
        }
    }

    class SolveDataList
    {
        readonly int solveSerialNumber;
        readonly HopsComponent parentComponent;
        readonly bool useMemoryCacheWhenSolving;
        readonly RemoteDefinition remoteDefinition;
        bool synchronous = false;

        List<SolveData> data = new List<SolveData>();
        bool solveStarted = false;

        public SolveDataList(int serialNumber, HopsComponent component, RemoteDefinition remoteDefinition, bool useMemoryCache)
        {
            solveSerialNumber = serialNumber;
            parentComponent = component;
            useMemoryCacheWhenSolving = useMemoryCache;
            this.remoteDefinition = remoteDefinition;
        }

        public void Add(Resthopper.IO.Schema inputSchema)
        {
            data.Add(new SolveData(inputSchema));
        }

        public void StartSolving(bool waitUntilComplete)
        {
            if (solveStarted || Canceled)
                return;
            solveStarted = true;
            synchronous = waitUntilComplete;

            SolveIterationQueue.Add(this);
        }

        public int Count => data.Count;
        public bool Canceled { get; set; } = false;
        public bool Synchronous => synchronous;

        public Task Solve(int index)
        {
            if (Canceled)
                return null;

            return data[index].Solve(remoteDefinition, useMemoryCacheWhenSolving, OnItemSolved);
        }

        int solvedCount = 0;
        void OnItemSolved()
        {
            System.Threading.Interlocked.Increment(ref solvedCount);
            if (solvedCount == data.Count && !synchronous)
            {
                SolveIterationQueue.AddIdleCallback(parentComponent.InstanceGuid, () => parentComponent.OnWorkingListComplete());
            }
        }
        private void RhinoApp_Idle(object sender, EventArgs e)
        {
            if (solveSerialNumber != parentComponent.SolveSerialNumber || Canceled)
            {
                Rhino.RhinoApp.Idle -= RhinoApp_Idle;
                return;
            }

            if (SolvedFor(parentComponent.SolveSerialNumber))
            {
                parentComponent.ExpireSolution(true);
                Rhino.RhinoApp.Idle -= RhinoApp_Idle;
            }
        }


        public Resthopper.IO.Schema SolvedSchema(int index)
        {
            return data[index].Output;
        }

        public bool SolvedFor(int serialNumber)
        {
            if (!solveStarted || solveSerialNumber != serialNumber)
                return false;
            foreach(var item in data)
            {
                if (!item.HasSolveData)
                    return false;
            }
            return true;
        }
    }
}
