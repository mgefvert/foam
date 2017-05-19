using System;
using Foam.API.Providers;

namespace Foam.API.Transactions
{
    public abstract class CommitItem
    {
        public IProvider Provider { get; set; }
        public abstract void Execute();

        protected CommitItem(IProvider provider)
        {
            Provider = provider;
        }
    }
}
