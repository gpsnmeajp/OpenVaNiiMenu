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
Shader "Unlit/GreenDisplay"
{
	Properties{
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader{
			Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			Pass{
				CGPROGRAM
				#pragma vertex vertexFunction
				#pragma fragment fragmentFunction
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				float4    _MainTex_ST;

				struct appdata {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f {
					float4 position : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				float mod(float a, float b) {
					return a - b * (int)(a / b);
				}

				v2f vertexFunction(appdata IN) {
					v2f OUT;
					OUT.position = UnityObjectToClipPos(IN.vertex);
					OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
					return OUT;
				}

				fixed4 fragmentFunction(v2f IN) : SV_TARGET{
					float t = mod(_Time.x, 1.0 / 3.0) * 4;
					float4 now =  tex2D(_MainTex, IN.uv);
//					float4 now2 = tex2D(_MainTex, float2(IN.uv.x + 0.002, IN.uv.y + 0.002));
//					float4 now3 = tex2D(_MainTex, float2(IN.uv.x - 0.002, IN.uv.y - 0.002));

//					now.x = (now.x + now2.x) / 2;
//					now.z = (now.z + now2.z) / 2;
					float m = now.x;
					m = max(m, now.y);
					m = max(m, now.z);



					if ((t - 0.1) < IN.uv.y && IN.uv.y < t) {
						m /= 1.1;
					}
					if (mod(IN.uv.y+t/30,0.002)>0.001) {
						m /= 0.98;
					}
					
					float4 output = float4(m*0.4, m*m * 3, m*0.5, 0.95);
//					float4 output = float4(now.x, now.y, now.z, 1);
//					float4 output = float4(m*m * 3, m*m*1.5, m*0.5, 1);

					return output;
				}
			ENDCG
		}
	}
}