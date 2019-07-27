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
using DG.Tweening;

public class ScreenAspectWorkerScript : MonoBehaviour {
    public MenuManager menu;

    //モニタ画面の一時大きさ
    Vector3 startScale;
    //モニタ画面の通常大きさ
    Vector3 startNormalScale;
    //モニタ画面のフルスクリーン大きさ
    Vector3 startFullScale;

    //モニタ画面の移動
    Vector3 startPos;

    //デスクトップ画面の表示処理
    uDesktopDuplication.Texture desktop;

    //全画面表示オブジェクト
    public GameObject FullscrrenObject;
    //通常表示オブジェクト
    public GameObject NormalScrrenObject;
    //サイドメニューオブジェクト
    public GameObject SideMenuObject;

    //全画面表示時操作ボタン位置
    Vector3 FullscrrenObjectStartPos;
    //全画面表示時ローカル位置(これもとに戻す必要ある？)
    Vector3 FullscrrenObjectStartPosLocal;

    //モニタ画面情報
    public uDesktopDuplication.Manager desktopManager;
    //ルーペ
    Loupe loupe;
    FreeLoupeWorkerScript FreeLoupe;

    //画面サイズ比率(取得した際のモニタリング用)
    public float aspect = 0;

    //全画面スイッチ
    public bool fullscreen = false;
    bool oldfullscreen = false;

    //ズームスイッチ
    public bool zoom = false;
    private bool oldzoom = true;
    private float zoomRate = 1; //ズームレート

    //ズーム移動モード
    public int ZoomMode = 0;//標準はマウスモード

    //モニタ番号
    int monitorSelect = 0;

    void Start() {
        //座標を記憶、初期スケールに設定
        startNormalScale = transform.localScale;
        startFullScale = new Vector3(119f, 76f, 0f);
        startScale = startNormalScale;
        startPos = transform.localPosition;

        //ボタン座標を記録
        FullscrrenObjectStartPosLocal = FullscrrenObject.transform.localPosition;
        FullscrrenObjectStartPos = new Vector3(-0.1732039f, 36.17097f, 90f);

        //インスタンス取得
        desktop = GetComponent<uDesktopDuplication.Texture>();
        loupe = GetComponent<Loupe>(); //uDD公式ルーペ
        FreeLoupe = GetComponent<FreeLoupeWorkerScript>(); //自作フリールーペ

        //初期状態設定
        loupe.enabled = false;
        desktop.useClip = false;
        fullscreen = false;
        zoom = false;
    }

    //ズーム率リセット
    public void ZoomReset()
    {
        zoom = false;
        zoomRate = 1f;
    }


    //ズームインボタン
    public void zoomin() {
        zoom = true;
        zoomRate += 1f;
        if (zoomRate > 5f)
        {
            zoomRate = 5f;
        }

        //マウスモード
        if (ZoomMode == 0)
        {
            loupe.zoom = zoomRate;
        }
        //自由移動モード
        if (ZoomMode == 1)
        {
            FreeLoupe.scale = zoomRate;
        }

    }

    //ズームアウトボタン
    public void zoomout()
    {
        zoomRate -= 1f;
        if (zoomRate < 1.5)
        {
            zoom = false;
            zoomRate = 1;
        }
        else
        {
            zoom = true;
        }

        //マウスモード
        if (ZoomMode == 0)
        {
            loupe.zoom = zoomRate;
        }
        //自由移動モード
        if (ZoomMode == 1)
        {
            FreeLoupe.scale = zoomRate;
        }
    }

    //次のモニタに切り替え
    public void nextMonitor() {
        monitorSelect++;
        if (monitorSelect > uDesktopDuplication.Manager.monitorCount - 1) //staticなのでインスタンス不要
        {
            monitorSelect = 0; //モニタ数をオーバーしたら初期に戻す
        }
        desktop.monitor = uDesktopDuplication.Manager.monitors[monitorSelect];
    }


    // Update is called once per frame
    void Update () {
        //フルスクリーン処理
        if (fullscreen)
        {
            //フルスクリーンにする(メニュー操作時位置ずれ対策)
            transform.position = new Vector3(0, transform.position.y, transform.position.z);
            FullscrrenObject.transform.position = new Vector3(FullscrrenObjectStartPos.x, FullscrrenObject.transform.position.y, FullscrrenObject.transform.position.z);
        }

        if (fullscreen != oldfullscreen)
        {
            oldfullscreen = fullscreen;
            aspect = 0;//縦横比を再設定させるため
            if (fullscreen)
            {
                //フルスクリーンにする
                transform.position = new Vector3(0, 0, transform.position.z);
                startScale = startFullScale;

                //フルスクリーンボタンの表示
                FullscrrenObject.transform.position = FullscrrenObjectStartPos;
                FullscrrenObject.SetActive(true);
                NormalScrrenObject.SetActive(false);
                SideMenuObject.transform.DOScale(0f, 0.5f);
            }
            else
            {
                //フルスクリーンから戻す
                transform.localPosition = startPos;
                startScale = startNormalScale;

                //フルスクリーンボタンを戻す
                FullscrrenObject.transform.position = FullscrrenObjectStartPosLocal; //これいる？
                FullscrrenObject.SetActive(false);
                NormalScrrenObject.SetActive(true);
                SideMenuObject.transform.DOScale(1f, 0.5f);
            }
        }


        //ズーム処理
        if (zoom != oldzoom) {
            oldzoom = zoom;
            if (zoom)
            {
                //マウスモード
                if (ZoomMode == 0)
                {
                    desktop.useClip = true; //クリップ機能有効
                    loupe.enabled = true; //uDD公式ルーペ有効
                    FreeLoupe.enable = false; //自作ルーペ無効
                }
                //自由移動モード
                if (ZoomMode == 1)
                {
                    desktop.useClip = true; //クリップ機能有効
                    loupe.enabled = false; //uDD公式ルーペ無効
                    FreeLoupe.enable = true; //自作ルーペ有効
                }
            }
            else {
                loupe.enabled = false; //uDD公式ルーペ無効
                FreeLoupe.enable = false; //自作ルーペ無効
                desktop.useClip = false; //クリップ機能無効
            }
        }
        

        //画面比率を更新。動的にモニタの縦横比が変わることがあるため
        if (aspect != desktop.monitor.aspect) {
            aspect = desktop.monitor.aspect;
            loupe.aspect = aspect;

            var width = startScale.x;
            var height = startScale.x / aspect;

            if (height > startScale.y)
            {
                width = startScale.y * aspect;
                height = startScale.y;
            }

            transform.DOScaleX(width, 0.1f);
            transform.DOScaleY(height, 0.1f);
            //transform.localScale = new Vector2(width, height);
        }



    }
}
