using System;

namespace Foam.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ShortDescriptionAttribute : Attribute
    {
        public string Text { get; }

        public ShortDescriptionAttribute(string text)
        {
            Text = text;
        }
    }
}
