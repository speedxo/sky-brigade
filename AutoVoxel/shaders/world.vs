#version 410 core

#define WIDTH 32
#define DEPTH 32

layout(location = 0) in uint vPackedData;

uniform mat4 uCameraView;
uniform mat4 uCameraProjection;
uniform vec2 uChunkPosition;

layout(location = 0) out vec2 oTexCoords;
layout(location = 1) out vec3 oNormal;
layout(location = 2) out float oShade;

#define UVCoordinateTopLeft 0
#define UVCoordinateBottomLeft 1
#define UVCoordinateTopRight 2
#define UVCoordinateBottomRight 3

#define CubeFaceFront 0
#define CubeFaceBack 1
#define CubeFaceLeft 2
#define CubeFaceRight 3
#define CubeFaceTop 4
#define CubeFaceBottom 5

vec3 unpackPosition(uint packedData) 
{
    vec3 result;

    result.x = float(packedData & 0x1F); // 0 - 4 = x
    result.y = float((packedData >> 5) & 0x1F); // 5 - 9 = y
    result.z = float((packedData >> 10) & 0x1F); // 10 - 14 = z

    return result;
}

vec3 convertNormal(uint normal) {
    if (normal == CubeFaceFront) return vec3(0, 0, 1);
    if (normal == CubeFaceBack) return vec3(0, 0, -1);
    if (normal == CubeFaceLeft) return vec3(-1, 0, 0);
    if (normal == CubeFaceRight) return vec3(1, 0, 0);
    if (normal == CubeFaceTop) return vec3(0, 1, 0);
    if (normal == CubeFaceBottom) return vec3(0, -1, 0);

    return vec3(0, 0, 0); 
}

vec2 convertUV(uint uv) {
    if (uv == UVCoordinateTopLeft) return vec2(0, 1);
    if (uv == UVCoordinateBottomLeft) return vec2(0, 0);
    if (uv == UVCoordinateTopRight) return vec2(1, 1);
    if (uv == UVCoordinateBottomRight) return vec2(1, 0);

    return vec2(0, 0); 
}

vec3 unpackNormal(uint packedData)
{
    return convertNormal((packedData >> 15) & 0x1F);
}

vec2 unpackTexCoord(uint packedData)
{
	return convertUV((packedData >> 20) & 0x1F);
}


void main()
{
	oTexCoords = unpackTexCoord(vPackedData);
    oNormal = unpackNormal(vPackedData);
    oShade = float(((vPackedData >> 22) & 0xF)) / 8.0;

	gl_Position = uCameraProjection * uCameraView * vec4(unpackPosition(vPackedData) + vec3(uChunkPosition.x * (WIDTH - 1), 0, uChunkPosition.y * (DEPTH - 1)), 1.0);
}