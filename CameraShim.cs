using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;
using HarmonyLib;

namespace COM3D2.CameraUtility2.Plugin
{
    public class CameraMainPatch : MonoBehaviour
    {
        private Harmony harmony;
        public static Vector3 lastTargetPos { get; private set; }
        public static Vector2 lastAngle { get; private set; }
        public static float lastRadius { get; private set; }
        public static bool enableDebug { get; set; }
        public static Vector3 lastTargetOffset { get; private set; }

        public static CameraMainPatch Instance { get; private set; }

        public static void Create()
        {
            var go = new GameObject("CameraMainPatch");
            go.AddComponent<CameraMainPatch>();
        }


        public void Awake()
        {
            if (Instance != null)
            {
                throw new Exception("Already initialized");
            }

            Instance = this;
            GameObject.DontDestroyOnLoad(this);
            enableDebug = true;

            harmony = new Harmony("org.bepinex.plugins.com3d2.camerautility.camerashim");
            harmony.PatchAll(typeof(CameraPatch));
            harmony.PatchAll(typeof(OvrCameraPatch));
        }

        public void OnLevelWasLoaded(int level)
        {
            var camera = GameMain.Instance.MainCamera;
            lastTargetPos = camera.GetTargetPos();
            lastAngle = camera.GetAroundAngle();
            lastRadius = camera.GetDistance();
            lastTargetOffset = Vector3.zero;

            LogDebug($"Reset last camera values:\ntarget:{lastTargetPos}\nangle:{lastAngle}\noffset:{lastTargetOffset}");
        }

        public void OnDestroy()
        {
            harmony.UnpatchSelf();
        }


        [HarmonyPatch(typeof(CameraMain))]
        class CameraPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("SetTargetPos")]
            //public virtual void SetTargetPos(UnityEngine.Vector3 f_vecWorldPos, [bool f_bSelf = True])
            public static void SetTargetPos(UnityEngine.Vector3 f_vecWorldPos, bool f_bSelf = true)
            {
                lastTargetPos = f_vecWorldPos;
                LogDebug("SetTargetPos {0}, {1}", f_vecWorldPos, f_bSelf);
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetTargetOffset")]
            //public virtual void SetTargetOffset(UnityEngine.Vector3 f_vOffs, bool is_animation)
            public static void SetTargetOffset(UnityEngine.Vector3 f_vOffs, bool is_animation)
            {
                lastTargetOffset = f_vOffs;
                LogDebug("SetTargetOffset {0}, {1}", f_vOffs, is_animation);
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetRotation")]
            //public virtual void SetRotation(UnityEngine.Vector3 f_vecWorldRot)
            public static void SetRotation(UnityEngine.Vector3 f_vecWorldRot)
            {
                LogDebug("SetRotation {0}", f_vecWorldRot);
            }

            //public virtual void UpdateCamAngle(UnityEngine.Vector2 f_vAngle)
            [HarmonyPrefix]
            [HarmonyPatch("UpdateCamAngle")]
            public static void UpdateCamAngle(UnityEngine.Vector2 f_vAngle)
            {
                LogDebug("UpdateCamAngle {0}", f_vAngle);
            }

            //public virtual void UpdateCamTgt(UnityEngine.Vector3 f_vValue)
            [HarmonyPrefix]
            [HarmonyPatch("UpdateCamTgt")]
            public static void UpdateCamTgt(UnityEngine.Vector3 f_vValue)
            {
                LogDebug("UpdateCamTgt {0}", f_vValue);
            }

            //public virtual void SetFromScriptOnTarget(UnityEngine.Vector3 f_vCenter, float f_fRadius, UnityEngine.Vector2 f_vRotate)
            [HarmonyPrefix]
            [HarmonyPatch("SetFromScriptOnTarget")]
            public static void SetFromScriptOnTarget(UnityEngine.Vector3 f_vCenter, float f_fRadius, UnityEngine.Vector2 f_vRotate)
            {
                lastTargetPos = f_vCenter;
                lastRadius = f_fRadius;
                lastAngle = f_vRotate;

                LogDebug("SetFromScriptOnTarget {0}, {1}, {2}", f_vCenter, f_fRadius, f_vRotate);
            }

            //public virtual void SetAroundAngle(UnityEngine.Vector2 f_vecAngle, [bool f_bSelf = True])
            [HarmonyPrefix]
            [HarmonyPatch("SetAroundAngle")]
            public static void SetAroundAngle(UnityEngine.Vector2 f_vecAngle, bool f_bSelf = true)
            {
                lastAngle = f_vecAngle;
                LogDebug("SetAroundAngle {0}, {1}", f_vecAngle, f_bSelf);
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetDistance")]
            public static void SetDistance(float f_fDistance, bool f_bSelf = true)
            {
                lastRadius = f_fDistance;
                LogDebug("SetDistance {0}, {1}", f_fDistance, f_bSelf);
            }
        }



        [HarmonyPatch(typeof(OvrCamera))]
        class OvrCameraPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("SetTargetPos")]
            //public virtual void SetTargetPos(UnityEngine.Vector3 f_vecWorldPos, [bool f_bSelf = True])
            public static void SetTargetPos(UnityEngine.Vector3 f_vecWorldPos, bool f_bSelf = true)
            {
                lastTargetPos = f_vecWorldPos;
                LogDebug("SetTargetPos {0}, {1}", f_vecWorldPos, f_bSelf);
            }

            //public virtual void SetAroundAngle(UnityEngine.Vector2 f_vecAngle, [bool f_bSelf = True])
            [HarmonyPrefix]
            [HarmonyPatch("SetAroundAngle")]
            public static void SetAroundAngle(UnityEngine.Vector2 f_vecAngle, bool f_bSelf = true)
            {
                lastAngle = f_vecAngle;
                LogDebug("SetAroundAngle {0}, {1}", f_vecAngle, f_bSelf);
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetDistance")]
            public static void SetDistance(float f_fDistance, bool f_bSelf = true)
            {
                lastRadius = f_fDistance;
                LogDebug("SetDistance {0}, {1}", f_fDistance, f_bSelf);
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetFootPos")]
            public static void SetFootPos(UnityEngine.Vector3 f_vecFootPos, CameraMain.STAND_SIT f_eState)
            {
                LogDebug("SetFootPos {0}, {1}", f_vecFootPos, f_eState);
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetTransform")]
            public static void SetTransform(Vector3 f_vecTargetPosWorld, Vector2 f_vecRot, float f_fDistance)
            {
                lastTargetPos = f_vecTargetPosWorld;
                lastRadius = f_fDistance;
                lastAngle = f_vecRot;

                LogDebug($"SetTransform {f_vecTargetPosWorld} {f_vecRot} {f_fDistance}");
            }
        }

        private static void LogDebug(string msg, params object[] args)
        {
            if (!enableDebug) return;
            UnityEngine.Debug.Log(string.Format(msg, args));
        }
    }
}