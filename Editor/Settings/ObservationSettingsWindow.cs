using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Voyage.ObservationToolkit.Editor.Settings
{
    [Serializable]
    public class ObservationWeaverConfig
    {
        public bool enableWeaver = true;
        public bool weaveAssemblyCSharp = true;
        public List<string> extraAssemblies = new List<string>()
        {
            "Voyage.ObservationToolkit.Sample"
        };
    }

    public class ObservationSettingsWindow : EditorWindow
    {
        private const string ConfigPath = "ProjectSettings/ObservationWeaverSettings.json";
        private ObservationWeaverConfig _config;
        private ScrollView _listContainer;
        private TextField _searchField;
        private VisualElement _manualAddContainer;
        private TextField _manualAddField;

        // Load UXML and USS
        [SerializeField] private VisualTreeAsset _visualTree;
        [SerializeField] private StyleSheet _styleSheet;

        [MenuItem("Voyage/Observation Settings")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<ObservationSettingsWindow>();
            wnd.titleContent = new GUIContent("静态编织配置");
            wnd.minSize = new Vector2(400, 450);
        }

        private void OnEnable()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            if (File.Exists(ConfigPath))
            {
                try
                {
                    var json = File.ReadAllText(ConfigPath);
                    _config = JsonUtility.FromJson<ObservationWeaverConfig>(json);
                    
                    // Migration check: if old config structure was loaded (where assemblies list contained "Assembly-CSharp")
                    // we might want to clean it up, but JsonUtility is simple.
                    // If weaveAssemblyCSharp is false by default but user had it in list, we might miss it.
                    // But for now let's assume standard behavior.
                    if (_config.extraAssemblies.Contains("Assembly-CSharp"))
                    {
                        _config.extraAssemblies.Remove("Assembly-CSharp");
                        _config.weaveAssemblyCSharp = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load Observation Weaver settings: {e.Message}");
                    _config = new ObservationWeaverConfig();
                }
            }
            else
            {
                _config = new ObservationWeaverConfig();
                SaveConfig(); // Create default file
            }
        }

        private void SaveConfig()
        {
            try
            {
                var json = JsonUtility.ToJson(_config, true);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save Observation Weaver settings: {e.Message}");
            }
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;

            if (_visualTree == null)
            {
                var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
                var dir = Path.GetDirectoryName(scriptPath);
                _visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(dir, "ObservationSettingsWindow.uxml"));
                _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(dir, "ObservationSettingsWindow.uss"));
            }

            if (_visualTree != null)
            {
                _visualTree.CloneTree(root);
            }
            
            if (_styleSheet != null)
            {
                root.styleSheets.Add(_styleSheet);
            }

            // Bind Global Toggles
            var enableWeaverToggle = root.Q<Toggle>("enable-weaver-toggle");
            if (enableWeaverToggle != null)
            {
                enableWeaverToggle.value = _config.enableWeaver;
                enableWeaverToggle.RegisterValueChangedCallback(evt =>
                {
                    _config.enableWeaver = evt.newValue;
                    SaveConfig();
                });
            }

            var weaveAssemblyCSharpToggle = root.Q<Toggle>("weave-assembly-csharp-toggle");
            if (weaveAssemblyCSharpToggle != null)
            {
                weaveAssemblyCSharpToggle.value = _config.weaveAssemblyCSharp;
                weaveAssemblyCSharpToggle.RegisterValueChangedCallback(evt =>
                {
                    _config.weaveAssemblyCSharp = evt.newValue;
                    SaveConfig();
                });
            }

            // References
            _listContainer = root.Q<ScrollView>("list-container");
            _searchField = root.Q<TextField>("search-field");
            _manualAddContainer = root.Q<VisualElement>("manual-add-container");
            _manualAddField = root.Q<TextField>("manual-add-field");
            var manualAddBtn = root.Q<Button>("manual-add-button");
            var gearBtn = root.Q<Button>("gear-button");

            // Search Logic
            if (_searchField != null)
            {
                _searchField.RegisterValueChangedCallback(evt => RefreshList(evt.newValue));
            }

            // Gear Menu Logic
            if (gearBtn != null && _manualAddContainer != null)
            {
                // Initially hidden
                _manualAddContainer.style.display = DisplayStyle.None;

                gearBtn.clicked += () =>
                {
                    bool isVisible = _manualAddContainer.style.display == DisplayStyle.Flex;
                    _manualAddContainer.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
                };
            }

            // Manual Add Logic
            if (_manualAddField != null)
            {
                _manualAddField.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.Return) ManualAddAssembly();
                });
            }

            if (manualAddBtn != null)
            {
                manualAddBtn.clicked += ManualAddAssembly;
            }

            RefreshList();
        }

        private void RefreshList(string filter = "")
        {
            if (_listContainer == null) return;
            _listContainer.Clear();

            // 1. Find all .asmdef files in Assets
            var guids = AssetDatabase.FindAssets("t:asmdef", new[] { "Assets" });
            var foundAssemblies = new HashSet<string>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEditorInternal.AssemblyDefinitionAsset>(path);
                if (asset != null)
                {
                    // Need to parse JSON to get the actual name, or use filename as fallback?
                    // Unity's AssemblyDefinitionAsset.name is just the file name. 
                    // To get the actual assembly name defined inside, we need to parse content.
                    var content = File.ReadAllText(path);
                    var asmDef = JsonUtility.FromJson<AssemblyDefWrapper>(content);
                    var assemblyName = !string.IsNullOrEmpty(asmDef.name) ? asmDef.name : asset.name;
                    
                    foundAssemblies.Add(assemblyName);
                }
            }

            // Also include any currently configured assemblies that might not be found (e.g. system ones or manual adds)
            foreach (var asm in _config.extraAssemblies)
            {
                foundAssemblies.Add(asm);
            }

            var sortedList = foundAssemblies.ToList();
            sortedList.Sort((a, b) =>
            {
                bool aAdded = _config.extraAssemblies.Contains(a);
                bool bAdded = _config.extraAssemblies.Contains(b);

                if (aAdded && !bAdded) return -1;
                if (!aAdded && bAdded) return 1;

                return string.Compare(a, b, StringComparison.Ordinal);
            });

            foreach (var assemblyName in sortedList)
            {
                if (assemblyName == "Assembly-CSharp") continue; // Handled by separate toggle
                if (!string.IsNullOrEmpty(filter) && !assemblyName.ToLower().Contains(filter.ToLower())) continue;

                bool isAdded = _config.extraAssemblies.Contains(assemblyName);

                var itemRoot = new VisualElement();
                itemRoot.AddToClassList("list-item");
                
                var label = new Label(assemblyName);
                label.AddToClassList("assembly-label");
                itemRoot.Add(label);

                // Add/Remove Toggle Button
                var actionBtn = new Button(() => ToggleAssembly(assemblyName));
                actionBtn.AddToClassList("action-button");
                if (isAdded)
                {
                    actionBtn.text = "已添加";
                    actionBtn.AddToClassList("btn-added");
                }
                else
                {
                    actionBtn.text = "添加";
                    actionBtn.AddToClassList("btn-add");
                }
                itemRoot.Add(actionBtn);

                // Add background color for added items
                if (isAdded)
                {
                    itemRoot.AddToClassList("list-item-added");
                }
                else
                {
                    itemRoot.AddToClassList("list-item-not-added");
                }

                _listContainer.Add(itemRoot);
            }
        }

        private void ToggleAssembly(string assemblyName)
        {
            if (_config.extraAssemblies.Contains(assemblyName))
            {
                _config.extraAssemblies.Remove(assemblyName);
            }
            else
            {
                _config.extraAssemblies.Add(assemblyName);
            }
            SaveConfig();
            RefreshList(_searchField?.value);
        }

        private void ManualAddAssembly()
        {
            if (_manualAddField == null) return;
            var name = _manualAddField.value?.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                if (!_config.extraAssemblies.Contains(name))
                {
                    _config.extraAssemblies.Add(name);
                    SaveConfig();
                    RefreshList(_searchField?.value);
                }
                _manualAddField.value = "";
            }
        }

        [Serializable]
        private class AssemblyDefWrapper
        {
            public string name;
        }
    }
}
