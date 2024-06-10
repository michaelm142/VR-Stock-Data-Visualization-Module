#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#define WORLD_AXIS_X float3(1, 0, 0)
#define FLOAT_MAX 999999
struct Attributes
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uvOS : TEXCOORD;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct TessellationControlPoint
{
    float3 positionWS : INTERNALTESSPOS;
    float3 normalWS : NORMAL;
    float2 uvWS : TEXCOORD;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct TessellationFactors {
    float edge[3] : SV_TessFactor;
    float inside : SV_InsideTessFactor;
};

struct Interpolators
{
    float3 normalWS                 : NORMAL;
    float4 positionCS               : SV_POSITION;
    float2 uv                       : TEXCOORD;
    UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
};

sampler2D _MainTex;
float4 _MainTex_ST;
float _TessFactor;

TessellationControlPoint vert(Attributes v)
{
    TessellationControlPoint o;

    o.positionWS = GetVertexPositionInputs(v.positionOS).positionWS;
    o.normalWS = mul(v.normalOS, UNITY_MATRIX_MV);
    o.uvWS = float2(v.uvOS.x, 0.0f);

    return o;
}

[domain("tri")] // Signal we're inputting triangles
[outputcontrolpoints(3)] // Triangles have three points
[outputtopology("triangle_cw")] // Signal we're outputting triangles
[patchconstantfunc("PatchConstantFunction")] // Register the patch constant function
[partitioning("integer")] // Select a partitioning mode: integer, fractional_odd, fractional_even or pow2
TessellationControlPoint Hull(
    InputPatch<TessellationControlPoint, 3> patch, // Input triangle
    uint id : SV_OutputControlPointID) { // Vertex index on the triangle

    return patch[id];
}

// The domain function runs once per vertex in the final, tessellated mesh
// Use it to reposition vertices and prepare for the fragment stage
[domain("tri")] // Signal we're inputting triangles
Interpolators Domain(
    TessellationFactors factors, // The output of the patch constant function
    OutputPatch<TessellationControlPoint, 3> patch, // The Input triangle
    float3 barycentricCoordinates : SV_DomainLocation) { // The barycentric coordinates of the vertex on the triangle

    Interpolators output;

    // Setup instancing and stereo support (for VR)
    UNITY_SETUP_INSTANCE_ID(patch[0]);
    UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 positionWS = BARYCENTRIC_INTERPOLATE(positionWS);
    float3 normalWS = BARYCENTRIC_INTERPOLATE(normalWS);
    float2 uv = BARYCENTRIC_INTERPOLATE(uvWS);

    float value = tex2Dlod(_MainTex, float4(uv, 0, 0)).r;

    output.positionCS = TransformWorldToHClip(positionWS) + float4(0, value * 10, 0, 0);
    output.normalWS = normalWS;
    output.uv = uv;

    return output;
}

// The patch constant function runs once per triangle, or "patch"
// It runs in parallel to the hull function
TessellationFactors PatchConstantFunction(
    InputPatch<TessellationControlPoint, 3> patch) {
    UNITY_SETUP_INSTANCE_ID(patch[0]); // Set up instancing
    float3 edge0 = patch[2].positionWS - patch[1].positionWS;
    float3 edge1 = patch[2].positionWS - patch[0].positionWS;
    float3 edge2 = patch[1].positionWS - patch[0].positionWS;
    // Calculate tessellation factors
    TessellationFactors f;
    f.edge[0] = abs(dot(edge0, WORLD_AXIS_X)) * _TessFactor;
    f.edge[1] = abs(dot(edge1, WORLD_AXIS_X)) * _TessFactor;
    f.edge[2] = abs(dot(edge2, WORLD_AXIS_X)) * _TessFactor;
    f.inside = (f.edge[0] + f.edge[1] + f.edge[2]) * _TessFactor;
    return f;
}

float4 frag(Interpolators input) : SV_Target{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);



    return float4(1, 0, 0, 1);
}