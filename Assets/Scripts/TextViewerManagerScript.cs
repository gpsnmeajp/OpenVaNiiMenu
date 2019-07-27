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

public class TextViewerManagerScript : MonoBehaviour {
    [SerializeField]
    private Text filename;
    [SerializeField]
    private Text text;
    [SerializeField]
    private TestScrollWorkerScript worker;
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private uOSC.uOscClient client;

    public string mode = "UTF8";
    public int no = 0;

    const int jsonVerMaster = 2; //設定ファイルバージョン
    const string jsonPath = "config\\TextViewer.json";
    TextViewerConfig config = null; //読み込まれた設定

    [Serializable]
    class TextViewerConfig
    {
        public string path = "text\\";
        public int fontsize = 40;
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
        var json = JsonUtility.ToJson(new TextViewerConfig());
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
            config = new TextViewerConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<TextViewerConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f, () => {
                        //OK
                        makeJSON();
                    }, () => {
                        //キャンセル
                    });
                config = new TextViewerConfig();
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
            config = new TextViewerConfig();
            menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.CORRUPT_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.CORRUPT_CONFIG_BODY, 3f, () => {
                //OK
                makeJSON();
            }, () => {
                //キャンセル
            });
        }
    }

    // Use this for initialization
    void Start () {
        loadJSON();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void nextText() {
        no++;
        loadText();
        worker.resetPos();
    }

    public void prevText() {
        no--;
        loadText();
        worker.resetPos();
    }

    //ファイル一覧を取得し、テキストを読み込む。あとがない場合は繰り返す
    public void loadText() {
        loadJSON();
        text.fontSize = config.fontsize; //フォントサイズを反映

        if (!Directory.Exists(config.path)) {
            config = new TextViewerConfig();
            menu.ShowDialogOKCancel(LanguageManager.config.showdialog.FOLDER_NOT_FOUND_TITLE, LanguageManager.config.showdialog.FOLDER_NOT_FOUND_BODY1 + jsonPath + LanguageManager.config.showdialog.FOLDER_NOT_FOUND_BODY2, 3f,
            () => {
                //OK
                makeJSON();
            }, () => {
                //キャンセル
            });
            return;
        }

        string[] files = Directory.GetFiles(config.path, "*.txt"); //帰ってくるのはフォルダ含む相対パス

        Array.Sort(files, (arg1, arg2) => {
            return DateTime.Compare(File.GetLastWriteTime(arg2), File.GetLastWriteTime(arg1));
        });

        if (files.Length == 0)
        {
            menu.ShowDialogOK(LanguageManager.config.showdialog.FILE_NOT_FOUND, "", 0.1f, () => { });
            return;
        }

        if (no > files.Length - 1) {
            no = 0;
        }
        if (no < 0) {
            no = files.Length - 1;
        }

        if (mode == "SJIS")
        {
            text.text = File.ReadAllText(files[no], Encoding.GetEncoding(932));//SJIS
        }
        else {
            text.text = File.ReadAllText(files[no], new UTF8Encoding(false));
        }

        filename.text = Path.GetFileName(files[no]);
        /*
        for (int i = 0; i < files.Length; i++) {
            Debug.Log(i.ToString() + " : "+files[i]);
        }
        */

    }
    //現在のテキストをOSC連携アプリケーションに送信する
    public void send()
    {
        menu.ShowDialogOKCancel(LanguageManager.config.showdialog.OSC_APP_SEND, LanguageManager.config.showdialog.OSC_TEXT_SEND + text.text, 0.1f, () => {
            client.Send("/VaNiiMenu/TextSend", text.text);

            menu.ShowDialogOK(LanguageManager.config.showdialog.SEND_COMPLETE, text.text, 1f, () => { });
        }, () => {
            menu.ShowDialogOK(LanguageManager.config.showdialog.SEND_CANCEL, "", 1f, () => { });
        });
    }
}
