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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;

public class LanguageManager : MonoBehaviour {
    public static bool loadfailed = true;//読み込み失敗

    [Serializable]
    public class JsonLoaders
    {
        public string OLD_CONFIG_HEAD = "古い設定ファイル";
        public string OLD_CONFIG_BODY = "が古くなっています。\nデフォルト設定で起動しました。\nOKで古い設定を削除\nキャンセルで無視して続行";

        public string CORRUPT_CONFIG_HEAD = "設定ファイルの破損";
        public string CORRUPT_CONFIG_BODY = "が破損しています。\nデフォルト設定で起動しました。\nOKで破損した設定を削除\nキャンセルで無視して続行";

    }
    [Serializable]
    public class Tutorial
    {
        //LanguageManager.config.tutorial.
        public string TUTORIAL1_TITLE = "VaNiiMenuへようこそ(1/4)";
        public string TUTORIAL1_BODY = @"
本ツールでは、他のソフトウェアの起動や音楽の制御などが行なえます。
Open VR空間内であれば、使用しているVRソフトウェアに関係なく
いつでもメニューを開くことができます

OKボタンをタッチしてください
(コントローラを近づけてください)";

        public string TUTORIAL2_TITLE = "操作説明(2/4)";
        public string TUTORIAL2_BODY = @"
・コントローラを勢いよく振り下げるとメニューが開きます。
・コントローラを勢いよく振り上げると閉じることができます。
・ボタンを押す際は、コントローラをボタンに押し当ててください。
・コントローラの場所は十字のカーソルで表示されます。
・カーソルが出ないときは、コントローラを体に近づけてみてください。";

        public string TUTORIAL3_TITLE = "便利な使い方(3/4)";
        public string TUTORIAL3_BODY = @"
・Miscを開くと、デスクトップや写真が見れる機能が使用できます。
・Settingからロックや開閉、スキン変更などの設定ができます。
・コントローラの充電状態が右下に表示されています。
・設定ファイルを編集するとさらに応用的な使い方ができます。
・VRAutorunHelper.exeを使用するとVR使用時に自動起動するようになります。";

        public string TUTORIAL4_TITLE = "Discordのご案内(4/4)";
        public string TUTORIAL4_BODY_1 = @"
本ツールの更新情報はDiscordサーバーで配信しています。
ご意見・ご要望等もこちらまで。

";//https://discord.gg/QSrDhE8
        public string TUTORIAL4_BODY_2 = @"
このURLを開きますか？(ブラウザを開きます)

OKで開く / キャンセルで無視して続行";


        public string TUTORIAL_STARTUP_TITLE = "自動起動設定";
        public string TUTORIAL_STARTUP_BODY = @"
VR起動時に本ツールを自動で起動するように設定できます。
VRAutorunHelper.exeを起動しますか？
(起動後、Installをクリックしてください)";

        public string TUTORIAL_ONLINE_TITLE = "オンライン機能";
        public string TUTORIAL_ONLINE_BODY = @"
アップデートお知らせ機能、ツイート機能を有効できます。
VaNiiTweetHelperを起動しますか？
(起動後、利用規約をお読みください)";

        public string TUTORIAL_TRANSLATOR_TITLE = "言語パック情報";

        public string TUTORIAL_END_TITLE = "チュートリアル終了";
        public string TUTORIAL_END_BODY = "これらメッセージを次回から表示しないようにしますか？";
    }

    [Serializable]
    public class Misc
    {
        public string DESKTOP = "デスクトップ: ";
        public string CHANGE_BUTTON = "Chageボタンを押してください";
        public string FUNCTIONKEY = "ファンクションキーの説明\nこのテキストは以下のファイルを編集すると変更できます\n";
    }

    [Serializable]
    public class ShowDialog
    {
        //LanguageManager.config.showdialog.
        public string FROM_EXTERNAL_APP = "外部アプリケーションからの通知";
        public string BUTTON_NOT_FOUND = "ボタンを認識できませんでした";
        public string BUTTON_REGISTER = "ボタンを登録しました";
        public string PAGE_INVAILD = "無効なページ";
        public string WINDOW_NOT_FOUND = "有効なウィンドウを発見できませんでした";
        public string WINDOW_INVAILD = "ウィンドウが無効です";
        public string ERROR = "エラー";
        public string MOVESCREEN_TITLE = "画面の移動";
        public string MOVESCREEN_BODY = "操作したいコントローラのボタンを押しながら、Moveを押してください\n(タイトルバーでも移動できます)";
        public string PATHINDEX_INVAILD = "PathIndexが範囲外です";
        public string TOOMANYFILES_TITLE = "ファイルが多すぎます(2000枚以上)";
        public string TOOMANYFILES_BODY = "ファイルが多すぎると動作が停止する場合があります。";//v0.11a
        public string FOLDER_NOT_FOUND_TITLE = "フォルダがありません";
        public string FOLDER_NOT_FOUND_BODY1 = "フォルダが見つかりません。\n";
        public string FOLDER_NOT_FOUND_BODY2 = "が破損している可能性があります。\nデフォルト設定で起動しました。\nOKで破損した設定を削除\nキャンセルで無視して続行";
        public string FILE_NOT_FOUND = "ファイルがありません";
        public string OSC_APP_SEND = "OSC連携アプリケーションに送信";
        public string OSC_PATH_SEND = "以下のファイルパスを送信しますか？\n";
        public string SEND_COMPLETE = "送信しました";
        public string SEND_CANCEL = "送信を中止しました";
        public string PORTINDEX_INVAILD = "異常なportIndex";
        public string OSC_TEXT_SEND = "以下のテキストを送信しますか？\n";
        public string PROGRAM_ERROR = "プログラムエラー";
        public string ALARM_ERROR = "アラーム機能が異常動作しました。\nアラーム動作が信頼できない可能性があります。";
        public string ONTIME = "時間になりました";
        public string ALARMSTOP = "停止するにはアラーム設定画面でReleaseをタップしてください。";
        public string OPEN_DISCORD = "Discordを開きますか？";
        public string OPEN_VANIIMENU = "VaNiiMenuを起動しますか？";
        public string OPEN_VANIIMENU_BODY = "";//v0.11
        public string EXIT_VANIIMENU_TITLE = "VaNiiMenuを終了しますか？";
        public string EXIT_VANIIMENU_SUB = "これはサブインスタンスです";
        public string EXIT_VANIIMENU_MAIN = "これはメインインスタンスです";
        public string EXIT_VANIIMENU_OK = "本当に終了しますか？";
        public string LOCK_ON_TITLE = "誤操作ロックを設定しました";
        public string LOCK_ON_BODY = "以降は設定したボタン(デフォルトはグリップボタン)を\n押しながら腕を振ってください";
        public string FULLLOCK_ON_TITLE = "警告";
        public string FULLLOCK_ON_BODY = "全操作ロックを設定しますか？\nこれにより指定のボタンを押しながらでなければ、\n一切の操作ができなくなります。";
        public string FULLLOCK_SETED_TITLE = "全操作ロックを設定しました";
        public string FULLLOCK_SETED_BODY = "以降は設定したボタンを\n押しながら操作してください。\n(わからなくなったときはGesture.jsonを削除して再起動してください)";
        public string FULLLOCK_CANCEL = "全操作ロックは設定されませんでした";
        public string UNLOCK = "誤操作ロックを解除しました";
        public string LOCK_REGISTER_TITLE = "誤操作ロックボタンの登録";
        public string LOCK_REGISTER_BODY = "登録したいボタンを押しながらOKをタップしてください";
        public string VOICE_CHANGED_TITLE = "音声が変更されました";
        public string VOICE_CHANGED_BODY = "再起動すると反映されます";
        public string ARE_YOU_OK = "よろしいですか？";
        public string SPEED_CHANGE = "メニューの呼び出し・格納時の腕の加速度を変更します。";
        public string SPEED_CHANGED = "加速度が変更されました";
        public string SPEED_UNCHANGED = "加速度は変更されませんでした";

        //v0.11
        public string IMG_AUTOSETUP_TITLE = "自動セットアップ";
        public string IMG_AUTOSETUP_BODY = "VRChatを検出しました。\nVRChatのスクリーンショットを表示するようにしますか？";
        public string IMG_AUTOSETUP_OK_TITLE = "設定完了";
        public string IMG_AUTOSETUP_OK_BODY = "VRChatをパス1に設定しました";

        //v0.11a
        public string ONLINE_UPDATE_AVAILABLE = "VaNiiMenuのアップデートがあります";
        public string ONLINE_UPDATE_AVAILABLE_TWEETHELPER = "VaNiiTweetHelperのアップデートがあります";

        public string ONLINE_NOT_AVAILABLE = "オンライン機能は利用できません\n理由: ";
        public string ONLINE_NOT_AGREE = "VaNiiTweetHelperを起動してオンライン利用規約に同意すると\nアップデート通知機能が利用できるようになります。";

        public string TWEET_SEND_TITLE = "本当に送信しますか？";
        public string TWEET_SEND_OK = "送信しました";
        public string TWEET_SEND_ERROR = "送信エラー";

        public string TWEET_CLEAR_TITLE = "本当に消しますか？";

        public string FORCEREAD_TITLE = "強制読み込みを有効にしました";
        public string FORCEREAD_BODY = "不安定になった場合は、コンピュータを再起動してください";

    }




    //-----------HelpTexts------------------

    public Text UI_HELPTEXT_FUNCTIONKEY;// "ファンクションキーを送信します";
    public Text UI_HELPTEXT_DEBUG;// "デバッグ情報を取得します。";
    public Text UI_HELPTEXT_ALARM;// "アラームを設定します";
    public Text UI_HELPTEXT_OSCREMOTE;// "OSC信号を送信します";
    public Text UI_HELPTEXT_DESKTOP;// "プライマリモニタの画面を見ることができます";
    public Text UI_HELPTEXT_IMAGE;// "指定したフォルダの画像ファイルを見ることができます";
    public Text UI_HELPTEXT_TEXT;// "Textフォルダに入れたテキストファイルを表示することができます。";
    public Text UI_HELPTEXT_MUSIC;// "マルチメディアキー対応の音楽プレーヤーを操作できます";
    public Text UI_HELPTEXT_WINDOW;// "指定したWindowを見ることができます";
    public Text UI_HELPTEXT_LAUNCHER;// "Launcher.jsonに登録したアプリケーションを起動することができます";
    public Text UI_HELPTEXT_MISC;// "他のアプリケーションとの連携ができます";
    public Text UI_HELPTEXT_MOVE;// "画面の移動";
    public Text UI_HELPTEXT_VOL;// "操作音の音量設定";
    public Text UI_HELPTEXT_STYLE;// "ホーム画面の表示スタイルの設定";
    public Text UI_HELPTEXT_SCREEN;// "画面の大きさ・解像度の設定をします";
    public Text UI_HELPTEXT_HAND;// "呼び出し操作に反応する手の選択";
    public Text UI_HELPTEXT_ALARMMON;// "アラームがセットされています";
    public Text UI_HELPTEXT_SETTING;// "本ツールの設定ができます";
    public Text UI_HELPTEXT_LOCK;// "操作ロックの有効・無効と、ボタンの設定";
    public Text UI_HELPTEXT_MENUSET;// "メニューの開く・閉じるの設定";
    public Text UI_HELPTEXT_EXIT;// "終了";

    public Text UI_HELPTEXT_TIMELINE;// "タイムラインを取得します"; //v0.11
    public Text UI_HELPTEXT_TWEET;// "ツイートを送信します"; //v0.11


    [Serializable]
    public class HelpText {
        public string HELPTEXT_FUNCTIONKEY = "ファンクションキーを送信します";
        public string HELPTEXT_DEBUG = "デバッグ情報を取得します。";
        public string HELPTEXT_ALARM = "アラームを設定します";
        public string HELPTEXT_OSCREMOTE = "OSC信号を送信します";
        public string HELPTEXT_DESKTOP = "プライマリモニタの画面を見ることができます";
        public string HELPTEXT_IMAGE = "指定したフォルダの画像ファイルを見ることができます";
        public string HELPTEXT_TEXT = "Textフォルダに入れたテキストファイルを表示することができます。";
        public string HELPTEXT_MUSIC = "マルチメディアキー対応の音楽プレーヤーを操作できます";
        public string HELPTEXT_WINDOW = "指定したWindowを見ることができます";
        public string HELPTEXT_LAUNCHER = "Launcher.jsonに登録したアプリケーションを起動することができます";
        public string HELPTEXT_MISC = "他のアプリケーションとの連携ができます";
        public string HELPTEXT_MOVE = "画面の移動";
        public string HELPTEXT_VOL = "操作音の音量設定";
        public string HELPTEXT_STYLE = "ホーム画面の表示スタイルの設定";
        public string HELPTEXT_SCREEN = "画面の大きさ・解像度の設定をします";
        public string HELPTEXT_HAND = "呼び出し操作に反応する手の選択";
        public string HELPTEXT_ALARMMON = "アラームがセットされています";
        public string HELPTEXT_SETTING = "本ツールの設定ができます";
        public string HELPTEXT_LOCK = "操作ロックの有効・無効と、ボタンの設定";
        public string HELPTEXT_MENUSET = "メニューの開く・閉じるの設定";
        public string HELPTEXT_EXIT = "終了";

        public string HELPTEXT_TIMELINE = "タイムラインを取得します";//v0.11
        public string HELPTEXT_TWEET = "ツイートを送信します";//v0.11
    }

    public void ApplyHelpText() {
        UI_HELPTEXT_FUNCTIONKEY.text = config.helptext.HELPTEXT_FUNCTIONKEY;
        UI_HELPTEXT_DEBUG.text = config.helptext.HELPTEXT_DEBUG;
        UI_HELPTEXT_ALARM.text = config.helptext.HELPTEXT_ALARM;
        UI_HELPTEXT_OSCREMOTE.text = config.helptext.HELPTEXT_OSCREMOTE;
        UI_HELPTEXT_DESKTOP.text = config.helptext.HELPTEXT_DESKTOP;
        UI_HELPTEXT_IMAGE.text = config.helptext.HELPTEXT_IMAGE;
        UI_HELPTEXT_TEXT.text = config.helptext.HELPTEXT_TEXT;
        UI_HELPTEXT_MUSIC.text = config.helptext.HELPTEXT_MUSIC;
        UI_HELPTEXT_WINDOW.text = config.helptext.HELPTEXT_WINDOW;
        UI_HELPTEXT_LAUNCHER.text = config.helptext.HELPTEXT_LAUNCHER;
        UI_HELPTEXT_MISC.text = config.helptext.HELPTEXT_MISC;
        UI_HELPTEXT_MOVE.text = config.helptext.HELPTEXT_MOVE;
        UI_HELPTEXT_VOL.text = config.helptext.HELPTEXT_VOL;
        UI_HELPTEXT_STYLE.text = config.helptext.HELPTEXT_STYLE;
        UI_HELPTEXT_SCREEN.text = config.helptext.HELPTEXT_SCREEN;
        UI_HELPTEXT_HAND.text = config.helptext.HELPTEXT_HAND;
        UI_HELPTEXT_ALARMMON.text = config.helptext.HELPTEXT_ALARMMON;
        UI_HELPTEXT_SETTING.text = config.helptext.HELPTEXT_SETTING;
        UI_HELPTEXT_LOCK.text = config.helptext.HELPTEXT_LOCK;
        UI_HELPTEXT_MENUSET.text = config.helptext.HELPTEXT_MENUSET;
        UI_HELPTEXT_EXIT.text = config.helptext.HELPTEXT_EXIT;

        UI_HELPTEXT_TIMELINE.text = config.helptext.HELPTEXT_TIMELINE;//v0.11
        UI_HELPTEXT_TWEET.text = config.helptext.HELPTEXT_TWEET;//v0.11
    }






    //-----------------------------

    const int jsonVerMaster = 2; //設定ファイルバージョン
    const string jsonPath = "Language.json";
    static public LanguagePack config = null; //読み込まれた設定

    [Serializable]
    public class LanguagePack
    {
        public string Author = "gpsnmeajp";
        public string URLorMail = "https://sabowl.sakura.ne.jp/gpsnmeajp/";
        public string Version = "v0.02";
        public string Language = "ja";
        public string Comment = "デフォルトの言語設定です(日本語)";
        public int jsonVer = jsonVerMaster; //設定ファイルバージョン

        //GUI上日本語説明
        public HelpText helptext = new HelpText();
        public JsonLoaders jsonloaders = new JsonLoaders();
        public ShowDialog showdialog = new ShowDialog();
        public Tutorial tutorial = new Tutorial();
        public Misc misc = new Misc();
    }

    static public void saveJSON()
    {
        //設定ファイルを書き込み
        var json = JsonUtility.ToJson(config);
        File.WriteAllText(jsonPath, json, new UTF8Encoding(false));
    }

    static public void makeJSON()
    {
        //初期設定を生成
        config = new LanguagePack();

        //初期設定ファイルを生成
        var json = JsonUtility.ToJson(config);
        //初期設定ファイルを書き込み
        File.WriteAllText(jsonPath, json, new UTF8Encoding(false));
    }

    static public void loadJSON()
    {
        config = null;

        //ファイルがない場合: 初期設定ファイルの生成
        if (!File.Exists(jsonPath))
        {
            config = new LanguagePack();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<LanguagePack>(jsonString);
            loadfailed = false;//読み込み成功

            //ファイルのバージョンが古い場合は、デフォルト設定にする
            if (config.jsonVer != jsonVerMaster)
            {
                config = new LanguagePack();
                loadfailed = true;//読み込み失敗
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
            config = new LanguagePack();
            loadfailed = true;//読み込み失敗
        }
    }

    //多言語対応のために最速で呼び出される
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void startup() {
        Debug.Log("Loding Language...");

        loadJSON();
    }

    //読み込み済み
    private void Start()
    {
        ApplyHelpText();
    }

}
