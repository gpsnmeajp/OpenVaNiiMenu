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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
public class GlobalOnClickManagerScript : MonoBehaviour {
    [SerializeField]
    private MenuManager menu;
    [SerializeField]
    private AudioManagerScript AudioMan;
    [SerializeField]
    private LauncherManagerScript Launch;
    [SerializeField]
    private GestureDetectorScript Gesture;
    [SerializeField]
    private TextViewerManagerScript TextViewer;
    [SerializeField]
    private ImageViewerManagerScript ImageViewer;
    [SerializeField]
    private ScreenAspectWorkerScript ScreenWorker;
    [SerializeField]
    private CommunicationManagerScript Comm;
    [SerializeField]
    private OSCRemoteWorkerScript OSCRemote;
    [SerializeField]
    private FunctionKeyWorkerScript FunctionKeyWorker;
    [SerializeField]
    private EasyOpenVROverlayForUnity EOVRO;
    [SerializeField]
    private DebugManagerScript DebugMan;
    [SerializeField]
    private AlarmManagerScript AlarmMan;
    [SerializeField]
    private ResolutionManagerScript ResoMan;
    [SerializeField]
    private MovePadManagerScript MovePadMan;
    [SerializeField]
    private WindowAspectWorkerScript WindowsWorker;
    [SerializeField]
    private MovePadWindowManagerScript MovePadWindowMan;
    [SerializeField]
    private GameObject SideMenuObject;
    [SerializeField]
    private TimelineManagerScript timeline;
    [SerializeField]
    private TweetManagerScript tweet;

    public UnityAction DialogOkCallback;
    public UnityAction DialogCancelCallback;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
    }

    public void GlobalOnClick(string objectid) {
        Debug.Log("GlobalOnClick" + objectid);
        if (objectid.EndsWith("/CloseButton"))
        {
            AudioMan.PlaySelectSound();
            menu.MenuPage = 0; //Return to Home
            return;
        }
        if (objectid.EndsWith("/SettingsBackButton"))
        {
            AudioMan.PlaySelectSound();
            menu.MenuPage = 4; //Return to Settings
            return;
        }
        if (objectid.EndsWith("/SettingsBackButton"))
        {
            AudioMan.PlaySelectSound();
            menu.MenuPage = 4; //Return to Settings
            return;
        }


        switch (objectid) {
            case "MiniClock/HideSideButton":
                AudioMan.PlaySelectSound();
                if (SideMenuObject.transform.localScale.x > 0.5)
                {
                    SideMenuObject.transform.DOScale(0f, 0.5f);
                }
                else {
                    SideMenuObject.transform.DOScale(1f, 0.5f);
                }

                break;
            //---------------------------------------------
            case "SideMenu/LauncherButton":
                AudioMan.PlaySelectSound();
                menu.MenuPage = 1;
                Launch.loadJSON();
                Launch.AutoSetup();
                break;
            case "SideMenu/MusicButton":
                AudioMan.PlaySelectSound();
                menu.MenuPage = 2;
                break;
            case "SideMenu/MiscButton":
                AudioMan.PlaySelectSound();
                menu.MenuPage = 3;
                break;
            case "SideMenu/SettingsButton":
                AudioMan.PlaySelectSound();
                menu.MenuPage = 4;
                break;
            case "SideMenu/ExitButton":
                if (menu.MenuPage == 0)
                {
                    menu.MenuEndFunc(0);
                }
                else {
                    AudioMan.PlaySelectSound();
                    menu.MenuPage = 0;
                }
                break;

            case "AlarmWindow/AlarmButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 17;
                break;

            //---------------------------------------------

            case "DialogOK/OKButton":
                AudioMan.PlayApplySound();
                menu.CloseDialogOK();
                DialogOkCallback();
                break;
            case "DialogOKCancel/OKButton":
                AudioMan.PlayApplySound();
                menu.CloseDialogOKCancel();
                DialogOkCallback();
                break;
            case "DialogOKCancel/CancelButton":
                AudioMan.PlayCancelSound();
                menu.CloseDialogOKCancel();
                DialogCancelCallback();
                break;

            //---------------------------------------------

            case "WelcomePage/SafeModeButton":
                AudioMan.PlaySelectSound();
                ResoMan.LowResolution(true);
                break;
            case "WelcomePage/NormalModeButton":
                AudioMan.PlaySelectSound();
                ResoMan.NormalResolution(true);
                break;

            //---------------------------------------------

            case "LauncherPage/PrevButton":
                AudioMan.PlaySelectSound();
                Launch.pagePrev();
                break;
            case "LauncherPage/NextButton":
                AudioMan.PlaySelectSound();
                Launch.pageNext();
                break;

            case "App1/RunButton":
                Launch.OnClick(1);
                break;
            case "App2/RunButton":
                Launch.OnClick(2);
                break;
            case "App3/RunButton":
                Launch.OnClick(3);
                break;
            case "App4/RunButton":
                Launch.OnClick(4);
                break;

            //---------------------------------------------

            case "MiniClock/PlayPauseButton":
                AudioMan.PlayApplySound();
                Launch.MediaKey("7");
                break;
            case "MusicPage/PlayPauseButton":
                AudioMan.PlayApplySound();
                Launch.MediaKey("7");
                break;
            case "MusicPage/StopButton":
                AudioMan.PlaySelectSound();
                Launch.MediaKey("6");
                break;
            case "MusicPage/NextButton":
                AudioMan.PlaySelectSound();
                Launch.MediaKey("4");
                break;
            case "MusicPage/BackButton":
                AudioMan.PlaySelectSound();
                Launch.MediaKey("5");
                break;
            case "MusicPage/VolDownButton":
                AudioMan.PlaySelectSound();
                Launch.MediaKey("2");
                break;
            case "MusicPage/VolUpButton":
                AudioMan.PlaySelectSound();
                Launch.MediaKey("3");
                break;
            case "MusicPage/MuteButton":
                AudioMan.PlayApplySound();
                Launch.MediaKey("1");
                break;
            //---------------------------------------------
            case "MiscPage/TextViewerButton":
                AudioMan.PlayApplySound();
                TextViewer.loadText();
                menu.MenuPage = 11;
                break;

            case "MiscPage/ImageViewerButton":
                AudioMan.PlayApplySound();
                ImageViewer.loadImage();
                menu.MenuPage = 12;
                break;

            case "MiscPage/DesktopViewerButton":
                AudioMan.PlayApplySound();
                ScreenWorker.fullscreen = false;
                menu.MenuPage = 13;
                break;

            case "MiscPage/OSCRemoteButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 14;
                break;

            case "MiscPage/FunctionKeyButton":
                AudioMan.PlayApplySound();
                FunctionKeyWorker.FunctionKeyTexts();
                menu.MenuPage = 15;
                break;

            case "MiscPage/DebugButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 16;
                DebugMan.renew();
                break;

            case "MiscPage/ClockAlarmButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 17;
                break;
/*
            case "MiscPage/VoiceCallButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 18;
                break;
                */
            case "MiscPage/WindowViewerButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 18;
                break;


            case "MiscPage/TimelineButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 19;
                break;

            case "MiscPage/TweetButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 20;
                break;



            case "MiscPage/PrevButton":
                AudioMan.PlaySelectSound();
                break;
            case "MiscPage/NextButton":
                AudioMan.PlaySelectSound();
                break;

            //---------------------------------------------

            case "SettingsPage/DiscordButton":
                menu.ShowDialogOKCancel(LanguageManager.config.showdialog.OPEN_DISCORD, LanguageManager.config.tutorial.TUTORIAL4_BODY_1 +"https://discord.gg/QSrDhE8"+ LanguageManager.config.tutorial.TUTORIAL4_BODY_2, 0.05f,
                    () => {
                        Launch.Launch("https://discord.gg/QSrDhE8", "", "");
                    }, () => { });

                break;

            //case "SettingsPage/MoveButton":
            //    menu.MoveMode(true);
            //break;
            case "SettingsPage/VaNiiMenuButton":
                menu.ShowDialogOKCancel(LanguageManager.config.showdialog.OPEN_VANIIMENU, LanguageManager.config.showdialog.OPEN_VANIIMENU_BODY, 0.05f,
                    () => {
                        Launch.LaunchVaNiiMenu();
                    }, () => { });
                break;
            case "MainScreen/MoveButton":
                menu.MoveMode(false);
                break;

            case "SettingsPage/LockModePageButton":
                AudioMan.PlaySelectSound();
                menu.MenuPage = 5;
                break;

            case "SettingsPage/HandSelectButton":
                AudioMan.PlaySelectSound();
                menu.MenuPage = 6;
                break;

            case "SettingsPage/SEVolPageButton":
                AudioMan.PlaySelectSound();
                menu.MenuPage = 7;
                break;

            case "SettingsPage/HomeSettingPageButton":
                AudioMan.PlaySelectSound();
                menu.MenuPage = 8;
                break;

            case "SettingsPage/ScreenSettingsButton":
                AudioMan.PlaySelectSound();
                menu.MenuPage = 9;
                break;

            case "SettingsPage/OpenCloseSettingsButton":
                AudioMan.PlaySelectSound();
                menu.MenuPage = 10;
                break;

            case "SettingsPage/PrevButton":
                AudioMan.PlaySelectSound();
                break;
            case "SettingsPage/NextButton":
                AudioMan.PlaySelectSound();
                break;
            case "SettingsPage/ExitButton":
                //複数起動用キーが指定されている場合、
                if (Environment.GetCommandLineArgs().Length >= 3)
                {
                    if (Environment.GetCommandLineArgs()[1] == "overlaykey")
                    {
                        menu.ShowDialogOKCancel(LanguageManager.config.showdialog.EXIT_VANIIMENU_TITLE, LanguageManager.config.showdialog.EXIT_VANIIMENU_SUB, 0.05f,
                        () =>
                        {
                            DOVirtual.DelayedCall(1f, () =>
                            {
                                menu.MenuEndFunc(0);
                                DOVirtual.DelayedCall(2f, () =>
                                {
                                    new EasyLazyLibrary.EasyOpenVRUtil().ApplicationQuit();
                                });
                            });
                        }, () => { });
                        break;
                    }
                }

                menu.ShowDialogOKCancel(LanguageManager.config.showdialog.EXIT_VANIIMENU_TITLE, LanguageManager.config.showdialog.EXIT_VANIIMENU_MAIN, 0.05f,
                () =>
                {
                    menu.ShowDialogOKCancel(LanguageManager.config.showdialog.EXIT_VANIIMENU_OK, LanguageManager.config.showdialog.EXIT_VANIIMENU_MAIN, 0.2f,
                        () =>
                        {
                            DOVirtual.DelayedCall(1f, () =>
                            {
                                menu.MenuEndFunc(0);
                                DOVirtual.DelayedCall(2f, () =>
                                {
                                    new EasyLazyLibrary.EasyOpenVRUtil().ApplicationQuit();
                                });
                            });
                        },
                        () =>
                        {
                        }
                    );
                },
                () =>
                {
                }
            );
                break;

            //---------------------------------------------

            case "LockSettingsPage/LockModeEnableButton":
                menu.ShowDialogOK(LanguageManager.config.showdialog.LOCK_ON_TITLE, LanguageManager.config.showdialog.LOCK_ON_BODY, 0.05f,
                    () => {
                        Gesture.lockmode(true);
                    }
                );
                break;
            case "LockSettingsPage/FullLockModeEnableButton":
                menu.ShowDialogOKCancel(LanguageManager.config.showdialog.FULLLOCK_ON_TITLE, LanguageManager.config.showdialog.FULLLOCK_ON_BODY, 0.05f,
                    () => {
                        Gesture.fulllockmode(true);
                        menu.ShowDialogOK(LanguageManager.config.showdialog.FULLLOCK_SETED_TITLE, LanguageManager.config.showdialog.FULLLOCK_SETED_BODY, 0.2f,
                            () => {}
                        );
                    },
                    () => {
                        Gesture.fulllockmode(false);
                        menu.ShowDialogOK(LanguageManager.config.showdialog.FULLLOCK_CANCEL, "", 0.2f,
                            () => {
                            }
                        );
                    }
                );
                break;
            case "LockSettingsPage/LockModeDisableButton":
                Gesture.fulllockmode(false);
                menu.ShowDialogOK(LanguageManager.config.showdialog.UNLOCK, "", 0.05f,
                    () => {}
                );
                break;
            case "LockSettingsPage/LockButtonSetButton":
                menu.ShowDialogOK(LanguageManager.config.showdialog.LOCK_REGISTER_TITLE, LanguageManager.config.showdialog.LOCK_REGISTER_BODY, 0.05f,
                    () => {
                        Gesture.learnUnlockKey();
                    }
                );
                break;

            //---------------------------------------------

            case "HandSelectSettiongsPage/LeftHandButton":
                AudioMan.PlayApplySound();
                Gesture.detectLeftHand(true);
                Gesture.detectRightHand(false);
                break;
            case "HandSelectSettiongsPage/RightHandButton":
                AudioMan.PlayApplySound();
                Gesture.detectLeftHand(false);
                Gesture.detectRightHand(true);
                break;
            case "HandSelectSettiongsPage/BothHandButton":
                AudioMan.PlayApplySound();
                Gesture.detectLeftHand(true);
                Gesture.detectRightHand(true);
                break;

            //---------------------------------------------
            case "SEVolumeSettingsPage/UpButton":
                AudioMan.upVolume();
                break;
            case "SEVolumeSettingsPage/DownButton":
                AudioMan.downVolume();
                break;

            case "SEVolumeSettingsPage/02Button":
                AudioMan.setVolume(0.2f);
                AudioMan.PlayApplySound();
                break;
            case "SEVolumeSettingsPage/0Button":
                AudioMan.setVolume(0f);
                AudioMan.PlayApplySound();
                break;

            //---------------------------------------------
            case "HomeSettingsPage/HideHomeButton":
                menu.hideHome(true);
                AudioMan.PlayApplySound();
                break;
            case "HomeSettingsPage/ShowHomeButton":
                menu.hideHome(false);
                AudioMan.PlayApplySound();
                break;
            case "HomeSettingsPage/GreenSkinButton":
                AudioMan.PlayApplySound();
                AudioMan.setBeepSound();
                menu.ChangeSkin(1);

                menu.ShowDialogOK(LanguageManager.config.showdialog.VOICE_CHANGED_TITLE, LanguageManager.config.showdialog.VOICE_CHANGED_BODY, 0.2f,
                    () => { }
                );
                break;
            case "HomeSettingsPage/NormalSkinButton":
                AudioMan.PlayApplySound();
                AudioMan.setNormalSound();
                menu.ChangeSkin(0);

                menu.ShowDialogOK(LanguageManager.config.showdialog.VOICE_CHANGED_TITLE, LanguageManager.config.showdialog.VOICE_CHANGED_BODY, 0.2f,
                    () => { }
                );
                break;

            //---------------------------------------------

            case "ScreenSettingsPage/PosResetButton":
                AudioMan.PlayApplySound();
                menu.setPosition();
                menu.setFixedPosition(false);
                break;

            case "ScreenSettingsPage/FixedPosButton":
                AudioMan.PlayApplySound();
                menu.setFixedPosition(true);
                break;

            case "ScreenSettingsPage/WidthUpButton":
                AudioMan.PlayApplySound();
                EOVRO.upWidth();
                break;

            case "ScreenSettingsPage/WidthDownButton":
                AudioMan.PlayApplySound();
                EOVRO.downWidth();
                break;

            case "ScreenSettingsPage/HighResolutionButton":
                AudioMan.PlayApplySound();
                ResoMan.HighResolution(true);
                break;

            case "ScreenSettingsPage/NormalResolutionButton":
                AudioMan.PlayApplySound();
                ResoMan.NormalResolution(true);
                break;


            //---------------------------------------------

            case "OpenCloseSettingsPage/UpDownOnlyButton":
                AudioMan.PlayApplySound();
                Gesture.slideclose(false);
                break;
            case "OpenCloseSettingsPage/SlideCloseButton":
                AudioMan.PlayApplySound();
                Gesture.slideclose(true);
                break;
            case "OpenCloseSettingsPage/AccelerationPeakResetButton":
                AudioMan.PlayApplySound();
                Gesture.resetPeak();
                break;
            case "OpenCloseSettingsPage/AccelerationSetButton":
                AudioMan.PlayApplySound();
                menu.ShowDialogOKCancel(LanguageManager.config.showdialog.ARE_YOU_OK, LanguageManager.config.showdialog.SPEED_CHANGE, 0.05f,
                    () => {
                        Gesture.setPeak();
                        menu.ShowDialogOK(LanguageManager.config.showdialog.SPEED_CHANGED, "", 0.2f,
                            () => { }
                        );
                    },
                    () => {
                        menu.ShowDialogOK(LanguageManager.config.showdialog.SPEED_UNCHANGED, "", 0.2f,
                            () => {
                            }
                        );
                    }
                );
                break;

            //---------------------------------------------


            case "MiscTextViewer/PrevButton":
                AudioMan.PlaySelectSound();
                TextViewer.prevText();
                break;
            case "MiscTextViewer/NextButton":
                AudioMan.PlaySelectSound();
                TextViewer.nextText();
                break;
            case "MiscTextViewer/SJISButton":
                AudioMan.PlaySelectSound();
                TextViewer.mode = "SJIS";
                TextViewer.loadText();
                break;
            case "MiscTextViewer/UTF8Button":
                AudioMan.PlaySelectSound();
                TextViewer.mode = "UTF8";
                TextViewer.loadText();
                break;
            case "MiscTextViewer/BackToMiscButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 3; //Return to Misc
                break;
            case "TextEx/TextSendButton":
                AudioMan.PlaySelectSound();
                TextViewer.send();
                break;

            //---------------------------------------------

            case "MiscImageViewer/PrevButton":
                AudioMan.PlaySelectSound();
                ImageViewer.prevFile();
                break;
            case "MiscImageViewer/NextButton":
                AudioMan.PlaySelectSound();
                ImageViewer.nextFile();
                break;
            case "MiscImageViewer/NewButton":
                AudioMan.PlaySelectSound();
                ImageViewer.newFile();
                break;
            case "MiscImageViewer/BackToMiscButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 3; //Return to Misc
                break;
            case "ImageEx/ImagePathSendButton":
                AudioMan.PlaySelectSound();
                ImageViewer.send();
                break;
            case "NextImageFolder/NextFolderButton":
                AudioMan.PlayApplySound();
                ImageViewer.nextPathIndex();
                break;
            case "TweetButton/NextFolderButton":
                AudioMan.PlayApplySound();
                ImageViewer.AttachImage();
                menu.MenuPage = 20;
                break;

            //---------------------------------------------

            case "MiscDesktopViewerNormalScreen/BackToMiscButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 3; //Return to Misc
                break;
            case "MiscDesktopViewerNormalScreen/ZoomOutButton":
                AudioMan.PlayApplySound();
                ScreenWorker.zoomout();
                break;
            case "MiscDesktopViewerNormalScreen/ZoomInButton":
                AudioMan.PlayApplySound();
                ScreenWorker.zoomin();
                break;
            case "MiscDesktopViewerNormalScreen/MonitorChangeButton":
                AudioMan.PlayApplySound();
                ScreenWorker.nextMonitor();
                break;
            case "MiscDesktopViewerNormalScreen/FullScreenButton":
                AudioMan.PlayApplySound();
                ScreenWorker.fullscreen = true;
                break;

            case "FullScreenObject/MoveButton":
                menu.MoveMode(false);
                break;
            case "FullScreenObject/FullScreenBackButton":
                AudioMan.PlayApplySound();
                ScreenWorker.fullscreen = false;
                break;

            case "FunctionKeyBackground/EscButton":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F0");
                break;

            case "FunctionKeyBackground/F1Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F1");
                break;

            case "FunctionKeyBackground/F2Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F2");
                break;

            case "FunctionKeyBackground/F3Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F3");
                break;

            case "FunctionKeyBackground/F4Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F4");
                break;

            case "FunctionKeyBackground/F5Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F5");
                break;

            case "FunctionKeyBackground/F6Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F6");
                break;

            case "FunctionKeyBackground/F7Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F7");
                break;

            case "FunctionKeyBackground/F8Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F8");
                break;

            case "FunctionKeyBackground/F9Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F9");
                break;

            case "FunctionKeyBackground/F10Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("FA");
                break;

            case "FunctionKeyBackground/F11Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("FB");
                break;

            case "FunctionKeyBackground/F12Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("FC");
                break;

            case "FunctionKeyBackground/SPButton":
                AudioMan.PlayApplySound();
                Launch.MediaKey("FD");
                break;

            case "FunctionKeyBackground/AltTabButton":
                AudioMan.PlayApplySound();
                Launch.MediaKey("FE");
                break;

            case "FunctionKeyBackground/FButton":
                AudioMan.PlayApplySound();
                Launch.MediaKey("FF");
                break;

            //Desktopのほか、ImageViewerにも搭載
            case "ScaleBackground/WPlusButton":
                AudioMan.PlayApplySound();
                EOVRO.upWidth();
                break;

            case "ScaleBackground/WMinusButton":
                AudioMan.PlayApplySound();
                EOVRO.downWidth();
                break;

            //Widthプリセット
            case "ScalePresetBackground/Preset1Button":
                AudioMan.PlayApplySound();
                EOVRO.WidthPreset(1);
                break;

            case "ScalePresetBackground/Preset2Button":
                AudioMan.PlayApplySound();
                EOVRO.WidthPreset(2);
                break;

            case "ScalePresetBackground/Preset3Button":
                AudioMan.PlayApplySound();
                EOVRO.WidthPreset(3);
                break;

            //ズームモード
            case "ZoomModeBackgorund/MouseLinkModeButton":
                AudioMan.PlayApplySound();
                ScreenWorker.ZoomMode = 0; //通常マウスモード
                ScreenWorker.ZoomReset(); //反映のために一旦無効にする
                break;

            case "ZoomModeBackgorund/MovePadModeButton":
                AudioMan.PlayApplySound();
                ScreenWorker.ZoomMode = 1; //自由移動モード
                ScreenWorker.ZoomReset(); //反映のために一旦無効にする
                break;

            //自由移動パッド
            case "MiscDesktopViewerNormalScreen/MovePadButton":
                AudioMan.PlaySelectSound();
                MovePadMan.MovePadStart();
                break;
            case "FullScreenObject/MovePadButton":
                AudioMan.PlaySelectSound();
                MovePadMan.MovePadStart();
                break;

            //---------------------------------------------

            //WindowViewerは新規だがDesktopViewerの別モードなのでここに配置
            case "MiscWindowViewerNormalScreen/BackToMiscButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 3; //Return to Misc
                break;
            case "MiscWindowViewerNormalScreen/ZoomOutButton":
                AudioMan.PlayApplySound();
                WindowsWorker.zoomout();
                break;
            case "MiscWindowViewerNormalScreen/ZoomInButton":
                AudioMan.PlayApplySound();
                WindowsWorker.zoomin();
                break;
            case "MiscWindowViewerNormalScreen/ChangeButton":
                AudioMan.PlayApplySound();
                WindowsWorker.Change();
                break;
            case "MiscWindowViewerNormalScreen/DesktopWindowButton":
                AudioMan.PlayApplySound();
                WindowsWorker.DesktopWindowSwitch();
                break;
            case "uWC_Board/MovePadButton":
                AudioMan.PlaySelectSound();
                MovePadWindowMan.MovePadStart();
                WindowsWorker.ShowBackground();
                break;
            case "HideBackground/HideBackgroundButton":
                AudioMan.PlayApplySound();
                WindowsWorker.HideBackground();
                break;

            //FunctionKeyBackgroundはDesktopViewerの方で処理される
            //ScaleBackgroundはDesktopViewerの方で処理される


            //---------------------------------------------
            case "MiscOSCRemote/BackToMiscButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 3; //Return to Misc
                break;

            case "MiscOSCRemote/AUX1Button":
                AudioMan.PlayApplySound();
                OSCRemote.OnClick(1);
                break;

            case "MiscOSCRemote/AUX2Button":
                AudioMan.PlayApplySound();
                OSCRemote.OnClick(2);
                break;

            case "MiscOSCRemote/AUX3Button":
                AudioMan.PlayApplySound();
                OSCRemote.OnClick(3);
                break;

            case "MiscOSCRemote/AUX4Button":
                AudioMan.PlayApplySound();
                OSCRemote.OnClick(4);
                break;

            case "MiscOSCRemote/AUX5Button":
                AudioMan.PlayApplySound();
                OSCRemote.OnClick(5);
                break;

            case "MiscOSCRemote/AUX6Button":
                AudioMan.PlayApplySound();
                OSCRemote.OnClick(6);
                break;

            case "MiscOSCRemote/AUX7Button":
                AudioMan.PlayApplySound();
                OSCRemote.OnClick(7);
                break;

            case "MiscOSCRemote/AUX8Button":
                AudioMan.PlayApplySound();
                OSCRemote.OnClick(8);
                break;

            case "MiscOSCRemote/AUX9Button":
                AudioMan.PlayApplySound();
                OSCRemote.OnClick(9);
                break;


            //---------------------------------------------

            case "MiscFunctionKey/BackToMiscButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 3; //Return to Misc
                break;

            case "MiscFunctionKey/F1Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F1");
                break;

            case "MiscFunctionKey/F2Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F2");
                break;

            case "MiscFunctionKey/F3Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F3");
                break;

            case "MiscFunctionKey/F4Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F4");
                break;

            case "MiscFunctionKey/F5Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F5");
                break;

            case "MiscFunctionKey/F6Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F6");
                break;

            case "MiscFunctionKey/F7Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F7");
                break;

            case "MiscFunctionKey/F8Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F8");
                break;

            case "MiscFunctionKey/F9Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("F9");
                break;

            case "MiscFunctionKey/F10Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("FA");
                break;

            case "MiscFunctionKey/F11Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("FB");
                break;

            case "MiscFunctionKey/F12Button":
                AudioMan.PlayApplySound();
                Launch.MediaKey("FC");
                break;
            //---------------------------------------------

            case "MiscDebug/BackToMiscButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 3; //Return to Misc
                break;


            //---------------------------------------------

            case "MiscClockAlarm/BackToMiscButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 3; //Return to Misc
                break;

            case "MiscClockAlarm/AlarmSetButton":
                AudioMan.PlayApplySound();
                AlarmMan.AlarmEnable();
                break;
            case "MiscClockAlarm/AlarmReleaseButton":
                AudioMan.PlayApplySound();
                AlarmMan.AlarmDisable();
                break;


            case "MiscClockAlarm/UPHHButton":
                AudioMan.PlaySelectSound();
                AlarmMan.HourHighUp();
                break;
            case "MiscClockAlarm/UPHLButton":
                AudioMan.PlaySelectSound();
                AlarmMan.HourLowUp();
                break;
            case "MiscClockAlarm/UPMHButton":
                AudioMan.PlaySelectSound();
                AlarmMan.MinutesHighUp();
                break;
            case "MiscClockAlarm/UPMLButton":
                AudioMan.PlaySelectSound();
                AlarmMan.MinutesLowUp();
                break;
            case "MiscClockAlarm/DownHHButton":
                AudioMan.PlaySelectSound();
                AlarmMan.HourHighDown();
                break;
            case "MiscClockAlarm/DownHLButton":
                AudioMan.PlaySelectSound();
                AlarmMan.HourLowDown();
                break;
            case "MiscClockAlarm/DownMHButton":
                AudioMan.PlaySelectSound();
                AlarmMan.MinutesHighDown();
                break;
            case "MiscClockAlarm/DownMLButton":
                AudioMan.PlaySelectSound();
                AlarmMan.MinutesLowDown();
                break;
            //---------------------------------------------

            case "MiscTimeline/BackToMiscButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 3; //Return to Misc
                break;
            case "MiscTimeline/HomeButton":
                timeline.getHome();
                break;
            case "MiscTimeline/ReplyButton":
                timeline.getReply();
                break;

            //---------------------------------------------

            case "MiscTweet/BackToMiscButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 3; //Return to Misc
                break;

            case "MiscTweet/TextButton1":
                tweet.append(1);
                break;
            case "MiscTweet/TextButton2":
                tweet.append(2);
                break;
            case "MiscTweet/TextButton3":
                tweet.append(3);
                break;
            case "MiscTweet/TextButton4":
                tweet.append(4);
                break;
            case "MiscTweet/TextButton5":
                tweet.append(5);
                break;
            case "MiscTweet/TextButton6":
                tweet.append(6);
                break;
            case "MiscTweet/TextButton7":
                tweet.append(7);
                break;
            case "MiscTweet/TextButton8":
                tweet.append(8);
                break;
            case "MiscTweet/TextButton9":
                tweet.append(9);
                break;
            case "MiscTweet/TweetButton":
                tweet.Tweet();
                break;
            case "MiscTweet/ClearButton":
                tweet.Clear();
                break;
            case "MiscTweet/BackSpaceButton":
                tweet.BackSpace();
                break;
            case "MiscTweet/SlideButton":
                tweet.StartSlide();
                break;
            case "TweetImageButton/ImageButton":
                AudioMan.PlayApplySound();
                ImageViewer.loadImage();
                menu.MenuPage = 12;
                break;
            case "TweetImageButton/DetachButton":
                AudioMan.PlayApplySound();
                tweet.Detach();
                break;
            case "TweetScrrenShotButton/ScrrenShotButton":
                AudioMan.PlayApplySound();
                tweet.takePhoto();
                break;
            case "MiscTweet/PrevButton":
                AudioMan.PlaySelectSound();
                tweet.Next(true);
                break;
            case "MiscTweet/NextButton":
                AudioMan.PlaySelectSound();
                tweet.Prev(true);
                break;


            //---------------------------------------------
            /*
            case "MiscVoiceCall/BackToMiscButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 3; //Return to Misc
                break;

            case "MiscVoiceCall/LauncherButton":
                AudioMan.PlayApplySound();
                menu.MenuPage = 1;
                Launch.loadJSON();
                break;

            case "MiscVoiceCall/RedialButton":
                menu.ShowDialogOKCancel("現在の相手に発信しますか？", "", 0.05f, () =>
                {
                    menu.ShowDialogOK("発信しています...", "", 0.05f, () => { });
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        Launch.MediaKey("S1");
                    });
                }, () =>
                {
                    menu.ShowDialogOK("発信を中止しました", "", 0.05f, () => { });
                });
                break;

            case "MiscVoiceCall/DisconnectButton":
                menu.ShowDialogOKCancel("通話を切断しますか？", "", 0.05f, () =>
                {
                    menu.ShowDialogOK("切断しています...", "", 0.05f, () => { });
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        Launch.MediaKey("S2");
                    });
                }, () =>
                {

                });
                break;
                */

            //---------------------------------------------

            default:
                Debug.LogError("Handler not found: " + objectid);
                break;
        }

    }

}
