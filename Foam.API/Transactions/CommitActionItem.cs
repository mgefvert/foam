using System;
using Foam.API.Providers;

namespace Foam.API.Transactions
{
    public class CommitActionItem : CommitItem
    {
        public Action<IProvider> Action { get; }
        public string Name { get; }

        public CommitActionItem(IProvider provider, string name, Action<IProvider> action) : base(provider)
        {
            Name = name;
            Action = action;
        }

        public override void Execute()
        {
            Action(Provider);
        }

        public override string ToString()
        {
            return Provider.GetType().Name + ": " + Name;
        }
    }
}
