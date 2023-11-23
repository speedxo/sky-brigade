#version 410 core

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec3 vNormal;
layout(location = 2) in vec2 vTexCoord;

uniform mat4 uCameraView;
uniform mat4 uCameraProjection;

layout(location = 0) out vec2 oTexCoords;

void main()
{
	oTexCoords = vTexCoord;
	gl_Position = uCameraProjection  * uCameraView * vec4(vPosition, 1.0);
}