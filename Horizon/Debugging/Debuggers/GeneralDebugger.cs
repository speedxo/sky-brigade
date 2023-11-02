using Horizon.Logging;
using Horizon.Rendering;
using ImGuiNET;

namespace Horizon.Debugging.Debuggers
{
    public class GeneralDebugger : DebuggerComponent
    {
        private class GeneralDebuggerCatagory
        {
            internal int TotalWatchedValues
            {
                get => _singleUseValues.Count + _cachedValues.Count;
            }

            private readonly GeneralDebugger _generalDebugger;

            private Dictionary<string, Func<object>> _singleUseValues;
            private Dictionary<string, object> _cachedValues;
            private Dictionary<string, Func<object>> _monitoredVariables;

            internal GeneralDebuggerCatagory(GeneralDebugger debugger)
            {
                _generalDebugger = debugger;

                _monitoredVariables = new();
                _singleUseValues = new();
                _cachedValues = new();
            }

            internal void Clear()
            {
                _monitoredVariables.Clear();
                _singleUseValues.Clear();
                _cachedValues.Clear();
            }

            internal void Draw()
            {
                foreach ((string name, object value) in _cachedValues)
                {
                    ImGui.Text($"{name}:");
                    ImGui.NextColumn();
                    ImGui.Text($"{value}");
                    ImGui.NextColumn();
                }
                foreach ((string name, object value) in _singleUseValues)
                {
                    ImGui.Text($"{name}:");
                    ImGui.NextColumn();
                    ImGui.Text($"{value}");
                    ImGui.NextColumn();
                }

                _singleUseValues.Clear();
            }

            internal void UpdateValues()
            {
                foreach ((string name, Func<object> valueExpr) in _monitoredVariables)
                {
                    _cachedValues[name] = valueExpr();
                }
            }

            internal void AddWatch(string name, Func<object> objExpr)
            {
                if (_monitoredVariables.ContainsKey(name))
                    _generalDebugger.Log(
                        LogLevel.Warning,
                        $"Attempt to assign watch '{name}' which already exists. Overriding!"
                    );

                _monitoredVariables[name] = objExpr;
            }
        }

        private readonly object _catagoriesLock = new();
        private Dictionary<string, GeneralDebuggerCatagory> _catagories;

        private float _updateCachedValuesTimer = 0.0f;
        private int watchedCount = 0;

        public override void Initialize()
        {
            _catagories = new();
            _catagories["Misc"] = new GeneralDebuggerCatagory(this);

            Name = "General Information";
        }

        public void AddWatch(string name, Func<object> objExpr) => AddWatch(name, "Misc", objExpr);

        public void AddWatch(string name, string catagory, Func<object> objExpr)
        {
            lock (_catagoriesLock)
            {
                if (!_catagories.ContainsKey(catagory))
                    _catagories.Add(catagory, new GeneralDebuggerCatagory(this));

                _catagories[catagory].AddWatch(name, objExpr);
            }
        }

        public override void Draw(float dt, ref RenderOptions options)
        {
            if (!Visible)
                return;

            if (ImGui.Begin(Name))
            {
                ImGui.Text($"Watched Variables ({watchedCount})");
                ImGui.Separator();

                foreach ((string name, GeneralDebuggerCatagory monitor) in _catagories)
                {
                    if (monitor.TotalWatchedValues < 1)
                        continue;

                    if (ImGui.CollapsingHeader(name))
                    {
                        ImGui.Columns(2, $"generalDebuggerColumns_{name}", true); // 2 columns

                        ImGui.Text("Name:");
                        ImGui.NextColumn();
                        ImGui.Text("Value:");
                        ImGui.NextColumn();

                        monitor.Draw();
                        ImGui.Columns(1);
                    }
                }

                ImGui.End();
            }
        }

        public override void Dispose() { }

        public override void Update(float dt)
        {
            if (!Visible)
                return;

            _updateCachedValuesTimer += dt;
            if (_updateCachedValuesTimer > 0.5f)
            {
                watchedCount = 0;
                foreach (var item in _catagories.Values)
                {
                    item.UpdateValues();
                    watchedCount += item.TotalWatchedValues;
                }
            }
        }

        ~GeneralDebugger()
        {
            foreach (var item in _catagories.Values)
            {
                item.Clear();
            }
            _catagories.Clear();
        }
    }
}
