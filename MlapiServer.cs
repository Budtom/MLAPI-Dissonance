using Dissonance.Networking;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using System;
using MLAPI.Transports;

public class MlapiServer : BaseServer<MlapiServer, MlapiClient, MlapiConn>
{
    private MlapiCommsNetwork _network;

    public MlapiServer(MlapiCommsNetwork network)
    {
        _network = network;
    }
    public override void Connect()
    {
        // Register receiving packets on the server from the client
        CustomMessagingManager.RegisterNamedMessageHandler("DissonanceToServer", (senderClientId, stream) =>
        {
            var client = new MlapiConn();
            client.clientId = senderClientId;

            Int32 length = stream.Length > Int32.MaxValue ? Int32.MaxValue : Convert.ToInt32(stream.Length);
            Byte[] buffer = new Byte[length];
            stream.Read(buffer, 0, length);

            base.NetworkReceivedPacket(client, new ArraySegment<byte>(buffer));
        });
        base.Connect();
    }

    public override void Disconnect()
    {
        base.Disconnect();
    }

    protected override void ReadMessages()
    {

    }

    protected override void SendReliable(MlapiConn destination, ArraySegment<byte> packet)
    {
        using (PooledNetworkBuffer writer = NetworkBufferPool.GetBuffer())
        {
            var count = packet.Count;
            var offset = packet.Offset;
            for (var i = 0; i < count; i++)
            {
                writer.WriteByte(packet.Array[i + offset]);
            }
            if (NetworkManager.Singleton.LocalClientId == destination.clientId)
            {
                // As we are the host in this scenario we should send the packet directly to the client rather than over the network and avoid loopback issues
                _network.client.NetworkReceivedPacket(packet);
            }
            else
            {
                CustomMessagingManager.SendNamedMessage("DissonanceToClient", destination.clientId, writer, NetworkChannel.AnimationUpdate);
            }
        }
    }

    protected override void SendUnreliable(MlapiConn destination, ArraySegment<byte> packet)
    {
        using (PooledNetworkBuffer writer = NetworkBufferPool.GetBuffer())
        {
                var count = packet.Count;
                var offset = packet.Offset;
                for (var i = 0; i < count; i++)
                {
                    writer.WriteByte(packet.Array[i + offset]);
                }
                if (NetworkManager.Singleton.LocalClientId == destination.clientId)
                {
                    // As we are the host in this scenario we should send the packet directly to the client rather than over the network and avoid loop-back issues
                    _network.client.NetworkReceivedPacket(packet);
                }
                else
                {
                    CustomMessagingManager.SendNamedMessage("DissonanceToClient", destination.clientId, writer, NetworkChannel.TimeSync);
                }
        }

    }

    public void ClientDisconnected()
    {

    }
}