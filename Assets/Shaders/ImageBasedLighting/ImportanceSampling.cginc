
float radicalInverse_VdC(uint bits)
{
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}

float2 hammersley(uint i, uint N)
{
    return float2(float(i)/float(N), radicalInverse_VdC(i));
}

float3 importanceSampleGGX(float2 xi, float3 normal, float roughness)
{
    float a = roughness * roughness;

    float phi = UNITY_TWO_PI * xi.x;
    float cosTheta = sqrt((1.0 - xi.y) / (1.0 + (a*a - 1.0) * xi.y));
    float sinTheta = sqrt(1.0 - cosTheta*cosTheta);

    //from spherical coords to cartesian
    float3 halfway = 0;
    halfway.x = cos(phi) * sinTheta;
    halfway.y = sin(phi) * sinTheta;
    halfway.z = cosTheta;

    //from tangent space to world space
    float3 up = abs(normal.z) < 0.999 ? float3(0,0,1) : float3(1,0,0);
    float3 tangent = normalize(cross(up, normal));
    float3 bitangent = normalize(cross(normal, tangent));

    float3 sampleVec = tangent * halfway.x + bitangent * halfway.y + normal * halfway.z;
    return normalize(sampleVec);
}