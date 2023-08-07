using System;
using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using ImGuiNET;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Debugging.Debuggers
{
    public class SceneEntityDebugger : IGameComponent
    {
        public string Name { get; set; }


        public Entity Parent { get; set; }
        private Debugger Debugger { get; set; }

        public void Initialize()
        {
            Debugger = Parent as Debugger;
        }

        public bool Visible = false;

        public void Draw(float dt, RenderOptions? options = null)
        {
            if (!Visible) return;

            if (ImGui.Begin("Scene Tree"))
            {
                if (ImGui.TreeNode("Scene"))
                {
                    GameManager.Instance.GameScreenManager.GetCurrentInstance().Entities.ForEach(DrawEntityTree);

                    ImGui.TreePop();
                }

                ImGui.End();
            }
        }

        static void DrawEntityTree(IEntity? entity)
        {
            if (entity == null) return;

            if (ImGui.TreeNode(entity.Name))
            {
                DrawProperties(entity);

                foreach (IGameComponent? component in entity.Components.Values)
                {
                    if (component == null) continue;

                    if (ImGui.TreeNode(component.Name))
                    {
                        DrawProperties(component);

                        ImGui.TreePop();
                    }
                }

                foreach (IEntity? child in entity.Entities)
                    DrawEntityTree(child);


                ImGui.TreePop();
            }
            ImGui.NewLine();
        }

        static void DrawProperties(object component)
        {
            Type type = component.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (var property in properties)
            {
                object? value = property.GetValue(component);

                // Check if the property is read-only
                if (!property.CanWrite)
                {
                    ImGui.Text($"{property.Name}: {value}");
                    continue;
                }

                if (property.PropertyType == typeof(int))
                {
                    int intValue = (int)value;
                    if (ImGui.InputInt(property.Name, ref intValue))
                        property.SetValue(component, intValue);
                }
                else if (property.PropertyType == typeof(float))
                {
                    float floatValue = (float)value;
                    if (ImGui.InputFloat(property.Name, ref floatValue))
                    {
                        property.SetValue(component, floatValue);
                    }
                }
                else if (property.PropertyType == typeof(Vector2))
                {
                    Vector2 vectorValue = (Vector2)value;
                    if (ImGui.InputFloat2(property.Name, ref vectorValue))
                    {
                        property.SetValue(component, vectorValue);
                    }
                }
                else if (property.PropertyType == typeof(Vector3))
                {
                    Vector3 vectorValue = (Vector3)value;
                    if (ImGui.InputFloat3(property.Name, ref vectorValue))
                    {
                        property.SetValue(component, vectorValue);
                    }
                }
                else if (property.PropertyType == typeof(bool))
                {
                    bool vectorValue = (bool)value;
                    if (ImGui.Checkbox(property.Name, ref vectorValue))
                    {
                        property.SetValue(component, vectorValue);
                    }
                }
                // Add more property types as needed...

                // You can handle other property types here using ImGui controls
            }
        }



        public void Update(float dt)
        {

        }
    }
}

