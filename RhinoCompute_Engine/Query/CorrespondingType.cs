using System;
using System.Collections.Generic;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        private static Dictionary<Type, Type> m_correspondingTypes = new Dictionary<Type, Type>();

        public static Type CorrespondingType(this Type t)
        {
            if (m_correspondingTypes != null)
                return m_correspondingTypes[t];

            return null;
        }
    }
}
