#ifndef ANTI_DISTORTION_FUNC
#define ANTI_DISTORTION_FUNC

float2 uv2xy(float2 uv, float2 center, float D, float F, float2 texSize, float ScreenSize, float2 offset)
{
    // return uv * texSize;
    
    float2 duv = (uv - center) * texSize;
    float R = sqrt(duv.x * duv.x + duv.y * duv.y);
    float cosR = duv.x / R;
    float sinR = duv.y / R;
    float tantheta = R / D;
    
    float s = 2 * F * (tantheta + sqrt(1 + tantheta * tantheta));
    
    float x = s * cosR / ScreenSize;
    float y = s * sinR / ScreenSize;
    return float2(x, y) + offset;
}

float2 xy2uv(float2 xy, float2 center, float D, float F, float2 texSize, float ScreenSize, float2 offset)
{
    float2 dxy = (xy - center) * texSize;
    float r = sqrt(dxy.x * dxy.x + dxy.y * dxy.y);
    float cosr = dxy.x / r;
    float sinr = dxy.y / r;

    float s = r / ScreenSize;
    float tantheta = s / (4 * F) - F / s;
    float R = tantheta / D;

    float u = R * cosr;
    float v = R * sinr;
    return float2(u, v) + offset;
}

#endif