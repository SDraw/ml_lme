using System;
using System.Collections.Generic;
using UnityEngine;

namespace ml_lme
{
    class LeapTracked : MonoBehaviour
    {
        static readonly string[] gs_parameterNames =
        {
            "_LeftHandPresent",
            "_RightHandPresent",
            "_LeftHandThumbCurl",
            "_LeftHandIndexCurl",
            "_LeftHandMiddleCurl",
            "_LeftHandRingCurl",
            "_LeftHandPinkyCurl",
            "_LeftHandThumbSpread",
            "_LeftHandIndexSpread",
            "_LeftHandMiddleSpread",
            "_LeftHandRingSpread",
            "_LeftHandPinkySpread",
            "_RightHandThumbCurl",
            "_RightHandIndexCurl",
            "_RightHandMiddleCurl",
            "_RightHandRingCurl",
            "_RightHandPinkyCurl",
            "_RightHandThumbSpread",
            "_RightHandIndexSpread",
            "_RightHandMiddleSpread",
            "_RightHandRingSpread",
            "_RightHandPinkySpread"
        };
        enum CustomParameterType
        {
            LeftHandPresent,
            RightHandPresent,
            LeftHandThumbCurl,
            LeftHandIndexCurl,
            LeftHandMiddleCurl,
            LeftHandRingCurl,
            LeftHandPinkyCurl,
            LeftHandThumbSpread,
            LeftHandIndexSpread,
            LeftHandMiddleSpread,
            LeftHandRingSpread,
            LeftHandPinkySpread,
            RightHandThumbCurl,
            RightHandIndexCurl,
            RightHandMiddleCurl,
            RightHandRingCurl,
            RightHandPinkyCurl,
            RightHandThumbSpread,
            RightHandIndexSpread,
            RightHandMiddleSpread,
            RightHandRingSpread,
            RightHandPinkySpread
        }

        // Struct would be better, but it's not C++
        class CustomParameter
        {
            public VRC.Playables.AvatarParameter.EnumNPublicSealedvaUnBoInFl5vUnique m_valueType;
            public bool m_boolValue;
            public float m_floatValue;
            public int m_intValue;
            public int m_paramHash;
            public CustomParameterType m_parameterType;
        }

        VRCPlayer m_player = null;
        AvatarPlayableController m_playableController = null;
        HandGestureController m_handGestureController = null;
        RootMotion.FinalIK.IKSolverVR m_ikSolverVR = null;

        List<CustomParameter> m_parameters = null;

        bool m_fingersOnly = false;
        bool m_updateParameters = false;

        public VRCPlayer Player
        {
            set => m_player = value;
        }

        [UnhollowerBaseLib.Attributes.HideFromIl2Cpp]
        public bool FingersOnly
        {
            set => m_fingersOnly = value;
        }

        [UnhollowerBaseLib.Attributes.HideFromIl2Cpp]
        public bool Sdk3Parameters
        {
            set => m_updateParameters = value;
        }

        public LeapTracked(IntPtr ptr) : base(ptr) { }

        void Awake()
        {
            m_parameters = new List<CustomParameter>();
        }

        void Update()
        {
            if((m_playableController != null) && (m_parameters.Count != 0) && m_updateParameters)
            {
                foreach(var l_param in m_parameters)
                {
                    switch(l_param.m_valueType)
                    {
                        case VRC.Playables.AvatarParameter.EnumNPublicSealedvaUnBoInFl5vUnique.Bool:
                            MethodsResolver.SetAvatarBoolParam?.Invoke(m_playableController, new object[] { l_param.m_paramHash, l_param.m_boolValue });
                            break;
                        case VRC.Playables.AvatarParameter.EnumNPublicSealedvaUnBoInFl5vUnique.Float:
                            MethodsResolver.SetAvatarFloatParam?.Invoke(m_playableController, new object[] { l_param.m_paramHash, l_param.m_floatValue, false });
                            break;
                        case VRC.Playables.AvatarParameter.EnumNPublicSealedvaUnBoInFl5vUnique.Int:
                            MethodsResolver.SetAvatarIntParam?.Invoke(m_playableController, new object[] { l_param.m_paramHash, l_param.m_intValue });
                            break;
                    }
                }
            }
        }

        [UnhollowerBaseLib.Attributes.HideFromIl2Cpp]
        public void UpdateFromGestures(GestureMatcher.GesturesData f_gesturesData)
        {
            if((m_parameters.Count != 0) && m_updateParameters)
            {
                foreach(var l_param in m_parameters)
                {
                    switch(l_param.m_parameterType)
                    {
                        case CustomParameterType.LeftHandPresent:
                        case CustomParameterType.RightHandPresent:
                            l_param.m_boolValue = f_gesturesData.m_handsPresenses[(int)l_param.m_parameterType];
                            break;

                        case CustomParameterType.LeftHandThumbCurl:
                        case CustomParameterType.LeftHandIndexCurl:
                        case CustomParameterType.LeftHandMiddleCurl:
                        case CustomParameterType.LeftHandRingCurl:
                        case CustomParameterType.LeftHandPinkyCurl:
                            l_param.m_floatValue = f_gesturesData.m_leftFingersBends[(int)l_param.m_parameterType -(int)CustomParameterType.LeftHandThumbCurl];
                            break;

                        case CustomParameterType.RightHandThumbCurl:
                        case CustomParameterType.RightHandIndexCurl:
                        case CustomParameterType.RightHandMiddleCurl:
                        case CustomParameterType.RightHandRingCurl:
                        case CustomParameterType.RightHandPinkyCurl:
                            l_param.m_floatValue = f_gesturesData.m_rightFingersBends[(int)l_param.m_parameterType - (int)CustomParameterType.RightHandThumbCurl];
                            break;

                        case CustomParameterType.LeftHandThumbSpread:
                        case CustomParameterType.LeftHandIndexSpread:
                        case CustomParameterType.LeftHandMiddleSpread:
                        case CustomParameterType.LeftHandRingSpread:
                        case CustomParameterType.LeftHandPinkySpread:
                            l_param.m_floatValue = f_gesturesData.m_leftFingersSpreads[(int)l_param.m_parameterType - (int)CustomParameterType.LeftHandThumbSpread];
                            break;

                        case CustomParameterType.RightHandThumbSpread:
                        case CustomParameterType.RightHandIndexSpread:
                        case CustomParameterType.RightHandMiddleSpread:
                        case CustomParameterType.RightHandRingSpread:
                        case CustomParameterType.RightHandPinkySpread:
                            l_param.m_floatValue = f_gesturesData.m_rightFingersSpreads[(int)l_param.m_parameterType - (int)CustomParameterType.RightHandThumbSpread];
                            break;
                    }
                }
            }

            if(m_handGestureController != null)
            {
                m_handGestureController.field_Internal_Boolean_0 = true;
                m_handGestureController.field_Private_EnumNPublicSealedvaKeMoCoGaViOcViDaWaUnique_0 = VRCInputManager.EnumNPublicSealedvaKeMoCoGaViOcViDaWaUnique.Index;

                for(int i = 0; i < 2; i++)
                {
                    if(f_gesturesData.m_handsPresenses[i])
                    {
                        for(int j = 0; j < 5; j++)
                        {
                            int l_dataIndex = i * 5 + j;
                            m_handGestureController.field_Private_ArrayOf_VRCInput_0[l_dataIndex].field_Public_Single_0 = 1.0f - ((i == 0) ? f_gesturesData.m_leftFingersBends[j] : f_gesturesData.m_rightFingersBends[j]); // Curl
                            m_handGestureController.field_Private_ArrayOf_VRCInput_1[l_dataIndex].field_Public_Single_0 = ((i == 0) ? f_gesturesData.m_leftFingersSpreads[j] : f_gesturesData.m_rightFingersSpreads[j]); // Spread
                        }
                    }
                }
            }
        }

        [UnhollowerBaseLib.Attributes.HideFromIl2Cpp]
        public void UpdateHandsPositions(GestureMatcher.GesturesData f_gesturesData, Transform f_left, Transform f_right)
        {
            if((m_ikSolverVR != null) && !m_fingersOnly)
            {
                if(f_gesturesData.m_handsPresenses[0] && (m_ikSolverVR.leftArm != null) && (m_ikSolverVR.leftArm.target != null))
                {
                    m_ikSolverVR.leftArm.positionWeight = 1f;
                    m_ikSolverVR.leftArm.rotationWeight = 1f;
                    m_ikSolverVR.leftArm.target.position = f_left.position;
                    m_ikSolverVR.leftArm.target.rotation = f_left.rotation;
                }

                if(f_gesturesData.m_handsPresenses[1] && (m_ikSolverVR.rightArm != null) && (m_ikSolverVR.rightArm.target != null))
                {
                    m_ikSolverVR.rightArm.positionWeight = 1f;
                    m_ikSolverVR.rightArm.rotationWeight = 1f;
                    m_ikSolverVR.rightArm.target.position = f_right.position;
                    m_ikSolverVR.rightArm.target.rotation = f_right.rotation;
                }
            }

            if(m_handGestureController != null)
            {
                m_handGestureController.field_Internal_Boolean_0 = true;
                m_handGestureController.field_Private_EnumNPublicSealedvaKeMoCoGaViOcViDaWaUnique_0 = VRCInputManager.EnumNPublicSealedvaKeMoCoGaViOcViDaWaUnique.Index;
                for(int i = 0; i < 2; i++)
                {
                    if(f_gesturesData.m_handsPresenses[i])
                    {
                        for(int j = 0; j < 5; j++)
                        {
                            int l_dataIndex = i * 5 + j;
                            m_handGestureController.field_Private_ArrayOf_VRCInput_0[l_dataIndex].field_Public_Single_0 = 1.0f - ((i == 0) ? f_gesturesData.m_leftFingersBends[j] : f_gesturesData.m_rightFingersBends[j]); // Curl
                            m_handGestureController.field_Private_ArrayOf_VRCInput_1[l_dataIndex].field_Public_Single_0 = ((i == 0) ? f_gesturesData.m_leftFingersSpreads[j] : f_gesturesData.m_rightFingersSpreads[j]); // Spread
                        }
                    }
                }
            }

        }

        public void ResetParameters()
        {
            m_parameters.Clear();
            m_playableController = null;

            m_handGestureController = m_player?.field_Private_VRC_AnimationController_0?.field_Private_HandGestureController_0;
            m_ikSolverVR = m_player?.field_Private_VRC_AnimationController_0?.field_Private_VRIK_0?.solver;

            RebuildParameters();
        }

        public void ResetTracking()
        {
            if(m_handGestureController != null)
            {
                m_handGestureController.field_Internal_Boolean_0 = false;
                m_handGestureController.field_Private_EnumNPublicSealedvaKeMoCoGaViOcViDaWaUnique_0 = VRCInputManager.EnumNPublicSealedvaKeMoCoGaViOcViDaWaUnique.Mouse;
            }
        }

        void RebuildParameters()
        {
            m_playableController = m_player?.field_Private_AnimatorControllerManager_0?.field_Private_AvatarAnimParamController_0?.field_Private_AvatarPlayableController_0;
            if(m_playableController != null)
            {
                foreach(var l_param in m_playableController.field_Private_ArrayOf_ObjectNPublicInObInPaInUnique_0)
                {
                    for(int i = 0; i < gs_parameterNames.Length; i++)
                    {
                        if(l_param.field_Public_AvatarParameter_0?.field_Private_String_0 == gs_parameterNames[i])
                        {
                            m_parameters.Add(new CustomParameter
                            {
                                m_boolValue = false,
                                m_intValue = 0,
                                m_floatValue = 0f,
                                m_parameterType = (CustomParameterType)i,
                                m_paramHash = l_param.field_Public_Int32_0,
                                m_valueType = l_param.field_Public_AvatarParameter_0.field_Private_EnumNPublicSealedvaUnBoInFl5vUnique_0
                            });
                            break;
                        }
                    }
                }
            }
        }
    }
}
