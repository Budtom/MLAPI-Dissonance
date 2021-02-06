using Dissonance;
using Dissonance.Networking;
using JetBrains.Annotations;
using MLAPI;

public class MlapiCommsNetwork
  : BaseCommsNetwork<
      MlapiServer,      // A class which implements BaseServer
      MlapiClient,      // A class which implements BaseClient
      MlapiConn,        // A struct which contains a HLAPI NetworkConnection
      Unit,             // Nothing
      Unit              // Nothing
  >
{
    public MlapiClient client;
    public MlapiServer server;


    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override MlapiClient CreateClient([CanBeNull] Unit connectionParameters)
    {
        client = new MlapiClient(this);
        return client;
    }

    protected override MlapiServer CreateServer([CanBeNull] Unit connectionParameters)
    {
        server = new MlapiServer(this);
        return server;
    }

    protected override void Update()
    {
        // Check if Dissonance is ready
        if (IsInitialized)
        {
            // Check if the MLAPI is ready
            var networkActive = NetworkingManager.Singleton.isActiveAndEnabled &&
                (NetworkingManager.Singleton.IsClient || NetworkingManager.Singleton.IsHost);
            if (networkActive)
            {
                // Check what mode the MLAPI is in
                var server = NetworkingManager.Singleton.IsHost;
                var client = NetworkingManager.Singleton.IsClient;

                // Check what mode Dissonance is in and if
                // they're different then call the correct method
                if (Mode.IsServerEnabled() != server
                    || Mode.IsClientEnabled() != client)
                {
                    // MLAPI is server and client, so run as a non-dedicated
                    // host (passing in the correct parameters)
                    if (server && client)
                        RunAsHost(Unit.None, Unit.None);

                    // MLAPI is just a server, so run as a dedicated host
                    else if (server)
                        RunAsDedicatedServer(Unit.None);

                    // MLAPI is just a client, so run as a client
                    else if (client)
                        RunAsClient(Unit.None);
                }
            }
            else if (Mode != NetworkMode.None)
            {
                //Network is not active, make sure Dissonance is not active
                Stop();
            }
        }

        base.Update();
    }
}