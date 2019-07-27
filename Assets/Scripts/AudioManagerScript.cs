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
using UnityEngine.Events;
using UnityEngine.UI;

public class AudioManagerScript : MonoBehaviour {
    public MenuManager menu;

    [SerializeField]
    private AudioClip StartUpSound;
    [SerializeField]
    private AudioClip OpenSound;
    [SerializeField]
    private AudioClip CloseSound;
    [SerializeField]
    private AudioClip SelectSound;
    [SerializeField]
    private AudioClip ApplySound;
    [SerializeField]
    private AudioClip NotificationSound;
    [SerializeField]
    private AudioClip QuestionSound;
    [SerializeField]
    private AudioClip CancelSound;
    [SerializeField]
    private AudioClip AlarmSound;

    private AudioSource audiosrc;
    [SerializeField]
    private AudioSource AlarmAudioSrc;
    [SerializeField]
    private Text VolumeText;


    //-----------------------------

    const int jsonVerMaster = 1; //設定ファイルバージョン
    const string jsonPath = "config\\Audio.json";
    AudioConfig config = null; //読み込まれた設定

    [Serializable]
    class AudioConfig
    {
        public string path = "Sounds/";
        public float volume = 0.2f;
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
        var json = JsonUtility.ToJson(new AudioConfig());
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
            config = new AudioConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<AudioConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f, () => {
                        //OK
                        makeJSON();
                    }, () => {
                        //キャンセル
                    });
                config = new AudioConfig();
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
            config = new AudioConfig();
            menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.CORRUPT_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.CORRUPT_CONFIG_BODY, 3f, () => {
                //OK
                makeJSON();
            }, () => {
                //キャンセル
            });
        }
    }

    public void setNormalSound()
    {
        config.path = "Sounds/";
        saveJSON();
    }

    public void setBeepSound()
    {
        config.path = "BeepSounds/";
        saveJSON();
    }

    public void ApplyVolume() {
        audiosrc.volume = config.volume;
        AlarmAudioSrc.volume = config.volume;
        VolumeText.text = "Vol: "+((int)(config.volume*100.0+0.5)).ToString()+"%";
    }
    
    //音量保存する機能なんで今までなかったんだ？
    public void setVolume(float vol)
    {
        config.volume = vol;
        ApplyVolume();
        saveJSON();
    }
    public void upVolume()
    {
        if (config.volume > 1.0f-0.05f)
        {
            config.volume = 1.0f;
            PlayCancelSound();
        }
        else {
            config.volume += 0.05f;
            PlayApplySound();
        }
        ApplyVolume();
        saveJSON();
    }
    public void downVolume()
    {
        if (config.volume < 0.05f)
        {
            config.volume = 0f;
            PlayCancelSound();
        }
        else {
            config.volume -= 0.05f;
            PlayApplySound();
        }
        ApplyVolume();
        saveJSON();
    }

    //動的音声読み込み
    IEnumerator Start()
    {
        audiosrc = GetComponent<AudioSource>();
        loadJSON();
        ApplyVolume();

        var path = "file://" + Application.dataPath + "/StreamingAssets/"+config.path;
        string f;

        //---------------------------
        f = path + "Startup.wav";
        using (WWW www = new WWW(f))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error + " in " + f);
            }
            StartUpSound = www.GetAudioClip(false);
        }

        //---------------------------
        f = path + "Open.wav";
        using (WWW www = new WWW(f))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error + " in " + f);
            }
            OpenSound = www.GetAudioClip(false);
        }

        //---------------------------
        f = path + "Close.wav";
        using (WWW www = new WWW(f))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error + " in " + f);
            }
            CloseSound = www.GetAudioClip(false);
        }

        //---------------------------
        f = path + "Select.wav";
        using (WWW www = new WWW(f))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error + " in " + f);
            }
            SelectSound = www.GetAudioClip(false);
        }


        //---------------------------
        f = path + "Apply.wav";
        using (WWW www = new WWW(f))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error + " in " + f);
            }
            ApplySound = www.GetAudioClip(false);
        }

        //---------------------------
        f = path + "Notification.wav";
        using (WWW www = new WWW(f))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error + " in " + f);
            }
            NotificationSound = www.GetAudioClip(false);
        }

        //---------------------------
        f = path + "Question.wav";
        using (WWW www = new WWW(f))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error + " in " + f);
            }
            QuestionSound = www.GetAudioClip(false);
        }

        //---------------------------
        f = path + "Cancel.wav";
        using (WWW www = new WWW(f))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error + " in " + f);
            }
            CancelSound = www.GetAudioClip(false);
        }
        //---------------------------
        f = path + "Alarm.wav";
        using (WWW www = new WWW(f))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error + " in " + f);
            }
            AlarmSound = www.GetAudioClip(false);
        }
        AlarmAudioSrc.clip = AlarmSound;

        //---------------------------
        audiosrc.PlayOneShot(StartUpSound);
    }
    /*
        private void Start()
        {
            audiosrc = GetComponent<AudioSource>();

            var path = Application.dataPath + "/StreamingAssets";



        }
    */

    public void PlayOpenSound()
    {
        audiosrc.PlayOneShot(OpenSound);
    }

    public void PlayCloseSound()
    {
        audiosrc.PlayOneShot(CloseSound);
    }

    public void PlaySelectSound()
    {
        audiosrc.PlayOneShot(SelectSound);
    }

    public void PlayApplySound()
    {
        audiosrc.PlayOneShot(ApplySound);
    }
    public void PlayNotificationSound()
    {
        audiosrc.PlayOneShot(NotificationSound);
    }
    public void PlayQuestionSound()
    {
        audiosrc.PlayOneShot(QuestionSound);
    }
    public void PlayCancelSound()
    {
        audiosrc.PlayOneShot(CancelSound);
    }
    //Alarmの再生・停止はAlarmManagerで行う
}
