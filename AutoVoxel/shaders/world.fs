#version 410 core

layout(location = 0) out vec4 AlbedoColor;

uniform sampler2D uTexAlbedo;

layout(location = 0) in vec2 texCoords;
layout(location = 1) in vec3 normal;

vec3 lightDir = vec3(0.6, 0.9, 0.36);

void main() 
{
    vec4 texColor = texture(uTexAlbedo, texCoords);
    if (texColor.a == 0.0) discard;
    AlbedoColor = vec4(vec3(clamp(dot(abs(normal), lightDir), 0.4, 1.0)) * texColor.rgb, texColor.a);
}
