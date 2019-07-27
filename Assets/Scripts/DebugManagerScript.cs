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

public class DebugManagerScript : MonoBehaviour
{
    [SerializeField]
    private Text text;
    [SerializeField]
    private TestScrollWorkerScript worker;
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private EasyOpenVROverlayForUnity eovro;

    private EasyOpenVRUtil util = new EasyOpenVRUtil();

    void Start()
    {

    }

    void Update()
    {
    }

    //デバッグ情報を更新
    public void renew() {
        //初期化されていないとき初期化する
        if (!util.IsReady())
        {
            util.Init();
        }
        text.text = "--- OpenVR ---\n";
        text.text += util.PutDeviceInfoListStringFormatted();
        text.text += "--- VaNiiMenu ---\n";
        text.text += "Position:" + eovro.Position + "\n";
        text.text += "Rotation:" + eovro.Rotation + "\n";
        text.text += "OverlayFriendlyName:" + eovro.OverlayFriendlyName + "\n";
        text.text += "OverlayKeyName:" + eovro.OverlayKeyName + "\n";
        text.text += "Width:" + eovro.config.width + "\n";
    }
}
