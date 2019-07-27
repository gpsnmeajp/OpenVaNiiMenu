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
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LauncherManagerScript : MonoBehaviour {
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private GameObject[] AppBox;
    [SerializeField]
    private int page = 0;

    const string jsonPath = "config\\Launcher.json";
    LauncherConfig config = null; //読み込まれた設定

    ApplicationConfig OnClickApp; //OnClickでコールバックする用の一時変数

    const int jsonVerMaster = 1; //設定ファイルバージョン
    [Serializable]
    class LauncherConfig
    {
        public ApplicationConfig App1 = new ApplicationConfig();
        public ApplicationConfig App2 = new ApplicationConfig();
        public ApplicationConfig App3 = new ApplicationConfig();
        public ApplicationConfig App4 = new ApplicationConfig();
        public ApplicationConfig App5 = new ApplicationConfig();
        public ApplicationConfig App6 = new ApplicationConfig();
        public ApplicationConfig App7 = new ApplicationConfig();
        public ApplicationConfig App8 = new ApplicationConfig();
        public ApplicationConfig App9 = new ApplicationConfig();
        public ApplicationConfig App10 = new ApplicationConfig();
        public ApplicationConfig App11 = new ApplicationConfig();
        public ApplicationConfig App12 = new ApplicationConfig();
        public ApplicationConfig App13 = new ApplicationConfig();
        public ApplicationConfig App14 = new ApplicationConfig();
        public ApplicationConfig App15 = new ApplicationConfig();
        public ApplicationConfig App16 = new ApplicationConfig();
        public int jsonVer = jsonVerMaster; //設定ファイルバージョン
    }
    [Serializable]
    class ApplicationConfig
    {
        public string ApplicationName = "None";
        public string FilePath = @"";
        public string WorkingDirectory = @"";
        public string Arguments = @"";
        public string StartupDialogMainText = @"%name%を起動しますか？";
        public string StartupDialogSubText = @"";
    }

    public void pageNext()
    {
        page++;
        if (page > 3)
        {
            page = 0;
        }
        SetMetas(page);
    }

    public void pagePrev()
    {
        page--;
        if (page < 0) {
            page = 3;
        }
        SetMetas(page);
    }

    // Use this for initialization
    void Start () {
        page = 0;
        SetMetas(page);
    }

    public void SetMetas(int page) {
        loadJSON();
        switch (page){
            case 0:
                SetMeta(1, config.App1);
                SetMeta(2, config.App2);
                SetMeta(3, config.App3);
                SetMeta(4, config.App4);
                break;
            case 1:
                SetMeta(1, config.App5);
                SetMeta(2, config.App6);
                SetMeta(3, config.App7);
                SetMeta(4, config.App8);
                break;
            case 2:
                SetMeta(1, config.App9);
                SetMeta(2, config.App10);
                SetMeta(3, config.App11);
                SetMeta(4, config.App12);
                break;
            case 3:
                SetMeta(1, config.App13);
                SetMeta(2, config.App14);
                SetMeta(3, config.App15);
                SetMeta(4, config.App16);
                break;
            default:
                menu.ShowDialogOK(LanguageManager.config.showdialog.PAGE_INVAILD, "page: "+page.ToString(),0.05f,()=> { });
                break;
        }
    }

    void SetMeta(int n, ApplicationConfig app) {
        AppBox[n-1].transform.Find("Title").GetComponent<Text>().text = app.ApplicationName;

        if (app.Arguments != "")
        {
            AppBox[n - 1].transform.Find("Path").GetComponent<Text>().text = app.WorkingDirectory + "\n" + app.Arguments;
        }
        else {
            AppBox[n - 1].transform.Find("Path").GetComponent<Text>().text = app.WorkingDirectory;
        }
    }

    ApplicationConfig GetMeta(int n) {
        switch (n) {
            case 1:
                return config.App1;
            case 2:
                return config.App2;
            case 3:
                return config.App3;
            case 4:
                return config.App4;
            case 5:
                return config.App5;
            case 6:
                return config.App6;
            case 7:
                return config.App7;
            case 8:
                return config.App8;
            case 9:
                return config.App9;
            case 10:
                return config.App10;
            case 11:
                return config.App11;
            case 12:
                return config.App12;
            case 13:
                return config.App13;
            case 14:
                return config.App14;
            case 15:
                return config.App15;
            case 16:
                return config.App16;
        }
        throw (new ArgumentException("App not found"));
    }

    // Update is called once per frame
    void Update () {
		
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
        var json = JsonUtility.ToJson(new LauncherConfig());
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
            config = new LauncherConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<LauncherConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f,
                () => {
                    //OK
                    makeJSON();
                }, () => {
                    //キャンセル
                });
                config = new LauncherConfig();
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
            config = new LauncherConfig();
            menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.CORRUPT_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.CORRUPT_CONFIG_BODY, 3f,
            () => {
                    //OK
                    makeJSON();
            }, () => {
                    //キャンセル
                });
        }
    }

    public void OnClick(int n)
    {
        OnClickApp = GetMeta(n + 4*page); //1-4 + ページ番号

        string mainText = OnClickApp.StartupDialogMainText;
        string subText = OnClickApp.StartupDialogSubText;

        mainText = mainText.Replace("%name%", OnClickApp.ApplicationName);
        subText = subText.Replace("%name%", OnClickApp.ApplicationName);

        menu.ShowDialogOKCancel(mainText, subText, 0.05f,()=> {
            Launch(OnClickApp.FilePath, OnClickApp.Arguments, OnClickApp.WorkingDirectory);
        },()=> { });
    }

    public void MediaKey(string key)
    {
        System.Diagnostics.Process proc = new System.Diagnostics.Process();
        proc.StartInfo.UseShellExecute = false; //既知のため不要
        proc.StartInfo.CreateNoWindow = true; //ウィンドウを生成しない
        proc.StartInfo.ErrorDialog = false;
        proc.StartInfo.FileName = "VaNiiSendKeyHelper2017.exe";
        proc.StartInfo.Arguments = key;
        proc.StartInfo.WorkingDirectory = "";
        proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized; //最小化して起動
        proc.Start();
    }

    public void LaunchVaNiiMenu()
    {
        System.Diagnostics.Process proc = new System.Diagnostics.Process();
        proc.StartInfo.UseShellExecute = false; //既知のため不要
        proc.StartInfo.CreateNoWindow = false;
        proc.StartInfo.ErrorDialog = false;
        proc.StartInfo.FileName = Environment.GetCommandLineArgs()[0];
        proc.StartInfo.Arguments = "overlaykey VaNiiMenuSub" + ((int)(UnityEngine.Random.value * 10000)).ToString();
        proc.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
        proc.Start();
    }

    public void LaunchTweetHelper(string arg)
    {
        System.Diagnostics.Process proc = new System.Diagnostics.Process();
        proc.StartInfo.UseShellExecute = false; //既知のため不要
        proc.StartInfo.CreateNoWindow = false;
        proc.StartInfo.ErrorDialog = false;
        proc.StartInfo.FileName = "VaNiiTweetHelper\\VaNiiTweetHelper.exe";
        proc.StartInfo.Arguments = arg;
        proc.StartInfo.WorkingDirectory = "";
        proc.Start();
    }

    public void Launch(string filename, string arg,string WorkingDirectory)
    {
        System.Diagnostics.Process proc = new System.Diagnostics.Process();
        proc.StartInfo.UseShellExecute = true;
        proc.StartInfo.CreateNoWindow = false;
        proc.StartInfo.ErrorDialog = false;
        proc.StartInfo.FileName = filename;
        proc.StartInfo.Arguments = arg;
        proc.StartInfo.WorkingDirectory = WorkingDirectory;
        proc.Start();
    }

    public void AutoSetup() {
        //
    }
}
