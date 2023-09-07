#version 410 core

uniform sampler2D uAlbedo;
uniform vec4 uColor;

out vec4 FragColor;
out vec4 NormalColor;
out vec4 FragPosColor;

in vec2 texCoords;
in vec3 normals;
in vec3 vertexPos;

void main() { FragColor = texture(uAlbedo, texCoords); }