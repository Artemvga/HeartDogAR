Shader "HeartDog/LungAirflow" {
 Properties { _Color ("Air colour",Color)=(.4,.9,.8,.7) }
 SubShader { Tags { "Queue"="Transparent+20" "RenderType"="Transparent" }
 Pass { Blend SrcAlpha One ZWrite Off ZTest Always Cull Back
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 fixed4 _Color;
 struct v2f { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 view:TEXCOORD1; };
 v2f vert(appdata_base v){ v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.view=WorldSpaceViewDir(v.vertex);return o; }
 fixed4 frag(v2f i):SV_Target { float glow=pow(saturate(dot(normalize(i.normal),normalize(i.view))),1.5);return fixed4(_Color.rgb,_Color.a*glow); }
 ENDCG
 } }
}
