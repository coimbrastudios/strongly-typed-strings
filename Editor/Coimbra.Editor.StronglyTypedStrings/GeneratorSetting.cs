using UnityEditor.SettingsManagement;

namespace Coimbra.StronglyTypedStrings
{
    /// <summary>
    /// Specialized <see cref="UserSetting{T}"/> for any <see cref="GeneratorUnit"/>.
    /// </summary>
    public sealed class GeneratorSetting<T> : UserSetting<T>
    {
        public GeneratorSetting(string key, T value)
            : base(Generator.Instance, key, value) { }
    }
}
