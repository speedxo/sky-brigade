#version 410 core

layout (location = 0) in vec2 vPos;
layout (location = 1) in vec2 vTexCoords;
layout (location = 2) in vec3 vColor;

uniform mat4 uView;
uniform mat4 uProjection;

out vec2 texCoords;
out vec3 color;

void main()
{
    texCoords = vTexCoords;
    color = vColor;
    gl_Position = uProjection * uView * vec4(vPos, 0.0, 1.0);
}
