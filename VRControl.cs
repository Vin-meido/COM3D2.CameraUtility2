
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

namespace COM3D2.CameraUtility2.Plugin
{
    class VRControl
    {
        private static bool enableDebug = false;

        public static IEnumerator Coroutine()
        {
            var vrButtons = GameMain.Instance.OvrMgr.GetVRControllerButtons(false);
            Log("VR Control Shortcuts Coroutine started");
            var skipping = false;

            while (true)
            {
                if (vrButtons.GetPressDown(AVRControllerButtons.BTN.VIRTUAL_L_CLICK))
                {
                    Log("VIRTUAL_L_CLICK");
                }

                if (vrButtons.GetPressDown(AVRControllerButtons.BTN.VIRTUAL_R_CLICK))
                {
                    Log("VIRTUAL_R_CLICK");
                }

                if (vrButtons.GetPressDown(AVRControllerButtons.BTN.TRIGGER))
                {
                    Log("TRIGGER");
                }

                if (vrButtons.GetPressDown(AVRControllerButtons.BTN.VIRTUAL_GRUB))
                {
                    Log("VIRTUAL_GRUB");
                }

                if (vrButtons.GetPressDown(AVRControllerButtons.BTN.GRIP))
                {
                    Log("GRIP");
                }

                if (vrButtons.GetPressDown(AVRControllerButtons.BTN.MENU))
                {
                    Log("MENU");
                }

                if (vrButtons.GetPressDown(AVRControllerButtons.BTN.STICK_PAD))
                {
                    Log("STICK_PAD");
                }

                if (vrButtons.GetTouchDown(AVRControllerButtons.TOUCH.XA))
                {
                    Log("TOUCH_XA");
                }

                if (vrButtons.GetTouchDown(AVRControllerButtons.TOUCH.YB))
                {
                    Log("TOUCH_YB");
                }

                if (IsPressed(AVRControllerButtons.BTN.VIRTUAL_L_CLICK, AVRControllerButtons.BTN.TRIGGER))
                {
                    // trigger+a
                    var messagemgr = GameMain.Instance.ScriptMgr.adv_kag.MessageWindowMgr;
                    messagemgr.CallEvent(MessageWindowMgr.MessageWindowUnderButton.Skip);
                }

                if (vrButtons.GetPress(AVRControllerButtons.BTN.TRIGGER) && vrButtons.GetPress(AVRControllerButtons.BTN.GRIP))
                {
                    if (!skipping)
                    {
                        GameMain.Instance.ScriptMgr.adv_kag.SetSettingSkipMode(true);
                        skipping = true;
                    }
                } else if (skipping)
                {
                    GameMain.Instance.ScriptMgr.adv_kag.SetSettingSkipMode(false);
                    skipping = false;

                }

                if (IsPressed(AVRControllerButtons.BTN.VIRTUAL_L_CLICK, AVRControllerButtons.BTN.GRIP))
                {
                    // grip+a
                    CameraUtility.Instance.ToggleFirstPersonCameraMode();
                }

                yield return null;

            }
        }

        private static bool IsPressed(AVRControllerButtons.BTN main, params AVRControllerButtons.BTN[] modifiers)
        {
            var vrButtons = new List<AVRControllerButtons> { GameMain.Instance.OvrMgr.GetVRControllerButtons(false), GameMain.Instance.OvrMgr.GetVRControllerButtons(true) };
            foreach(var button in vrButtons)
            {
                if (button.GetPressDown(main))
                {
                    if(modifiers.Length > 0)
                    {
                        foreach(var modifier in modifiers)
                        {
                            if(!button.GetPress(modifier)) return false;
                        }
                        return true;
                    } else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void Log(string message, params object[] args)
        {
            if (!enableDebug) return;
            UnityEngine.Debug.Log(string.Format(message, args));
        }
    }
}

