Shader "Gradient Shader Property/Demo/GradientBuiltIn"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Gradients_Foldout ("Gradients", Float) = 0
		_Gradient1_GradientTexture ("Gradient 1", 2D) = "white" {}
		_Gradient2_GradientTexture ("Gradient 2", 2D) = "white" {}
		_Lighting_Foldout ("Lighting", Float) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _Gradient1_GradientTexture;
		sampler2D _Gradient2_GradientTexture;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by the two gradients
            fixed4 m = tex2D (_MainTex, IN.uv_MainTex);
            fixed4 g1 = tex2D (_Gradient1_GradientTexture, IN.uv_MainTex);
            fixed4 g2 = tex2D (_Gradient2_GradientTexture, IN.uv_MainTex);
            fixed4 g = lerp(g1, g2, 0.5);
            fixed4 c = m * g;
            o.Albedo = m.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = m.a;
        }
        ENDCG
    }
    CustomEditor "GradientShaderEditor"
    FallBack "Diffuse"
}
