using Dissonance;
using UnityEditor;

#if UNITY_EDITOR

    [CustomEditor(typeof(MlapiCommsNetwork))]
    public class MlapiCommsNetworkEditor
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