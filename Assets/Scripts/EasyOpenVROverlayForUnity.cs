/**
 * EasyOpenVROverlayForUnity by gpsnmeajp v0.23
 * 2018/10/28
 * 
 * v0.23 Side-by-Side 3D対応
 *
 * v0.22 デバイスアップデート
 *  デバイス情報を取得して一覧をログに出すように
 *  選択したデバイスの詳細情報を取得できるように
 *  トラッカーやベースステーションの位置にオーバーレイを出せるように
 *  Tagに対するWarnningsを抑制
 * 
 * v0.21 微修正
 * v0.2 大規模更新
 *  デバッグタグの方法を変更
 *  uGUIのクリックに対応
 *  コントローラーを選択できるように
 *  外部からのエラーチェック、表示状態管理関数を追加。
 *  各処理を関数化
 *  終了時に開放する処理を追加
 *  エラー時に開放する処理を追加
 *  マウススケールの処理を追加
 *  終了イベントのキャッチを追加
 * v0.1 公開 2018/08/25
 * 
 * 2DのテクスチャをVR空間にオーバーレイ表示します。
 * 現在動作中のアプリケーションに関係なくオーバーレイすることができます。
 * 
 * 入力機能は正常に動作していないようなので省いています。
 * ダッシュボードオーバーレイは省略しています。
 *  *
 * These codes are licensed under CC0.
 * http://creativecommons.org/publicdomain/zero/1.0/deed.ja
 */

/** uGUIのクリックについて
 * 
 * 使い方
 * 1. LaycastRootObjectには、操作したいCanvas(シーン直下に配置)を設定
 * 2. Buttonのクリックだけ対応(コントローラーの先端でOverlayを叩くとクリック)
 * 3. ButtonのRaycast TargetはONに。ButtonのTextにあるRaycast TargetはOFFに
 * 4. CanvasのRender Modeは"Screen Space - Camera"に。
 * 5. CanvasのRender Cameraは、RenderTextureを設定したCameraと同じものにすること
 * なお、LaycastRootObjectをnull(None)にするとGUI機能は無効化される
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using Valve.VR; //Steam VR

public class EasyOpenVROverlayForUnity : MonoBehaviour
{
    //エラーフラグ
    public bool error = true; //初期化失敗
    //イベントに関するログを表示するか
    public bool eventLog = false;
    [Header("Menu Manager")]
    public MenuManager menu;
    public bool tapEnable = true;
    public Text InstanceKeyText;

    [Header("RenderTexture")]
    //取得元のRenderTexture
    public RenderTexture renderTexture;

    [Header("Transform")]
    //Unity準拠の位置と回転
    public Vector3 Position = new Vector3(0, 0, 0.5f);
    public Vector3 Rotation = new Vector3(0, 0, 0);
    public Vector3 Scale = new Vector3(1, 1, 1);
    //鏡像反転できるように
    public bool MirrorX = false;
    public bool MirrorY = false;

    [Header("Setting")]
    //表示するか否か
    public bool show = false;
    private bool oldshow = false;

    //サイドバイサイド3D
    public bool SideBySide = false;

    //ユーザーが確認するためのオーバーレイの名前
    public string OverlayFriendlyName = "VaNiiMenu";

    //グローバルキー(システムのオーバーレイ同士の識別名)。
    //ユニークでなければならない。乱数やUUIDなどを勧める
    public string OverlayKeyName = "VaNiiMenu";

    [Header("DeviceTracking")]
    //絶対空間か
    public bool DeviceTracking = false;

    //追従対象デバイス。HMD=0
    //public uint DeviceIndex = OpenVR.k_unTrackedDeviceIndex_Hmd;
    public TrackingDeviceSelect DeviceIndex = TrackingDeviceSelect.None;
    private int DeviceIndexOld = (int)TrackingDeviceSelect.None;

    [Header("Absolute space")]
    //(絶対空間の場合)ルームスケールか、着座状態か
    public bool Seated = false;

    //着座カメラのリセット(リセット後自動でfalseに戻ります)
    public bool ResetSeatedCamera = false;

    //追従対象リスト。コントロラーは変動するので特別処理
    public enum TrackingDeviceSelect
    {
        None = -99,
        RightController = -2,
        LeftController = -1,
        HMD = (int)OpenVR.k_unTrackedDeviceIndex_Hmd,
        Device1 = 1,
        Device2 = 2,
        Device3 = 3,
        Device4 = 4,
        Device5 = 5,
        Device6 = 6,
        Device7 = 7,
        Device8 = 8,
    }

    //--------------------------------------------------------------------------

    [Header("Device Info")]
    //現在接続されているデバイス一覧をログに出力(自動でfalseに戻ります)
    public bool putLogDevicesInfo = false;
    //(デバイスを選択した時点で)現在接続されているデバイス数
    public int ConnectedDevices = 0;
    //選択デバイス番号
    public int SelectedDeviceIndex = 0;
    //選択デバイスのシリアル番号
    public string DeviceSerialNumber = null;
    //選択デバイスのモデル名
    public string DeviceRenderModelName = null;


    [Header("GUI Tap")]
    //レイキャスト対象識別用ルートCanvasオブジェクト
    public GameObject LaycastRootObject = null;

    //タップ状態管理
    public bool tappedLeft = false;
    public bool tappedRight = false;

    //カーソル位置表示用変数
    public float LeftHandU = -1f;
    public float LeftHandV = -1f;
    public float LeftHandDistance = -1f;
    public float RightHandU = -1f;
    public float RightHandV = -1f;
    public float RightHandDistance = -1f;

    //右手か左手か
    enum LeftOrRight
    {
        Left = 0,
        Right = 1
    }

    //--------------------------------------------------------------------------

    //オーバーレイのハンドル(整数)
    private ulong overlayHandle = INVALID_HANDLE;

    //OpenVRシステムインスタンス
    private CVRSystem openvr = null;

    //Overlayインスタンス
    private CVROverlay overlay = null;

    //オーバーレイに渡すネイティブテクスチャ
    private Texture_t overlayTexture;

    //HMD視点位置変換行列
    private HmdMatrix34_t p;

    //無効なハンドル
    private const ulong INVALID_HANDLE = 0;

    //-----------------------------

    const int jsonVerMaster = 2; //設定ファイルバージョン
    const string jsonPath = "config\\Overlay.json";
    public OverlayConfig config = null; //読み込まれた設定

    [Serializable]
    public class OverlayConfig
    {
        //ユーザーが確認するためのオーバーレイの名前
        public string OverlayFriendlyName = "VaNiiMenu";

        //グローバルキー(システムのオーバーレイ同士の識別名)。
        //ユニークでなければならない。乱数やUUIDなどを勧める
        public string OverlayKeyName = "VaNiiMenu";

        //オーバーレイの大きさ設定(幅のみ。高さはテクスチャの比から自動計算される)
        public float width = 0.6f;

        //オーバーレイの透明度を設定
        public float alpha = 1.0f;

        //タップ距離
        public float TapOnDistance = 0.04f;
        public float TapOffDistance = 0.043f;

        public float widthPreset1 = 0.3f;
        public float widthPreset2 = 0.6f;
        public float widthPreset3 = 1.2f;

        public int jsonVer = jsonVerMaster; //設定ファイルバージョン
    }

    public void saveJSON()
    {
        //設定ファイルを書き込み
        var json = JsonUtility.ToJson(config);
        File.WriteAllText(jsonPath, json, new UTF8Encoding(false));
    }

    public void makeJSON()
    {
        //初期設定ファイルを生成
        var json = JsonUtility.ToJson(new OverlayConfig());
        //初期設定ファイルを書き込み
        File.WriteAllText(jsonPath, json, new UTF8Encoding(false));
    }

    public void loadJSON()
    {
        config = null;
        menu.DirectoryCheck();

        //ファイルがない場合: 初期設定ファイルの生成
        if (!File.Exists(jsonPath))
        {
            config = new OverlayConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<OverlayConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f, () => {
                        //OK
                        makeJSON();
                    }, () => {
                        //キャンセル
                    });
                config = new OverlayConfig();
            }
        }
        catch (System.Exception e)
        {
            //JSONデコードに失敗した場合
            Debug.Log(e.ToString());
            config = null;
        }

        //デコード失敗した場合は、デフォルト設定にして警告
        if (config == null)
        {
            config = new OverlayConfig();
            menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.CORRUPT_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.CORRUPT_CONFIG_BODY, 3f, () => {
                //OK
                makeJSON();
            }, () => {
                //キャンセル
            });
        }
    }

    //大きくする(操作不能になることもあるので保存しない)
    public void upWidth()
    {
        config.width += 0.1f;
        overlay.SetOverlayWidthInMeters(overlayHandle, config.width);
    }
    //小さくする(操作不能になることもあるので保存しない)
    public void downWidth()
    {
        config.width -= 0.1f;
        if (config.width < 0.1f)
        {
            config.width = 0.1f; //消滅防止
        }
        overlay.SetOverlayWidthInMeters(overlayHandle, config.width);
    }

    //プリセットを呼び出す
    public void WidthPreset(int no)
    {
        float width = 0.6f;
        if (no == 1)
        {
            width = config.widthPreset1;
        }
        if (no == 2)
        {
            width = config.widthPreset2;
        }
        if (no == 3)
        {
            width = config.widthPreset3;
        }
        config.width = width;
        overlay.SetOverlayWidthInMeters(overlayHandle, config.width);
    }

    //--------------------------------------------------------------------------

    //外部から透明度設定切り替え
    public void setAlpha(float a)
    {
        config.alpha = a;
    }

    //外部からdevice切り替え
    public void changeToHMD()
    {
        DeviceIndex = TrackingDeviceSelect.HMD;
    }
    //外部からdevice切り替え
    public void changeToLeftController()
    {
        DeviceIndex = TrackingDeviceSelect.LeftController;
    }
    //外部からdevice切り替え
    public void changeToRightController()
    {
        DeviceIndex = TrackingDeviceSelect.RightController;
    }

    //--------------------------------------------------------------------------

    //Overlayが表示されているかどうか外部からcheck
    public bool IsVisible()
    {
        return overlay.IsOverlayVisible(overlayHandle) && !IsError();
    }

    //エラー状態かをチェック
    public bool IsError()
    {
        return error || overlayHandle == INVALID_HANDLE || overlay == null || openvr == null;
    }

    //エラー処理(開放処理)
    private void ProcessError()
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219
        Debug.Log(Tag + "Begin");

        //ハンドルを解放
        if (overlayHandle != INVALID_HANDLE && overlay != null)
        {
            overlay.DestroyOverlay(overlayHandle);
        }

        overlayHandle = INVALID_HANDLE;
        overlay = null;
        openvr = null;
        error = true;
    }

    //オブジェクト破棄時
    private void OnDestroy()
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219
        Debug.Log(Tag + "Begin");

        //ハンドル類の全開放
        ProcessError();
    }

    //アプリケーションの終了を検出した時
    private void OnApplicationQuit()
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219
        Debug.Log(Tag + "Begin");

        //ハンドル類の全開放
        ProcessError();
    }

    //アプリケーションを終了させる
    private void ApplicationQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    //--------------------------------------------------------------------------

    //初期化処理
    private void Start()
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219
        Debug.Log(Tag + "Begin");

        var openVRError = EVRInitError.None;
        var overlayError = EVROverlayError.None;
        error = false;
        oldshow = !show;

        //フレームレートを90fpsにする。(しないと無限に早くなることがある)
        Application.targetFrameRate = 90;
        Debug.Log(Tag + "Set Frame Rate 90");

        //JSON読み込み
        Debug.Log(Tag + "Load JSON");
        loadJSON();

        InstanceKeyText.text = "";
        //複数起動用キー
        if (Environment.GetCommandLineArgs().Length >= 3)
        {
            Debug.Log(Environment.GetCommandLineArgs()[1]);
            Debug.Log(Environment.GetCommandLineArgs()[2]);

            //引数に合わせて一時的にKeyを変更する
            if (Environment.GetCommandLineArgs()[1] == "overlaykey")
            {
                config.OverlayKeyName = Environment.GetCommandLineArgs()[2];
                config.OverlayFriendlyName = Environment.GetCommandLineArgs()[2];

                //サブインスタンス表示
                InstanceKeyText.text = config.OverlayKeyName;
            }
        }

        //表示用
        OverlayKeyName = config.OverlayKeyName;
        OverlayFriendlyName = config.OverlayFriendlyName;


        //OpenVRの初期化
        openvr = OpenVR.Init(ref openVRError, EVRApplicationType.VRApplication_Overlay);
        if (openVRError != EVRInitError.None)
        {
            Debug.LogError(Tag + "OpenVRの初期化に失敗." + openVRError.ToString());

            //エラーを告知し5秒後に終了
            DOVirtual.DelayedCall(5f, () => {
                ApplicationQuit();
            });

            ProcessError();
            return;
        }

        //オーバーレイ機能の初期化
        overlay = OpenVR.Overlay;
        overlayError = overlay.CreateOverlay(config.OverlayKeyName, config.OverlayFriendlyName, ref overlayHandle);
        if (overlayError != EVROverlayError.None)
        {
            Debug.LogError(Tag + "Overlayの初期化に失敗. " + overlayError.ToString());

            //エラーを告知し5秒後に終了
            DOVirtual.DelayedCall(5f, () => {
                ApplicationQuit();
            });

            ProcessError();
            return;
        }

        //オーバーレイに渡すテクスチャ種類の設定
        var OverlayTextureBounds = new VRTextureBounds_t();
        var isOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
        if (isOpenGL)
        {
            //pGLuintTexture
            overlayTexture.eType = ETextureType.OpenGL;
            //上下反転しない
            OverlayTextureBounds.uMin = 1;
            OverlayTextureBounds.vMin = 0;
            OverlayTextureBounds.uMax = 1;
            OverlayTextureBounds.vMax = 0;
            overlay.SetOverlayTextureBounds(overlayHandle, ref OverlayTextureBounds);
        }
        else
        {
            //pTexture
            overlayTexture.eType = ETextureType.DirectX;
            //上下反転する
            OverlayTextureBounds.uMin = 0;
            OverlayTextureBounds.vMin = 1;
            OverlayTextureBounds.uMax = 1;
            OverlayTextureBounds.vMax = 0;
            overlay.SetOverlayTextureBounds(overlayHandle, ref OverlayTextureBounds);
        }

        //--------
        showDevices();

        Debug.Log(Tag + "初期化完了しました");
    }

    //すべての処理後
    private void LateUpdate()
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

        //エラーが発生した場合や、ハンドルが無効な場合は実行しない
        if (IsError())
        {
            return;
        }

        if (show != oldshow)
        {
            oldshow = show;
        }
        if (show)
        {
            //オーバーレイを表示する
            overlay.ShowOverlay(overlayHandle);
        }
        else
        {
            //オーバーレイを非表示にする
            overlay.HideOverlay(overlayHandle);
        }

        //イベントを処理する(終了された時true)
        if (ProcessEvent())
        {
            Debug.Log(Tag + "VRシステムが終了されました");
            ApplicationQuit();
        }

        //オーバーレイが表示されている時
        if (show && overlay.IsOverlayVisible(overlayHandle))
        {
            //位置情報と各種設定の更新
            updatePosition();
            //表示情報の更新
            updateTexture();

            //Canvasが設定されている場合
            if (LaycastRootObject != null)
            {
                //GUIタッチ機能の処理
                updateVRTouch();
            }
        }

        if (putLogDevicesInfo)
        {
            showDevices();
            putLogDevicesInfo = false;
        }
    }

    //位置情報を更新
    private void updatePosition()
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

        //RenderTextureが生成されているかチェック
        if (!renderTexture.IsCreated())
        {
            Debug.Log(Tag + "RenderTextureがまだ生成されていない");
            return;
        }

        //回転を生成
        Quaternion quaternion = Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z);
        //座標系を変更(右手系と左手系の入れ替え)
        Vector3 position = Position;
        position.z = -Position.z;
        //HMD視点位置変換行列に書き込む。
        Matrix4x4 m = Matrix4x4.TRS(position, quaternion, Scale);

        //鏡像反転
        Vector3 Mirroring = new Vector3(MirrorX ? -1 : 1, MirrorY ? -1 : 1, 1);

        //4x4行列を3x4行列に変換する。
        p.m0 = Mirroring.x * m.m00; p.m1 = Mirroring.y * m.m01; p.m2 = Mirroring.z * m.m02; p.m3 = m.m03;
        p.m4 = Mirroring.x * m.m10; p.m5 = Mirroring.y * m.m11; p.m6 = Mirroring.z * m.m12; p.m7 = m.m13;
        p.m8 = Mirroring.x * m.m20; p.m9 = Mirroring.y * m.m21; p.m10 = Mirroring.z * m.m22; p.m11 = m.m23;

        //回転行列を元に相対位置で表示
        if (DeviceTracking)
        {
            //deviceindexを処理(コントローラーなどはその時その時で変わるため)
            var idx = OpenVR.k_unTrackedDeviceIndex_Hmd;
            switch (DeviceIndex)
            {
                case TrackingDeviceSelect.LeftController:
                    idx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
                    break;
                case TrackingDeviceSelect.RightController:
                    idx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
                    break;
                default:
                    idx = (uint)DeviceIndex;
                    break;
            }
            //device情報に変化があったらInspectorに反映
            if (DeviceIndexOld != (int)idx)
            {
                Debug.Log(Tag + "Device Updated");
                UpdateDeviceInfo(idx);
                DeviceIndexOld = (int)idx;
            }

            //HMDからの相対的な位置にオーバーレイを表示する。
            overlay.SetOverlayTransformTrackedDeviceRelative(overlayHandle, idx, ref p);
        }
        else
        {
            //空間の絶対位置にオーバーレイを表示する
            if (!Seated)
            {
                overlay.SetOverlayTransformAbsolute(overlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref p);
            }
            else
            {
                overlay.SetOverlayTransformAbsolute(overlayHandle, ETrackingUniverseOrigin.TrackingUniverseSeated, ref p);
            }
        }

        if (ResetSeatedCamera)
        {
            OpenVR.System.ResetSeatedZeroPose();
            ResetSeatedCamera = false;
        }

        //オーバーレイの大きさ設定(幅のみ。高さはテクスチャの比から自動計算される)
        overlay.SetOverlayWidthInMeters(overlayHandle, config.width);

        //オーバーレイの透明度を設定
        overlay.SetOverlayAlpha(overlayHandle, config.alpha);

        //マウスカーソルスケールを設定する(これにより表示領域のサイズも決定される)
        try
        {
            HmdVector2_t vecMouseScale = new HmdVector2_t
            {
                v0 = renderTexture.width,
                v1 = renderTexture.height
            };
            overlay.SetOverlayMouseScale(overlayHandle, ref vecMouseScale);
        }
        catch (UnassignedReferenceException e)
        {
            Debug.LogError(Tag + "RenderTextureがセットされていません " + e.ToString());
            ProcessError();
            return;
        }

    }

    //表示情報を更新
    private void updateTexture()
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

        overlay.SetOverlayFlag(overlayHandle, VROverlayFlags.SideBySide_Parallel, SideBySide);

        //RenderTextureが生成されているかチェック
        if (!renderTexture.IsCreated())
        {
            Debug.Log(Tag + "RenderTextureがまだ生成されていない");
            return;
        }

        //RenderTextureからネイティブテクスチャのハンドルを取得
        try
        {
            overlayTexture.handle = renderTexture.GetNativeTexturePtr();
        }
        catch (UnassignedReferenceException e)
        {
            Debug.LogError(Tag + "RenderTextureがセットされていません " + e.ToString());
            ProcessError();
            return;
        }

        //オーバーレイにテクスチャを設定
        var overlayError = EVROverlayError.None;
        overlayError = overlay.SetOverlayTexture(overlayHandle, ref overlayTexture);
        if (overlayError != EVROverlayError.None)
        {
            Debug.LogError(Tag + "Overlayにテクスチャをセットできませんでした. " + overlayError.ToString());
            //致命的なエラーとしない
            return;
        }

    }

    //終了イベントをキャッチした時に戻す
    private bool ProcessEvent()
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

        //イベント構造体のサイズを取得
        uint uncbVREvent = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));

        //イベント情報格納構造体
        VREvent_t Event = new VREvent_t();
        //イベントを取り出す
        while (overlay.PollNextOverlayEvent(overlayHandle, ref Event, uncbVREvent))
        {
            //イベントのログを表示
            if (eventLog)
            {
                Debug.Log(Tag + "Event:" + ((EVREventType)Event.eventType).ToString());
            }

            //イベント情報で分岐
            switch ((EVREventType)Event.eventType)
            {
                case EVREventType.VREvent_Quit:
                    Debug.Log(Tag + "Quit");
                    return true;
            }
        }
        return false;
    }


    //----------おまけ(deviceの詳細情報)-------------

    //全てのdeviceの情報をログに出力する
    private void showDevices()
    {
#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

        //すべてのdeviceの接続状態を取得
        TrackedDevicePose_t[] allDevicePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        openvr.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, allDevicePose);

        //接続されているdeviceの数をカウントする
        uint connectedDeviceNum = 0;
        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (allDevicePose[i].bDeviceIsConnected)
            {
                connectedDeviceNum++;
            }
        }

        //deviceの詳細情報を1つづつ読み出す
        uint connectedDeviceCount = 0;
        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            //接続中だったら、読み取り完了数を1増やす
            if (GetPropertyAndPutLog(i, allDevicePose))
            {
                connectedDeviceCount++;
            }
            //接続されている数だけ読み取り終わったら終了する
            if (connectedDeviceCount >= connectedDeviceNum)
            {
                break;
            }
        }
    }

    //deviceの情報をログに出力する(1項目)
    private bool GetPropertyAndPutLog(uint idx, TrackedDevicePose_t[] allDevicePose)
    {
#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

        //接続されているかをチェック
        if (allDevicePose[idx].bDeviceIsConnected)
        {
            //接続されているdevice

            //デバイスシリアル番号(Trackerの識別によく使う)と、deviceモデル名(device種類)を取得
            string s1 = GetProperty(idx, ETrackedDeviceProperty.Prop_SerialNumber_String);
            string s2 = GetProperty(idx, ETrackedDeviceProperty.Prop_RenderModelName_String);
            if (s1 != null && s2 != null)
            {
                //ログに表示
                Debug.Log(Tag + "Device " + idx + ":" + s1 + " : " + s2);
            }
            else
            {
                //何らかの理由で取得失敗した
                Debug.Log(Tag + "Device " + idx + ": Error");
            }
            return true;
        }
        else
        {
            //接続されていないdevice
            Debug.Log(Tag + "Device " + idx + ": Not connected");
            return false;
        }
    }

    //指定されたdeviceの情報をInspectorに反映する
    private void UpdateDeviceInfo(uint idx)
    {
        //すべてのdeviceの接続状態を取得
        TrackedDevicePose_t[] allDevicePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        openvr.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, allDevicePose);

        //接続されているdeviceの数をカウントする
        ConnectedDevices = 0;
        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (allDevicePose[i].bDeviceIsConnected)
            {
                ConnectedDevices++;
            }
        }

        //deviceの情報をInspectorに反映する
        SelectedDeviceIndex = (int)idx;
        DeviceSerialNumber = GetProperty(idx, ETrackedDeviceProperty.Prop_SerialNumber_String);
        DeviceRenderModelName = GetProperty(idx, ETrackedDeviceProperty.Prop_RenderModelName_String);
    }

    //device情報を取得する
    private string GetProperty(uint idx, ETrackedDeviceProperty prop)
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

        ETrackedPropertyError error = new ETrackedPropertyError();
        //device情報を取得するのに必要な文字数を取得
        uint size = openvr.GetStringTrackedDeviceProperty(idx, prop, null, 0, ref error);
        if (error != ETrackedPropertyError.TrackedProp_BufferTooSmall)
        {
            return null;
        }

        StringBuilder s = new StringBuilder();
        s.Length = (int)size; //文字長さ確保
        //device情報を取得する
        openvr.GetStringTrackedDeviceProperty(idx, prop, s, size, ref error);
        if (error != ETrackedPropertyError.TrackedProp_Success)
        {
            return null;
        }
        return s.ToString();
    }


    //----------おまけ(コントローラーでOverlayを叩いてuGUIをクリックできるやつ)-------------

    //uGUIクリックを実現する
    private void updateVRTouch()
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

        //コントローラのindex

        //VR接続されているすべてのデバイスの情報を取得
        TrackedDevicePose_t[] allDevicePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        openvr.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, allDevicePose);

        //Overlayのレイ走査結果を格納する変数
        VROverlayIntersectionResults_t results = new VROverlayIntersectionResults_t();

        /*
        //視線による操作
        uint Hmdidx = OpenVR.k_unTrackedDeviceIndex_Hmd;
        if (checkRay(Hmdidx, allDevicePose, ref results))
        {
            parent.setCursorPosition(results, LeftOrRight.Right, channel);
            Debug.Log(DEBUG_TAG + "HMD u:"+results.vUVs.v0+" v:"+ results.vUVs.v1+" d:"+results.fDistance);
        }
        */
        //左手コントローラーの情報取得
        uint Leftidx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
        if (checkRay(Leftidx, allDevicePose, ref results))
        {
            //線上にオーバーレイがある場合は続けて処理
            CheckTapping(results, LeftOrRight.Left, ref tappedLeft);

            //カーソル表示用に更新
            LeftHandU = results.vUVs.v0 * renderTexture.width;
            LeftHandV = renderTexture.height - results.vUVs.v1 * renderTexture.height;
            LeftHandDistance = results.fDistance;
        }
        else
        {
            LeftHandU = -1f;
            LeftHandV = -1f;
            LeftHandDistance = -1f;
        }
        //右手コントローラーの情報取得
        uint Rightidx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
        if (checkRay(Rightidx, allDevicePose, ref results))
        {
            //線上にオーバーレイがある場合は続けて処理
            CheckTapping(results, LeftOrRight.Right, ref tappedRight);

            //カーソル表示用に更新
            RightHandU = results.vUVs.v0 * renderTexture.width;
            RightHandV = renderTexture.height - results.vUVs.v1 * renderTexture.height;
            RightHandDistance = results.fDistance;
        }
        else
        {
            RightHandU = -1f;
            RightHandV = -1f;
            RightHandDistance = -1f;
        }

    }

    //指定されたdeviceが有効かチェックした上で、オーバーレイと交点を持つかチェック
    private bool checkRay(uint idx, TrackedDevicePose_t[] allDevicePose, ref VROverlayIntersectionResults_t results)
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

        //device indexが有効
        if (idx != OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            //接続されていて姿勢情報が有効
            if (allDevicePose[idx].bDeviceIsConnected && allDevicePose[idx].bPoseIsValid)
            {
                //姿勢情報などを変換してもらう
                TrackedDevicePose_t Pose = allDevicePose[idx];
                SteamVR_Utils.RigidTransform Trans = new SteamVR_Utils.RigidTransform(Pose.mDeviceToAbsoluteTracking);

                //コントローラー用に45度前方に傾けた方向ベクトルを計算
                Vector3 vect = (Trans.rot * Quaternion.AngleAxis(45, Vector3.right)) * Vector3.forward;

                return ComputeOverlayIntersection(Trans.pos, vect, ref results);
            }
        }
        return false;
    }

    //オーバーレイと交点を持つかチェック
    private bool ComputeOverlayIntersection(Vector3 pos, Vector3 rotvect, ref VROverlayIntersectionResults_t results)
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

        //レイ照射情報
        VROverlayIntersectionParams_t param = new VROverlayIntersectionParams_t();
        //レイ発射元位置
        param.vSource = new HmdVector3_t
        {
            v0 = pos.x,
            v1 = pos.y,
            v2 = -pos.z //右手系 to 左手系
        };
        //レイ発射単位方向ベクトル
        param.vDirection = new HmdVector3_t
        {
            v0 = rotvect.x,
            v1 = rotvect.y,
            v2 = -rotvect.z //右手系 to 左手系
        };
        //ルーム空間座標系で照射
        param.eOrigin = ETrackingUniverseOrigin.TrackingUniverseStanding;

        //Overlayと交差していればtrue、していなければfalseで、詳細情報がresultsに入る
        return overlay.ComputeOverlayIntersection(overlayHandle, ref param, ref results);
    }

    //タップされているかどうかを調べる
    private void CheckTapping(VROverlayIntersectionResults_t results, LeftOrRight lr, ref bool tapped)
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

        //コントローラとオーバーレイの距離が一定以下なら
        if (results.fDistance < config.TapOnDistance && !tapped)
        {
            //タップされた
            tapped = true;
            haptic(lr);

            //クリック処理(完全ロックでない場合)
            if (tapEnable)
            {
                uGUIclick(results);
            }
        }
        //コントローラとオーバーレイの距離が一定以上なら
        if (results.fDistance > config.TapOffDistance && tapped)
        {
            //離れた
            tapped = false;
            haptic(lr);
        }
    }

    //振動フィードバックを行う
    private void haptic(LeftOrRight lr)
    {

#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

        //左手コントローラーが有効かチェック
        uint Leftidx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
        if (Leftidx != OpenVR.k_unTrackedDeviceIndexInvalid && lr == LeftOrRight.Left)
        {
            //ぶるっと
            openvr.TriggerHapticPulse(Leftidx, 0, 3000);
        }
        //右手コントローラーが有効かチェック
        uint Rightidx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
        if (Rightidx != OpenVR.k_unTrackedDeviceIndexInvalid && lr == LeftOrRight.Right)
        {
            //ぶるっと
            openvr.TriggerHapticPulse(Rightidx, 0, 3000);
        }
    }

    //Canvas上の要素を特定してクリックする
    private void uGUIclick(VROverlayIntersectionResults_t results)
    {
#pragma warning disable 0219
        string Tag = "[" + this.GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

        //クリック用uv座標を計算
        float u = results.vUVs.v0 * renderTexture.width;
        float v = renderTexture.height - results.vUVs.v1 * renderTexture.height;

        //Canvas上のレイキャストのために座標をセット
        Vector2 ScreenPoint = new Vector2(u, v);
        PointerEventData pointer = new PointerEventData(EventSystem.current)
        {
            position = ScreenPoint
        };

        //レイキャスト結果格納用リストを確保
        List<RaycastResult> result = new List<RaycastResult>();

        //RaycastAllはレイキャスターを叩く。
        //CanvasについていたりCameraについていたりするすべてのレイキャスターを叩く(要らないものは切っておくとよい)
        EventSystem.current.RaycastAll(pointer, result);

        //検出した要素の数と座標

        Debug.Log(Tag + "count:" + result.Count + " u:" + u + " / v:" + v);

        //一番最初に見つけた要素にクリック処理を行う
        for (int i = 0; i < result.Count; i++)
        {
            var res = result[i];

            //一番最初に引っ掛けたものを叩く(target以外はcheckを外しておく)
            if (res.isValid)
            {
                Debug.Log(Tag + res.gameObject.name + " at " + res.gameObject.transform.root.name);
                //対象にしたいルートオブジェクトの子かを調べる
                if (res.gameObject.transform.root.name == LaycastRootObject.name)
                {
                    ExecuteEvents.Execute(res.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                    break;
                }
            }
        }
    }


}