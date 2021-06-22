using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.VR;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace COM3D2.CameraUtility2.Plugin
{
    [BepInPlugin("org.bepinex.plugins.com3d2.camerautility.camerashim", "CameraUtility.CameraShim", "1.0.0.0")]
    [HarmonyPatch(typeof(CameraMain))]
    class CameraMainPatch: BaseUnityPlugin
    {
        private Harmony harmony;
        public static Vector3 lastTargetPos { get; private set; }
        public static Vector2 lastAngle { get; private set; }
        public static float lastRadius { get; private set; }
        public static bool enableDebug { get; set; }

        public void Awake()
        {
            GameObject.DontDestroyOnLoad(this);
            harmony = new Harmony("org.bepinex.plugins.com3d2.camerautility.camerashim");
            harmony.PatchAll(typeof(CameraMainPatch));
            enableDebug = true;
        }

        public void OnLevelWasLoaded(int level)
        {
            lastAngle = new Vector2(10, 180);
        }

        public void OnDestroy()
        {
            harmony.UnpatchSelf();
        }

        [HarmonyPrefix]
        [HarmonyPatch("SetTargetPos")]
        //public virtual void SetTargetPos(UnityEngine.Vector3 f_vecWorldPos, [bool f_bSelf = True])
        public static void SetTargetPos(UnityEngine.Vector3 f_vecWorldPos, bool f_bSelf = true)
        {
            lastTargetPos = f_vecWorldPos;
            LogDebug("CameraMain.SetTargetPos {0}, {1}", f_vecWorldPos, f_bSelf);
        }

        [HarmonyPrefix]
        [HarmonyPatch("SetTargetOffset")]
        //public virtual void SetTargetOffset(UnityEngine.Vector3 f_vOffs, bool is_animation)
        public static void SetTargetOffset(UnityEngine.Vector3 f_vOffs, bool is_animation)
        {
            LogDebug("CameraMain.SetTargetOffset {0}, {1}", f_vOffs, is_animation);
        }

        [HarmonyPrefix]
        [HarmonyPatch("SetRotation")]
        //public virtual void SetRotation(UnityEngine.Vector3 f_vecWorldRot)
        public static void SetRotation(UnityEngine.Vector3 f_vecWorldRot)
        {
            LogDebug("CameraMain.SetRotation {0}", f_vecWorldRot);
        }

        //public virtual void UpdateCamAngle(UnityEngine.Vector2 f_vAngle)
        [HarmonyPrefix]
        [HarmonyPatch("UpdateCamAngle")]
        public static void UpdateCamAngle(UnityEngine.Vector2 f_vAngle)
        {
            LogDebug("CameraMain.UpdateCamAngle {0}", f_vAngle);
        }

        //public virtual void UpdateCamTgt(UnityEngine.Vector3 f_vValue)
        [HarmonyPrefix]
        [HarmonyPatch("UpdateCamTgt")]
        public static void UpdateCamTgt(UnityEngine.Vector3 f_vValue)
        {
            LogDebug("CameraMain.UpdateCamTgt {0}", f_vValue);
        }

        //public virtual void SetFromScriptOnTarget(UnityEngine.Vector3 f_vCenter, float f_fRadius, UnityEngine.Vector2 f_vRotate)
        [HarmonyPrefix]
        [HarmonyPatch("SetFromScriptOnTarget")]
        public static void SetFromScriptOnTarget(UnityEngine.Vector3 f_vCenter, float f_fRadius, UnityEngine.Vector2 f_vRotate)
        {
            lastTargetPos = f_vCenter;
            lastRadius = f_fRadius;
            lastAngle = f_vRotate;

            LogDebug("CameraMain.SetFromScriptOnTarget {0}, {1}, {2}", f_vCenter, f_fRadius, f_vRotate);
        }

        //public virtual void SetAroundAngle(UnityEngine.Vector2 f_vecAngle, [bool f_bSelf = True])
        [HarmonyPrefix]
        [HarmonyPatch("SetAroundAngle")]
        public static void SetAroundAngle(UnityEngine.Vector2 f_vecAngle, bool f_bSelf=true)
        {
            lastAngle = f_vecAngle;
            LogDebug("CameraMain.SetAroundAngle {0}, {1}", f_vecAngle, f_bSelf);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OvrCamera))]
        [HarmonyPatch("SetFootPos")]
        public static void SetFootPos(UnityEngine.Vector3 f_vecFootPos, CameraMain.STAND_SIT f_eState)
        {
            LogDebug("OvrCamera.SetFootPos {0}, {1}", f_vecFootPos, f_eState);
        }

        private static void LogDebug(string msg, params object[] args)
        {
            if (!enableDebug) return;
            UnityEngine.Debug.Log(string.Format(msg, args));
        }
    }
}