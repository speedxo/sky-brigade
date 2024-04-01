#version 460

layout(location = 0) in uint vType; 
layout(location = 1) in vec2 vPos; 
layout(location = 2) in vec2 vScale; 
layout(location = 3) in float vRot; 
layout(location = 4) in vec3 vCol; 

layout(location = 0) out VS_OUT {
    uint type;
    vec2 translation;
    vec2 scale;
    float rotation;
    vec3 colour;
} vs_out;

void main() {
  vs_out.type = vType;
  vs_out.translation = vPos;
  vs_out.scale = vScale;
  vs_out.rotation = vRot;
  vs_out.colour = vCol;
}