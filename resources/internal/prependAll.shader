#version 410 core


[include uniformBufferObjects.inc.glsl]

[include gBuffer.inc.glsl]

[include lightingModels.inc.glsl]

[include snoise.inc.glsl]


// from http://stackoverflow.com/questions/6652253/getting-the-true-z-value-from-the-depth-buffer
float linearDepth(float depthSample)
{
    depthSample = 2.0 * depthSample - 1.0;
    float zLinear = 2.0 * engine.nearClipPlane * engine.farClipPlane / (engine.farClipPlane + engine.nearClipPlane - depthSample * (engine.farClipPlane - engine.nearClipPlane));
    return zLinear;
}

// result suitable for assigning to gl_FragDepth
float depthSample(float linearDepth)
{
    float nonLinearDepth = (engine.farClipPlane + engine.nearClipPlane - 2.0 * engine.nearClipPlane * engine.farClipPlane / linearDepth) / (engine.farClipPlane - engine.nearClipPlane);
    nonLinearDepth = (nonLinearDepth + 1.0) / 2.0;
    return nonLinearDepth;
}