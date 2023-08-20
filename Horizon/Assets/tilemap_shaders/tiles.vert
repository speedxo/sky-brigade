#version 410 core

layout (location = 0) in vec2 vPos;
layout (location = 1) in vec2 vTexCoords;

uniform mat4 uView;
uniform mat4 uProjection;

out vec2 texCoords;

void main()
{
    texCoords = vTexCoords;

    gl_Position = uProjection * uView * vec4(vPos, 0.0, 1.0);
}
