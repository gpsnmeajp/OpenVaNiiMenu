/*
VaNiiMenu

Copyright (c) 2019, gpsnmeajp
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.IO;
using System.Text;
using EasyLazyLibrary;
using DG.Tweening;

public class TweetManagerScript : MonoBehaviour {
    [SerializeField]
    private Text text;
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private OnlineManagerScript online;
    [SerializeField]
    private AudioManagerScript AudioMan;
    [SerializeField]
    private EasyOpenVROverlayForUnity eovro;
    [SerializeField]
    private Text AttachedImagePathText;
    [SerializeField]
    private ImageViewerManagerScript imageViewer;

    [SerializeField]
    private Text[] ButtonText;

    int selector = 0; //定型句セレクタ
    string imagePath = ""; //添付ファイルパス

    public bool slide = false; //スライドタッチ判定
    bool tapedRight = false; //右手左手
    Vector2 slidePos = Vector2.zero; //タッチ開始位置

    public bool photoshot = false; //OpenVR撮影中
    public float checkTime = 0; //次チェック時間

    private EasyOpenVRUtil util = new EasyOpenVRUtil(); //撮影用

    string shotpath = null; //OpenVR撮影パス
    string shotpathvr = null; //OpenVR撮影パス

    //-----------------------------

    const int jsonVerMaster = 1; //設定ファイルバージョン
    const string jsonPath = "Template.json";
    TemplateConfig config = null; //読み込まれた設定

    [Serializable]
    class Texts
    {
        public string[] text;
        public int jsonVer = jsonVerMaster; //設定ファイルバージョン
    }

    [Serializable]
    class TemplateConfig
    {
        public Texts[] texts;
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
        var json = JsonUtility.ToJson(new TemplateConfig());
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
            config = new TemplateConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<TemplateConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f, () => {
                    //OK
                    makeJSON();
                }, () => {
                    //キャンセル
                });
                config = new TemplateConfig();
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
            config = new TemplateConfig();
            menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.CORRUPT_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.CORRUPT_CONFIG_BODY, 3f, () => {
                //OK
                makeJSON();
            }, () => {
                //キャンセル
            });
        }
    }

    //押したボタンの文字を追加する
    public void append(int i)
    {
        AudioMan.PlaySelectSound();
        text.text += ButtonText[i-1].text;
    }

    //画像を添付
    public void Attach(string path)
    {
        //拡張子チェックで画像以外を弾く
        string ext = Path.GetExtension(path);
        if (ext == ".png" || ext == ".jpg")
        {
            imagePath = path;
            AttachedImagePathText.text = Path.GetFileName(imagePath);
        }
        else {
            //弾いた場合は添付無効
            imagePath = "";
            AttachedImagePathText.text = "Not Supported: "+ext;
        }
    }

    //添付を解除
    public void Detach()
    {
        imagePath = "";
        AttachedImagePathText.text = "";
    }

    //ツイートする
    public void Tweet()
    {
        //ハッシュタグ自動付加
        string status = text.text + " #VR";

        //ツイート内に#VRを含んでいる場合はつけない。
        if (text.text.Contains("#VR"))
        {
            status = text.text;
        }

        //送信中はCancel
        if (online.sending)
        {
            AudioMan.PlayCancelSound();
            return;
        }

        //画像添付がない場合
        AudioMan.PlayApplySound();
        if (imagePath == "")
        {
            //ツイートしますか？
            menu.ShowDialogOKCancel(LanguageManager.config.showdialog.TWEET_SEND_TITLE, "Image: "+"None"+"\n"+"Text: " + status, 0.2f, () =>
            {
                //ツイート処理開始
                online.tweet(status, () =>
                {
                    //成功
                    if (online.response.successed)
                    {
                        menu.ShowDialogOK(LanguageManager.config.showdialog.TWEET_SEND_OK, "", 0.1f, () => { });
                    }
                    else
                    {
                        //失敗したら例外
                        menu.ShowDialogOK(LanguageManager.config.showdialog.TWEET_SEND_ERROR, online.response.exception, 0.1f, () => { });
                    }
                });
            }, () => { });
        }
        else {
            //画像添付時
            menu.ShowDialogOKCancel(LanguageManager.config.showdialog.TWEET_SEND_TITLE, "Image: "+Path.GetFileName(imagePath)+"\n"+"Text: " + status, 0.2f, () => {
                //画像をアップロードして送信
                online.tweetWithImage(status, imagePath, () => {
                    //成功
                    if (online.response.successed)
                    {
                        menu.ShowDialogOK(LanguageManager.config.showdialog.TWEET_SEND_OK, "", 0.1f, () => { });
                    }
                    else
                    {
                        //失敗
                        menu.ShowDialogOK(LanguageManager.config.showdialog.TWEET_SEND_ERROR, online.response.exception, 0.1f, () => { });
                    }
                });
            }, () => { });
        }
    }

    //全クリア
    public void Clear()
    {
        AudioMan.PlayApplySound();
        //本当に消しますか？
        menu.ShowDialogOKCancel(LanguageManager.config.showdialog.TWEET_CLEAR_TITLE, "", 0.2f, () => {
            text.text = "";
        }, () => { });
    }

    //1文字削る
    public void BackSpace()
    {
        //文字削る
        if (text.text.Length >= 1)
        {
            text.text = text.text.Remove(text.text.Length - 1);
            AudioMan.PlayApplySound();
        }
        else {
            //削れないよ
            AudioMan.PlayCancelSound();
        }
    }

    //ボタンに反映
    public void show() {
        for (int i = 0; i < 9; i++)
        {
            ButtonText[i].text = config.texts[selector].text[i];
        }
    }

    //切り替え
    public void Next(bool load)
    {
        //読み込み有効なら読み込む
        if (load)
        {
            loadJSON();
        }
        //セレクタオーバーなら0に戻す
        selector++;
        if (selector >= config.texts.Length) {
            selector = 0;
        }
        show();
    }

    //切り替え
    public void Prev(bool load)
    {
        //読み込み有効なら読み込む
        if (load) {
            loadJSON();
        }
        //セレクタアンダーなら戻す
        selector--;
        if (selector < 0)
        {
            selector = config.texts.Length - 1;
        }
        //読み込んだ結果減ってた場合はリセット
        if (selector >= config.texts.Length)
        {
            selector = 0;
        }
        show();
    }

    //タッチしてスライドを開始
    public void StartSlide()
    {
        slide = true;
        //右手か左手か記録&開始位置記録
        if (eovro.tappedLeft)
        {
            tapedRight = false;
            slidePos = new Vector2(eovro.LeftHandU, eovro.LeftHandV);
        }
        if (eovro.tappedRight) {
            tapedRight = true;
            slidePos = new Vector2(eovro.RightHandU, eovro.RightHandV);
        }
    }

    //OpenVRスクリーンショット撮影
    public void takePhoto()
    {
        //現在のパスを設定
        shotpath = Environment.CurrentDirectory + "\\OpenVRScreenshot";
        shotpathvr = Environment.CurrentDirectory + "\\OpenVRScreenshot_vr";

        //初期化されていないとき初期化する
        if (!util.IsReady())
        {
            util.Init();
        }

        //すでにあれば削除
        if (File.Exists(shotpath + ".png"))
        {
            File.Delete(shotpath + ".png");
        }
        //すでにあれば削除
        if (File.Exists(shotpathvr + ".png"))
        {
            File.Delete(shotpathvr + ".png");
        }

        //スクショ撮影開始
        if (!util.TakeScreenShot(shotpath, shotpathvr))
        {
            Debug.LogError("screenshot failed");
        }
        else {
            Debug.Log("Shot!");
        }

        photoshot = true;

    }

    // Use this for initialization
    void Start () {
        //初期化
        text.text = "";
        loadJSON();
        show();
        Detach();

    }
	
	// Update is called once per frame
	void Update () {

        //OpenVRスクショ処理
        if (photoshot)
        {
            //チェック時間になったかをチェックして
            if (Time.time > checkTime)
            {
                checkTime = Time.time + 0.5f; //0.5秒おきにチェック

                //スクショファイルをチェック
                if (File.Exists(shotpath + ".png"))
                {
                    //検出したら停止
                    photoshot = false;

                    //念の為1秒待って
                    DOVirtual.DelayedCall(1f, () => {
                        //添付パスに設定
                        Attach(shotpath);

                        //プレビュー
                        imageViewer.ImageLoadLocal(shotpath + ".png");
                        //イメージビューアに切り替え
                        menu.MenuPage = 12;
                    });
                }
            }
        }


        //スライダー処理
        if (slide)
        {
            //手が離れたなら終了
            if (!eovro.tappedLeft && !tapedRight)
            {
                slide = false;
            }
            if (!eovro.tappedRight && tapedRight)
            {
                slide = false;
            }

            //差分を計算
            Vector2 delta = Vector2.zero;
            if (!tapedRight)
            {
                delta = new Vector2(eovro.LeftHandU, eovro.LeftHandV) - slidePos;
            }
            else {
                delta = new Vector2(eovro.RightHandU, eovro.RightHandV)- slidePos;
            }

            //一定以上ずれてたらページ送り。そして差分をリセットする
            if (delta.x > 25)
            {
                StartSlide();
                AudioMan.PlaySelectSound();
                Next(false);
            }
            if (delta.x < -25)
            {
                StartSlide();
                AudioMan.PlaySelectSound();
                Prev(false);
            }


        }
    }
}
