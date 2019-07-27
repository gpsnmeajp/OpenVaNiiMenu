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

public class MovePadWindowManagerScript : MonoBehaviour
{
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private EasyOpenVROverlayForUnity EOVRO;
    [SerializeField]
    private FreeLoupeForWindowWorkerScript FreeLoupe;

    Vector2 LeftStartPos = Vector2.zero;
    Vector2 RightStartPos = Vector2.zero;

    bool moveLeft = false; //左手で移動中か
    bool moveRight = false; //右手で移動中か

    //移動開始
    public void MovePadStart()
    {
        //左手を検出した
        if (EOVRO.tappedLeft)
        {
            moveLeft = true;
            //初期位置の記憶
            if (EOVRO.LeftHandU > 0 && EOVRO.LeftHandV > 0)
            {
                LeftStartPos = new Vector2(EOVRO.LeftHandU, EOVRO.LeftHandV);
            }
        }

        //右手を検出した
        if (EOVRO.tappedRight)
        {
            moveRight = true;
            //初期位置の記憶
            if (EOVRO.RightHandU > 0 && EOVRO.RightHandV > 0)
            {
                RightStartPos = new Vector2(EOVRO.RightHandU, EOVRO.RightHandV);
            }
        }

        //どちらの手も検出できなかったときは特に何も発生しない
    }

    void Start()
    {

    }

    void Update()
    {
        //左手で移動中
        if (moveLeft)
        {
            //左手が離れてるなら移動終了
            if (!EOVRO.tappedLeft)
            {
                moveLeft = false;
            }
            //移動があって有効(-1ではない)なら
            if (EOVRO.LeftHandU > 0 && EOVRO.LeftHandV > 0)
            {
                //差分を取得
                Vector2 deltaPos = new Vector2(EOVRO.LeftHandU, EOVRO.LeftHandV) - LeftStartPos;
                //前回位置を更新
                LeftStartPos = new Vector2(EOVRO.LeftHandU, EOVRO.LeftHandV);

                //移動位置に加算(クリッピング処理は向こうに任せる)
                FreeLoupe.clipPos += new Vector2(-deltaPos.x / EOVRO.renderTexture.width, deltaPos.y / EOVRO.renderTexture.height);
            }
        }
        //右手で移動中
        if (moveRight)
        {
            //右手が離れてるなら移動終了
            if (!EOVRO.tappedRight)
            {
                moveRight = false;
            }
            //移動があって有効(-1ではない)なら
            if (EOVRO.RightHandU > 0 && EOVRO.RightHandV > 0)
            {
                //差分を取得
                Vector2 deltaPos = new Vector2(EOVRO.RightHandU, EOVRO.RightHandV) - RightStartPos;
                //前回位置を更新
                RightStartPos = new Vector2(EOVRO.RightHandU, EOVRO.RightHandV);

                //移動位置に加算(クリッピング処理は向こうに任せる)
                FreeLoupe.clipPos += new Vector2(-deltaPos.x / EOVRO.renderTexture.width, deltaPos.y / EOVRO.renderTexture.height);
            }
        }
    }
}
