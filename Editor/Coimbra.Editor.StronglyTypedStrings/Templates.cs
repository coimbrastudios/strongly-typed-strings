namespace Coimbra.StronglyTypedStrings
{
    internal static class Templates
    {
        internal static class Tags
        {
            internal const string Switch = "#SWITCH#";
            internal const string Enum = "#ENUM#";
            internal const string Namespace = "#NAMESPACE#";
            internal const string NamespaceTab = "#NAMESPACETAB#";
            internal const string Newline = "#NEWLINE#";
            internal const string Type = "#TYPE#";
        }

        internal const string EnumType = Tags.Type + "Type";
        internal const string EnumTabs = Tags.NamespaceTab + Tab;
        internal const string SwitchTabs = Tags.NamespaceTab + Tab + Tab + Tab;
        internal const string WithNamespace = Header + "namespace " + Tags.Namespace + Tags.Newline + "{" + Tags.Newline + Body + "}" + Tags.Newline;
        internal const string WithoutNamespace = Header + Body;
        internal const string Tab = "    ";

        // @formatter:off
        private const string Body = Tags.NamespaceTab + "/// <summary>" + Tags.Newline
                                  + Tags.NamespaceTab + "/// Use ToValue() to get the actual string value." + Tags.Newline
                                  + Tags.NamespaceTab + "/// </summary>" + Tags.Newline
                                  + Tags.NamespaceTab + "public enum " + EnumType + Tags.Newline
                                  + Tags.NamespaceTab + "{" + Tags.Enum + Tags.Newline
                                  + Tags.NamespaceTab + "}" + Tags.Newline + Tags.Newline
                                  + Tags.NamespaceTab + "public static class " + EnumType + "Extensions" + Tags.Newline
                                  + Tags.NamespaceTab + "{" + Tags.Newline
                                  + Tags.NamespaceTab + Tab + "/// <summary>" + Tags.Newline
                                  + Tags.NamespaceTab + Tab + "/// Get the actual string value for the " + Tags.Type + "." + Tags.Newline
                                  + Tags.NamespaceTab + Tab + "/// </summary>" + Tags.Newline
                                  + Tags.NamespaceTab + Tab + "public static string GetValue(this " + EnumType + " enumValue)" + Tags.Newline
                                  + Tags.NamespaceTab + Tab + "{" + Tags.Newline
                                  + Tags.NamespaceTab + Tab + Tab + "switch (enumValue)" + Tags.Newline
                                  + Tags.NamespaceTab + Tab + Tab + "{" + Tags.Switch + Tags.Newline
                                  + Tags.NamespaceTab + Tab + Tab + Tab + "default: throw new System.ArgumentOutOfRangeException();" + Tags.Newline
                                  + Tags.NamespaceTab + Tab + Tab + "}" + Tags.Newline
                                  + Tags.NamespaceTab + Tab + "}" + Tags.Newline
                                  + Tags.NamespaceTab + "}" + Tags.Newline;
        // @formatter:on

        private const string Header = "// This file is auto-generated!" + Tags.Newline + Tags.Newline;
    }
}
