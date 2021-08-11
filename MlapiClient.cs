using Dissonance.Networking;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using MLAPI.Transports;
using System;

public class MlapiClient : BaseClient<MlapiServer, MlapiClient, MlapiConn>
{
    private MlapiCommsNetwork _network;

    public MlapiClient(MlapiCommsNetwork network) : base(network)
    {
        _network = network;
    }

    public override void Connect()
    {
        // Register receiving packets on the client from the server
        CustomMessagingManager.RegisterNamedMessageHandler("DissonanceToClient", (senderClientId, stream) =>
        {
            Int32 length = stream.Length > Int32.MaxValue ? Int32.MaxValue : Convert.ToInt32(stream.Length);
            Byte[] buffer = new Byte[length];
            stream.Read(buffer, 0, length);

            base.NetworkReceivedPacket(new ArraySegment<byte>(buffer));
        });
        Connected();
    }

    protected override void ReadMessages()
    {

    }

    protected override void SendReliable(ArraySegment<byte> packet)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            // As we are the host in this scenario we should send the packet directly to the server rather than over the network and avoid loop-back issues
            _network.server.NetworkReceivedPacket(new MlapiConn(), packet);
        }
        else
        {
            using (PooledNetworkBuffer writer = NetworkBufferPool.GetBuffer())
            {
                for (var i = 0; i < packet.Count; i++)
                {
                    writer.WriteByte(packet.Array[i + packet.Offset]);
                }
                CustomMessagingManager.SendNamedMessage("DissonanceToServer", NetworkManager.Singleton.ServerClientId, writer, NetworkChannel.AnimationUpdate);
            }
        }
    }

    protected override void SendUnreliable(ArraySegment<byte> packet)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            // As we are the host in this scenario we should send the packet directly to the server rather than over the network and avoid loopback issues
            _network.server.NetworkReceivedPacket(new MlapiConn(), packet);
        }
        else
        {

            using (PooledNetworkBuffer writer = NetworkBufferPool.GetBuffer())
            {
                for (var i = 0; i < packet.Count; i++)
                {
                    writer.WriteByte(packet.Array[i + packet.Offset]);
                }
                CustomMessagingManager.SendNamedMessage("DissonanceToServer", NetworkManager.Singleton.ServerClientId, writer, NetworkChannel.TimeSync);
            }
        }

    }
}