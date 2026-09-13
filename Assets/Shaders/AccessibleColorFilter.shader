Shader "UI/Accessible Color Theme"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color ("Tint", Color) = (1,1,1,1)

        _Strength ("Effect Strength", Range(0,1)) = 1

        _RedAmount ("Red Change", Range(0,1)) = 0.9
        _GreenAmount ("Green Change", Range(0,1)) = 0.9

        _RedTargetColor (
            "Red Replacement",
            Color
        ) = (0.8, 0.475, 0.655, 1)

        _GreenTargetColor (
            "Green Replacement",
            Color
        ) = (0.0, 0.447, 0.698, 1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        Blend SrcAlpha OneMinusSrcAlpha

        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;

            fixed4 _Color;
            fixed4 _TextureSampleAdd;

            float4 _ClipRect;

            float _Strength;

            float _RedAmount;
            float _GreenAmount;

            float4 _RedTargetColor;
            float4 _GreenTargetColor;

            // =================================================
            // VERTEX
            // =================================================

            v2f vert(appdata_t v)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.worldPosition = v.vertex;

                OUT.vertex =
                    UnityObjectToClipPos(v.vertex);

                OUT.texcoord =
                    v.texcoord;

                OUT.color =
                    v.color * _Color;

                return OUT;
            }

            // =================================================
            // RGB -> HSV
            // =================================================

            float3 RGBToHSV(float3 c)
            {
                float4 K = float4(
                    0.0,
                    -1.0 / 3.0,
                    2.0 / 3.0,
                    -1.0
                );

                float4 p = lerp(
                    float4(c.bg, K.wz),
                    float4(c.gb, K.xy),
                    step(c.b, c.g)
                );

                float4 q = lerp(
                    float4(p.xyw, c.r),
                    float4(c.r, p.yzx),
                    step(p.x, c.r)
                );

                float d =
                    q.x - min(q.w, q.y);

                float e =
                    1.0e-10;

                return float3(
                    abs(
                        q.z +
                        (q.w - q.y) /
                        (6.0 * d + e)
                    ),
                    d / (q.x + e),
                    q.x
                );
            }

            // =================================================
            // HSV -> RGB
            // =================================================

            float3 HSVToRGB(float3 c)
            {
                float4 K = float4(
                    1.0,
                    2.0 / 3.0,
                    1.0 / 3.0,
                    3.0
                );

                float3 p =
                    abs(
                        frac(c.xxx + K.xyz) *
                        6.0 -
                        K.www
                    );

                return c.z *
                    lerp(
                        K.xxx,
                        saturate(p - K.xxx),
                        c.y
                    );
            }

            // =================================================
            // HUE HELPERS
            // =================================================

            float HueDistance(
                float a,
                float b
            )
            {
                float d =
                    abs(a - b);

                return min(
                    d,
                    1.0 - d
                );
            }

            float LerpHue(
                float a,
                float b,
                float t
            )
            {
                float d =
                    b - a;

                if (d > 0.5)
                {
                    d -= 1.0;
                }

                if (d < -0.5)
                {
                    d += 1.0;
                }

                return frac(
                    a + d * t + 1.0
                );
            }

            // =================================================
            // FRAGMENT
            // =================================================

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 source =
                    (
                        tex2D(
                            _MainTex,
                            IN.texcoord
                        )
                        +
                        _TextureSampleAdd
                    )
                    *
                    IN.color;

                // Запазваме оригиналната прозрачност.
                fixed originalAlpha =
                    source.a;

                float3 hsv =
                    RGBToHSV(
                        saturate(source.rgb)
                    );

                // =================================================
                // COLOR MASK
                // =================================================

                // Почти безцветните части не ги променяме.
                float colorMask =
                    smoothstep(
                        0.18,
                        0.40,
                        hsv.y
                    );

                // =================================================
                // RED
                // =================================================

                // Тесен диапазон, за да не хваща оранжевото.
                float redMask =
                    1.0 -
                    smoothstep(
                        0.025,
                        0.07,
                        HueDistance(
                            hsv.x,
                            0.0
                        )
                    );

                redMask *=
                    colorMask;

                // =================================================
                // GREEN
                // =================================================

                float greenMask =
                    1.0 -
                    smoothstep(
                        0.045,
                        0.12,
                        HueDistance(
                            hsv.x,
                            0.33
                        )
                    );

                greenMask *=
                    colorMask;

                float3 redTargetHSV =
                    RGBToHSV(
                        _RedTargetColor.rgb
                    );

                float3 greenTargetHSV =
                    RGBToHSV(
                        _GreenTargetColor.rgb
                    );

                float redStrength =
                    redMask *
                    _RedAmount;

                float greenStrength =
                    greenMask *
                    _GreenAmount;

                // =================================================
                // RED -> PINK / PURPLE
                // =================================================

                hsv.x =
                    LerpHue(
                        hsv.x,
                        redTargetHSV.x,
                        redStrength
                    );

                hsv.y =
                    lerp(
                        hsv.y,
                        redTargetHSV.y,
                        redStrength * 0.75
                    );

                // =================================================
                // GREEN -> BLUE
                // =================================================

                hsv.x =
                    LerpHue(
                        hsv.x,
                        greenTargetHSV.x,
                        greenStrength
                    );

                hsv.y =
                    lerp(
                        hsv.y,
                        greenTargetHSV.y,
                        greenStrength * 0.75
                    );

                // =================================================
                // BRIGHTNESS DIFFERENCE
                // =================================================

                hsv.z *=
                    1.0 +
                    redMask * 0.07 -
                    greenMask * 0.10;

                hsv.z =
                    saturate(hsv.z);

                float3 accessibleColor =
                    HSVToRGB(hsv);

                source.rgb =
                    lerp(
                        source.rgb,
                        accessibleColor,
                        _Strength
                    );

                // =================================================
                // UI CLIPPING / MASKS
                // =================================================

                #ifdef UNITY_UI_CLIP_RECT

                source.a *=
                    UnityGet2DClipping(
                        IN.worldPosition.xy,
                        _ClipRect
                    );

                #endif

                #ifdef UNITY_UI_ALPHACLIP

                clip(
                    source.a - 0.001
                );

                #endif

                // Никога не променяме alpha заради color filter-а.
                source.a =
                    min(
                        source.a,
                        originalAlpha
                    );

                return source;
            }

            ENDCG
        }
    }
}