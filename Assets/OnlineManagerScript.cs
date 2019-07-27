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

public class OnlineManagerScript : MonoBehaviour {
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private LauncherManagerScript Launcher;
    [SerializeField]
    private Text versionLabel;
    [SerializeField]
    private Text HomeText;

    //コマンド・レスポンスファイル
    const string ResponseFilePath = "VaNiiTweetHelper\\response.json";
    const string CommandFilePath = "VaNiiTweetHelper\\command.json";

    public int lastsequence = 0; //シーケンス番号
    public bool sending = false; //通信中
    public float checkTime = 0; //次チェック時間

    //イベントコールバック
    UnityAction callback = ()=> { Debug.LogError("null callback!"); };
    public ResponseStruct response;

    //コマンドの定義(TweetHelperと合わせること)
    public struct CommandStruct
    {
        public string command; //命令
        public string status; //ツイート本体
        public string imagePath; //画像パス
        public int sequence; //シーケンス番号
    }

    //レスポンスの定義(TweetHelperと合わせること)
    public struct ResponseStruct
    {
        public bool successed; //成功可否
        public string[] text; //返却するテキスト
        public string exception; //例外があるならその内容
        public int sequence; //シーケンス番号
    }

    //コマンドを送信する
    void SendCommand(CommandStruct c, UnityAction call) {
        if (sending) {
            throw new IOException("Already sending command");
        }
        //応答ファイルがあれば削除する
        if (File.Exists(ResponseFilePath))
        {
            File.Delete(ResponseFilePath);
        }

        //シーケンス番号を更新
        c.sequence = lastsequence + 1;

        //コマンドを書き込み
        var json = JsonUtility.ToJson(c);
        File.WriteAllText(CommandFilePath, json, new UTF8Encoding(false));

        //シーケンス番号を記憶
        lastsequence = c.sequence;

        //コールバック設定
        callback = call;

        //受信処理を開始
        sending = true;
        checkTime = 0;

        //実行開始
        Launcher.LaunchTweetHelper("command");
    }

    //レスポンスを受信する
    ResponseStruct ReceiveResponse() {
        //レスポンスファイルがない場合はエラー
        if(!File.Exists(ResponseFilePath))
        {
            var x = new ResponseStruct();
            x.successed = false;
            x.exception = "File not found(Unity)";
            x.sequence = 0;
            x.text = new string[0] {};
            return x;
        }

        //json解釈
        string jsonString = File.ReadAllText(ResponseFilePath, new UTF8Encoding(false));
        var r = JsonUtility.FromJson<ResponseStruct>(jsonString);
        
        //シーケンスが異常な場合は処理を中止
        if (r.sequence != lastsequence) {
            throw new IOException("Communication Sequence unmatch");
        }

        return r;
    }

    //ツイートする
    public void tweet(string status, UnityAction call)
    {
        CommandStruct c = new CommandStruct();
        c.command = "update";
        c.status = status;
        c.imagePath = "";
        SendCommand(c, call);
    }

    //画像つきツイートする
    public void tweetWithImage(string status, string imagePath, UnityAction call)
    {
        CommandStruct c = new CommandStruct();
        c.command = "updateWithImage";
        c.status = status;
        c.imagePath = imagePath;
        SendCommand(c,call);
    }

    //ホームを取得
    public void getHome(UnityAction call)
    {
        CommandStruct c = new CommandStruct();
        c.command = "home";
        c.status = "";
        c.imagePath = "";
        SendCommand(c, call);
    }

    //リプライを取得
    public void getReply(UnityAction call)
    {
        CommandStruct c = new CommandStruct();
        c.command = "reply";
        c.status = "";
        c.imagePath = "";
        SendCommand(c,call);
    }

    //バージョン情報を取得
    public void getVersion(UnityAction call)
    {
        CommandStruct c = new CommandStruct();
        c.command = "version";
        c.status = "";
        c.imagePath = "";
        SendCommand(c, call);
    }

    // Use this for initialization
    void Start()
    {
        //バージョンチェック
        getVersion(() => {
            //成功した場合
            if (response.successed)
            {
                string result = "";
                //VaNiiMenuのVersionをチェック
                if (response.text[0] != versionLabel.text)
                {
                    result += LanguageManager.config.showdialog.ONLINE_UPDATE_AVAILABLE+"\n[" + versionLabel.text + "] → [" + response.text[0]+ "]\n";
                }
                
                //TweetHelperのVersionをチェック
                if (response.text[1] != response.text[2])
                {
                    result += LanguageManager.config.showdialog.ONLINE_UPDATE_AVAILABLE_TWEETHELPER+"\n[" + response.text[2] + "] → [" + response.text[1]+ "]";
                }
                HomeText.text = result;
            }
            else {
                //オンライン利用に同意していないよ表示
                if (response.exception == "Not Agree")
                {
                    HomeText.text = LanguageManager.config.showdialog.ONLINE_NOT_AGREE;
                }
                else {
                    //ネットワークが使えないなど
                    HomeText.text = LanguageManager.config.showdialog.ONLINE_NOT_AVAILABLE + response.exception;
                }
            }
            
        });
    }
	
	// Update is called once per frame
	void Update () {
        //送信中なら
        if (sending)
        {
            //チェック時間になったかをチェックして
            if (Time.time > checkTime)
            {
                checkTime = Time.time + 0.5f; //0.5秒おきにチェック

                //Responseファイルをチェック
                if (File.Exists(ResponseFilePath))
                {
                    //検出したら停止
                    sending = false;

                    //受信できたら
                    response = ReceiveResponse();
                    //コールバックを呼び出す
                    callback();
                }
            }
        }
		
	}
}
