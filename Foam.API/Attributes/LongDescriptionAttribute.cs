using System;

namespace Foam.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LongDescriptionAttribute : Attribute
    {
        public string Text { get; }

        public LongDescriptionAttribute(string text)
        {
            Text = text;
        }
    }
}
