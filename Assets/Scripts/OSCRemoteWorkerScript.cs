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

public class OSCRemoteWorkerScript : MonoBehaviour {
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private GameObject[] AUXButton;
    [SerializeField]
    private uOSC.uOscClient client9000;
    [SerializeField]
    private uOSC.uOscClient client39973;
    [SerializeField]
    private uOSC.uOscClient client39974;
    [SerializeField]
    private uOSC.uOscClient client39975;

    const string jsonPath = "config\\OSCRemote.json";
    OSCRemoteConfig config = null; //読み込まれた設定

    const int jsonVerMaster = 2; //設定ファイルバージョン
    [Serializable]
    class OSCRemoteConfig
    {
        public OSCRemoteButtonConfig AUX1 = new OSCRemoteButtonConfig();
        public OSCRemoteButtonConfig AUX2 = new OSCRemoteButtonConfig();
        public OSCRemoteButtonConfig AUX3 = new OSCRemoteButtonConfig();
        public OSCRemoteButtonConfig AUX4 = new OSCRemoteButtonConfig();
        public OSCRemoteButtonConfig AUX5 = new OSCRemoteButtonConfig();
        public OSCRemoteButtonConfig AUX6 = new OSCRemoteButtonConfig();
        public OSCRemoteButtonConfig AUX7 = new OSCRemoteButtonConfig();
        public OSCRemoteButtonConfig AUX8 = new OSCRemoteButtonConfig();
        public OSCRemoteButtonConfig AUX9 = new OSCRemoteButtonConfig();
        public int jsonVer = jsonVerMaster; //設定ファイルバージョン
    }
    [Serializable]
    class OSCRemoteButtonConfig
    {
        public string title = "AUX";
        public string address = "/VaNiiMenu/AUX";
        public int value = 1;
        public int portIndex = 0;
    }


    public void makeJSON()
    {
        //初期設定ファイルを生成
        var json = JsonUtility.ToJson(new OSCRemoteConfig());
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
            config = new OSCRemoteConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<OSCRemoteConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f, () => {
                    //OK
                    makeJSON();
                }, () => {
                    //キャンセル
                });
                config = new OSCRemoteConfig();
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
            config = new OSCRemoteConfig();
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
        reload();
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void reload()
    {
        loadJSON();
        SetMeta(1, config.AUX1);
        SetMeta(2, config.AUX2);
        SetMeta(3, config.AUX3);
        SetMeta(4, config.AUX4);
        SetMeta(5, config.AUX5);
        SetMeta(6, config.AUX6);
        SetMeta(7, config.AUX7);
        SetMeta(8, config.AUX8);
        SetMeta(9, config.AUX9);
    }

    public void OnClick(int n)
    {
        reload();

        OSCRemoteButtonConfig c = GetMeta(n);

        switch (c.portIndex) {
            case 0:
                client9000.Send(c.address, 1);
                DOVirtual.DelayedCall(0.2f, () =>
                {
                    client9000.Send(c.address, 0);
                });
                break;
            case 1:
                client39973.Send(c.address, c.value);
                break;
            case 2:
                client39974.Send(c.address, c.value);
                break;
            case 3:
                client39975.Send(c.address, c.value);
                break;
            default:
                menu.ShowDialogOK(LanguageManager.config.showdialog.PORTINDEX_INVAILD, "", 0.1f,() => {});
                break;
        }
    }


    void SetMeta(int n, OSCRemoteButtonConfig button)
    {
        AUXButton[n - 1].transform.Find("Text").GetComponent<Text>().text = button.title;
    }

    OSCRemoteButtonConfig GetMeta(int n)
    {
        switch (n)
        {
            case 1:
                return config.AUX1;
            case 2:
                return config.AUX2;
            case 3:
                return config.AUX3;
            case 4:
                return config.AUX4;
            case 5:
                return config.AUX5;
            case 6:
                return config.AUX6;
            case 7:
                return config.AUX7;
            case 8:
                return config.AUX8;
            case 9:
                return config.AUX9;
        }
        throw (new ArgumentException("AUX not found"));
    }

}
