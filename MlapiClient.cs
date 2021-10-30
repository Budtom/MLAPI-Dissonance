using Dissonance.Networking;
using Unity.Netcode;
using System;
using Allocator = Unity.Collections.Allocator;

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
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("DissonanceToClient", OnDissonanceToClient);
        Connected();
    }

    protected void OnDissonanceToClient(ulong id, FastBufferReader reader)
    {
        int length = reader.Length;
        Byte[] buffer = new Byte[length];
        reader.ReadBytes(ref buffer, length);

        base.NetworkReceivedPacket(new ArraySegment<byte>(buffer));
    }

    protected override void ReadMessages()
    {

    }

    protected override void SendReliable(ArraySegment<byte> packet)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // As we are the host in this scenario we should send the packet directly to the server rather than over the network and avoid loop-back issues
            _network.server.NetworkReceivedPacket(new MlapiConn(), packet);
        }
        else
        {
            using (FastBufferWriter writer = new FastBufferWriter(packet.Count, Allocator.TempJob))
            {
                writer.TryBeginWrite(packet.Count);
                writer.WriteBytes(packet.Array, packet.Count, packet.Offset);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("DissonanceToServer",
                        NetworkManager.Singleton.ServerClientId, writer, NetworkDelivery.Reliable);
            }
        }
    }

    protected override void SendUnreliable(ArraySegment<byte> packet)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // As we are the host in this scenario we should send the packet directly to the server rather than over the network and avoid loopback issues
            _network.server.NetworkReceivedPacket(new MlapiConn(), packet);
        }
        else
        {
            using (FastBufferWriter writer = new FastBufferWriter(packet.Count, Allocator.TempJob))
            {
                writer.TryBeginWrite(packet.Count);
                writer.WriteBytes(packet.Array, packet.Count, packet.Offset);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("DissonanceToServer", 
                    NetworkManager.Singleton.ServerClientId, writer, NetworkDelivery.Unreliable);
            }
        }

    }
}