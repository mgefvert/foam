using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCommons;
using Foam.API.Providers;

namespace Foam.API.Transactions
{
    public class CommitBuffer
    {
        private readonly List<CommitItem> _onCommit = new List<CommitItem>();
        private readonly List<CommitItem> _onRollback = new List<CommitItem>();

        public void DeleteOnSuccess(IProvider provider, string filename)
        {
            _onCommit.Add(new CommitFileItem(provider, filename));
        }

        public void DeleteOnRollback(IProvider provider, string filename)
        {
            _onRollback.Add(new CommitFileItem(provider, filename));
        }

        public void ExecuteOnSuccess(IProvider provider, string name, Action<IProvider> action)
        {
            _onCommit.Add(new CommitActionItem(provider, name, action));
        }

        public void ExecuteOnRollback(IProvider provider, string name, Action<IProvider> action)
        {
            _onRollback.Add(new CommitActionItem(provider, name, action));
        }

        public void Commit()
        {
            if (!_onCommit.Any())
                return;

            Logger.Log("Committing transaction");
            foreach (var item in _onCommit.ExtractAll(x => true))
            {
                try
                {
                    Logger.Debug("Committing " + item);
                    item.Execute();
                }
                catch (Exception e)
                {
                    Logger.Warn($"Unable to commit {item}: {e.Message}");
                }
            }
        }

        public void Rollback()
        {
            if (_onRollback.Any())
                return;

            Logger.Log("Rolling back transaction");
            foreach (var item in _onRollback.ExtractAll(x => true))
            {
                try
                {
                    Logger.Log("Rolling back " + item);
                    item.Execute();
                }
                catch (Exception e)
                {
                    Logger.Warn($"Unable to roll back {item}: {e.Message}");
                }
            }
        }
    }
}
