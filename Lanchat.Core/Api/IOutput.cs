namespace Lanchat.Core.Api
{
    public interface IOutput
    {
        /// <summary>
        ///     Send data.
        /// </summary>
        /// <param name="content">Object to send.</param>
        void SendData(object content);

        /// <summary>
        ///     Send the data before marking the node as ready (Handshake, KeyInfo...).
        /// </summary>
        /// <param name="content">Object to send.</param>
        void SendPrivilegedData(object content);
    }
}