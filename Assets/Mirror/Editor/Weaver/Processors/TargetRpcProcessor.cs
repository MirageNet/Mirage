using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirror.Weaver
{
    /// <summary>
    /// Processes [TargetRpc] methods in NetworkBehaviour
    /// </summary>
    public static class TargetRpcProcessor
    {
        /// <summary>
        /// Replaces the user code with a stub.
        /// Moves the original code to a new method
        /// </summary>
        /// <param name="td">The class containing the method </param>
        /// <param name="md">The method to be stubbed </param>
        /// <param name="ServerRpcAttr">The attribute that made this an RPC</param>
        /// <returns>The method containing the original code</returns>
        /// <remarks>
        /// Generates code like this:
        /// <code>
        /// public void TargetTest(NetworkConnection conn, int param)
        /// {
        ///     NetworkWriter writer = new NetworkWriter();
        ///     writer.WritePackedUInt32((uint)param);
        ///     base.SendTargetRPCInternal(conn, typeof(class), "TargetTest", val);
        /// }
        /// 
        /// public void UserCode_TargetTest(NetworkConnection conn, int param)
        /// {
        ///     // whatever the user did before
        /// }
        /// </code>
        /// or if no connection is specified
        ///
        /// <code>
        /// public void TargetTest (int param)
        /// {
        ///     NetworkWriter writer = new NetworkWriter();
        ///     writer.WritePackedUInt32((uint) param);
        ///     base.SendTargetRPCInternal(null, typeof(class), "TargetTest", val);
        /// }
        /// 
        /// public void UserCode_TargetTest(int param)
        /// {
        ///     // whatever the user did before
        /// }
        /// </code>
        /// </remarks>
    }
}
