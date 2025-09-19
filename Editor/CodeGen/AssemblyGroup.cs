using System;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEditorInternal;
using UnityEngine;

namespace LJVoyage.ObservationToolkit.Editor.CodeGen
{
    [CreateAssetMenu(menuName = "ObservationToolkit/AssemblyGroup")]
    public class AssemblyGroup : ScriptableObject
    {
        public List<AssemblyDefinitionAsset> Assemblies;
        
        private readonly string path = $"{Application.dataPath}/ObservationToolkit/Editor/CodeGen/Config";

#if UNITY_EDITOR
        private void OnValidate()
        {
            
        }
#endif
    }
}