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

public class WindowAspectWorkerScript : MonoBehaviour
{
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private Text TitleText;

    [SerializeField]
    private GameObject parentCanvas;
    [SerializeField]
    private GameObject parentMiscWindowViewer;
    [SerializeField]
    private GameObject MainScreen;
    [SerializeField]
    private GameObject SideMenu;

    //モニタ画面の一時大きさ
    Vector3 startScale;
    //モニタ画面の通常大きさ
    Vector3 startNormalScale;

    //位置復旧用
    Vector3 startPos;

    //デスクトップ全体or選択ウィンドウ
    bool desktopMode = true;

    //Enableワークアラウンド
    bool OnceEnable = false;

    //ズーム
    FreeLoupeForWindowWorkerScript freeLoupe;
    private float zoomRate = 1f;

    //切り替えindex
    public int windowIndex = 0;
    public int desktopIndex = 0;

    //画面サイズ比率(取得した際のモニタリング用)
    public float aspect = 0;
    public float Width = 0;
    public float Height = 0;

    public uWindowCapture.UwcWindowTexture winTex;

    bool started = false; //スタート済みか判定

    //-----------------------------

    const int jsonVerMaster = 2; //設定ファイルバージョン
    const string jsonPath = "config\\WindowViewer.json";
    WindowViewerConfig config = null; //読み込まれた設定

    [Serializable]
    class WindowViewerConfig
    {
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
        //初期設定を生成
        config = new WindowViewerConfig();

        //初期設定ファイルを生成
        var json = JsonUtility.ToJson(config);
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
            config = new WindowViewerConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<WindowViewerConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f, () => {
                        //OK
                        makeJSON();
                    }, () => {
                        //キャンセル
                    });
                config = new WindowViewerConfig();
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
            config = new WindowViewerConfig();
            menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.CORRUPT_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.CORRUPT_CONFIG_BODY, 3f, () => {
                //OK
                makeJSON();
            }, () => {
                //キャンセル
            });
        }
    }

    //背景を隠す
    public void HideBackground()
    {
        //初期化前には実行しない
        if (!started)
        {
            return;
        }

        //親を切り替え
        transform.SetParent(parentCanvas.transform);
        //余計なものを隠す
        SideMenu.transform.DOScale(0f, 0.1f);
        MainScreen.transform.DOScale(0f, 0.1f);
    }
    //背景を戻す
    public void ShowBackground()
    {
        //初期化前には実行しない
        if (!started)
        {
            return;
        }

        //親を戻し
        transform.SetParent(parentMiscWindowViewer.transform);
        //隠していたものを戻す
        SideMenu.transform.DOScale(1f, 0.1f);
        MainScreen.transform.DOScale(1f, 0.1f);

        //位置ずれ起こしたら戻しておく
        transform.localPosition = startPos;
        transform.localScale = startNormalScale;
        aspect = -1;
    }

    void Start()
    {
        
        winTex = GetComponent<uWindowCapture.UwcWindowTexture>();
        freeLoupe = GetComponent<FreeLoupeForWindowWorkerScript>();

        //Managerが初期化されていないのかデスクトップ表示の初期化に失敗するため遅延を入れている
        DOVirtual.DelayedCall(1f, () =>
        {
            DesktopMode();
        });

        loadJSON();

        started = true;
    }

    //画面が開かれた時にデスクトップに切り替える
    //これはStartよりも先に実行される
    private void OnEnable()
    {
        if (OnceEnable == false) {
            //座標を記憶、初期スケールに設定
            startNormalScale = transform.localScale;
            startScale = startNormalScale;

            startPos = transform.localPosition; //位置ずれ対策
            DOVirtual.DelayedCall(0.5f, () =>
            {
                DesktopMode();
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    DesktopMode();
                    //起動直後に発動した場合は無視する
                    if (Time.time > 5f)
                    {
                        OnceEnable = true;
                    }
                });
            });
        }

        //背景を無効にしてしまっているならもとに戻す
        ShowBackground();
    }

    // Update is called once per frame
    void Update()
    {
        if (winTex == null)
        {
            winTex = GetComponent<uWindowCapture.UwcWindowTexture>();
        }
        if (winTex.window != null)
        {
            //画面比率を更新。動的にモニタの縦横比が変わることがあるため
            Width = winTex.window.width;
            Height = winTex.window.height;
            var nowaspect = Width / Height;

            if (aspect != nowaspect)
            {
                aspect = nowaspect;

                var width = startScale.x;
                var height = startScale.x / aspect;

                if (height > startScale.y)
                {
                    width = startScale.y * aspect;
                    height = startScale.y;
                }
                if (!float.IsNaN(width + height))
                {
                    transform.localScale = new Vector3(width, height, 1);
                }

            }

        }
    }

    //モニタ・ウィンドウ切り替え
    public void Change()
    {
        uWindowCapture.UwcWindow win = null;

        if (desktopMode)
        {
            //デスクトップモードの場合
            //次のデスクトップへ
            desktopIndex++;

            //デスクトップインデックスを超える場合は、0に戻す
            if (desktopIndex > uWindowCapture.UwcManager.desktopCount - 1)
            {
                desktopIndex = 0;
            }
            winTex.desktopIndex = desktopIndex;
            //表示
            TitleText.text = LanguageManager.config.misc.DESKTOP + desktopIndex;
        }
        else {
            //ウィンドウモードの場合
            loadJSON();//読み込み

            //次のウィンドウへ
            windowIndex++;

            //更新掛ける
            uWindowCapture.UwcManager.UpdateAltTabWindowTitles();
            //最新のウィンドウ辞書を取得
            Dictionary<int,uWindowCapture.UwcWindow> Windows = uWindowCapture.UwcManager.windows;
            //有効なウィンドウリストを作成する
            List<uWindowCapture.UwcWindow> WindowList = new List<uWindowCapture.UwcWindow>();

            //隙間ないウィンドウリストを作成する
            int i = 0;
            foreach (KeyValuePair<int,uWindowCapture.UwcWindow> winPair in Windows)
            {
                //無限ループにはまらないためにループ制限をしている
                if (i > 500) {
                    break;
                }
                i++;

                //条件に合うものだけ追加していく
                if (winPair.Value.isVisible && !winPair.Value.isMinimized && !winPair.Value.isBackground && winPair.Value.isAltTabWindow)
                {
                    WindowList.Add(winPair.Value);
                }
            }

            //ウィンドウ切り替え番号が大きすぎるなら0に戻す
            if (windowIndex >= WindowList.Count)
            {
                windowIndex = 0;
            }

            Debug.Log("WindowList.Count: " + WindowList.Count);
            Debug.Log("windowIndex: " + windowIndex);

            //それでも大きすぎる(ウィンドウがないかなにかおかしい時)
            if (windowIndex >= WindowList.Count)
            {
                menu.ShowDialogOK(LanguageManager.config.showdialog.ERROR, LanguageManager.config.showdialog.WINDOW_NOT_FOUND, 0.1f, () => { });
                DesktopMode();
                return;
            }
            else {
                win = WindowList[windowIndex];
            }

            /*
            Debug.Log("Check:" + windowIndex);
            Debug.Log("Title:" + win.title);
            Debug.Log("isVisible:" + win.isVisible);
            Debug.Log("isAltTabWindow:" + win.isAltTabWindow);
            Debug.Log("isBackground:" + win.isBackground);
            Debug.Log("isMinimized:" + win.isMinimized);
            */

            //条件を満たした状態ではない状態で脱出した場合
            if (win == null) {
                menu.ShowDialogOK(LanguageManager.config.showdialog.ERROR, LanguageManager.config.showdialog.WINDOW_NOT_FOUND, 0.1f, () => { });
                DesktopMode();
                return;
            }

            //切り替え
            winTex.window = win;
            //表示
            TitleText.text = win.title;
            //何があろうとまずPrintWindow
            winTex.captureMode = uWindowCapture.CaptureMode.PrintWindow;
        }
    }

    public void DesktopMode()
    {
        //デスクトップモードに切り替え
        desktopMode = true;
        winTex.type = uWindowCapture.WindowTextureType.Desktop;
        winTex.desktopIndex = 0; //0番モニタに設定
        winTex.captureMode = uWindowCapture.CaptureMode.BitBlt;
        winTex.captureRequestTiming = uWindowCapture.WindowTextureCaptureTiming.OnlyWhenVisible;

        TitleText.text = LanguageManager.config.misc.DESKTOP + winTex.desktopIndex;
        desktopIndex = 0;
    }
    private void WindowMode()
    {
        //ウィンドウモードに切り替え
        desktopMode = false;
        winTex.type = uWindowCapture.WindowTextureType.Window;
        winTex.captureMode = uWindowCapture.CaptureMode.PrintWindow;
        winTex.captureRequestTiming = uWindowCapture.WindowTextureCaptureTiming.OnlyWhenVisible;
        //winTex.partialWindowTitle = "Chageボタンを押してください";

        TitleText.text = LanguageManager.config.misc.CHANGE_BUTTON;
        Change();
    }

    //デスクトップ・ウィンドウ切り替え
    public void DesktopWindowSwitch()
    {
        //ウィンドウモードのとき
        if (!desktopMode)
        {
            DesktopMode();
        }
        else {
            WindowMode();
        }
        zoomReset();
    }

    private void zoomReset() {
        //ズーム状態をリセット
        freeLoupe.enable = false;
        freeLoupe.scale = 1;
        zoomRate = 1;
    }

    //ズームインボタン
    public void zoomin()
    {
        zoomRate += 1f;
        freeLoupe.enable = true;
        if (zoomRate > 5f)
        {
            zoomRate = 5f;
        }
        freeLoupe.scale = zoomRate;
    }

    //ズームアウトボタン
    public void zoomout()
    {
        zoomRate -= 1f;
        if (zoomRate < 1.5)
        {
            zoomRate = 1;
            freeLoupe.enable = false;
        }

        freeLoupe.scale = zoomRate;
    }


}
