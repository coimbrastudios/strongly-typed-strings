using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Coimbra.StronglyTypedStrings
{
    /// <summary>
    /// Inherit from that class to create your own file generator.
    /// <para></para>
    /// You can also add the attribute <see cref="GeneratorExtensionAttribute"/> to expose it on the Project Settings windows.
    /// </summary>
    public abstract class GeneratorUnit
    {
        internal sealed class LabelComparer : IComparer<GeneratorUnit>
        {
            int IComparer<GeneratorUnit>.Compare(GeneratorUnit x, GeneratorUnit y)
            {
                Assert.IsNotNull(x);
                Assert.IsNotNull(y);

                return string.Compare(x.Label.text, y.Label.text, StringComparison.Ordinal);
            }
        }

        internal sealed class PriorityComparer : IComparer<GeneratorUnit>
        {
            int IComparer<GeneratorUnit>.Compare(GeneratorUnit x, GeneratorUnit y)
            {
                Assert.IsNotNull(x);
                Assert.IsNotNull(y);

                return x.Priority.CompareTo(y.Priority);
            }
        }

        private readonly StringBuilder EnumBuilder = new StringBuilder();
        private readonly StringBuilder SwitchBuilder = new StringBuilder();

        /// <summary>
        /// The label to be displayed on the Project Settings menu.
        /// </summary>
        public abstract GUIContent Label { get; }
        /// <summary>
        /// The generated file name.
        /// </summary>
        public string FileName => $"{Type}Type.cs";

        /// <summary>
        /// Define what your generated type represent.
        /// </summary>
        protected abstract string Type { get; }
        /// <summary>
        /// Customize the order in the Project Settings menu.
        /// </summary>
        protected virtual int Priority { get; } = 0;

        /// <summary>
        /// This method is used to draw it on the Project Settings if the <see cref="GeneratorExtensionAttribute"/> is present.
        /// </summary>
        public virtual void DrawSettings(string searchContext) { }

        /// <summary>
        /// Generate the file.
        /// </summary>
        /// <param name="destination">The system-relative output path for the file.</param>
        /// <param name="namespaceContent">The content to be put in the namespace section of the file.</param>
        /// <param name="lineEnding">The line ending character to be used.</param>
        /// <param name="refresh">If true it will call <see cref="AssetDatabase.Refresh()"/> after generating the file.</param>
        public void Generate(string destination, string namespaceContent, string lineEnding, bool refresh)
        {
            EnumBuilder.Clear();
            SwitchBuilder.Clear();

            OnGenerate();

            string enumContent = EnumBuilder.ToString();
            string switchContent = SwitchBuilder.ToString();
            bool hasNamespace = !string.IsNullOrEmpty(namespaceContent);
            string content = hasNamespace ? Templates.WithNamespace : Templates.WithoutNamespace;
            content = content.Replace(Templates.Tags.Switch, switchContent);
            content = content.Replace(Templates.Tags.Enum, enumContent);
            content = content.Replace(Templates.Tags.Namespace, namespaceContent);
            content = content.Replace(Templates.Tags.NamespaceTab, hasNamespace ? Templates.Tab : "");
            content = content.Replace(Templates.Tags.Type, Type);

            using (var writer = new StreamWriter(Path.Combine(destination, FileName)))
            {
                writer.Write(content.Replace(Templates.Tags.Newline, lineEnding));
            }

            if (refresh)
            {
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Implement your generator logic here.
        /// </summary>
        protected abstract void OnGenerate();

        /// <summary>
        /// Append a value.
        /// </summary>
        protected void Append(string value)
        {
            Append(value, value);
        }

        /// <summary>
        /// Append a value with a custom name.
        /// </summary>
        protected void Append(string name, string value)
        {
            name = GeneratorUtility.GetValidMemberName(name);
            EnumBuilder.Append(Templates.Tags.Newline);
            EnumBuilder.Append($"{Templates.EnumTabs}{name},");
            SwitchBuilder.Append(Templates.Tags.Newline);
            SwitchBuilder.Append($"{Templates.SwitchTabs}case {Templates.EnumType}.{name}: return \"{value}\";");
        }

        /// <summary>
        /// Append a value and an id.
        /// </summary>
        protected void Append(string value, int id)
        {
            Append(value, value, id);
        }

        /// <summary>
        /// Append a value and an id with a custom name.
        /// </summary>
        protected void Append(string name, string value, int id)
        {
            name = GeneratorUtility.GetValidMemberName(name);
            EnumBuilder.Append(Templates.Tags.Newline);
            EnumBuilder.Append($"{Templates.EnumTabs}{name} = {id},");
            SwitchBuilder.Append(Templates.Tags.Newline);
            SwitchBuilder.Append($"{Templates.SwitchTabs}case {Templates.EnumType}.{name}: return \"{value}\";");
        }
    }
}
