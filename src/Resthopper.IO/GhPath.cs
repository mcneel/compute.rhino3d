using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resthopper.IO
{
    public class GhPath
    {
        int[] _path;

        public GhPath(int[] path)
        {
            _path = path;
        }

        public GhPath(GhPath pathObj, int i)
        {
            int[] path = pathObj._path;
            _path = new int[path.Length + 1];

            for (int j = 0; j < path.Length; j++)
            {
                _path[j] = path[j];
            }
            _path[path.Length] = i;
        }

        public bool LastIndexSame(int i)
        {
            return _path.Last() == i;
        }
    }
}
