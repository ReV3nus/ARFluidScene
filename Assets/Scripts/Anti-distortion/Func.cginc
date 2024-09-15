#ifndef ANTI_DISTORTION_FUNC
#define ANTI_DISTORTION_FUNC

float2 uv2xy(float2 uv, float2 center, float FovX, float FovY, float F, float ScreenSize, float2 offset)
{
    float2 duv = uv - center;

    float alpha = duv.x * FovX / 180 * UNITY_PI;
    float beta = duv.y * FovY / 180 * UNITY_PI;
    float tantheta = cos(alpha) * tan(beta);
    
    float s = 2 * F * (tantheta + sqrt(1 + tantheta * tantheta));
    
    float x = s * sin(alpha) / ScreenSize;
    float y = s * cos(alpha) / ScreenSize;
    return float2(x, y) + offset;
}

float2 xy2uv(float2 xy, float2 center, float FovX, float FovY, float F, float ScreenSize, float2 offset)
{
    float2 dxy = (xy - center) * ScreenSize;

    float s = sqrt(dxy.x * dxy.x + dxy.y * dxy.y);
    float cosalpha = dxy.y / s;

    float tantheta = s / (4 * F) - F / s;
    float tanbeta = tantheta / cosalpha;

    float u = acos(cosalpha) / UNITY_PI * 180 / FovX;
    float v = atan(tanbeta) / UNITY_PI * 180 / FovY;
    return float2(u, v) + offset;
}

float2 calculate_uv(float2 uv, float2 center, float2 screenSize, float2 fovSize, float3 K)
{
    float2 duv = (uv - center) * screenSize;
    float r = sqrt(duv.x * duv.x + duv.y * duv.y);
    float R = K.x * pow(r, 3) + K.y * pow(r, 2) + K.z * r;
    return (duv / r * R) / fovSize + center;
}

#endif