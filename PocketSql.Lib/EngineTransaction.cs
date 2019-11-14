using System.Data;

namespace PocketSql
{
    public class EngineTransaction : IDbTransaction
    {
        public EngineTransaction(EngineConnection connection, IsolationLevel il)
        {
            this.connection = connection;
            IsolationLevel = il;
        }

        private readonly EngineConnection connection;

        // Transactions don't do anything
        public void Dispose() { }
        public void Commit()
        {
        }
        public void Rollback()
        {
        }

        public IDbConnection Connection => connection;
        public IsolationLevel IsolationLevel { get; }
    }
}
