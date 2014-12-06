﻿using CoreLibrary.Settings.Attributes;
using System;
using System.Reflection;

namespace CoreLibrary.Settings.Types
{
    public class IntSettings : SettingsAbstract
    {
        public static Type ForType { get { return typeof(int); } }

        public override int NumberOfParams { get { return 1; } }

        public IntSettings(object instance, FieldInfo field, SettingsAttribute attribute)
            : base(instance, field, attribute)
        {
        }

        public override void SetValue(params string[] value)
        {
            Field.SetValue(Instance, int.Parse(value[0]));
            WasSet = true;
        }
    }
}
