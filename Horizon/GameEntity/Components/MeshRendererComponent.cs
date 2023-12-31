﻿using Horizon.Rendering;

namespace Horizon.GameEntity.Components
{
    [RequiresComponent(typeof(TransformComponent))]
    public class MeshRendererComponent : Mesh3D, IGameComponent
    {
        public string Name { get; set; }

        public Entity Parent { get; set; }
        public TransformComponent Transform { get; private set; }

        public void Initialize()
        {
            Transform = Parent.GetComponent<TransformComponent>()!;
            // ! We attached a [RequiresComponent(typeof(TransformComponent))]
        }

        protected override void BindAndSetUniforms(in RenderOptions options)
        {
            base.BindAndSetUniforms(in options);
            SetUniform("uModel", Transform.ModelMatrix);
        }

        public void UpdateState(float dt) { }

        public void UpdatePhysics(float dt) { }
    }
}
