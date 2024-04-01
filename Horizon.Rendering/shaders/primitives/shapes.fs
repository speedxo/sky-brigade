#version 460

layout(location = 0) in GS_OUT {
    vec3 colour;
} gs_in;

out vec4 FragColor;

void main() {
    FragColor = vec4(gs_in.colour, 1.0);
}
