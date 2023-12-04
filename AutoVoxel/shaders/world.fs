#version 410 core

layout(location = 0) out vec4 AlbedoColor;

layout(location = 0) in vec2 texCoords;
layout(location = 1) in vec3 normal;
layout(location = 2) in float shade;

vec3 lightDir = vec3(0.707, 0.9, 0.36);

void main() {
  AlbedoColor = vec4(vec3(dot(normalize(normal), lightDir)), 1.0);
}
