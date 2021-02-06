using Dissonance.Networking;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using MLAPI.Transports.UNET;
using System;
using System.IO;
using UnityEngine;

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
        if (NetworkingManager.Singleton.IsHost)
        {
            // As we are the host in this scenario we should send the packet directly to the server rather than over the network and avoid loopback issues
            _network.server.NetworkReceivedPacket(new MlapiConn(), packet);
        }
        else
        {

            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    for (var i = 0; i < packet.Count; i++)
                    {
                        writer.WriteByte(packet.Array[i + packet.Offset]);
                    }
                    CustomMessagingManager.SendNamedMessage("DissonanceToServer", NetworkingManager.Singleton.ServerClientId, stream, "MLAPI_ANIMATION_UPDATE");
                }
            }
        }
    }

    protected override void SendUnreliable(ArraySegment<byte> packet)
    {
        if (NetworkingManager.Singleton.IsHost)
        {
            // As we are the host in this scenario we should send the packet directly to the server rather than over the network and avoid loopback issues
            _network.server.NetworkReceivedPacket(new MlapiConn(), packet);
        }
        else
        {

            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    for (var i = 0; i < packet.Count; i++)
                    {
                        writer.WriteByte(packet.Array[i + packet.Offset]);
                    }
                    CustomMessagingManager.SendNamedMessage("DissonanceToServer", NetworkingManager.Singleton.ServerClientId, stream, "MLAPI_TIME_SYNC");
                }
            }
        }

    }
}