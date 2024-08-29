#ifndef ANTI_DISTORTION_FUNC
#define ANTI_DISTORTION_FUNC

float2 calculate_uv(float2 uv, float2 center, float2 size, float3 K)
{
    float2 duv = (uv - center) * size;
    float r = sqrt(duv.x * duv.x + duv.y * duv.y);
    float R = K.x * pow(r, 3) + K.y * pow(r, 2) + K.z * r;
    return (duv / r * R) / size + center;
}

#endif