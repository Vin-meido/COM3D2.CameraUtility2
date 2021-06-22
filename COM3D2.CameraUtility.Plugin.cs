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
    [BepInPlugin("org.bepinex.plugins.com3d2.camerautility2", "CameraUtility2", "1.0.0.0")]
    public class CameraUtility : BaseUnityPlugin
    {
        #region Constants

        /// <summary>CM3D2のシーンリスト</summary>
        private enum Scene
        {
            /// <summary>メイド選択(夜伽、品評会の前など)</summary>
            SceneCharacterSelect = 1,

            /// <summary>品評会</summary>
            SceneCompetitiveShow = 2,

            /// <summary>昼夜メニュー、仕事結果</summary>
            SceneDaily = 3,

            /// <summary>ダンス1(ドキドキ Fallin' Love)</summary>
            SceneDance_DDFL = 4,

            /// <summary>メイドエディット</summary>
            SceneEdit = 5,

            /// <summary>メーカーロゴ</summary>
            SceneLogo = 6,

            /// <summary>メイド管理</summary>
            SceneMaidManagement = 7,

            /// <summary>ショップ</summary>
            SceneShop = 8,

            /// <summary>タイトル画面</summary>
            SceneTitle = 9,

            /// <summary>トロフィー閲覧</summary>
            SceneTrophy = 10,

            /// <summary>Chu-B Lip 夜伽</summary>
            SceneYotogi_ChuB = 10,

            /// <summary>？？？</summary>
            SceneTryInfo = 11,

            /// <summary>主人公エディット</summary>
            SceneUserEdit = 12,

            /// <summary>起動時警告画面</summary>
            SceneWarning = 13,

            /// <summary>夜伽</summary>
            SceneYotogi = 14,

            /// <summary>ADVパート(kgスクリプト処理)</summary>
            SceneADV = 15,

            /// <summary>日付画面</summary>
            SceneStartDaily = 16,

            /// <summary>タイトルに戻る</summary>
            SceneToTitle = 17,

            /// <summary>MVP</summary>
            SceneSingleEffect = 18,

            /// <summary>スタッフロール</summary>
            SceneStaffRoll = 19,

            /// <summary>ダンス2(Entrance To You)</summary>
            SceneDance_ETY = 20,

            /// <summary>ダンス3(Scarlet Leap)</summary>
            SceneDance_SL = 22,

            /// <summary>回想モード</summary>
            SceneRecollection = 24,

            /// <summary>撮影モード</summary>
            ScenePhotoMode = 27,

            /// <summary>Guest mode</summary>
            SceneGuestMode = 53,

            /// <sumary>Scout mode</sumary>
            SceneScoutMode = 114
        }

        /// <summary>FPS モードを有効化するシーンリスト</summary>
        private static int[] EnableFpsScenes = {
            (int)Scene.SceneYotogi,
            (int)Scene.SceneADV,
            (int)Scene.SceneRecollection,
            (int)Scene.SceneGuestMode,
            (int)Scene.SceneScoutMode,
        };

        /// <summary>Chu-B Lip で FPS モードを有効化するシーンリスト</summary>
        private static int[] EnableFpsScenesChuB = {
            (int)Scene.SceneYotogi_ChuB,
        };

        /// <summary>Hide UI モードを有効化するシーンリスト</summary>
        private static int[] EnableHideUIScenes = {
            (int)Scene.SceneEdit,
            (int)Scene.SceneYotogi,
            (int)Scene.SceneADV,
            (int)Scene.SceneRecollection,
            (int)Scene.ScenePhotoMode,
        };

        /// <summary>Chu-B Lip で Hide UI モードを有効化するシーンリスト</summary>
        private static int[] EnableHideUIScenesChuB = {
            (int)Scene.SceneYotogi_ChuB,
        };

        /// <summary>モディファイアキー</summary>
        public enum ModifierKey
        {
            None = 0,
            Shift,
            Alt,
            Ctrl
        }

        /// <summary>状態変化チェック間隔</summary>
        private const float stateCheckInterval = 1f;

        #endregion
        #region Variables

        //設定
        private Config.CameraUtilityConfig config;

        //オブジェクト
        private Maid maid;
        private CameraMain mainCamera;
        private Transform mainCameraTransform;
        private Transform maidTransform;
        private Transform bg;
        private Man currentMan;
        private GameObject uiObject;
        private GameObject profilePanel;

        //状態フラグ
        private bool isOVR = false;
        private bool isChuBLip = false;
        private bool fpsMode = false;
        private bool fpsShakeCorrection = false;
        private bool eyetoCamToggle = false;
        private int eyeToCamIndex = 0;
        private bool uiVisible = true;
        private int sceneLevel;

        //状態退避変数
        private float defaultFoV = 35f;
        private Vector3 oldCameraPos;
        private Vector3 oldTargetPos;
        private float oldDistance;
        private float oldFoV;
        private Quaternion oldRotation;
        private int oldEyeToCamIndex;

        //コルーチン一覧
        private LinkedList<Coroutine> mainCoroutines = new LinkedList<Coroutine>();

        private Quaternion oldCameraRotation;
        private Vector3 oldCameraPosition;
        private bool CameraDirectionLock {
            get {
                return isOVR ? config.Camera.enableVrFpsCameraDirectionLock.Value : config.Camera.enableFpsCameraDirectionLock.Value;
            }
        }

        private int fpsUpOffset = 7;
        private int fpsForwardOffset = 1;

        public static CameraUtility Instance { get; private set; }
        #endregion
        #region Override Methods

        public void Awake()
        {
            GameObject.DontDestroyOnLoad(this);
            
            string path = Application.dataPath;
            isChuBLip = path.Contains("CM3D2OH");
            isOVR = GameMain.Instance.VRMode;
            config = new Config.CameraUtilityConfig(Config);
            Instance = this;
        }

        public void Start()
        {
        }

        public void OnLevelWasLoaded(int level)
        {
            sceneLevel = level;
            Log("OnLevelWasLoaded: {0}", sceneLevel);

            StopMainCoroutines();
            if (InitializeSceneObjects())
            {
                StartMainCoroutines();
            }
            //Man.VisibleAllManHead();
        }

        public void OnDestroy()
        {
            Log("OnDestroy");
        }

        #endregion
        #region Properties

        private Config.KeyConfig Keys
        {
            get
            {
                return config.Keys;
            }
        }

        private bool AllowUpdate
        {
            get
            {
                // 文字入力パネルがアクティブの場合 false
                return profilePanel == null || !profilePanel.activeSelf;
            }
        }

        #endregion
        #region Private Methods

        #region Methods for Main

        private bool InitializeSceneObjects()
        {
            // maid = GameMain.Instance.CharacterMgr.GetMaid(0);
            // maidTransform = maid ? maid.body0.transform : null;

            maid = null;
            maidTransform = null;

            bg = GameObject.Find("__GameMain__/BG").transform;
            mainCamera = GameMain.Instance.MainCamera;
            mainCameraTransform = mainCamera.gameObject.transform;
            Log("Main camera is {0}", mainCamera);

            currentMan = null;

            uiObject = GameObject.Find("/UI Root/Camera");
            if (uiObject == null)
            {
                uiObject = GameObject.Find("SystemUI Root/Camera");
            }
            defaultFoV = Camera.main.fieldOfView;

            if (sceneLevel == (int)Scene.SceneEdit)
            {
                GameObject uiRoot = GameObject.Find("/UI Root");
                profilePanel = uiRoot.transform.Find("ProfilePanel").gameObject;
            }
            else if (sceneLevel == (int)Scene.SceneUserEdit)
            {
                GameObject uiRoot = GameObject.Find("/UI Root");
                profilePanel = uiRoot.transform.Find("UserEditPanel").gameObject;
            }
            else
            {
                profilePanel = null;
            }

            fpsShakeCorrection = false;
            fpsMode = config.Camera.fpsModeAuto.Value;

            return bg && mainCamera;
        }

        private void AddCoroutine(IEnumerator routine)
        {
            mainCoroutines.AddLast(StartCoroutine(routine));
        }

        private void StartMainCoroutines()
        {
            AddCoroutine(MaidSelectCoroutine());

            // Start FirstPersonCamera
            if (config.Modules.firstPersonCamera.Value && (isChuBLip && EnableFpsScenesChuB.Contains(sceneLevel)) || EnableFpsScenes.Contains(sceneLevel))
            {
                if (!isOVR || config.Camera.enableVrFpsCamera.Value)
                {
                    Log("FPS Camera available!");
                    AddCoroutine(FirstPersonCameraManHeadSelectorCoroutine());
                    AddCoroutine(FirstPersonCameraManHeadAutoSelectCoroutine());
                    AddCoroutine(FirstPersonCameraNewPositionAdjustCoroutine());
                    AddCoroutine(FirstPersonCameraAutoExit());
                    AddCoroutine(FirstPersonCameraCoroutine());
                    AddCoroutine(FpsControlCoroutine());
                }
            }
            else
            {
                Log("FPS Camera not available for current scene level");
            }

            if (config.Modules.lookAtThis.Value)
            {
                // Start LookAtThis
                mainCoroutines.AddLast(StartCoroutine(LookAtThisCoroutine()));
            }

            if (config.Modules.floorMover.Value)
            {
                // Start FloorMover
                mainCoroutines.AddLast(StartCoroutine(FloorMoverCoroutine()));
            }

            if (config.Modules.extendedCameraControl.Value && !isOVR)
            {
                // Start ExtendedCameraHandle
                mainCoroutines.AddLast(StartCoroutine(ExtendedCameraHandleCoroutine()));
            }

            // Start HideUI
            if (config.Modules.hideUI.Value && ((isChuBLip && EnableHideUIScenesChuB.Contains(sceneLevel)) || EnableHideUIScenes.Contains(sceneLevel)))
            {
                if (!isOVR)
                {
                    mainCoroutines.AddLast(StartCoroutine(HideUICoroutine()));
                }
            }

            // VR Control shortcuts
            if(config.Modules.vrFirstPersonControl.Value && isOVR)
            {
                mainCoroutines.AddLast(StartCoroutine(VRControl.Coroutine()));
            }
        }

        private void StopMainCoroutines()
        {
            foreach (var coroutine in mainCoroutines)
            {
                StopCoroutine(coroutine);
            }
            mainCoroutines.Clear();
        }

        #endregion

        #region Methods for Misc

        private void Log(string format, params object[] args)
        {
            Logger.LogInfo(string.Format(format, args));
        }

        private bool IsModKeyPressing(ModifierKey key)
        {
            switch (key)
            {
                case ModifierKey.Shift:
                    return (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

                case ModifierKey.Alt:
                    return (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));

                case ModifierKey.Ctrl:
                    return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));

                default:
                    return false;
            }
        }

        private int GetFadeState()
        {
            Assert.IsNotNull(mainCamera);
            var field = mainCamera.GetType().GetField("m_eFadeState", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            return (int)field.GetValue(mainCamera);
        }
        #endregion

        #region Methods for FirstPersonCamera

        public void ToggleFirstPersonCameraMode()
        {
            var current = fpsMode;

            if (fpsShakeCorrection)
            {
                fpsShakeCorrection = false;
                fpsMode = false;
            }
            else if (fpsMode)
            {
                fpsShakeCorrection = true;
            }
            else if (config.Camera.fpsModeAuto.Value || Man.GetVisibleMan() != null)
            { 
                fpsMode = true;
            }
            else
            {
                Log("Unable to enter FpsMode, fpsModeAuto is off and no visible man");
            }

            Log("FpsMode = {0}, ShakeCorrection = {1}", fpsMode, fpsShakeCorrection);

            if (current && currentMan != null && !fpsMode)
            {
                Log("Exiting first person mode");
                ExitFpsMode();
            }
        }

        private void UpdateFirstPersonCamera(bool initial=false)
        {
            Assert.IsNotNull(currentMan);
            Assert.IsNotNull(mainCamera);
            Assert.IsNotNull(mainCameraTransform);

            Vector3 cameraPos = currentMan.HeadTransform.position
                + currentMan.HeadTransform.up * config.Camera.fpsOffsetUp.Value * fpsForwardOffset
                + currentMan.HeadTransform.right * config.Camera.fpsOffsetRight.Value
                + currentMan.HeadTransform.forward * config.Camera.fpsOffsetForward.Value * fpsUpOffset;

            var rotation = currentMan.LookRotation;

            if (initial)
            {
                bool vrLevelHorizon = !isOVR && !CameraDirectionLock;
                SetCameraPosition(cameraPos);
                SetCameraRotation(rotation, vrLevelHorizon);
                return;
            }


            if (fpsShakeCorrection)
            {
                var factor = isOVR ? config.Camera.vrFpsShakeCorrectionFactor.Value : config.Camera.fpsShakeCorrectionFactor.Value;
                cameraPos = Vector3.Lerp(GetCurrentCameraPosition(), cameraPos, factor);
                rotation = Quaternion.Lerp(GetCurrentCameraRotation(), rotation, factor);
            }

            SetCameraPosition(cameraPos);

            if(isOVR || CameraDirectionLock)
            {
                SetCameraRotation(rotation);
            }
        }

        private void SetCameraPosition(Vector3 position)
        {
            oldCameraPosition = position;
            if (isOVR)
            {
                mainCameraTransform.transform.localPosition = position;
            }
            else
            {
                mainCameraTransform.transform.position = position;
            }
        }

        private void SetCameraRotation(Quaternion rotation, bool vrLevelHorizon=false) {
            oldCameraRotation = rotation;
            if (vrLevelHorizon)
            {
                rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
            }

            if (isOVR)
            {
                mainCameraTransform.transform.localRotation = rotation;
            }
            else
            {
                mainCameraTransform.rotation = rotation;
            }
        }

        private void SetCameraFOV(float fov)
        {
            if (GameMain.Instance.VRMode) return;
            Camera.main.fieldOfView = fov;
        }

        public Quaternion GetCurrentCameraRotation()
        {
            return oldCameraRotation;
        }

        public Vector3 GetCurrentCameraPosition()
        {
            return oldCameraPosition;
        }

        private void ResetCamera()
        {
            mainCamera.SetFromScriptOnTarget(CameraMainPatch.lastTargetPos, CameraMainPatch.lastRadius, CameraMainPatch.lastAngle);
            if (!isOVR)
            {
                SetCameraFOV(defaultFoV);
            }
        }

        #endregion

        #region Methods for ExtendedCameraHandle

        private void UpdateCameraFoV()
        {
            if (Keys.cameraFoVInitialize.Value.IsDown())
            {
                Camera.main.fieldOfView = defaultFoV;
                return;
            }

            float fovChangeSpeed = config.Camera.cameraFoVChangeSpeed.Value * Time.deltaTime;
            if (IsModKeyPressing(Keys.speedDownModifier.Value))
            {
                fovChangeSpeed *= config.Camera.speedMagnification.Value;
            }

            if (Input.GetKey(Keys.cameraFoVMinus.Value.MainKey))
            {
                Camera.main.fieldOfView += -fovChangeSpeed;
            }
            if (Input.GetKey(Keys.cameraFoVPlus.Value.MainKey))
            {
                Camera.main.fieldOfView += fovChangeSpeed;
            }
        }

        private void UpdateCameraRotation()
        {
            Assert.IsNotNull(mainCameraTransform);

            if (Keys.cameraRollInitialize.Value.IsDown())
            {
                mainCameraTransform.eulerAngles = new Vector3(
                        mainCameraTransform.rotation.eulerAngles.x,
                        mainCameraTransform.rotation.eulerAngles.y,
                        0f);
                return;
            }

            float rotateSpeed = config.Camera.cameraRotateSpeed.Value * Time.deltaTime;
            if (IsModKeyPressing(Keys.speedDownModifier.Value))
            {
                rotateSpeed *= config.Camera.speedMagnification.Value;
            }

            if (Input.GetKey(Keys.cameraLeftRoll.Value.MainKey))
            {
                mainCameraTransform.Rotate(0, 0, rotateSpeed);
            }
            if (Input.GetKey(Keys.cameraRightRoll.Value.MainKey))
            {
                mainCameraTransform.Rotate(0, 0, -rotateSpeed);
            }
        }

        #endregion

        #region Methods for FloorMover

        private void UpdateBackgroudPosition()
        {
            Assert.IsNotNull(bg);

            if (Keys.bgInitialize.Value.IsDown())
            {
                bg.localPosition = Vector3.zero;
                bg.RotateAround(maidTransform.position, Vector3.up, -bg.rotation.eulerAngles.y);
                bg.RotateAround(maidTransform.position, Vector3.right, -bg.rotation.eulerAngles.x);
                bg.RotateAround(maidTransform.position, Vector3.forward, -bg.rotation.eulerAngles.z);
                bg.RotateAround(maidTransform.position, Vector3.up, -bg.rotation.eulerAngles.y);
                return;
            }

            if (IsModKeyPressing(Keys.initializeModifier.Value))
            {
                if (Input.GetKey(Keys.bgLeftRotate.Value.MainKey) || Input.GetKey(Keys.bgRightRotate.Value.MainKey))
                {
                    bg.RotateAround(maidTransform.position, Vector3.up, -bg.rotation.eulerAngles.y);
                }
                if (Input.GetKey(Keys.bgLeftRoll.Value.MainKey) || Input.GetKey(Keys.bgRightRoll.Value.MainKey))
                {
                    bg.RotateAround(maidTransform.position, Vector3.forward, -bg.rotation.eulerAngles.z);
                    bg.RotateAround(maidTransform.position, Vector3.right, -bg.rotation.eulerAngles.x);
                }
                if (Input.GetKey(Keys.bgLeftMove.Value.MainKey) || Input.GetKey(Keys.bgRightMove.Value.MainKey) || Input.GetKey(Keys.bgBackMove.Value.MainKey) || Input.GetKey(Keys.bgForwardMove.Value.MainKey))
                {
                    bg.localPosition = new Vector3(0f, bg.localPosition.y, 0f);
                }
                if (Input.GetKey(Keys.bgUpMove.Value.MainKey) || Input.GetKey(Keys.bgDownMove.Value.MainKey))
                {
                    bg.localPosition = new Vector3(bg.localPosition.x, 0f, bg.localPosition.z);
                }
                return;
            }
    
            Vector3 cameraForward = mainCameraTransform.TransformDirection(Vector3.forward);
            Vector3 cameraRight = mainCameraTransform.TransformDirection(Vector3.right);
            Vector3 cameraUp = mainCameraTransform.TransformDirection(Vector3.up);
            Vector3 direction = Vector3.zero;

            float moveSpeed = config.Camera.bgMoveSpeed.Value * Time.deltaTime;
            float rotateSpeed = config.Camera.bgRotateSpeed.Value * Time.deltaTime;
            if (IsModKeyPressing(Keys.speedDownModifier.Value))
            {
                moveSpeed *= config.Camera.speedMagnification.Value;
                rotateSpeed *= config.Camera.speedMagnification.Value;
            }

            if (Input.GetKey(Keys.bgLeftMove.Value.MainKey))
            {
                direction += new Vector3(cameraRight.x, 0f, cameraRight.z) * moveSpeed;
            }
            if (Input.GetKey(Keys.bgRightMove.Value.MainKey))
            {
                direction += new Vector3(cameraRight.x, 0f, cameraRight.z) * -moveSpeed;
            }
            if (Input.GetKey(Keys.bgBackMove.Value.MainKey))
            {
                direction += new Vector3(cameraForward.x, 0f, cameraForward.z) * moveSpeed;
            }
            if (Input.GetKey(Keys.bgForwardMove.Value.MainKey))
            {
                direction += new Vector3(cameraForward.x, 0f, cameraForward.z) * -moveSpeed;
            }
            if (Input.GetKey(Keys.bgUpMove.Value.MainKey))
            {
                direction += new Vector3(0f, cameraUp.y, 0f) * -moveSpeed; }
            if (Input.GetKey(Keys.bgDownMove.Value.MainKey))
            {
                direction += new Vector3(0f, cameraUp.y, 0f) * moveSpeed;
            }

            //bg.position += direction;
            bg.localPosition += direction;

            if (Input.GetKey(Keys.bgLeftRotate.Value.MainKey))
            {
                bg.RotateAround(maidTransform.transform.position, Vector3.up, rotateSpeed);
            }
            if (Input.GetKey(Keys.bgRightRotate.Value.MainKey))
            {
                bg.RotateAround(maidTransform.transform.position, Vector3.up, -rotateSpeed);
            }
            if (Input.GetKey(Keys.bgLeftRoll.Value.MainKey))
            {
                bg.RotateAround(maidTransform.transform.position, new Vector3(cameraForward.x, 0f, cameraForward.z), rotateSpeed);
            }
            if (Input.GetKey(Keys.bgRightRoll.Value.MainKey))
            {
                bg.RotateAround(maidTransform.transform.position, new Vector3(cameraForward.x, 0f, cameraForward.z), -rotateSpeed);
            }
        }

        #endregion

        #region Methods for EyeToCam

        private void SetEyeToCamIndex(int index)
        {
            Assert.IsNotNull(maid);

            var eyeMoveTypes = (Maid.EyeMoveType[])Enum.GetValues(typeof(Maid.EyeMoveType));
            eyeToCamIndex = index % eyeMoveTypes.Length;
            if (eyeToCamIndex < 0)
            {
                eyeToCamIndex += eyeMoveTypes.Length;
            }
            var eyeMoveType = eyeMoveTypes[eyeToCamIndex];
            maid.EyeToCamera(eyeMoveType, 0f);
            eyetoCamToggle = (eyeMoveType != Maid.EyeMoveType.無し);
            Log("EyeToCam = {0}, EyeMoveType = [{1}]{2}", eyetoCamToggle, eyeToCamIndex, eyeMoveType);
        }

        private void SetEyeToCamToggle(bool enable)
        {
            Assert.IsNotNull(maid);

            eyetoCamToggle = enable;
            var eyeMoveType = (eyetoCamToggle) ? Maid.EyeMoveType.目と顔を向ける : Maid.EyeMoveType.無し;
            maid.EyeToCamera(eyeMoveType, 0f);
            var eyeMoveTypes = (Maid.EyeMoveType[])Enum.GetValues(typeof(Maid.EyeMoveType));
            eyeToCamIndex = Array.IndexOf(eyeMoveTypes, eyeMoveType);
            Log("EyeToCam = {0}, EyeMoveType = [{1}]{2}", eyetoCamToggle, eyeToCamIndex, eyeMoveType);
        }

        #endregion

        #region Methods for HideUI

        private void ToggleUIVisible()
        {
            uiVisible = !uiVisible;
            if (uiObject)
            {
                uiObject.SetActive(uiVisible);
                Log("UIVisible:{0}", uiVisible);
            }
        }

        #endregion

        #endregion
        #region Coroutines

        private IEnumerator MaidSelectCoroutine()
        {
            while(maid == null)
            {
                var newMaid = GameMain.Instance.CharacterMgr.GetMaid(0);
                if (newMaid != null && newMaid.isActiveAndEnabled && newMaid.Visible) {
                    maid = newMaid;
                    maidTransform = maid.body0.transform;
                }

                yield return null;
            }
            Log("Found maid {0}", maid);
        }

        private bool FirstPersonCoroutinesEnabled
        {
            get
            {
                return AllowUpdate && (!isOVR || config.Camera.enableVrFpsCamera.Value) && maid != null;
            }
        }
        private void EnterFpsMode(Man newMan)
        {
            Assert.IsNotNull(newMan);

            if (currentMan != null)
            {
                currentMan.HeadVisible = true;
            }
            else
            {
                SetCameraFOV(config.Camera.fpsModeFoV.Value);
            }

            currentMan = newMan;
            currentMan.HeadVisible = false;
            UpdateFirstPersonCamera(true);
        }

        private void ExitFpsMode()
        {
            currentMan = null;
            Man.VisibleAllManHead();
            ResetCamera();
        }

        private IEnumerator FirstPersonCameraCoroutine()
        {
            while (true)
            {
                yield return null;
                if (!FirstPersonCoroutinesEnabled) continue;

                if (Keys.cameraFPSModeToggle.Value.IsDown())
                {
                    ToggleFirstPersonCameraMode();
                }

                if (fpsMode && currentMan != null) {
                    UpdateFirstPersonCamera();
                }
            }
        }

        private IEnumerator FirstPersonCameraManHeadSelectorCoroutine()
        {
            while(true)
            {
                yield return null;
                if (!FirstPersonCoroutinesEnabled || !fpsMode) continue;
                if (currentMan == null || !currentMan.Visible) continue;

                if (Keys.manHeadChange.Value.IsDown())
                {
                    var newMan = Man.GetVisibleMan(currentMan.ManNumber) ?? Man.GetVisibleMan();
                    if (newMan != null && newMan.ManNumber != currentMan.ManNumber)
                    {
                        Log("Switch man to {0} => {1}", currentMan, newMan);
                        EnterFpsMode(newMan);
                    }
                }
            }
        }

        private IEnumerator FirstPersonCameraAutoExit()
        {
            // exit first person view if the man turns invisible
            while (true)
            {
                yield return null;
                if (!FirstPersonCoroutinesEnabled || !fpsMode) continue;
                if (currentMan == null) continue;

                if (!currentMan.Visible)
                {
                    Log("Current man no longer visible {0}. Exiting first person view.", currentMan);
                    ExitFpsMode();
                }
            }
        }

        private IEnumerator FirstPersonCameraManHeadAutoSelectCoroutine()
        {
            while(true)
            {
                yield return null;
                if (!FirstPersonCoroutinesEnabled || !fpsMode) continue;
                if (currentMan != null) continue;

                var newMan = Man.GetVisibleMan();
                if (newMan != null)
                {
                    Log("Found and switching new man {0}", newMan);
                    EnterFpsMode(newMan);
                }
            }
        }

        private IEnumerator FirstPersonCameraNewPositionAdjustCoroutine()
        {
            Vector3 oldTargetFromScript = CameraMainPatch.lastTargetPos;

            while (true)
            {
                yield return null;
                if (!FirstPersonCoroutinesEnabled || !fpsMode) continue;

                Vector3 currentTargetFromScript = CameraMainPatch.lastTargetPos;
                if (currentTargetFromScript != oldTargetFromScript)
                {
                    oldTargetFromScript = currentTargetFromScript;

                    if (currentMan == null) continue;
                    if (CameraDirectionLock) continue;

                    Log("Position changed");
                    UpdateFirstPersonCamera(true);
                }
            }
        }

        private IEnumerator FloorMoverCoroutine()
        {
            while (true)
            {
                UpdateBackgroudPosition();
                yield return null;
            }
        }

        private IEnumerator ExtendedCameraHandleCoroutine()
        {
            while (true)
            {
                UpdateCameraFoV();
                UpdateCameraRotation();
                yield return null;
            }
        }

        private IEnumerator LookAtThisCoroutine()
        {
            while (true)
            {
                if (Keys.eyetoCamChange.Value.IsDown())
                {
                    SetEyeToCamIndex(eyeToCamIndex + 1);
                }
                if (Keys.eyetoCamToggle.Value.IsDown())
                {
                    SetEyeToCamToggle(!eyetoCamToggle);
                }
                yield return null;
            }
        }

        private IEnumerator HideUICoroutine()
        {
            while (true)
            {
                if (Keys.hideUIToggle.Value.IsDown())
                {
                    if (GetFadeState() == 0)
                    {
                        ToggleUIVisible();
                    }
                }
                yield return null;
            }
        }

        private IEnumerator FpsControlCoroutine()
        {
            while (true)
            {
                yield return null;

                if (!fpsMode) continue;

                var offset = 0;
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    offset = 1;
                }

                if (Input.GetKey(KeyCode.DownArrow))
                {
                    offset = -1;
                }
                if (offset != 0)
                {
                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        fpsUpOffset += offset;
                        Log("fpsOffset UP {0} FORWARD {1}", fpsUpOffset, fpsForwardOffset);
                    }
                    else
                    {
                        fpsForwardOffset += offset;
                        Log("fpsOffset UP {0} FORWARD {1}", fpsUpOffset, fpsForwardOffset);
                    }

                }

                if (Input.GetKeyDown(KeyCode.Home) && currentMan != null && currentMan.Visible)
                {
                    UpdateFirstPersonCamera(true);
                    Log("Realign camera to man head");
                }
            }
        }

        #endregion
    }

    #region Helper Classes

    public static class Assert
    {
        //[System.Diagnostics.Conditional("DEBUG")]
        public static void IsNotNull(object obj)
        {
            if (obj == null)
            {
                string msg = "Assertion failed. Value is null.";
                throw new Exception(msg);
                UnityEngine.Debug.LogError(msg);
            }
        }
    }

    #endregion
}

