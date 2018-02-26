using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Infrastructure.Interfaces
{
    public interface IDbContext : IDisposable
    {
        Guid SessionID { get; }
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        void CommitTransaction();
        Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, int? commandTimeout = null);
        Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id", int? commandTimeout = null);
        Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object param = null, int? commandTimeout = null);
        void RollbackTransaction();
    }
}
