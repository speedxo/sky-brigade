#version 410 core


layout(location = 0) in vec2 iTexCoords;

out vec4 frag_out;

void main() {
    frag_out = vec4(vec3(iTexCoords.x), 1.0);
}