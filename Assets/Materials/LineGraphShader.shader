Shader "Unlit/LineGraphShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TessFactor("Tessalation Factor", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {

            HLSLPROGRAM
            // Call this macro to interpolate between a triangle patch, passing the field name
            #define BARYCENTRIC_INTERPOLATE(fieldName) \
		                        patch[0].fieldName * barycentricCoordinates.x + \
		                        patch[1].fieldName * barycentricCoordinates.y + \
		                        patch[2].fieldName * barycentricCoordinates.z
            #pragma vertex vert
            #pragma fragment frag
            #pragma hull Hull
            #pragma domain Domain
            // make fog work
            #pragma target 5.0

            #include "LineGraphShader.hlsl"
            ENDHLSL
        }
    }
}
