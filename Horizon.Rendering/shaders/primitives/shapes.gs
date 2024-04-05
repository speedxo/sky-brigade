#version 460

layout (points) in;
layout (triangle_strip, max_vertices = 38) out;

layout(location = 0) in VS_OUT {
    uint type;
    vec2 translation;
    vec2 scale;
    float rotation;
    vec3 colour;
} gs_in[];

layout(location = 0) out GS_OUT {
    vec3 colour;
} gs_out;

out vec3 color;

uniform mat4 uModel;
uniform mat4 uView;

void main() {
    // premutate matrix
    mat4 transMat = uView * uModel;

    // convert rotation to radians
    float rotation = radians(gs_in[0].rotation); 
    vec2 scale = gs_in[0].scale;
    vec2 translation = gs_in[0].translation;
    uint type = gs_in[0].type;

    // pass to fragment shader
    gs_out.colour = gs_in[0].colour;

    if (type == 0u) {
        // Generate a triangle
        const vec2 triangleVertices[3] = vec2[3](
            vec2(-1.0, -sqrt(3.0) / 3.0),
            vec2(1.0, -sqrt(3.0) / 3.0),
            vec2(0.0, 2.0 * sqrt(3.0) / 3.0)
        );
        for (int i = 0; i < 3; ++i) {
            vec2 vertex = triangleVertices[i];
            vertex *= scale;
            float x = vertex.x * cos(rotation) - vertex.y * sin(rotation);
            float y = vertex.x * sin(rotation) + vertex.y * cos(rotation);
            vertex = vec2(x, y) + translation;
            gl_Position = transMat * vec4(vertex, 0.0, 1.0);
            EmitVertex();
        }
    } else if (type == 1u) {
        // Generate a square
        const vec2 squareVertices[4] = vec2[4](
            vec2(-1.0, -1.0),
            vec2(1.0, -1.0),
            vec2(-1.0, 1.0),
            vec2(1.0, 1.0)
        );
        for (int i = 0; i < 4; ++i) {
            vec2 vertex = squareVertices[i];
            vertex *= scale;
            float x = vertex.x * cos(rotation) - vertex.y * sin(rotation);
            float y = vertex.x * sin(rotation) + vertex.y * cos(rotation);
            vertex = vec2(x, y) + translation;
            gl_Position = transMat * vec4(vertex, 0.0, 1.0);
            EmitVertex();
        }
    } 
   else if (type == 2u) {
        // Generate a circle
        const int circleSegments = 12;
        vec2 circleVertices[circleSegments];

        for (int i = 0; i < circleSegments; ++i) {
            float theta = radians(float(i) / float(circleSegments) * 360.0);
            circleVertices[i] = vec2(cos(theta), sin(theta));
        }

        for (int i = 0; i <= circleSegments; i++) {
            int index = i % circleSegments; // Wrap around to the beginning
            vec2 vertex = circleVertices[index];
            vertex *= scale;
            float x = vertex.x * cos(rotation) - vertex.y * sin(rotation);
            float y = vertex.x * sin(rotation) + vertex.y * cos(rotation);
            vertex = vec2(x, y) + translation;
        
            gl_Position = transMat * vec4(vertex, 0.0, 1.0);
            EmitVertex();

            // Close the circle by emitting a vertex at the center for every second vertex
            if (i % 2 == 0) {
                gl_Position = transMat * vec4(translation, 0.0, 1.0);
                EmitVertex();
            }
        }
    }
    
    EndPrimitive();
}
