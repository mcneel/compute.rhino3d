using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hops
{
    class SolveData
    {
        readonly Resthopper.IO.Schema _input;

        public SolveData(Resthopper.IO.Schema input)
        {
            _input = input;
        }

        public bool HasSolveData
        {
            get
            {
                return Output != null;
            }
        }

        public Resthopper.IO.Schema Output { get; private set; }

        Task _workingTask;
        public Task Solve(RemoteDefinition remoteDefinition, bool useMemoryCache, Action completedCallback)
        {
            _workingTask = Task.Run(() =>
            {
                Output = remoteDefinition.Solve(_input, useMemoryCache);
                completedCallback();
            });
            return _workingTask;
        }
    }

    class SolveDataList
    {
        readonly int _solveSerialNumber;
        readonly HopsComponent _parentComponent;
        readonly bool _useMemoryCacheWhenSolving;
        readonly RemoteDefinition _remoteDefinition;
        bool _synchronous = false;

        List<SolveData> _data = new List<SolveData>();
        bool _solveStarted = false;

        public SolveDataList(int serialNumber, HopsComponent component, RemoteDefinition remoteDefinition, bool useMemoryCache)
        {
            _solveSerialNumber = serialNumber;
            _parentComponent = component;
            _useMemoryCacheWhenSolving = useMemoryCache;
            _remoteDefinition = remoteDefinition;
        }

        public void Add(Resthopper.IO.Schema inputSchema)
        {
            _data.Add(new SolveData(inputSchema));
        }

        public void StartSolving(bool waitUntilComplete)
        {
            if (_solveStarted || Canceled)
                return;
            _solveStarted = true;
            _synchronous = waitUntilComplete;

            SolveIterationQueue.Add(this);
        }

        public int Count => _data.Count;
        public bool Canceled { get; set; } = false;
        public bool Synchronous => _synchronous;

        public Task Solve(int index)
        {
            if (Canceled)
                return null;

            return _data[index].Solve(_remoteDefinition, _useMemoryCacheWhenSolving, OnItemSolved);
        }

        int _solvedCount = 0;
        void OnItemSolved()
        {
            System.Threading.Interlocked.Increment(ref _solvedCount);
            if (_solvedCount == _data.Count && !_synchronous)
            {
                SolveIterationQueue.AddIdleCallback(_parentComponent.InstanceGuid, () => _parentComponent.OnWorkingListComplete());
            }
        }
        private void RhinoApp_Idle(object sender, EventArgs e)
        {
            if (_solveSerialNumber != _parentComponent.SolveSerialNumber || Canceled)
            {
                Rhino.RhinoApp.Idle -= RhinoApp_Idle;
                return;
            }

            if (SolvedFor(_parentComponent.SolveSerialNumber))
            {
                _parentComponent.ExpireSolution(true);
                Rhino.RhinoApp.Idle -= RhinoApp_Idle;
            }
        }


        public Resthopper.IO.Schema SolvedSchema(int index)
        {
            return _data[index].Output;
        }

        public bool SolvedFor(int serialNumber)
        {
            if (!_solveStarted || _solveSerialNumber != serialNumber)
                return false;
            foreach(var item in _data)
            {
                if (!item.HasSolveData)
                    return false;
            }
            return true;
        }
    }
}
