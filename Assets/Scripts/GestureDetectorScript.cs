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
using EasyLazyLibrary;
using DG.Tweening;

public class GestureDetectorScript : MonoBehaviour {
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private EasyOpenVROverlayForUnity eovro;
    [SerializeField]
    private BatteryMeterWorkerScript batteryWorker;
    [SerializeField]
    private Text PeakText;

    [SerializeField]
    private MenuCursorScript CursorL;
    [SerializeField]
    private MenuCursorScript CursorR;

    [SerializeField]
    private AudioManagerScript AudioMan;

    [SerializeField]
    private Text FPSMonitor2;
    [SerializeField]
    private ResolutionManagerScript ResoMan;


    EasyOpenVRUtil util = new EasyOpenVRUtil();

    public float AccelerationPeak = 0f;
    public float BatteryUpdateTimer = 0f;
    public float MissSwingTime = 0f;

    public double FrameTiming = 0f;


    const int jsonVerMaster = 1; //設定ファイルバージョン
    const string jsonPath = "config\\Gesture.json";
    GestureConfig config = null; //読み込まれた設定

    [Serializable]
    class GestureConfig
    {
        public bool lockmode = false; //特定のキーと同時に操作する必要があるモード
        public bool taplockmode = false; //特定のキーと同時にタップする必要があるモード
        public ulong unlockkey = 4; //解除キー(デフォルトはグリップ)

        public bool detectLeftHand = true; //左手の操作に反応する
        public bool detectRightHand = true; //左手の操作に反応する
        public bool slideClose = false; //横振りで閉じれるようになる

        public float speeddown = 3.4f;
        public float speedup = 3.4f;
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
        var json = JsonUtility.ToJson(new GestureConfig());
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
            config = new GestureConfig();
            makeJSON();
        }

        //ファイルの読込を試行
        try
        {
            //ファイルの内容を一括読み出し
            string jsonString = File.ReadAllText(jsonPath, new UTF8Encoding(false));
            //設定クラスをJSONデコードして生成
            config = JsonUtility.FromJson<GestureConfig>(jsonString);

            //ファイルのバージョンが古い場合は、デフォルト設定にして警告(nullの可能性も考慮してtry内)
            if (config.jsonVer != jsonVerMaster)
            {
                menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.OLD_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.OLD_CONFIG_BODY, 3f, () => {
                            //OK
                            makeJSON();
                    }, () => {
                            //キャンセル
                        });
                config = new GestureConfig();
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
            config = new GestureConfig();
            menu.ShowDialogOKCancel(LanguageManager.config.jsonloaders.CORRUPT_CONFIG_HEAD, "" + jsonPath + LanguageManager.config.jsonloaders.CORRUPT_CONFIG_BODY, 3f, () => {
                    //OK
                    makeJSON();
            }, () => {
                    //キャンセル
                });
        }
    }

    //振りロックモードの変更と保存
    public void lockmode(bool islocked)
    {
        config.lockmode = islocked;
        config.taplockmode = false;
        saveJSON();
    }

    //タップ&振りロックモードの変更と保存
    public void fulllockmode(bool islocked)
    {
        config.lockmode = islocked;
        config.taplockmode = islocked;
        saveJSON();
    }

    //横振りクローズの有効無効
    public void slideclose(bool isEnable)
    {
        config.slideClose = isEnable;
        saveJSON();
    }

    //左手反応するか
    public void learnUnlockKey()
    {
        ulong Leftbutton, Rightbutton;
        ulong key = 0;

        if (util.GetControllerButtonPressed(util.GetLeftControllerIndex(), out Leftbutton)) {
            key |= Leftbutton;
        }
        if (util.GetControllerButtonPressed(util.GetRightControllerIndex(), out Rightbutton)) {
            key |= Rightbutton;
        }
        if (key == 0) {
            menu.ShowDialogOK(LanguageManager.config.showdialog.BUTTON_NOT_FOUND, "",0.5f, () => { });
            return;
        }

        config.unlockkey = key;
        saveJSON();
        menu.ShowDialogOK(LanguageManager.config.showdialog.BUTTON_REGISTER, "key: "+key.ToString(), 0.5f, () => { });
    }


    //左手反応するか
    public void detectLeftHand(bool isDetect)
    {
        config.detectLeftHand = isDetect;
        saveJSON();
    }

    //右手反応するか
    public void detectRightHand(bool isDetect)
    {
        config.detectRightHand = isDetect;
        saveJSON();
    }

    public void resetPeak() {
        AccelerationPeak = 0;
        PeakText.text = string.Format("Acceleration\nSet: {0:#.#}", AccelerationPeak);
    }
    public void setPeak() {
        config.speeddown = AccelerationPeak;
        config.speedup = AccelerationPeak;
        saveJSON();
    }
    

    // Use this for initialization
    void Start () {
        loadJSON();
        /*
        if (config.taplockmode == true)
        {
            menu.ShowDialogOK("全操作ロックが有効になっています", "設定したボタンを押しながら操作してください。\n(わからなくなったときはGesture.jsonを削除して再起動してください)", 4f, () => { });
        }
        */

    }

    // Update is called once per frame
    void FixedUpdate () {
        //初期化されていないとき初期化する
        if (!util.IsReady())
        {
            util.Init();
            return;
        }

        //fpsに依存しない更新にする
        util.SetAutoUpdate(false);
        util.ClearPredictedTime();
        util.Update();

        //フレームタイミングを表示
        FrameTiming = FrameTiming*0.99 + util.GetCompositorFrameTime()*0.01f;

        FPSMonitor2.text = String.Format("{0:###.#}", FrameTiming + 0.05f);
        //30fpsを切ったら自動でセーフモードへ
        if (FrameTiming > 33.3) {
            ResoMan.LowResolution(false);
        }

        //秒に一回Battery残量を反映
        if (BatteryUpdateTimer > 1.0f)
        {
            //コントローラ
            batteryWorker.PercentL = util.GetDeviceBatteryPercentage(util.GetLeftControllerIndex()) / 100.0f;
            batteryWorker.PercentR = util.GetDeviceBatteryPercentage(util.GetRightControllerIndex()) / 100.0f;

            //トラッカー
            List<uint> list = util.GetViveTrackerIndexList();
            if (list.Count > 0)
            {
                batteryWorker.PercentT1 = util.GetDeviceBatteryPercentage(list[0]) / 100.0f;
            }
            else {
                batteryWorker.PercentT1 = float.NaN;
            }
            if (list.Count > 1)
            {
                batteryWorker.PercentT2 = util.GetDeviceBatteryPercentage(list[1]) / 100.0f;
            }
            else
            {
                batteryWorker.PercentT2 = float.NaN;
            }
            if (list.Count > 2)
            {
                batteryWorker.PercentT3 = util.GetDeviceBatteryPercentage(list[2]) / 100.0f;
            }
            else
            {
                batteryWorker.PercentT3 = float.NaN;
            }

            BatteryUpdateTimer = 0f;
        }
        else {
            BatteryUpdateTimer += Time.deltaTime;
        }


        //コントローラのボタン入力と取得可能かをチェック
        ulong Leftbutton = 0, Rightbutton = 0, HMDbutton = 0;
        bool leftok = util.GetControllerButtonPressed(util.GetLeftControllerIndex(), out Leftbutton);
        bool rightok = util.GetControllerButtonPressed(util.GetRightControllerIndex(), out Rightbutton);
        //bool hmdok = util.GetControllerButtonPressed(util.GetHMDIndex(), out HMDbutton);

        /*
        if (hmdok) {
            if ((HMDbutton & 0x80000000) != 0)
            {
                //かぶってる
            }
            else {
                //かぶってない
            }
        }*/

        //左手の加速度を取得(左手が有効な場合)
        Vector3 leftHandVelocity = Vector3.zero;
        if (leftok && config.detectLeftHand)
        {
            var trans = util.GetLeftControllerTransform();
            if (trans != null)
            {
                leftHandVelocity = trans.velocity;
            }
        }

        //右手の加速度を取得(右手が有効な場合)
        Vector3 rightHandVelocity = Vector3.zero;
        if (rightok && config.detectRightHand)
        {
            var trans = util.GetRightControllerTransform();
            if (trans != null)
            {
                rightHandVelocity = trans.velocity;
            }

        }

        //タップロック反映
        if (config.taplockmode)
        {
            if ((Leftbutton == config.unlockkey) || (Rightbutton == config.unlockkey))
            {
                //ボタンが押されているとき
                eovro.tapEnable = true;

                CursorL.locked = false;
                CursorR.locked = false;
            }
            else {
                //ボタンが押されていないとき
                eovro.tapEnable = false;

                CursorL.locked = true;
                CursorR.locked = true;
            }
        }
        else {
            //ロック関係なし
            eovro.tapEnable = true;

            CursorL.locked = false;
            CursorR.locked = false;
        }

        //ピーク検出
        if (AccelerationPeak < Math.Abs(leftHandVelocity.y))
        {
            AccelerationPeak = Math.Abs(leftHandVelocity.y);
            PeakText.text = string.Format("Acceleration\nSet: {0:#.#}", AccelerationPeak);
        }
        if (AccelerationPeak < Math.Abs(rightHandVelocity.y))
        {
            AccelerationPeak = Math.Abs(rightHandVelocity.y);
            PeakText.text = string.Format("Acceleration\nSet: {0:#.#}", AccelerationPeak);
        }

        //腕振り下ろしを判定
        if (leftHandVelocity.y < -config.speeddown)
        {
            //ロックモードではない or グリップボタンだけが押されている(&ロックモード)
            if (!config.lockmode || (Leftbutton == config.unlockkey))
            {
                menu.IsRightHand = false;
                if (menu.showing == false)
                {
                    menu.MenuStart = true;
                    MissSwingTime = Time.time; //現在時刻を記録
                }
                else {
                    //空振り

                    //メニューを出してから十分時間経ってるのに振った
                    if (Time.time > MissSwingTime + 1) {
                        AudioMan.PlayCancelSound(); //振りミス音を鳴らす
                        MissSwingTime = Time.time; //現在時刻を記録
                    }
                }
            }
        }
        if (rightHandVelocity.y < -config.speeddown)
        {
            //ロックモードではない or グリップボタンだけが押されている(&ロックモード)
            if (!config.lockmode || (Rightbutton == config.unlockkey))
            {
                menu.IsRightHand = true;
                if (menu.showing == false)
                {
                    menu.MenuStart = true;
                    MissSwingTime = Time.time; //現在時刻を記録
                }
                else
                {
                    //空振り

                    //メニューを出してから十分時間経ってるのに振った
                    if (Time.time > MissSwingTime + 1)
                    {
                        AudioMan.PlayCancelSound(); //振りミス音を鳴らす
                        MissSwingTime = Time.time; //現在時刻を記録
                    }
                }
            }
        }

        //腕振り上げ判定
        if (leftHandVelocity.y > config.speedup || rightHandVelocity.y > config.speedup)
        {
            //ロックモードではない or グリップボタンだけが押されている(&ロックモード)
            if (!config.lockmode || (Rightbutton == config.unlockkey) || (Leftbutton == config.unlockkey))
            {
                menu.MenuEndFunc(0);
            }
        }

        //腕横振り判定
        if (Math.Abs(leftHandVelocity.z) > config.speedup && config.slideClose)
        {
            //ロックモードではない or グリップボタンだけが押されている(&ロックモード)
            if (!config.lockmode || Leftbutton == config.unlockkey)
            {
                //方向判定
                if (leftHandVelocity.z < 0)
                {
                    menu.MenuEndFunc(1);
                }
                else {
                    menu.MenuEndFunc(2);
                }
            }
        }
        if (Math.Abs(rightHandVelocity.z) > config.speedup && config.slideClose)
        {
            //ロックモードではない or グリップボタンだけが押されている(&ロックモード)
            if (!config.lockmode || Rightbutton == config.unlockkey)
            {
                //方向判定
                if (rightHandVelocity.z < 0)
                {
                    menu.MenuEndFunc(1);
                }
                else
                {
                    menu.MenuEndFunc(2);
                }
            }
        }


        //Debug.Log(util.GetRightControllerTransform().velocity.x);
    }
}
