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

namespace COM3D2.CameraUtility2.Plugin.Config
{

    class KeyConfig
    {
        public ConfigEntry<KeyboardShortcut> bgLeftMove;
        public ConfigEntry<KeyboardShortcut> bgRightMove;
        public ConfigEntry<KeyboardShortcut> bgForwardMove;
        public ConfigEntry<KeyboardShortcut> bgBackMove;
        public ConfigEntry<KeyboardShortcut> bgUpMove;
        public ConfigEntry<KeyboardShortcut> bgDownMove;

        public ConfigEntry<KeyboardShortcut> bgLeftRotate;
        public ConfigEntry<KeyboardShortcut> bgRightRotate;
        public ConfigEntry<KeyboardShortcut> bgLeftRoll;
        public ConfigEntry<KeyboardShortcut> bgRightRoll;
        public ConfigEntry<KeyboardShortcut> bgInitialize;

        public ConfigEntry<KeyboardShortcut> cameraLeftRoll;
        public ConfigEntry<KeyboardShortcut> cameraRightRoll;
        public ConfigEntry<KeyboardShortcut> cameraRollInitialize;
        public ConfigEntry<KeyboardShortcut> cameraFoVPlus;
        public ConfigEntry<KeyboardShortcut> cameraFoVMinus;
        public ConfigEntry<KeyboardShortcut> cameraFoVInitialize;

        public ConfigEntry<KeyboardShortcut> eyetoCamToggle;
        public ConfigEntry<KeyboardShortcut> eyetoCamChange;

        public ConfigEntry<KeyboardShortcut> hideUIToggle;
        public ConfigEntry<KeyboardShortcut> cameraFPSModeToggle;
        public ConfigEntry<KeyboardShortcut> manHeadChange;

        public ConfigEntry<CameraUtility.ModifierKey> speedDownModifier;
        public ConfigEntry<CameraUtility.ModifierKey> initializeModifier;

        public KeyConfig(ConfigFile conf)
        {
            FloorMoverConfig(conf);
            CameraConfig(conf);
            LookAtThisConfig(conf);
            MiscConfig(conf);
        }

        private void FloorMoverConfig(ConfigFile conf)
        {
            var section = "KeyConfig.FloorMover";

            bgLeftMove = conf.Bind(
                section,
                "bgLeftMove",
                new KeyboardShortcut(KeyCode.LeftArrow),
                "背景(メイド) 左移動");

            bgRightMove = conf.Bind(
                section,
                "bgRightMove",
                new KeyboardShortcut(KeyCode.RightArrow),
                "背景(メイド) 右移動");

            bgForwardMove = conf.Bind(
                section,
                "bgForwardMove",
                new KeyboardShortcut(KeyCode.UpArrow),
                "背景 前移動");

            bgBackMove = conf.Bind(
                section,
                "bgBackMove",
                new KeyboardShortcut(KeyCode.DownArrow),
                "背景 後移動");

            bgUpMove = conf.Bind(
                section,
                "bgUpMove",
                new KeyboardShortcut(KeyCode.PageUp),
                "背景 上移動");

            bgDownMove = conf.Bind(
                section,
                "bgDownMove",
                new KeyboardShortcut(KeyCode.PageDown),
                "背景 下移動");

            bgLeftRotate = conf.Bind(
                section,
                "bgLeftRotate",
                new KeyboardShortcut(KeyCode.Delete),
                "背景(メイド) 左回転");

            bgRightRotate = conf.Bind(
                section,
                "bgRightRotate",
                new KeyboardShortcut(KeyCode.End),
                "背景(メイド) 右回転");

            bgLeftRoll = conf.Bind(
                section,
                "bgLeftRoll",
                new KeyboardShortcut(KeyCode.Insert),
                "背景 左ロール");

            bgRightRoll = conf.Bind(
                section,
                "bgRightRoll",
                new KeyboardShortcut(KeyCode.Home),
                "背景 右ロール");

            bgInitialize = conf.Bind(
                section,
                "bgInitialize",
                new KeyboardShortcut(KeyCode.Backspace),
                "背景 初期化");
        }

        private void CameraConfig(ConfigFile conf)
        {
            var section = "KeyConfig.Camera";

            cameraLeftRoll = conf.Bind(
                section,
                "cameraLeftRoll",
                new KeyboardShortcut(KeyCode.Period),
                "カメラ 左ロール");

            cameraRightRoll = conf.Bind(
                section,
                "cameraRightRoll",
                new KeyboardShortcut(KeyCode.Backslash),
                "カメラ 右ロール");

            cameraRollInitialize = conf.Bind(
                section,
                "cameraRollInitialize",
                new KeyboardShortcut(KeyCode.Slash),
                "カメラ 水平");

            cameraFoVPlus = conf.Bind(
                section,
                "cameraFoVPlus",
                new KeyboardShortcut(KeyCode.RightBracket),
                "カメラ 視野拡大");

            cameraFoVMinus = conf.Bind(
                section,
                "cameraFoVMinus",
                new KeyboardShortcut(KeyCode.Equals),
                "カメラ 視野縮小 (初期値 Equals は日本語キーボードでは [; + れ])");

            cameraFoVInitialize = conf.Bind(
                section,
                "cameraFoVInitialize",
                new KeyboardShortcut(KeyCode.Semicolon),
                "カメラ 視野初期化 (初期値 Semicolon は日本語キーボードでは [: * け])");

        }

        private void LookAtThisConfig(ConfigFile conf)
        {
            var section = "KeyConfig.LookAtThis";

            eyetoCamToggle = conf.Bind(
                section,
                "eyetoCamToggle",
                new KeyboardShortcut(KeyCode.G),
                "こっち見て／通常切り替え (トグル)");

            eyetoCamChange = conf.Bind(
                section,
                "eyetoCamChange",
                new KeyboardShortcut(KeyCode.T),
                "視線及び顔の向き切り替え (ループ)");

        }

        private void MiscConfig(ConfigFile conf)
        {
            var section = "KeyConfig.Misc";

            hideUIToggle = conf.Bind(
                section,
                "hideUIToggle",
                new KeyboardShortcut(KeyCode.Tab),
                "操作パネル表示切り替え (トグル)");

            cameraFPSModeToggle = conf.Bind(
                section,
                "cameraFPSModeToggle",
                new KeyboardShortcut(KeyCode.F),
                "夜伽時一人称視点切り替え");

            manHeadChange = conf.Bind(
                section,
                "manHeadChange",
                new KeyboardShortcut(KeyCode.R),
                "FPSの対象とする男切り替え(ループ)");

            speedDownModifier = conf.Bind(
                section,
                "speedDownModifier",
                CameraUtility.ModifierKey.Shift,
                "低速移動モード (押下中は移動速度が減少)\n設定値: Shift, Alt, Ctrl");

            initializeModifier = conf.Bind(
                section,
                "initializeModifier",
                CameraUtility.ModifierKey.Alt,
                "初期化モード (押下中に移動キーを押すと対象の軸が初期化)\n設定値: Shift, Alt, Ctrl");
        }
    }

    public class CameraConfig
    {
        public ConfigEntry<float> bgMoveSpeed;
        public ConfigEntry<float> bgRotateSpeed;

        public ConfigEntry<float> cameraRotateSpeed;
        public ConfigEntry<float> cameraFoVChangeSpeed;
        public ConfigEntry<float> speedMagnification;

        public ConfigEntry<float> fpsModeFoV;
        public ConfigEntry<float> fpsOffsetForward;
        public ConfigEntry<float> fpsOffsetUp;
        public ConfigEntry<float> fpsOffsetRight;
        public ConfigEntry<bool> fpsModeAuto;
        public ConfigEntry<float> fpsShakeCorrectionFactor;
        public ConfigEntry<float> vrFpsShakeCorrectionFactor;

        public ConfigEntry<bool> enableFpsCameraDirectionLock;
        public ConfigEntry<bool> enableVrFpsCamera;
        public ConfigEntry<bool> enableVrFpsCameraDirectionLock;

        public CameraConfig(ConfigFile conf)
        {
            var section = "General";

            bgMoveSpeed = conf.Bind(
                section, 
                "bgMoveSpeed",
                3f,
                "背景 移動速度");

            bgRotateSpeed = conf.Bind(
                section,
                "bgRotateSpeed",
                120f,
                "背景(メイド) 回転速度");

            cameraRotateSpeed = conf.Bind(
                section,
                "cameraRotateSpeed",
                60f,
                "カメラ 回転速度");

            cameraFoVChangeSpeed = conf.Bind(
                section,
                "cameraFoVChangeSpeed",
                15f,
                "視野 変更速度");

            speedMagnification = conf.Bind(
                section,
                "speedMagnification",
                0.1f,
                "低速移動モード倍率");

            fpsModeFoV = conf.Bind(
                section,
                "fpsModeFoV",
                60f,
                "FPSモード 視野");

            fpsOffsetForward = conf.Bind(
                section,
                "fpsOffsetForward",
                0.02f,
                "FPSモード カメラ位置調整 前後\n"
                       + "(カメラ位置を男の目の付近にするには、以下の数値を設定する)\n"
                       + "(メイドが男の喉あたりを見ているため視線が合わない場合がある)\n"
                       + "  fpsOffsetForward = 0.1\n"
                       + "  fpsOffsetUp = 0.12");

            fpsOffsetUp = conf.Bind(
                section,
                "fpsOffsetUp",
                -0.06f,
                "FPSモード カメラ位置調整 上下");

            fpsOffsetRight = conf.Bind(
                section,
                "fpsOffsetRight",
                0f,
                "FPSモード カメラ位置調整 左右");

            fpsModeAuto = conf.Bind(
                section,
                "fpsModeAuto",
                false,
                "Automatically enter first person mode when available for the scene");

            fpsShakeCorrectionFactor = conf.Bind(
                section,
                "fpsShakeCorrectionFactor",
                0.9f,
                "Shake correction factor to use when shake correction is toggled");

            vrFpsShakeCorrectionFactor = conf.Bind(
                section,
                "vrFpsShakeCorrectionFactor",
                0.9f,
                "Shake correction factor to use when shake correction in VR is toggled");

            enableFpsCameraDirectionLock = conf.Bind(
                section,
                "enableFpsCameraDirectionLock",
                false,
                "Lock FPS camera to the direction of the current man head");

            enableVrFpsCamera = conf.Bind(
                section,
                "enableVrFpsCamera",
                false,
                "Enable usage of FPS Camera mode in VR. WARNING: May cause nausea");

            enableVrFpsCameraDirectionLock = conf.Bind(
                section,
                "enableVrFpsCameraDirectionLock",
                false,
                "Lock FPS camera to the direction of the current man head in VR. WARNING: May cause even more nausea");
        }
    }

    public class ModuleConfig
    {
        public ConfigEntry<bool> firstPersonCamera;
        public ConfigEntry<bool> vrFirstPersonControl;
        public ConfigEntry<bool> lookAtThis;
        public ConfigEntry<bool> floorMover;
        public ConfigEntry<bool> extendedCameraControl;
        public ConfigEntry<bool> hideUI;

        public ModuleConfig(ConfigFile conf)
        {
            var section = "modules";

            firstPersonCamera = conf.Bind(
                section,
                "firstPersonCamera",
                true,
                "First person camera mode");

            vrFirstPersonControl = conf.Bind(
                section,
                "vrFirstPersonControl",
                false,
                "First person camera controls in VR");

            lookAtThis = conf.Bind(
                section,
                "lookAtThis",
                true,
                "LookAtThis controls");

            floorMover = conf.Bind(
                section,
                "floorMover",
                true,
                "Floor mover");

            extendedCameraControl = conf.Bind(
                section,
                "extendedCameraControl",
                true,
                "More camera control utilities");

            hideUI = conf.Bind(
                section,
                "hideUI",
                true,
                "UI control");
        }
    }

    /// <summary>CM3D2.CameraUtility.Plugin 設定ファイル</summary>
    class CameraUtilityConfig
    {
        public CameraConfig Camera { get; private set; }
        public KeyConfig Keys { get; private set; }

        public ModuleConfig Modules { get; private set; }

        public CameraUtilityConfig(ConfigFile conf)
        {
            Keys = new KeyConfig(conf);
            Camera = new CameraConfig(conf);
            Modules = new ModuleConfig(conf);
        }
    }

}