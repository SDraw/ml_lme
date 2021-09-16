using System.Linq;
using UnityEngine;

namespace ml_lme
{
    public class LeapMotionExtention : MelonLoader.MelonMod
    {
        static readonly Quaternion gs_hmdRotationFix = new Quaternion(0f, 0.7071068f, 0.7071068f, 0f);

        Leap.Controller m_leapController = null;
        GestureMatcher.GesturesData m_gesturesData = null;

        GameObject m_leapTrackingRoot = null;
        GameObject m_leapLeftHand = null;
        GameObject m_leapRightHand = null;

        LeapTracked m_localTracked = null;

        public override void OnApplicationStart()
        {
            UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<LeapTracked>();

            DependenciesHandler.ExtractDependencies();
            MethodsResolver.ResolveMethods();
            Settings.LoadSettings();

            m_leapController = new Leap.Controller();
            m_gesturesData = new GestureMatcher.GesturesData();

            // Events
            VRChatUtilityKit.Utilities.VRCUtils.OnUiManagerInit += this.OnUiManagerInit;
            VRChatUtilityKit.Utilities.NetworkEvents.OnRoomJoined += this.OnRoomJoined;
            VRChatUtilityKit.Utilities.NetworkEvents.OnRoomLeft += this.OnRoomLeft;
            VRChatUtilityKit.Utilities.NetworkEvents.OnAvatarInstantiated += this.OnAvatarInstantiated;

            // Patches
            var l_patchMethod = new HarmonyLib.HarmonyMethod(typeof(LeapMotionExtention), nameof(VRCIM_ControllersType));
            typeof(VRCInputManager).GetMethods().Where(x =>
                    x.Name.StartsWith("Method_Public_Static_Boolean_EnumNPublicSealedvaKeMoCoGaViOcViDaWaUnique_")
                ).ToList().ForEach(m => HarmonyInstance.Patch(m, l_patchMethod));
        }

        void OnUiManagerInit()
        {
            // Create game objects
            m_leapTrackingRoot = new GameObject("LeapTrackingRoot");
            m_leapTrackingRoot.transform.parent = Utils.GetVRCTrackingManager().gameObject.transform;
            UnityEngine.Object.DontDestroyOnLoad(m_leapTrackingRoot);

            m_leapLeftHand = new GameObject("LeapLeftHand");
            m_leapLeftHand.transform.parent = m_leapTrackingRoot.transform;
            UnityEngine.Object.DontDestroyOnLoad(m_leapLeftHand);

            m_leapRightHand = new GameObject("LeapRightHand");
            m_leapRightHand.transform.parent = m_leapTrackingRoot.transform;
            UnityEngine.Object.DontDestroyOnLoad(m_leapRightHand);

            OnPreferencesSaved();
        }

        public override void OnApplicationQuit()
        {
            m_leapTrackingRoot = null;
            m_localTracked = null;

            m_leapController.StopConnection();
            m_leapController.Dispose();
            m_leapController = null;
        }

        public override void OnPreferencesSaved()
        {
            Settings.ReloadSettings();

            // Update Leap controller
            if(m_leapController != null)
            {
                if(Settings.Enabled)
                    m_leapController.StartConnection();
                else
                    m_leapController.StopConnection();

                if(Settings.LeapHmdMode)
                    m_leapController.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                else
                    m_leapController.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
            }

            // Update tracking transforms
            if(m_leapTrackingRoot != null)
            {
                m_leapTrackingRoot.transform.parent = (Settings.HeadRoot ? Utils.GetCamera().gameObject.transform : Utils.GetVRCTrackingManager().gameObject.transform);
                m_leapTrackingRoot.transform.localPosition = new Vector3(0f, (Settings.HeadRoot ? Settings.HmdOffsetY : Settings.DesktopOffsetY), (Settings.HeadRoot ? Settings.HmdOffsetZ : Settings.DesktopOffsetZ));
                m_leapTrackingRoot.transform.localRotation = Quaternion.identity;
            }

            if(m_localTracked != null)
            {
                m_localTracked.FingersOnly = Settings.FingersTracking;
                m_localTracked.Sdk3Parameters = Settings.SDK3Parameters;
                if(!Settings.Enabled)
                    m_localTracked.ResetTracking();
            }
        }

        public override void OnUpdate()
        {
            if(Settings.Enabled && m_leapController.IsConnected)
            {
                var l_frame = m_leapController.Frame();
                if(l_frame != null)
                {
                    GestureMatcher.GetGestures(l_frame, ref m_gesturesData);
                    m_localTracked?.UpdateFromGestures(m_gesturesData);

                    // Update transforms
                    if(m_gesturesData.m_handsPresenses[0] && (m_leapLeftHand != null))
                    {
                        Vector3 l_pos = m_gesturesData.m_handsPositons[0];
                        Quaternion l_rot = m_gesturesData.m_handsRotations[0];
                        ReorientateLeapToUnity(ref l_pos, ref l_rot);
                        m_leapLeftHand.transform.localPosition = l_pos;
                        m_leapLeftHand.transform.localRotation = l_rot;
                    }

                    if(m_gesturesData.m_handsPresenses[1] && (m_leapRightHand != null))
                    {
                        Vector3 l_pos = m_gesturesData.m_handsPositons[1];
                        Quaternion l_rot = m_gesturesData.m_handsRotations[1];
                        ReorientateLeapToUnity(ref l_pos, ref l_rot);
                        m_leapRightHand.transform.localPosition = l_pos;
                        m_leapRightHand.transform.localRotation = l_rot;
                    }
                }
            }
        }

        public override void OnLateUpdate()
        {
            if(Settings.Enabled && (m_localTracked != null))
            {
                m_localTracked.UpdateHandsPositions(m_gesturesData, m_leapLeftHand.transform, m_leapRightHand.transform);
            }
        }

        void OnRoomJoined()
        {
            MelonLoader.MelonCoroutines.Start(CreateLocalTracked());
        }
        System.Collections.IEnumerator CreateLocalTracked()
        {
            while(Utils.GetLocalPlayer()?.prop_VRCPlayer_0 == null)
                yield return null;
            m_localTracked = Utils.GetLocalPlayer().prop_VRCPlayer_0.gameObject.AddComponent<LeapTracked>();
            m_localTracked.Player = Utils.GetLocalPlayer().prop_VRCPlayer_0;
            m_localTracked.FingersOnly = Settings.FingersTracking;
            m_localTracked.Sdk3Parameters = Settings.SDK3Parameters;

            // Restart Leap Motion because of weird LeapCSharp bug
            if(Settings.Enabled)
            {
                m_leapController.StopConnection();
                m_leapController.StartConnection();
            }
        }

        void OnRoomLeft()
        {
            m_localTracked = null;
        }

        void OnAvatarInstantiated(VRCAvatarManager f_avatarManager, VRC.Core.ApiAvatar f_apiAvatar, GameObject f_avatarObject)
        {
            var l_player = f_avatarObject?.transform?.root?.GetComponentInChildren<VRC.Player>();
            if(l_player != null)
            {
                if(l_player == Utils.GetLocalPlayer())
                {
                    m_localTracked?.ResetParameters();
                }
            }
        }

        static void ReorientateLeapToUnity(ref Vector3 f_pos, ref Quaternion f_rot)
        {
            f_pos *= 0.001f;
            f_pos.z *= -1f;
            f_rot.x *= -1f;
            f_rot.y *= -1f;

            if(Settings.LeapHmdMode)
            {
                f_pos.x *= -1f;
                Utils.Swap(ref f_pos.y, ref f_pos.z);
                f_rot = (gs_hmdRotationFix * f_rot);
            }
        }

        static bool VRCIM_ControllersType(ref bool __result, VRCInputManager.EnumNPublicSealedvaKeMoCoGaViOcViDaWaUnique __0)
        {
            if(Settings.Enabled && (bool)MethodsResolver.IsInVR?.Invoke(null, null)) // Need to account presence of VR controllers
            {
                if(__0 == VRCInputManager.EnumNPublicSealedvaKeMoCoGaViOcViDaWaUnique.Index)
                {
                    __result = true;
                    return false;
                }
                else
                {
                    __result = false;
                    return false;
                }
            }
            else
                return true;
        }
    }
}
