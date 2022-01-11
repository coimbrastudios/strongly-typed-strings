using JetBrains.Annotations;
using UnityEditorInternal;
using UnityEngine;

namespace Coimbra.StronglyTypedStrings.DefaultUnits
{
    /// <summary>
    /// Generates strongly-typed strings for the Unity layers. It also generates strongly-typed ints for them.
    /// </summary>
    [GeneratorExtension, UsedImplicitly]
    public sealed class LayersGeneratorUnit : GeneratorUnit
    {
        /// <summary>
        /// The label to be displayed on the Project Settings menu.
        /// </summary>
        public override GUIContent Label { get; } = new GUIContent("Layers");

        protected override int Priority { get; } = 10;
        protected override string Type { get; } = "Layer";

        protected override void OnGenerate()
        {
            foreach (string layer in InternalEditorUtility.layers)
            {
                Append(layer, LayerMask.NameToLayer(layer));
            }
        }
    }
}
