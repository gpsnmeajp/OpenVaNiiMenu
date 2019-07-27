/*
VaNiiMenu

Copyright (c) 2018, gpsnmeajp
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using EasyLazyLibrary;

public class MenuManager : MonoBehaviour {
    [SerializeField]
    private EasyOpenVROverlayForUnity EOVRO;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private Transform MainScreen;
    [SerializeField]
    private RectTransform SideMenuRect;
    [SerializeField]
    private AudioManagerScript AudioMan;
    [SerializeField]
    private SideMenuAnimatorScript SideMenu;
    [SerializeField]
    private GlobalOnClickManagerScript GlobalOnClick;
    [SerializeField]
    private IndicatorManagerScript Indicator;
    [SerializeField]
    private LauncherManagerScript Launcher;
    [SerializeField]
    private CamepraPostProcessWorkerScript CameraPostProcessForMainCamera;
    [SerializeField]
    private CamepraPostProcessWorkerScript CameraPostProcessForOverlayCamera;
    [SerializeField]
    private GameObject SideMenuObject;    //サイドメニューオブジェクト
    [SerializeField]
    private WindowAspectWorkerScript WindowAspect;
    [SerializeField]
    private EnableManagerScript EnableManager;
    [SerializeField]
    private Text FPSMonitor;
    [SerializeField]
    private ResolutionManagerScript ResoMan;

    public bool MenuReset = false; //ONにするとリセットされる
    public bool MenuStart = false; //ONにすると開始処理が走る
    public bool MenuEnd = false; //ONにすると終了処理が走る

    public bool IsRightHand; //右手で呼び出されたとき

    public int MenuPage; //現在の画面番号
    private int OldMenuPage; //過去の画面番号(内部保存用)

    public bool busy = false; //処理を受け付けない時間

    public bool showing = false; //表示中フラグ
    private bool dialogShowing = false;

    private bool isScreenMoving = false;
    private bool screenMoveWithRight = false;

    private bool isInTutorial = false;
    private float TutorialTimer = 0f;

    private bool FixedPosition = false;

    private double fps = 90f;

    public int errorCounter = 0;

    //ログ管理
    Application.LogCallback LogHandlerDeligate;

    EasyOpenVRUtil util = new EasyOpenVRUtil();
    RectTransform canvasrect;

    float canvasdutation = 0.3f; //アニメーション速度
    //-----------------------------

    const string welcomeFile = "config\\welcome2.txt";
    const string AllowUpdateInfoFile = "config\\AllowUpdateInfo.txt";


    //-----------------------------

    const int jsonVerMaster = 2; //設定ファイルバージョン
    const string jsonPath = "config\\Menu.json";
    MenuConfig config = null; //読み込まれた設定

    [Serializable]
    class MenuConfig
    {
        public bool hideHome = false; //ホーム画面非表示
        public Vector3 OverlayPosition = new Vector3(0.03f, -0.25f, 0.5f);
        public Vector3 OverlayRotation = new Vector3(-20f, 0, 0);
        public int skin = 0;

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
        var json = JsonUtility.ToJson(new MenuConfig());
        //初期設定ファイルを書き込み
        File.WriteAllText(jsonPath, json, new UTF8Encoding(false));
    }

    //ディレクトリ存在チェック
    public void DirectoryCheck()
    {
        if (!Directory.Exists("config"))
        {
            Directory.CreateDirectory("config");
        }
    }

    public void loadJSON()
    {
        config = null;
        DirectoryCheck();

        //ファイルがない場合: 初期設定ファイルの生成
        if (!File.Exists(jsonPath))
        {
            config = new MenuConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<MenuConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                this.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f,
                    () => {
                        //OK
                        makeJSON();
                    }, () => {
                        //キャンセル
                    });
                config = new MenuConfig();
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
            config = new MenuConfig();
            this.ShowDialogOKCancel(LanguageManager.config.jsonloaders.CORRUPT_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.CORRUPT_CONFIG_BODY, 3f,
            () => {
                //OK
                makeJSON();
            }, () => {
                //キャンセル
            });
        }
    }


    //ログ中に例外やエラーを検出したらダイアログとして表示する(できれば)
    //https://www.urablog.xyz/entry/2017/04/25/195351
    private void LogHandler(string cond, string stack, LogType type)
    {
        try
        {
            if (string.IsNullOrEmpty(cond))
            {
                return;
            }
            switch (type)
            {
                case LogType.Assert:
                    break;
                case LogType.Error:
                    ShowDialogOK("Error", cond, 0.05f, () => { });
                    errorCounter++;
                    break;
                case LogType.Exception:
                    ShowDialogOK("Exception", cond, 0.05f, () => { });
                    errorCounter++;
                    break;
                case LogType.Log:
                    break;
                case LogType.Warning:
                    ShowDialogOK("Warning", cond, 0.05f, () => { });
                    errorCounter++;
                    break;
            }

            //エラーが450回起きたら緊急停止する
            if (errorCounter > 450) {
                new EasyOpenVRUtil().ApplicationQuit();
            }
        }
        catch (Exception e)
        {
            //連鎖防止の為Do noting
        }
    }

    //初回起動チェック
    void WelcomeCheck()
    {
        //初回起動か判定
        if (!File.Exists(welcomeFile))
        {
            
            //10秒後にWelcomeを表示
            isInTutorial = true; //見失い防止処理起動

            //操作説明
            ShowDialogOK(LanguageManager.config.tutorial.TUTORIAL1_TITLE, LanguageManager.config.tutorial.TUTORIAL1_BODY, 10f, () => {
                isInTutorial = false; //見失い防止処理停止
                //操作説明
                ShowDialogOK(LanguageManager.config.tutorial.TUTORIAL2_TITLE, LanguageManager.config.tutorial.TUTORIAL2_BODY, 0.5f, () =>
                {
                    //機能説明
                    ShowDialogOK(LanguageManager.config.tutorial.TUTORIAL3_TITLE, LanguageManager.config.tutorial.TUTORIAL3_BODY, 0.5f, () =>
                    {
                        //Discord案内
                        ShowDialogOKCancel(LanguageManager.config.tutorial.TUTORIAL4_TITLE, LanguageManager.config.tutorial.TUTORIAL4_BODY_1+ "https://discord.gg/QSrDhE8" + LanguageManager.config.tutorial.TUTORIAL4_BODY_2, 0.5f, () =>
                        {
                            Launcher.Launch("https://discord.gg/QSrDhE8", "", "");
                            WelcomeStartup();
                        }, () => {
                            WelcomeStartup();
                        });
                    });
                });
            });
        }
    }
    //スタートアップ設定
    void WelcomeStartup()
    {
        ShowDialogOKCancel(LanguageManager.config.tutorial.TUTORIAL_STARTUP_TITLE, LanguageManager.config.tutorial.TUTORIAL_STARTUP_BODY, 0.5f, () =>
        {
            Launcher.Launch("VRAutorunHelper.exe", "", "");
            WelcomeOnline();
        }, () => {
            WelcomeOnline();
        });
    }

    //オンライン機能
    void WelcomeOnline()
    {
        //Discord案内
        ShowDialogOKCancel(LanguageManager.config.tutorial.TUTORIAL_ONLINE_TITLE, LanguageManager.config.tutorial.TUTORIAL_ONLINE_BODY, 0.5f, () =>
        {
            Launcher.Launch("VaNiiTweetHelper\\VaNiiTweetHelper.exe", "", "");
            WelcomeTranslatorAndEnd();
        }, () => {
            WelcomeTranslatorAndEnd();
        });
    }


    void WelcomeTranslatorAndEnd()
    {
        string TRANSLATOR_INFO =
            LanguageManager.config.Language+" "+ LanguageManager.config.Version +"\n\n" +
            "Translated by " + LanguageManager.config.Author + "\n" +
            "" + LanguageManager.config.URLorMail + "\n\n" +
            "" + LanguageManager.config.Comment + "";

        //翻訳者情報
        ShowDialogOK(LanguageManager.config.tutorial.TUTORIAL_TRANSLATOR_TITLE, TRANSLATOR_INFO, 0.5f, () =>
        {
            //OKを押すと以後表示しない
            ShowDialogOKCancel(LanguageManager.config.tutorial.TUTORIAL_END_TITLE, LanguageManager.config.tutorial.TUTORIAL_END_BODY, 0.5f, () =>
            {
                //OKを押すと以後表示しない
                File.WriteAllText(welcomeFile, "", System.Text.Encoding.UTF8);
            }, () => { });
        });
    }

    //スキンを変更する
    public void ChangeSkin(int skin)
    {
        config.skin = skin;
        saveJSON();
        ApplySkin();
    }
    //スキンを適用する
    public void ApplySkin()
    {
        if (config.skin == 1)
        {
            CameraPostProcessForMainCamera.enable = true;
            CameraPostProcessForOverlayCamera.enable = true;
        }
        else {
            CameraPostProcessForMainCamera.enable = false;
            CameraPostProcessForOverlayCamera.enable = false;
        }
    }

    private void Awake()
    {
        //ログハンドラを初期化
        LogHandlerDeligate = new Application.LogCallback(LogHandler);
        Application.logMessageReceived += LogHandlerDeligate;
        errorCounter = 0;
    }

    void Start()
    {
        //ログハンドラを初期化
        Application.logMessageReceived -= LogHandlerDeligate;
        Application.logMessageReceived += LogHandlerDeligate;

        MenuPage = 0;
        OldMenuPage = -1;

        canvasrect = canvas.GetComponent<RectTransform>();
        MenuResetFunc();

        dialogShowing = false;
        isInTutorial = false; //見失い防止処理停止

        WelcomeCheck();
        loadJSON();
        ApplySkin();

        //翻訳ファイル読み込み失敗
        if (LanguageManager.loadfailed) {
            //すぐに出す
            ShowDialogOK("Language Pack load failed", "", 3, () => { });
            //チュートリアル等に隠されてももう一度出す
            ShowDialogOK("Language Pack load failed", "", 60, () => { });
        }

        //サブインスタンスなら既定で表示
        if (Environment.GetCommandLineArgs().Length >= 3)
        {
            if (Environment.GetCommandLineArgs()[1] == "overlaykey")
            {
                //少し遅れさせないとあらぬところに出る(出てしまったのでもっと遅らせる)
                DOVirtual.DelayedCall(10f, () => {
                    MenuStart = true;
                    //setFixedPosition(true);

                    //強制的に位置固定にする(でないと出し直して大変なことになるので)
                    DOVirtual.DelayedCall(15f, () =>
                    {
                        setFixedPosition(true);
                    });
                });
            }
        }

#if UNITY_EDITOR
                MenuStart = true; //デバッグ用
#endif
    }

    void Update() {
        ValueWatcher();
        //System.GC.Collect();

        //FPSモニタ
        if (showing && !busy)
        {
            fps = fps * 0.99f + (1f / Time.deltaTime) * 0.01f;
            FPSMonitor.text = String.Format("{0:###}", 1f / Time.deltaTime);

            //よほどのことがない限り下がらないfpsが一気に下がったらメニューを強制的に閉じる
            if (fps < 15f) //表示時で15fps以下
            {
                MenuEndFunc(0);
            }
        }
        else {
            fps = 90; //閉じてるときは90扱いにする
        }


        //初期化されていないとき初期化する
        if (!util.IsReady())
        {
            util.Init();
            return;
        }

        //チュートリアル中の見失い防止処理
        //初期位置から動いてないときは自動で再配置する
        if (isInTutorial || EOVRO.Position == Vector3.zero) {
            TutorialTimer += Time.deltaTime;
            if (TutorialTimer > 10f) {
                TutorialTimer = 0;
                setPosition();
            }
        }

        //移動モード
        if (isScreenMoving) {
            ulong button = 0;
            EasyOpenVRUtil.Transform pos = util.GetHMDTransform();
            EasyOpenVRUtil.Transform cpos = null;
            if (screenMoveWithRight)
            {
                if (util.GetControllerButtonPressed(util.GetRightControllerIndex(), out button))
                {
                    cpos = util.GetRightControllerTransform();
                }
            }
            else {
                if (util.GetControllerButtonPressed(util.GetLeftControllerIndex(), out button))
                {
                    cpos = util.GetLeftControllerTransform();
                }
            }
            if (button == 0)
            {
                AudioMan.PlayApplySound();
                isScreenMoving = false;
            }

            if (pos != null && cpos != null)
            {
                var z = 0;
                Vector3 ang = (cpos.rotation * Quaternion.AngleAxis(45,Vector3.right)).eulerAngles;
                //常にこっちに向き、ゆっくり追従する
                Vector3 BillboardPosition = cpos.position; //これが難しい...
                Vector3 BillboardRotation = new Vector3(-ang.x, -ang.y, ang.z); //こっち向く。これでオッケー

                EOVRO.Position = BillboardPosition;
                EOVRO.Rotation = BillboardRotation;
            }
        }
    }

    public void MoveMode(bool dialog) {
        //コントローラのボタン入力と取得可能かをチェック
        ulong Leftbutton, Rightbutton;
        if (util.GetControllerButtonPressed(util.GetLeftControllerIndex(), out Leftbutton))
        {
            if (Leftbutton != 0) {
                isScreenMoving = true;
                screenMoveWithRight = false;
                AudioMan.PlayApplySound();
                return;
            }
        }
        if (util.GetControllerButtonPressed(util.GetRightControllerIndex(), out Rightbutton))
        {
            if (Rightbutton != 0)
            {
                isScreenMoving = true;
                screenMoveWithRight = true;
                AudioMan.PlayApplySound();
                return;
            }
        }

        AudioMan.PlayCancelSound();
        if (dialog)
        {
            ShowDialogOK(LanguageManager.config.showdialog.MOVESCREEN_TITLE, LanguageManager.config.showdialog.MOVESCREEN_BODY, 0.2f,
                () =>
                {
                }
            );
        }
    }

    public void hideHome(bool isHide)
    {
        config.hideHome = isHide;
        saveJSON();
    }

    //新しいページに遷移する
    void MenuPageNoUpdate(int no)
    {
        try
        {
            int i = 0;
            while (PageEnable(i++, false)) ; //それ以外を無効にする
        }
        catch (Exception e)
        {
            //Do noting
            ShowDialogOK("Exception", e.ToString(), 0.3f, () => { MenuPage = 0; });
            Debug.LogError(e);
        }

        //スタイリッシュホームを適用
        StylishHomeApply();

        try
        {
            PageEnable(no, true); //指定したページを有効にする
        }
        catch (Exception e)
        {
            //Do noting
            ShowDialogOK("Exception",e.ToString(), 0.3f,() => { MenuPage = 0; });
            Debug.LogError(e);
        }
    }

    //ページを有効化・無効化する(もっといい方法がある気がする...)
    bool PageEnable(int no, bool active)
    {
        switch (no) {
            case -1:
                return true;
            case 0:
                ObjectSetActive("WelcomePage", active);
                return true;
            case 1:
                ObjectSetActive("LauncherPage", active);
                return true;
            case 2:
                ObjectSetActive("MusicPage", active);
                return true;
            case 3:
                ObjectSetActive("MiscPage", active);
                return true;
            case 4:
                ObjectSetActive("SettingsPage", active);
                return true;
            case 5:
                ObjectSetActive("LockSettingsPage", active);
                return true;
            case 6:
                ObjectSetActive("HandSelectSettiongsPage", active);
                return true;
            case 7:
                ObjectSetActive("SEVolumeSettingsPage", active);
                return true;
            case 8:
                ObjectSetActive("HomeSettingsPage", active);
                return true;
            case 9:
                ObjectSetActive("ScreenSettingsPage", active);
                return true;
            case 10:
                ObjectSetActive("OpenCloseSettingsPage", active);
                return true;
            case 11:
                ObjectSetActive("MiscTextViewer", active);
                return true;
            case 12:
                ObjectSetActive("MiscImageViewer", active);
                return true;
            case 13:
                ObjectSetActive("MiscDesktopViewer", active);
                return true;
            case 14:
                ObjectSetActive("MiscOSCRemote", active);
                return true;
            case 15:
                ObjectSetActive("MiscFunctionKey", active);
                return true;
            case 16:
                ObjectSetActive("MiscDebug", active);
                return true;
            case 17:
                ObjectSetActive("MiscClockAlarm", active);
                return true;
            case 18:
                ObjectSetActive("MiscWindowViewer", active);
                return true;
            case 19:
                ObjectSetActive("MiscTimeline", active);
                return true;
            case 20:
                ObjectSetActive("MiscTweet", active);
                return true;
            default:
                return false; //もうページ無いよ！
        }
    }

    public void ObjectSetActive(string name, bool active)
    {
        MainScreen.Find(name).gameObject.SetActive(active);
    }

    //------------------ダイアログ管理------------------------------

    public void ShowDialogOK(string MainText,string SubText,float delay,UnityAction OkCallback)
    {
        DOVirtual.DelayedCall(delay, () =>
        {
            //もしダイアログ表示時にシステムが非表示状態なら強制的に表示する
            if (!showing)
            {
                MenuStart = true;
            }
            else
            {
                AudioMan.PlayNotificationSound();
            }
            //ダイアログ表示がHideHomeに隠されないようにする
            MainScreen.DOScale(1f, canvasdutation);
            ApplyHandPosToSideMenu();

            //ダイアログ表示がデスクトップ画面に隠れないようにする(すべてに優先されるため)
            WindowAspect.ShowBackground();
            ObjectSetActive("MiscDesktopViewer", false);
            ObjectSetActive("MiscWindowViewer", false);
            //フルスクリーンで消されたサイドメニューを復活させる
            SideMenuObject.transform.DOScale(1f, 0.5f);

            MainScreen.Find("DialogOK").transform.Find("MainText").gameObject.GetComponent<Text>().text = MainText;
            MainScreen.Find("DialogOK").transform.Find("SubText").gameObject.GetComponent<Text>().text = SubText;
            GlobalOnClick.DialogOkCallback = OkCallback;

            MainScreen.Find("DialogOK").GetComponent<RectTransform>().DOScale(1f, 0.1f);
            ObjectSetActive("DialogOK", true);
            dialogShowing = true;
        });
    }
    public void CloseDialogOK() {
        DOTween.Sequence()
            .Append(
                MainScreen.Find("DialogOK").GetComponent<RectTransform>().DOScale(0.0f, 0.1f)
            )
            .AppendCallback(() =>
            {
                ObjectSetActive("DialogOK", false);
                dialogShowing = false;
            })
            .Play();
    }

    public void ShowDialogOKCancel(string MainText, string SubText, float delay, UnityAction OkCallback, UnityAction CancelCallback)
    {
        DOVirtual.DelayedCall(delay, () =>
        {
            //もしダイアログ表示時にシステムが非表示状態なら強制的に表示する
            if (!showing)
        {
            MenuStart = true;
        }
        else
        {
            AudioMan.PlayQuestionSound();
        }
            //ダイアログ表示がHideHomeに隠されないようにする
            MainScreen.DOScale(1f, canvasdutation);
            ApplyHandPosToSideMenu();

            //ダイアログ表示がデスクトップ画面に隠れないようにする(すべてに優先されるため)
            WindowAspect.ShowBackground();
            ObjectSetActive("MiscDesktopViewer", false);
            ObjectSetActive("MiscWindowViewer", false);
            //フルスクリーンで消されたサイドメニューを復活させる
            SideMenuObject.transform.DOScale(1f, 0.5f);

            MainScreen.Find("DialogOKCancel").transform.Find("MainText").gameObject.GetComponent<Text>().text = MainText;
            MainScreen.Find("DialogOKCancel").transform.Find("SubText").gameObject.GetComponent<Text>().text = SubText;

            GlobalOnClick.DialogOkCallback = OkCallback;
        GlobalOnClick.DialogCancelCallback = CancelCallback;

        MainScreen.Find("DialogOKCancel").GetComponent<RectTransform>().DOScale(1f, 0.1f);
        ObjectSetActive("DialogOKCancel", true);
        dialogShowing = true;
        });
    }
public void CloseDialogOKCancel()
    {
        DOTween.Sequence()
            .Append(
                MainScreen.Find("DialogOKCancel").GetComponent<RectTransform>().DOScale(0.0f, 0.1f)
            )
            .AppendCallback(() =>
            {
                ObjectSetActive("DialogOKCancel", false);
                dialogShowing = false;
            })
            .Play();
    }

    //----------------------座標設定------------------------------------
    public void setPosition() {
        var pos = util.GetHMDTransform();
        if (pos == null) {
            return; //更新しない
        }

        var z = 0;


        //常にこっちに向き、ゆっくり追従する
        Vector3 BillboardPosition = pos.position + pos.rotation * config.OverlayPosition; //これが難しい...
        Vector3 BillboardRotation = (new Vector3(-pos.rotation.eulerAngles.x, -pos.rotation.eulerAngles.y, z)) + config.OverlayRotation; //こっち向く。これでオッケー

        EOVRO.Position = BillboardPosition;
        EOVRO.Rotation = BillboardRotation;

    }

    public void setFixedPosition(bool enable) {
        FixedPosition = enable;
    }

    //----------------------表示関係処理----------------------------------

    //グローバル変数を監視する
    void ValueWatcher()
    {
        //RESETは優先
        if (MenuReset)
        {
            MenuReset = false;
            MenuResetFunc();
        }

        //それ以外は無効時間がある
        if (!busy)
        {
            if (MenuStart && !showing)
            {
                MenuStartFunc();
            }
            if (MenuEnd && showing)
            {
                MenuEndFunc(0);
            }
            if (MenuPage != OldMenuPage)
            {
                OldMenuPage = MenuPage;
                MenuPageNoUpdate(MenuPage);
            }
        }
    }

    //初期化する
    void MenuResetFunc()
    {
        MenuReset = false;
        MenuStart = false;
        MenuEnd = false;
        busy = false;
        showing = false;

        //初期位置に移動
        if (!dialogShowing)
        {
            MainScreen.Find("DialogOK").GetComponent<RectTransform>().DOScale(0.0f, 0.1f);
            MainScreen.Find("DialogOKCancel").GetComponent<RectTransform>().DOScale(0.0f, 0.1f);
        }
        DOTweenModuleUI.DOAnchorPosY(canvasrect, 500, canvasdutation);

        //デスクトップ画面が無効にされているなら
        if (MainScreen.Find("MiscDesktopViewer").gameObject.activeInHierarchy == false)
        {
            //フルスクリーンで消されたサイドメニューを復活させる
            SideMenuObject.transform.DOScale(1f, 0.5f);
        }
    }

    //右手左手をサイドメニューに反映する
    void ApplyHandPosToSideMenu() {
        if (IsRightHand)
        {
            MainScreen.GetComponent<RectTransform>().DOAnchorPosX(-306, 0.2f);
            SideMenuRect.DOAnchorPosX(854, 0.2f);
        }
        else
        {
            MainScreen.GetComponent<RectTransform>().DOAnchorPosX(0, 0.2f);
            SideMenuRect.DOAnchorPosX(0, 0.2f);
        }
        Indicator.right = IsRightHand;
    }

    void StylishHomeApply() {
        //Homeを隠すが有効なとき
        if (config.hideHome)
        {
            if (MenuPage == 0)
            {
                MainScreen.DOScale(0, canvasdutation);
                SideMenuObject.transform.DOScale(1f, 0.5f); //隠されていたサイドメニューは表示する
                //中央寄りに表示
                if (IsRightHand)
                {
                    SideMenuRect.DOAnchorPosX(854 - 200, 0.2f);
                }
                else
                {
                    SideMenuRect.DOAnchorPosX(0 + 200, 0.2f);
                }
                return;
            }
            else
            {
                MainScreen.DOScale(1f, canvasdutation);
                ApplyHandPosToSideMenu();
            }
        }
    }

    //UI表示開始処理をする
    void MenuStartFunc()
    {
        canvasrect.anchoredPosition = new Vector3(0, 500, 0);

        EnableManager.Enable(); //処理を有効化
        EOVRO.show = true; //オーバーレイ有効
        AudioMan.PlayOpenSound();
        //fpsを上げる
        ResoMan.HighFPS();

        //画面位置固定機能が有効の場合は位置を修正しない
        if (!FixedPosition) {
            setPosition();
        }
        //スタイリッシュホームを適用
        StylishHomeApply();

        busy = true;
        showing = true;
        MenuEnd = false;

        DOTween.Sequence()
            .Append(
                DOTweenModuleUI.DOAnchorPosY(canvasrect, 0, canvasdutation)
            )
            .AppendCallback(() =>
            {
                SideMenu.down = true;
            })
            .AppendInterval(0.2f) //Wait
            .AppendCallback(() =>
            {
                //ホームを隠すが有効でないときだけ
                if (!config.hideHome)
                {
                    ApplyHandPosToSideMenu();
                }
            })
            .AppendInterval(0.2f) //Wait
            .AppendCallback(() =>
            {
                MenuStart = false;
                MenuEnd = false;
                busy = false;
            })
            .Play();
    }
    //UI表示終了処理をする
    public void MenuEndFunc(int mode)
    {
        //すでに閉じてます
        if (!showing || busy)
        {
            return;
        }

        //0=Normal, 1=Left, 2=Right
        AudioMan.PlayCloseSound();

        MenuEnd = true;
        busy = true;

        TweenCallback callback = () =>
        {
            EnableManager.Disable(); //処理を無効化
            EOVRO.show = false; //オーバーレイ無効
            SideMenu.up = true; //左右の場合はここで閉じる

            //fpsを下げる
            ResoMan.LowFPS();

            MenuResetFunc();
        };

        var seq = DOTween.Sequence();

        if (mode == 1)
        {
            //Left
            seq.Append(
                DOTweenModuleUI.DOAnchorPosX(canvasrect, -260, canvasdutation)
            );
        }
        else if (mode == 2)
        {
            //Rgiht
            seq.Append(
                DOTweenModuleUI.DOAnchorPosX(canvasrect, +260, canvasdutation)
            );
        }
        else
        {
            //通常
            SideMenu.up = true; //格納してから
            seq.AppendInterval(0.3f); //Wait side menu
            seq.Append(
                DOTweenModuleUI.DOAnchorPosY(canvasrect, 150, canvasdutation)
            );
        }
        seq.AppendInterval(0.5f); //Wait
        seq.AppendCallback(callback);
        seq.Append(
            DOTweenModuleUI.DOAnchorPosY(canvasrect, 0, canvasdutation) //スワイプアウトのあともとに戻す

        );
        seq.Play();
    }
}
