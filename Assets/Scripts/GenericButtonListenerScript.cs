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
using DG.Tweening;

//すべてのボタンに作用させるスクリプト

public class GenericButtonListenerScript : MonoBehaviour {
    GlobalOnClickManagerScript global;
    Button button;
    RectTransform rect;

    [SerializeField]
    bool IgnoreGlobalOnClick = false;
    [SerializeField]
    bool IgnoreLocalOnClick = false;

    float pushsize = 0.9f;
    float dutation = 0.05f;

    public string ParentName;
    public string MyName;
    public string objectid;


    // Use this for initialization
    void Start () {
        ParentName = transform.parent.transform.name;
        MyName = transform.name;
        objectid = ParentName + "/" + MyName;

        //GlobalOnClickManagerを探す
        global = GameObject.Find("GlobalOnClickManager").GetComponent<GlobalOnClickManagerScript>();

        //RectTransform
        rect = GetComponent<RectTransform>();

        //ボタンを探す
        button = GetComponent<Button>();

        //LocalOnClickの有効無効
        if (!IgnoreLocalOnClick)
        {
            button.onClick.AddListener(localOnClick);
        }

        //GlobalOnClickの有効無効
        if (!IgnoreGlobalOnClick) {
            button.onClick.AddListener(globalOnClick);
        }
    }

    //ボタン内でローカル処理するもの
    void localOnClick()
    {
        DOTween.Sequence()
            .Append(
                rect.DOScale(pushsize, dutation)
            )
            .Append(
                rect.DOScale(1f, dutation)
            )
            .Play();
    }
    //GlobalOnClickManagerに通知するもの
    void globalOnClick()
    {
        Debug.Log("OnClick:" + objectid);

        DOVirtual.DelayedCall(0.05f, () =>
        {
            global.GlobalOnClick(objectid);
        });
    }
}
