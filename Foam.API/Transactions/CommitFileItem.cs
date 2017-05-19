using System;
using Foam.API.Providers;

namespace Foam.API.Transactions
{
    public class CommitFileItem : CommitItem
    {
        public string Filename { get; set; }

        public CommitFileItem(IProvider provider, string filename) : base(provider)
        {
            Filename = filename;
        }

        public override void Execute()
        {
            Provider.Delete(Filename);
        }

        public override string ToString()
        {
            return Provider.GetType().Name + ": " + Filename;
        }
    }
}
