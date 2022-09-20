﻿using System;
using System.ComponentModel;
using System.Reflection;

namespace Salvation.Core.Constants.Data
{
    public enum Spec
    {
        None = 0,
        HolyPriest = 257
    }

    public static class EnumExtensions
    {
        public static string GetDescription<T>(this T value)
        where T : Enum
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    if (Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }
    }
}
