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

public class BatteryMeterWorkerScript : MonoBehaviour {
    public RectTransform MeterL;
    public RectTransform MeterR;
    public RectTransform MeterT1;
    public RectTransform MeterT2;
    public RectTransform MeterT3;
    public GameObject MeterNA;

    private Vector2 MeterLstartPos;
    private Vector2 MeterRstartPos;
    private Vector2 MeterT1startPos;
    private Vector2 MeterT2startPos;
    private Vector2 MeterT3startPos;

    public float PercentL = 0.5f;
    public float PercentR = 0.5f;
    public float PercentT1 = 0.5f;
    public float PercentT2 = 0.5f;
    public float PercentT3 = 0.5f;
    private float OldPercentL = -1f;
    private float OldPercentR = -1f;
    private float OldPercentT1 = -1f;
    private float OldPercentT2 = -1f;
    private float OldPercentT3 = -1f;

    // Use this for initialization
    void Start () {
        MeterLstartPos = MeterL.anchoredPosition;
        MeterRstartPos = MeterR.anchoredPosition;
        MeterT1startPos = MeterT1.anchoredPosition;
        MeterT2startPos = MeterT2.anchoredPosition;
        MeterT3startPos = MeterT3.anchoredPosition;
    }

    // Update is called once per frame
    void Update () {
        //非表示表示をオンオフ
        if (PercentL != OldPercentL || PercentR != OldPercentR) {
            if (float.IsNaN(PercentL) && float.IsNaN(PercentR))
            {
                MeterNA.SetActive(true);
            }
            else {
                MeterNA.SetActive(false);
            }
        }

        if (PercentL != OldPercentL)
        {
            OldPercentL = PercentL;
            if (!float.IsNaN(PercentL))
            {
                //Battery残量を反映
                MeterL.anchoredPosition = new Vector2(MeterLstartPos.x - (16.8f * (1.0f - PercentL)), MeterLstartPos.y);
            }
            else {
                MeterL.anchoredPosition = new Vector2(MeterLstartPos.x - 16.8f, MeterLstartPos.y);
            }
        }

        if (PercentR != OldPercentR)
        {
            OldPercentR = PercentR;
            if (!float.IsNaN(PercentR))
            {
                MeterR.anchoredPosition = new Vector2(MeterRstartPos.x - (16.8f * (1.0f - PercentR)), MeterRstartPos.y);
            }
            else
            {
                MeterR.anchoredPosition = new Vector2(MeterRstartPos.x - 16.8f, MeterRstartPos.y);
            }
        }

        if (PercentT1 != OldPercentT1)
        {
            OldPercentT1 = PercentT1;
            if (!float.IsNaN(PercentT1))
            {
                MeterT1.anchoredPosition = new Vector2(MeterT1startPos.x, MeterT1startPos.y - (17f * (1.0f - PercentT1)));
            }
            else
            {
                MeterT1.anchoredPosition = new Vector2(MeterT1startPos.x, MeterT1startPos.y - 17f);
            }
        }

        if (PercentT2 != OldPercentT2)
        {
            OldPercentT2 = PercentT2;
            if (!float.IsNaN(PercentT2))
            {
                MeterT2.anchoredPosition = new Vector2(MeterT2startPos.x, MeterT2startPos.y - (17f * (1.0f - PercentT2)));
            }
            else
            {
                MeterT2.anchoredPosition = new Vector2(MeterT2startPos.x, MeterT2startPos.y - 17f);
            }
        }

        if (PercentT3 != OldPercentT3)
        {
            OldPercentT3 = PercentT3;
            if (!float.IsNaN(PercentT3))
            {
                MeterT3.anchoredPosition = new Vector2(MeterT3startPos.x, MeterT3startPos.y - (17f * (1.0f - PercentT3)));
            }
            else
            {
                MeterT3.anchoredPosition = new Vector2(MeterT3startPos.x, MeterT3startPos.y - 17f);
            }
        }
    }
}
