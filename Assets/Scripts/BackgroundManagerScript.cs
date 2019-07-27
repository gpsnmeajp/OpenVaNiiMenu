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

public class BackgroundManagerScript : MonoBehaviour
{
    [SerializeField]
    private RawImage bgimg;
    [SerializeField]
    private MenuManager menu;

    WWW www = null;
    bool loaded = false;

    const int jsonVerMaster = 1; //設定ファイルバージョン
    const string jsonPath = "config\\Background.json";
    BackgroundConfig config = null; //読み込まれた設定

    [Serializable]
    class BackgroundConfig
    {
        public bool enable = false;
        public string filename = "background.png";
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
        var json = JsonUtility.ToJson(new BackgroundConfig());
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
            config = new BackgroundConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<BackgroundConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f, () => {
                        //OK
                        makeJSON();
                    }, () => {
                        //キャンセル
                    });
                config = new BackgroundConfig();
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
            config = new BackgroundConfig();
            menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.CORRUPT_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.CORRUPT_CONFIG_BODY, 3f, () => {
                //OK
                makeJSON();
            }, () => {
                //キャンセル
            });
        }
    }



    public void ImageLoad(string url)
    {
        if (url != "")
        {
            www = new WWW(url);
        }
        else
        {
            loaded = false;
        }
    }

    // Use this for initialization
    void Start()
    {
        bgimg.enabled = false;
        loadJSON();
        if (config.enable) {
            ImageLoad("file://" + Directory.GetCurrentDirectory() + "\\" + config.filename);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (www != null)
        {
            if (www.isDone)
            {
                if (www.error != null)
                {
                    Debug.LogError(www.error);
                }
                else {
                    bgimg.texture = www.texture;
                    bgimg.enabled = true;
                }

                loaded = true;
                www.Dispose();
                www = null;
            }
        }
    }
}
