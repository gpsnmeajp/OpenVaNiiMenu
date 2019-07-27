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
using System.Net;
using System.Net.Sockets;
using uOSC;

public class CommunicationManagerScript : MonoBehaviour {
    [SerializeField]
    private Text HomeText;
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private GlobalOnClickManagerScript Gonclick;
    [SerializeField]
    private TweetManagerScript tweet;

    uOscServer server;
    //uOscClient client;

    public string address;
    public string value;

    const int jsonVerMaster = 1; //設定ファイルバージョン
    const string jsonPath = "config\\Communication.json";
    CommunicationConfig config = null; //読み込まれた設定

    [Serializable]
    class CommunicationConfig
    {
        public bool OSCReceive = false;
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
        var json = JsonUtility.ToJson(new CommunicationConfig());
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
            config = new CommunicationConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<CommunicationConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f, () => {
                        //OK
                        makeJSON();
                    }, () => {
                        //キャンセル
                    });
                config = new CommunicationConfig();
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
            config = new CommunicationConfig();
            menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.CORRUPT_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.CORRUPT_CONFIG_BODY, 3f, () => {
                //OK
                makeJSON();
            }, () => {
                //キャンセル
            });
        }
    }


    void Start () {
        server = GetComponent<uOscServer>();
        //client = GetComponent<uOscClient>();
        server.onDataReceived.AddListener(onReceived);

        loadJSON();

        //複数起動用キーが指定されている場合、
        if (Environment.GetCommandLineArgs().Length >= 3)
        {
            if (Environment.GetCommandLineArgs()[1] == "overlaykey")
            {
                //サブインスタンスではOSCサーバーはオープンしない
                server.enabled = false;
                return;
            }
        }

        if (config.OSCReceive)
        {
            server.enabled = true;
        }
        else
        {
            server.enabled = false;
        }
    }

    void onReceived(Message msg) {
        if (!config.OSCReceive)
        {
            return;
        }

        address = msg.address;

        foreach (var v in msg.values) {
            if (v is int) {
                value = "int: "+((int)v).ToString();
            }
            else if (v is float)
            {
                value = "float: " + ((float)v).ToString();
            }
            else if (v is string)
            {
                value = "string: " + ((string)v).ToString();
            }
            else if (v is byte[])
            {
                value = "byte: " + ((byte[])v).ToString();
            }

        }
        //------------------

        //ボタン押下エミュレート
        if (address == "/VaNiiMenu/Button" && msg.values[0] is string)
        {
            Gonclick.GlobalOnClick((string)msg.values[0]);
        }

        if (address == "/VaNiiMenu/Menu" && msg.values[0] is int)
        {
            if ((int)msg.values[0] == 0)
            {
                menu.MenuEnd = true;
            }
            else {
                menu.MenuStart = true;
            }
        }

        if (address == "/VaNiiMenu/HomeInfo" && msg.values[0] is string)
        {
            HomeText.text = (string)msg.values[0];
        }
        if (address == "/VaNiiMenu/TweetPhraseReload")
        {
            tweet.loadJSON();
            tweet.show();
        }
        if (address == "/VaNiiMenu/Alert")
        {
            if (msg.values.Length == 2) {
                if (msg.values[0] is string && msg.values[1] is string) {
                    menu.ShowDialogOK((string)msg.values[0], (string)msg.values[1], 0.05f, () => { });
                }
            } else if(msg.values.Length == 1){
                if (msg.values[0] is string)
                {
                    menu.ShowDialogOK(LanguageManager.config.showdialog.FROM_EXTERNAL_APP, (string)msg.values[0], 0.05f, () => { });
                }
            }
        }

    }
/*
    public void send(string adr,int num) {
        client.Send(adr, num);
    }
*/  

}
