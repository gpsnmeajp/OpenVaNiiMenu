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

//https://docs.unity3d.com/jp/460/ScriptReference/Texture2D.EncodeToPNG.html
//https://docs.unity3d.com/ja/current/ScriptReference/RenderTexture-active.html
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class SaveToPng : MonoBehaviour {
    public RenderTexture renderTexture;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [ContextMenu("ScreenShot")]
    public void Shot()
    {
        RenderTexture tmp = RenderTexture.active;

        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height,TextureFormat.ARGB32,false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0,0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        RenderTexture.active = tmp;

        byte[] data = texture.EncodeToPNG();
        File.WriteAllBytes("output"+((int)(Time.time*10)).ToString()+".png", data);

    }
}
