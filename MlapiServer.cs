using Dissonance.Networking;
using System;
using Unity.Netcode;
using Allocator = Unity.Collections.Allocator;
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
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("DissonanceToServer", OnDissonanceToServer);
        base.Connect();
    }

    protected void OnDissonanceToServer(ulong client_id, FastBufferReader reader)
    {
        var client = new MlapiConn();
        client.clientId = client_id;

        //JP : I think there would be a way to access the raw buffer from the reader and just send that to the array segment. Maybe with unsafe c#
        //Would skip the allocation below.
        //Alternativly, we could just preallocate a big buffer and reuse it for each message?
        Byte[] buffer = new Byte[reader.Length];
        reader.ReadBytesSafe(ref buffer, reader.Length);

        base.NetworkReceivedPacket(client, new ArraySegment<byte>(buffer));
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
        if (NetworkManager.Singleton.LocalClientId == destination.clientId)
        {
            // As we are the host in this scenario we should send the packet directly to the client rather than over the network and avoid loopback issues
            _network.client.NetworkReceivedPacket(packet);
        }
        else
        {
            using (FastBufferWriter writer = new FastBufferWriter(packet.Count, Allocator.TempJob))
            {
                writer.WriteBytesSafe(packet.Array, packet.Count, packet.Offset);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("DissonanceToClient",
                    destination.clientId, writer, NetworkDelivery.Reliable);
            }

        }
    }

    protected override void SendUnreliable(MlapiConn destination, ArraySegment<byte> packet)
    {
        if (NetworkManager.Singleton.LocalClientId == destination.clientId)
        {
            // As we are the host in this scenario we should send the packet directly to the client rather than over the network and avoid loop-back issues
            _network.client.NetworkReceivedPacket(packet);
        }
        else
        {
            using (FastBufferWriter writer = new FastBufferWriter(packet.Count, Allocator.TempJob))
            {
                writer.WriteBytesSafe(packet.Array, packet.Count, packet.Offset);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("DissonanceToClient",
                    destination.clientId, writer, NetworkDelivery.Unreliable);
            }
        }
    }

    public void ClientDisconnected()
    {

    }
}