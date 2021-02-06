using Dissonance;
using UnityEditor;

#if UNITY_EDITOR

    [CustomEditor(typeof(MlapiCommsNetwork))]
    public class UNetCommsNetworkEditor
        : Dissonance.Editor.BaseDissonnanceCommsNetworkEditor<
            MlapiCommsNetwork,
            MlapiServer,
            MlapiClient,
            MlapiConn,
            Unit,
            Unit
        >
    {
    }
#endif