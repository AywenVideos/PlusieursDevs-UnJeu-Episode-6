using NetSquare.Core;

namespace NetSquare.Client
{
    /// <summary>
    /// Internal scruct to store NetSquareActions and related NetworkMessages
    /// </summary>
    public struct NetSquareActionData
    {
        /// <summary>
        /// The action to execute
        /// </summary>
        public NetSquareAction Action;
        /// <summary>
        /// The message to pass to the action
        /// </summary>
        public NetworkMessage Message;

        /// <summary>
        /// Create a new instance of NetSquareActionData
        /// </summary>
        /// <param name="action"> The action to execute</param>
        /// <param name="message"> The message to pass to the action</param>
        public NetSquareActionData(NetSquareAction action, NetworkMessage message)
        {
            Action = action;
            Message = message;
        }
    }
}