#version 410 core

layout(location = 0) in vec2 vPosition;
layout(location = 1) in vec2 vTexCoords;

layout(location = 0) out vec2 oTexCoords;

void main()
{
	oTexCoords = vTexCoords;
	gl_Position = vec4(vPosition, 0.0, 1.0);
}