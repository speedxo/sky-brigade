#version 410 core

layout(location = 0) out vec4 AlbedoColor;

uniform sampler2D uTexAlbedo;

layout(location = 0) in vec2 texCoords;
layout(location = 1) in vec3 normal;
layout(location = 2) in float shade;

vec2 singleTextureSize = vec2(16.0 / 256.0);
vec3 lightDir = vec3(0.707, 0.9, 0.36);

void main() 
{
  AlbedoColor = vec4(vec3(clamp(dot(normalize(normal), lightDir), 0.3, 1.0))  * texture(uTexAlbedo, texCoords * singleTextureSize).xyz, 1.0);
}
