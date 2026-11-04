using System;
using System.Security.Cryptography;
using System.Text;

namespace compute.geometry
{
    // While the usage log is on, rhino.compute sends each /mcp call through the compute.geometry child that holds the
    // session, as POST /mcp with these headers, instead of straight to the RhinoAI plug-in's listener in that child, so
    // the work is measured and recorded like any other billable request. Also compiled into rhino.compute.
    static class McpForwarding
    {
        public const string PATH = "/mcp";
        // The listener to pass the call to: the session document's, or the child's startup document's.
        public const string LISTENER_HEADER = "Rhino-Compute-Mcp-Listener";
        // Opens a session first: a document with a listener of its own, whose port comes back in LISTENER_HEADER.
        public const string OPEN_HEADER = "Rhino-Compute-Mcp-Open";
        // Closes the session whose listener is on this port; there's no call to pass on.
        public const string CLOSE_HEADER = "Rhino-Compute-Mcp-Close";
        // The session the call belongs to, as SessionKey names it.
        public const string SESSION_HEADER = "Rhino-Compute-Mcp-Session";
        // The plug-in's control tools for rhino.compute, which agents don't get.
        public const string ROUTER_TOOL_PREFIX = "_router_";
        // Each child's listeners use a block of this many ports from the one it's given.
        public const int PORTS_PER_CHILD = 100;

        // Names a session in usage records without its id, which lets whoever holds it use the session.
        public static string SessionKey(string sessionId) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sessionId)), 0, 8).ToLowerInvariant();
    }
}
