using JetBrains.Annotations;
using UnityEngine;

namespace Coimbra.StronglyTypedStrings.DefaultUnits
{
    /// <summary>
    /// Generates strongly-typed strings for the Unity sorting layers. It also generates strongly-typed ints for them.
    /// </summary>
    [GeneratorExtension, UsedImplicitly]
    public sealed class SortingLayersGeneratorUnit : GeneratorUnit
    {
        /// <summary>
        /// The label to be displayed on the Project Settings menu.
        /// </summary>
        public override GUIContent Label { get; } = new GUIContent("Sorting Layers");

        protected override int Priority { get; } = 10;
        protected override string Type { get; } = "SortingLayer";

        protected override void OnGenerate()
        {
            foreach (SortingLayer layer in SortingLayer.layers)
            {
                Append(layer.name, layer.value);
            }
        }
    }
}
