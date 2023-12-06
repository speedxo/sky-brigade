#version 410 core

layout(location = 0) out vec4 AlbedoColor;

uniform sampler2D uTexAlbedo;

layout(location = 0) in vec2 texCoords;
layout(location = 1) in vec3 normal;

vec3 lightDir = vec3(0.6, 0.9, 0.36);

void main() 
{
  AlbedoColor = vec4(vec3(clamp(dot(abs(normal), lightDir), 0.4, 1.0)) * texture(uTexAlbedo, texCoords).rgb, 1.0);
}
