#version 460 core

// in the fragment shader we can specify and name the fragment output ourselves :)
out vec4 oFragColor;

void main()
{
	/* here we simply set it to white, it should be noted that in GLSL you are no longer required to specify your precision, 0.0f vs 0.0, 
	 * back a while ago you'd need a second preprocessor directive to specify your floating point precision, which would in turn have a slight 
	 * performance penelty for doubles, however this is generally no longer required as it isnt 2010 and we arent an android running ginderbread targetting GLES 1.5. */

	oFragColor = vec4(1.0);	
}