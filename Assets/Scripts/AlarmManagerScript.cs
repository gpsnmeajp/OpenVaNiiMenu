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

public class AlarmManagerScript : MonoBehaviour
{
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private AudioManagerScript AudioMan;
    [SerializeField]
    private Text AlarmHourSettingScreen;
    [SerializeField]
    private Text AlarmMinutesSettingScreen;
    [SerializeField]
    private Text AlarmHourAlarmWindow;
    [SerializeField]
    private Text AlarmMinutesAlarmWindow;
    [SerializeField]
    private GameObject AlarmWindowsObject;

    [SerializeField]
    private bool AlarmTestBegin = false;
    [SerializeField]
    private bool AlarmTestStop = false;
    [SerializeField]
    private bool AlarmRinging = false;


    private AudioSource AlarmAudioSrc;
    private Image AlarmWindowImage;
    private Color AlarmWindowImageColor;



    public int HourHigh = 0;
    public int HourLow = 0;
    public int MinutesHigh = 0;
    public int MinutesLow = 0;

    public bool Enabled = false;
    public int AlarmHour = 0;
    public int AlarmMinutes = 0;
    public int TimeHour = 0;
    public int TimeMinutes = 0;

    //-----------------------------

    const int jsonVerMaster = 1; //設定ファイルバージョン
    const string jsonPath = "config\\Alarm.json";
    AlarmConfig config = null; //読み込まれた設定

    [Serializable]
    class AlarmConfig
    {
        public int Hour = 0;
        public int Minutes = 0;
        public bool Enable = false;

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
        var json = JsonUtility.ToJson(new AlarmConfig());
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
            config = new AlarmConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<AlarmConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f, () => {
                        //OK
                        makeJSON();
                    }, () => {
                        //キャンセル
                    });
                config = new AlarmConfig();
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
            config = new AlarmConfig();
            menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.CORRUPT_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.CORRUPT_CONFIG_BODY, 3f, () => {
                //OK
                makeJSON();
            }, () => {
                //キャンセル
            });
        }
    }

    //致命的な事故を避けるため、常に変数内容を確認する
    public void ValidationTemporaryVariable() {
        bool error = false;
        if ((HourHigh * 10 + HourLow) > 23)
        {
            HourHigh = 0;
            HourLow = 0;
            error = true;
        }
        if ((MinutesHigh * 10 + MinutesLow) > 59)
        {
            MinutesHigh = 0;
            MinutesLow = 0;
            error = true;
        }

        if (HourHigh > 2 || HourHigh < 0)
        {
            HourHigh = 0;
            error = true;
        }
        if (HourLow > 9 || HourLow < 0)
        {
            HourLow = 0;
            error = true;
        }
        if (MinutesHigh > 5 || MinutesHigh < 0)
        {
            MinutesHigh = 0;
            error = true;
        }
        if (MinutesLow > 9 || MinutesLow < 0)
        {
            MinutesLow = 0;
            error = true;
        }
        if (config.Hour > 23 || config.Hour < 0)
        {
            config.Hour = 0;
            error = true;
        }
        if (config.Minutes > 59 || config.Minutes < 0)
        {
            config.Hour = 0;
            error = true;
        }
        if (error) {
            menu.ShowDialogOK(LanguageManager.config.showdialog.PROGRAM_ERROR, LanguageManager.config.showdialog.ALARM_ERROR, 00.5f, () => { });
        }
    }

    //Configから一時変数に反映する
    public void ApplyAlarmSettingFromConfig() {
        HourHigh = (int)(config.Hour / 10);
        HourLow = (config.Hour%10);
        MinutesHigh = (int)(config.Minutes / 10);
        MinutesLow = (config.Minutes % 10);
    }

    //Configへ一時変数から反映する
    public void ApplyAlarmSettingToConfig()
    {
        config.Hour = HourHigh * 10 + HourLow;
        config.Minutes = MinutesHigh * 10 + MinutesLow;
    }

    //一時変数から設定画面に反映する
    public void ApplyAlarmSettingFromTemporary() {
        AlarmHourSettingScreen.text = string.Format("{0:00}", HourHigh*10 + HourLow);
        AlarmMinutesSettingScreen.text = string.Format("{0:00}", MinutesHigh*10 + MinutesLow);
    }

    //Configからアラームウィンドウに反映する(Enableを反映する)
    public void ApplyAlarmWindowFromConfig() {
        AlarmHourAlarmWindow.text = string.Format("{0:00}", config.Hour);
        AlarmMinutesAlarmWindow.text = string.Format("{0:00}", config.Minutes);
        AlarmWindowsObject.SetActive(config.Enable);
    }

    //アラーム停止
    public void AlarmDisable()
    {
        AlarmRinging = false; //アラーム鳴動状態:停止
        config.Enable = false; //アラーム設定:無効
        saveJSON();//時刻と無効を反映
        //ApplyAlarmSettingFromConfig(); //設定画面に反映(するとユーザーが使いにくいのでコメントアウト)
        ApplyAlarmWindowFromConfig(); //アラーム画面に反映

        DOTween.Sequence()
            .Append(
                AlarmAudioSrc.DOFade(0, 0.5f)
            )
            .AppendCallback(() =>
            {
                AlarmAudioSrc.Stop();
                AudioMan.ApplyVolume(); //音量をもとに戻す
            })
            .Play();

        AlarmWindowImage.color = AlarmWindowImageColor; //色をもとに戻す
    }
    public void AlarmEnable() {
        ApplyAlarmSettingToConfig(); //設定画面からアラーム設定に反映
        AlarmRinging = false; //アラーム鳴動状態:停止
        config.Enable = true; //アラーム設定:有効
        saveJSON(); //時刻と有効を反映

        ApplyAlarmSettingFromConfig(); //設定画面に反映
        ApplyAlarmWindowFromConfig(); //アラーム画面に反映

        AudioMan.ApplyVolume(); //音量をもとに戻す
        AlarmWindowImage.color = AlarmWindowImageColor; //色をもとに戻す
    }

    public void OnTime() {
        AlarmAudioSrc.PlayDelayed(1f);
        AudioMan.ApplyVolume();
        menu.ShowDialogOK(LanguageManager.config.showdialog.ONTIME, LanguageManager.config.showdialog.ALARMSTOP, 0f, () => { });
        AlarmRinging = true; //アラーム鳴動状態:鳴動
    }

    public void HourHighUp()
    {
        HourHigh++;
        if (HourHigh > 2) {
            HourHigh = 0;
        }
        if (HourHigh == 2) {
            if (HourLow > 3)
            {
                HourLow = 0;
            }
        }
    }
    public void HourLowUp()
    {
        HourLow++;
        if (HourHigh >= 2)
        {
            if (HourLow > 3)
            {
                HourLow = 0;
            }
        }
        else {
            if (HourLow > 9)
            {
                HourLow = 0;
            }
        }
    }
    public void MinutesHighUp()
    {
        MinutesHigh++;
        if (MinutesHigh > 5)
        {
            MinutesHigh = 0;
        }
    }
    public void MinutesLowUp()
    {
        MinutesLow++;
        if (MinutesLow > 9) {
            MinutesLow = 0;
        }
    }

    public void HourHighDown()
    {
        HourHigh--;
        if (HourHigh < 0) {
            HourHigh = 2;
            if (HourLow > 3)
            {
                HourLow = 3;
            }
        }
    }
    public void HourLowDown()
    {
        HourLow--;
        if (HourHigh >= 2)
        {
            if (HourLow < 0)
            {
                HourLow = 3;
            }
        }
        else {
            if (HourLow < 0)
            {
                HourLow = 9;
            }
        }
    }
    public void MinutesHighDown()
    {
        MinutesHigh--;
        if (MinutesHigh < 0)
        {
            MinutesHigh = 5;
        }
    }
    public void MinutesLowDown()
    {
        MinutesLow--;
        if (MinutesLow < 0)
        {
            MinutesLow = 9;
        }
    }

    void Start()
    {
        AlarmWindowImage = AlarmWindowsObject.GetComponent<Image>();
        AlarmWindowImageColor = AlarmWindowImage.color;

        AlarmAudioSrc = GetComponent<AudioSource>();
        AlarmAudioSrc.loop = true;
        //音源のロードはAudioManagerで行う。

        loadJSON();
        ApplyAlarmSettingFromConfig(); //一時変数に反映
        ApplyAlarmSettingFromTemporary(); //設定画面に反映
        ApplyAlarmWindowFromConfig(); //アラームウィンドウに反映
    }



    void Update()
    {
        ValidationTemporaryVariable(); //変数バグチェック
        ApplyAlarmSettingFromTemporary();  //設定画面に反映

        //テスト用機能
        if (AlarmTestBegin)
        {
            AlarmEnable();
            //OnTime();
            AlarmTestBegin = false;
        }
        if (AlarmTestStop)
        {
            AlarmDisable();
            AlarmTestStop = false;
        }

        //時間チェック
        Enabled = config.Enable;
        AlarmHour = (int)config.Hour;
        AlarmMinutes = (int)config.Minutes;
        TimeHour = (int)DateTime.Now.Hour;
        TimeMinutes = (int)DateTime.Now.Minute;

        //アラーム時間チェック
        if (AlarmHour == TimeHour && AlarmMinutes == TimeMinutes && config.Enable && !AlarmRinging) {
            OnTime();
        }

        //点滅
        if (AlarmRinging) {
            if (DateTime.Now.Millisecond > 500)
            {
                AlarmWindowImage.color = AlarmWindowImageColor;
            }
            else {
                AlarmWindowImage.color = new Color32(255,0,0,255);
            }
        }

    }
}
