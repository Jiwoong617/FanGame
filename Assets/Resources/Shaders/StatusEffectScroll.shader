Shader "Custom/StatusEffectScroll"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // C#에서 조작할 속도 변수 (기본값 2.0)
        _ScrollSpeed ("Scroll Speed", Float) = 2.0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha // 투명도(Alpha) 처리를 위한 블렌딩

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR; // SpriteRenderer의 Color
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _ScrollSpeed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                
                // UV 스크롤링 로직 (Y축)
                OUT.texcoord = IN.texcoord;
                OUT.texcoord.y += _Time.y * _ScrollSpeed;
                
                // C#의 SpriteRenderer.color 와 머티리얼 기본 색상을 곱해줌
                OUT.color = IN.color * _Color;
                
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 텍스처 색상 추출 후 Vertex Color 곱하기 (DOTween Fade 적용을 위해 필수)
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // RGB에 Alpha값을 미리 곱해주는 처리 (스프라이트 테두리 검은색 방지)
                c.rgb *= c.a; 
                return c;
            }
            ENDCG
        }
    }
}