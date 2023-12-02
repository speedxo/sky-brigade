#version 410 core

#define WIDTH 16
#define DEPTH 16 

layout(location = 0) in uint vPackedData;

uniform mat4 uCameraView;
uniform mat4 uCameraProjection;
uniform vec2 uChunkPosition;

layout(location = 0) out vec2 oTexCoords;

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

    result.x = float(packedData & 0x1F); // 0 - 5 = x
    result.y = float((packedData >> 5) & 0x1F); // 6 - 10 = y
    result.z = float((packedData >> 10) & 0x1F); // 11 - 15 = z

    return result;
}

vec3 convertNormal(uint normal) {
    if (normal == CubeFaceFront) return vec3(1, 0, 0);
    if (normal == CubeFaceBack) return vec3(-1, 0, 0);
    if (normal == CubeFaceLeft) return vec3(0, -1, 0);
    if (normal == CubeFaceRight) return vec3(0, 1, 0);
    if (normal == CubeFaceTop) return vec3(0, 0, 1);
    if (normal == CubeFaceBottom) return vec3(0, 0, -1);

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
    return convertNormal((packedData >> 14) & 0x1F);
}

vec2 unpackTexCoord(uint packedData)
{
	return convertUV((packedData >> 19) & 0x1F);
}


void main()
{
	oTexCoords = unpackTexCoord(vPackedData);
	gl_Position = uCameraProjection * uCameraView * vec4(unpackPosition(vPackedData) + vec3(uChunkPosition.x * (WIDTH - 1), 0, uChunkPosition.y * (DEPTH - 1)), 1.0);
}