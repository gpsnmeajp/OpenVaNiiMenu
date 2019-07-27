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

public class ImageViewerManagerScript : MonoBehaviour
{
    [SerializeField]
    private RawImage img;
    [SerializeField]
    private RectTransform imgRect;
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private uOSC.uOscClient client;
    [SerializeField]
    private TweetManagerScript tweet;

    public int no = 0;
    string[] files = null;

    string path = ""; //現在のパス
    int pathIndex = 0; //現在のパスインデックス

    WWW www = null;
    bool loaded = false;

    Vector2 defaultSize;

    const int jsonVerMaster = 3; //設定ファイルバージョン
    const string jsonPath = "config\\ImageViewer.json";
    ImageViewerConfig config = null; //読み込まれた設定

    string lastImagePath = "";
    bool forceRead = false; //強制的に読み込む

    [Serializable]
    class ImageViewerConfig
    {
        public string path1 = "";
        public string path2 = "";
        public string path3 = "";
        public bool setup_done = false;
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
        var c = new ImageViewerConfig();
        c.path1 = Directory.GetCurrentDirectory() + "\\img\\";
        c.path2 = Directory.GetCurrentDirectory() + "\\img\\";
        c.path3 = Directory.GetCurrentDirectory() + "\\img\\";
        var json = JsonUtility.ToJson(c);
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
            config = new ImageViewerConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<ImageViewerConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f, () => {
                        //OK
                        makeJSON();
                    }, () => {
                        //キャンセル
                    });
                config = new ImageViewerConfig();
                config.path1 = Directory.GetCurrentDirectory() + "\\img\\";
                config.path2 = Directory.GetCurrentDirectory() + "\\img\\";
                config.path3 = Directory.GetCurrentDirectory() + "\\img\\";
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
            config = new ImageViewerConfig();
            config.path1 = Directory.GetCurrentDirectory() + "\\img\\";
            config.path2 = Directory.GetCurrentDirectory() + "\\img\\";
            config.path3 = Directory.GetCurrentDirectory() + "\\img\\";
            menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.CORRUPT_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.CORRUPT_CONFIG_BODY, 3f, () => {
                //OK
                makeJSON();
            }, () => {
                //キャンセル
            });
        }
    }
    public void ImageLoadLocal(string path)
    {
        lastImagePath = path;
        ImageLoad("file://" + path);
    }

    public void ImageLoad(string url)
    {
        //初回読み込み時自動セットアップ
        if (!config.setup_done)
        {
            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\VRChat\\"))
            {
                menu.ShowDialogOKCancel(LanguageManager.config.showdialog.IMG_AUTOSETUP_TITLE, LanguageManager.config.showdialog.IMG_AUTOSETUP_BODY, 0.2f, () => {
                    //OK
                    config.path1 = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\VRChat\\";
                    config.setup_done = true;
                    saveJSON();
                    newFile();

                    menu.ShowDialogOK(LanguageManager.config.showdialog.IMG_AUTOSETUP_OK_TITLE, LanguageManager.config.showdialog.IMG_AUTOSETUP_OK_BODY, 0.2f, () =>
                    {
                        //OK
                    });
                }, () => {
                    //キャンセル
                    config.setup_done = true;
                    saveJSON();
                });
            }
        }
        //------------

        if (url != "")
        {
            www = new WWW(url);
        }
        else
        {
            loaded = false;
        }
    }

    //パスインデックスを進める
    public void nextPathIndex()
    {
        pathIndex++;
        if (pathIndex > 2) {
            pathIndex = 0;
        }

        //リロードさせる
        newFile();
    }
    //現在のパスインデックスに基づいたパスを返す
    void setPath() {
        switch (pathIndex) {
            case 0: path = config.path1; break;
            case 1: path = config.path2; break;
            case 2: path = config.path3; break;
            default: menu.ShowDialogOK(LanguageManager.config.showdialog.PATHINDEX_INVAILD, pathIndex.ToString(), 0.1f, () => { }); path = config.path1; break;
        }
    }

    // Use this for initialization
    void Start()
    {
        defaultSize = imgRect.sizeDelta;
        loadJSON();
        setPath();
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
                    Debug.Log(www.error);
                }
                var pixelWidth = www.texture.width;
                var pixelHeight = www.texture.height;

                var width = defaultSize.x;
                var height = pixelHeight * defaultSize.x / pixelWidth;

                if (height > defaultSize.y)
                {
                    width = pixelWidth * defaultSize.y / pixelHeight;
                    height = defaultSize.y;
                }
                imgRect.sizeDelta = new Vector2(width,height);

//                img.material.SetTexture("_MainTex", www.texture);
                img.texture = www.texture;
                loaded = true;
                www.Dispose();
                www = null;
            }
        }
    }

    public void nextFile()
    {
        no++;
        loadImage();
    }

    public void prevFile()
    {
        no--;
        loadImage();
    }

    public void newFile()
    {
        no=0;
        if (!getList())
        {
            return;
        }
        loadImage();
    }

    public bool getList() {
        setPath();

        files = Directory.GetFiles(path, "*.*"); //帰ってくるのはフォルダ含む相対パス
        
        if (files.Length > 2000 && !forceRead) {
            files = null;
            menu.ShowDialogOKCancel(LanguageManager.config.showdialog.TOOMANYFILES_TITLE, LanguageManager.config.showdialog.TOOMANYFILES_BODY, 0.1f, () => {
                //無視して読み込む
                menu.ShowDialogOK(LanguageManager.config.showdialog.FORCEREAD_TITLE, LanguageManager.config.showdialog.FORCEREAD_BODY, 0.2f, () => { });
                forceRead = true;
            }, () => {
                //実行しない
            });
            return false;
        }

        Dictionary<string, DateTime> LastWriteList = new Dictionary<string, DateTime>();
        for (int i = 0; i < files.Length; i++)
        {
            LastWriteList.Add(files[i], File.GetLastWriteTime(files[i]));
//            Debug.Log(i.ToString() + " : " + files[i] + " : " + LastWriteList[files[i]]);
        }

        Array.Sort(files, (arg1, arg2) => {
            return DateTime.Compare(LastWriteList[arg2], LastWriteList[arg1]);
        });
        return true;
    }

    //ファイル一覧を取得し、画像を読み込む。あとがない場合は繰り返す
    public void loadImage()
    {
        loadJSON();
        setPath();

        if (!Directory.Exists(path))
        {
            config = new ImageViewerConfig();
            setPath();

            path = Directory.GetCurrentDirectory() + "\\img\\";
            menu.ShowDialogOKCancel(LanguageManager.config.showdialog.FOLDER_NOT_FOUND_TITLE, LanguageManager.config.showdialog.FOLDER_NOT_FOUND_BODY1 + jsonPath + LanguageManager.config.showdialog.FOLDER_NOT_FOUND_BODY2, 0.1f,
            () => {
                //OK
                makeJSON();
            }, () => {
                //キャンセル
            });
            return;
        }

        //ファイル一覧を取得
        if (files == null) {
            if (!getList()) {
                return;
            }
        }

        if (files.Length == 0) {
            menu.ShowDialogOK(LanguageManager.config.showdialog.FILE_NOT_FOUND, "",0.1f,()=>{ });
            return;
        }

        if (no > files.Length - 1)
        {
            no = 0;
        }
        if (no < 0)
        {
            no = files.Length - 1;
        }
        ImageLoadLocal(files[no]);
    }

    //現在の画像のパスをOSC連携アプリケーションに送信する
    public void send() {
        menu.ShowDialogOKCancel(LanguageManager.config.showdialog.OSC_APP_SEND, LanguageManager.config.showdialog.OSC_PATH_SEND + lastImagePath, 0.1f, () => {
            client.Send("/VaNiiMenu/ImagePathSend", lastImagePath);

            menu.ShowDialogOK(LanguageManager.config.showdialog.SEND_COMPLETE, lastImagePath, 0.2f, () => { });
        }, () => {
            menu.ShowDialogOK(LanguageManager.config.showdialog.SEND_CANCEL, "", 0.2f, () => { });
        });
    }

    //tweet側の画像を添付する
    public void AttachImage()
    {
        tweet.Attach(lastImagePath);
    }


}
