namespace ml_lme
{
    static class Utils
    {
        public static VRCPlayer GetLocalPlayer() => VRCPlayer.field_Internal_Static_VRCPlayer_0;
        public static VRCTrackingManager GetVRCTrackingManager() => VRCTrackingManager.field_Private_Static_VRCTrackingManager_0;
        public static VRCTrackingSteam GetVRCTrackingSteam() => GetVRCTrackingManager().field_Private_List_1_VRCTracking_0[0].TryCast<VRCTrackingSteam>();
        public static SteamVR_Camera GetCamera() => GetVRCTrackingSteam().field_Private_SteamVR_Camera_0;

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        // Extensions
        public static void SetAvatarIntParamEx(this AvatarPlayableController controller, int paramHash, int val)
        {
            MethodsResolver.SetAvatarIntParam?.Invoke(controller, new object[] { paramHash, val });
            controller.field_Private_Boolean_3 = true; // bool requiresNetworkSync;
        }
        public static void SetAvatarBoolParamEx(this AvatarPlayableController controller, int paramHash, bool val)
        {
            MethodsResolver.SetAvatarBoolParam?.Invoke(controller, new object[] { paramHash, val });
            controller.field_Private_Boolean_3 = true; // bool requiresNetworkSync;
        }
        public static void SetAvatarFloatParamEx(this AvatarPlayableController controller, int paramHash, float val, bool debug = false)
        {
            MethodsResolver.SetAvatarFloatParam?.Invoke(controller, new object[] { paramHash, val, debug });
            controller.field_Private_Boolean_3 = true; // bool requiresNetworkSync;
        }
    }
}
