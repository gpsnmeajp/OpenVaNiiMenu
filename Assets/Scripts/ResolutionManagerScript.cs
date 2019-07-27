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

public class ResolutionManagerScript : MonoBehaviour
{
    public bool SafeMode = false;

    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private Camera OverlayCamera;
    [SerializeField]
    private EasyOpenVROverlayForUnity EOVRO;
    [SerializeField]
    private RenderTexture NormalResolutionRenderTexture;
    [SerializeField]
    private RenderTexture HighResolutionRenderTexture;
    [SerializeField]
    private RenderTexture LowResolutionRenderTexture;
    [SerializeField]
    private Text SafeModeText;
    [SerializeField]
    private GameObject DisableSafeModeButton;

    //-----------------------------

    const int jsonVerMaster = 3; //設定ファイルバージョン
    const string jsonPath = "config\\Resolution.json";
    ResolutionConfig config = null; //読み込まれた設定

    [Serializable]
    class ResolutionConfig
    {
        public bool HighResolutionEnable = false;
        public bool LowResolutionEnable = false;
        public bool disableSafeMode = false;
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
        var json = JsonUtility.ToJson(new ResolutionConfig());
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
            config = new ResolutionConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<ResolutionConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f, () => {
                        //OK
                        makeJSON();
                    }, () => {
                        //キャンセル
                    });
                config = new ResolutionConfig();
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
            config = new ResolutionConfig();
            menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.CORRUPT_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.CORRUPT_CONFIG_BODY, 3f, () => {
                //OK
                makeJSON();
            }, () => {
                //キャンセル
            });
        }
    }




    void Start()
    {
        startup();
    }

    void startup()
    {
        loadJSON();
        if (config.HighResolutionEnable)
        {
            HighResolution(false);
            return;
        }
        if (config.LowResolutionEnable)
        {
            LowResolution(false);
            return;
        }
        NormalResolution(false);
    }

    //表示時
    public void HighFPS()
    {
        if (!SafeMode)
        {
            Application.targetFrameRate = 90;
            SafeModeText.text = "";
        }
        else {
            Application.targetFrameRate = 30;
            SafeModeText.text = "Safe\nMode";
        }
    }

    //待機時
    public void LowFPS()
    {
        Application.targetFrameRate = 15;
    }

    public void HighResolution(bool save)
    {
        EOVRO.renderTexture = HighResolutionRenderTexture;
        OverlayCamera.targetTexture = HighResolutionRenderTexture;

        if (save) {
            config.HighResolutionEnable = true;
            config.LowResolutionEnable = false;
            saveJSON();
        }

        SafeMode = false;
        HighFPS();
        DisableSafeModeButton.SetActive(false);
    }
    public void NormalResolution(bool save)
    {
        EOVRO.renderTexture = NormalResolutionRenderTexture;
        OverlayCamera.targetTexture = NormalResolutionRenderTexture;

        if (save)
        {
            config.HighResolutionEnable = false;
            config.LowResolutionEnable = false;
            saveJSON();
        }
        SafeMode = false;
        HighFPS();
        DisableSafeModeButton.SetActive(false);
    }

    public void LowResolution(bool save)
    {
        if (config.disableSafeMode) {
            SafeMode = false;
            HighFPS();
            DisableSafeModeButton.SetActive(false);
            return;
        }

        EOVRO.renderTexture = LowResolutionRenderTexture;
        OverlayCamera.targetTexture = LowResolutionRenderTexture;

        if (save)
        {
            config.HighResolutionEnable = false;
            config.LowResolutionEnable = true;
            saveJSON();
        }
        SafeMode = true;
        HighFPS();
        DisableSafeModeButton.SetActive(true);
    }

    void Update()
    {

    }
}
