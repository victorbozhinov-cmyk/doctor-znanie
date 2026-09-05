Shader "DoctorZnanie/AccessibleColorFilter"
{
    Properties
    {
        _Strength ("Effect Strength", Range(0, 1)) = 1

        _RedAmount ("Red Change", Range(0, 1)) = 0.9
        _GreenAmount ("Green Change", Range(0, 1)) = 0.9

        _RedTargetColor (
            "Red Replacement",
            Color
        ) = (0.8, 0.475, 0.655, 1)

        _GreenTargetColor (
            "Green Replacement",
            Color
        ) = (0.0, 0.447, 0.698, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "AccessibleColorFilter"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Strength;

            float _RedAmount;
            float _GreenAmount;

            float4 _RedTargetColor;
            float4 _GreenTargetColor;

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

                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;

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

            float HueDistance(float a, float b)
            {
                float d = abs(a - b);

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
                float d = b - a;

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

            half4 Frag(Varyings input)
                : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(
                    input
                );

                float2 uv =
                    input.texcoord.xy;

                half4 source =
                    SAMPLE_TEXTURE2D_X_LOD(
                        _BlitTexture,
                        sampler_LinearClamp,
                        uv,
                        _BlitMipLevel
                    );

                float3 hsv =
                    RGBToHSV(
                        saturate(source.rgb)
                    );

                // Не засягаме почти безцветните
                // части на изображението.
                float colorMask =
                    smoothstep(
                        0.18,
                        0.40,
                        hsv.y
                    );

                // =========================
                // ЧЕРВЕНО
                // =========================

                // Нарочно тесен диапазон.
                // Оранжевото не трябва да влиза тук.
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

                redMask *= colorMask;

                // =========================
                // ЗЕЛЕНО
                // =========================

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

                greenMask *= colorMask;

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

                // =========================
                // ЧЕРВЕНО -> РОЗОВО-ЛИЛАВО
                // =========================

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

                // =========================
                // ЗЕЛЕНО -> СИНЬО
                // =========================

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

                // Допълнителна разлика по яркост.
                // Това е важно, защото не разчитаме
                // единствено на hue.
                hsv.z *=
                    1.0 +
                    redMask * 0.07 -
                    greenMask * 0.10;

                hsv.z =
                    saturate(hsv.z);

                float3 accessibleColor =
                    HSVToRGB(hsv);

                float3 finalColor =
                    lerp(
                        source.rgb,
                        accessibleColor,
                        _Strength
                    );

                return half4(
                    finalColor,
                    source.a
                );
            }

            ENDHLSL
        }
    }

    Fallback Off
}