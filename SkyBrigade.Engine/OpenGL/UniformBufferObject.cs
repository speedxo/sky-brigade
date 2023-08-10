﻿using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace SkyBrigade.Engine.OpenGL
{
    public class UniformBufferObject : BufferObject
    {
        private readonly uint _bindingPoint;

        public UniformBufferObject(Shader shader, string point)
            : base(BufferTargetARB.UniformBuffer)
        {
            _bindingPoint = shader.GetUniformBlockIndex(point);
            GameManager.Instance.Gl.UniformBlockBinding(shader.Handle, _bindingPoint, _bindingPoint);
        }

        public void BindToBindingPoint()
        {
            GameManager.Instance.Gl.BindBufferBase(BufferTargetARB.UniformBuffer, _bindingPoint, Handle);
        }
    }
}