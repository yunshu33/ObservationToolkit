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
    }
}