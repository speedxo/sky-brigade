#version 410 core

layout(location = 0) in vec3 vPosition;

uniform mat4 uCameraView;
uniform mat4 uCameraProjection;

void main()
{
	gl_Position = uCameraProjection  * uCameraView * vec4(vPosition, 1.0);
}