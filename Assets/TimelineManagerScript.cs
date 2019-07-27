/*
VaNiiMenu

Copyright (c) 2019, gpsnmeajp
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
using UnityEngine.Events;
using System;
using System.IO;
using System.Text;

public class TimelineManagerScript : MonoBehaviour {
    [SerializeField]
    private Text text;
    [SerializeField]
    private TestScrollWorkerScript worker;
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private OnlineManagerScript online;
    [SerializeField]
    private AudioManagerScript AudioMan;

    //ホームTLを読み込む
    public void getHome()
    {
        //通信中に操作は受け付けない
        if (online.sending) {
            AudioMan.PlayCancelSound();
            return;
        }
        AudioMan.PlayApplySound();

        //受信開始
        text.text = "Wait...";
        online.getHome(() => {
            text.text = "-----------------------Home Timeline--------------------------\n";
            if (online.response.successed)
            {
                //1行ずつ表示(フォーマットはHelper側にお任せ)
                foreach (string s in online.response.text)
                {
                    text.text += s+ "\n---------------------------------------------------------------\n";
                }
            }
            else {
                //異常発生時
                text.text += online.response.exception;
            }
            //スクロールを戻す
            worker.resetPos();
        });
    }
    public void getReply()
    {
        //通信中に操作は受け付けない
        if (online.sending)
        {
            AudioMan.PlayCancelSound();
            return;
        }
        AudioMan.PlayApplySound();

        //受信開始
        text.text = "Wait...";
        online.getReply(() => {
            text.text = "------------------------Reply Timeline------------------------\n";
            if (online.response.successed)
            {
                //1行ずつ表示(フォーマットはHelper側にお任せ)
                foreach (string s in online.response.text)
                {
                    text.text += s + "\n---------------------------------------------------------------\n";
                }
            }
            else
            {
                //異常発生時
                text.text += online.response.exception;
            }
            worker.resetPos();
        });
    }

    // Use this for initialization
    void Start () {
        text.text = "";
    }

    // Update is called once per frame
    void Update () {
		
	}
}
