Shader "UI/Accessible Color Theme"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color ("Tint", Color) = (1,1,1,1)

        _Strength ("Theme Strength", Range(0,1)) = 0.6

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
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;

            fixed4 _Color;

            fixed4 _TextureSampleAdd;

            float4 _ClipRect;

            float _Strength;

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
            // ACCESSIBLE COLOR
            // =================================================

            fixed3 ApplyAccessibleTheme(
                fixed3 originalColor
            )
            {
                // ---------------------------------------------
                // Luminance
                // ---------------------------------------------

                float luminance =
                    dot(
                        originalColor,
                        float3(
                            0.299,
                            0.587,
                            0.114
                        )
                    );

                // ---------------------------------------------
                // Increase contrast slightly
                // ---------------------------------------------

                fixed3 contrastColor =
                    (originalColor - 0.5) * 1.18 + 0.5;

                contrastColor =
                    saturate(contrastColor);

                // ---------------------------------------------
                // Accessible colour separation
                //
                // Red becomes slightly warmer.
                // Green moves slightly towards cyan.
                // Blue remains clearly separated.
                // ---------------------------------------------

                fixed3 accessibleColor;

                accessibleColor.r =
                    contrastColor.r * 1.05 +
                    contrastColor.g * 0.05;

                accessibleColor.g =
                    contrastColor.g * 0.88 +
                    contrastColor.b * 0.16;

                accessibleColor.b =
                    contrastColor.b * 1.05 +
                    contrastColor.g * 0.08;

                accessibleColor =
                    saturate(accessibleColor);

                // Preserve brightness so dark/light UI
                // does not suddenly disappear.
                float newLuminance =
                    dot(
                        accessibleColor,
                        float3(
                            0.299,
                            0.587,
                            0.114
                        )
                    );

                float difference =
                    luminance - newLuminance;

                accessibleColor += difference;

                accessibleColor =
                    saturate(accessibleColor);

                // ---------------------------------------------
                // Strength
                // ---------------------------------------------

                return lerp(
                    originalColor,
                    accessibleColor,
                    saturate(_Strength)
                );
            }

            // =================================================
            // FRAGMENT
            // =================================================

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 color =
                    (tex2D(
                        _MainTex,
                        IN.texcoord
                    ) + _TextureSampleAdd)
                    * IN.color;

                // IMPORTANT:
                // Alpha is NOT recoloured.
                fixed originalAlpha =
                    color.a;

                color.rgb =
                    ApplyAccessibleTheme(
                        color.rgb
                    );

                color.a =
                    originalAlpha;

                // ---------------------------------------------
                // UI Mask / RectMask2D support
                // ---------------------------------------------

                #ifdef UNITY_UI_CLIP_RECT

                color.a *=
                    UnityGet2DClipping(
                        IN.worldPosition.xy,
                        _ClipRect
                    );

                #endif

                // ---------------------------------------------
                // Alpha Clip
                // ---------------------------------------------

                #ifdef UNITY_UI_ALPHACLIP

                clip(
                    color.a - 0.001
                );

                #endif

                return color;
            }

            ENDCG
        }
    }
}