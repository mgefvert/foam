using System;

namespace Foam.API.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyDescriptionAttribute : Attribute
    {
        public string Text { get; }

        public PropertyDescriptionAttribute(string text)
        {
            Text = text;
        }
    }
}
