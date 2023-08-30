#version 410 core

layout (location = 0) in vec3 inPosition;
layout (location = 1) in vec3 inNormal;
layout (location = 2) in vec2 inTexCoord;

uniform float uAspectRatio;
uniform vec2 uCenter;

out vec2 fragTexCoord;

void main() {
    gl_Position = vec4(inPosition, 1.0f);
    fragTexCoord = inPosition.xy * vec2(uAspectRatio * 10, 10) + uCenter;
}