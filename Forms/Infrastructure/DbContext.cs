using Dapper;
using Forms.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Infrastructure
{
    public class DbContext : IDbContext
    {
        private Guid _sessionID;
        private SqlConnection _connection;
        private SqlTransaction _transaction;

        public DbContext(string connectionString)
        {
            _sessionID = Guid.NewGuid();
            _connection = CreateConnection(connectionString);
        }

        public Guid SessionID
        {
            get { return _sessionID; }
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _transaction = _connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _transaction.Commit();
            _transaction = null;
        }

        public void Dispose()
        {
            if (_connection != null && _connection.State != ConnectionState.Closed)
            {
                if (_transaction != null)
                    _transaction.Rollback();

                _connection.Close();
                _connection = null;
            }
        }

        public async Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null)
        {
            try
            {
                return await SqlMapper.ExecuteAsync(_connection, sql, param, _transaction, commandTimeout);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, int? commandTimeout = null)
        {
            try
            {
                return await SqlMapper.QueryAsync<T>(_connection, sql, param, _transaction, commandTimeout: commandTimeout);
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id", int? commandTimeout = null)
        {
            return await SqlMapper.QueryAsync(_connection, sql, map, param, _transaction, true, splitOn, commandTimeout);
        }

        public async Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object param = null, int? commandTimeout = null)
        {
            return await SqlMapper.QueryMultipleAsync(_connection, sql, param, _transaction, commandTimeout);
        }

        public void RollbackTransaction()
        {
            _transaction.Rollback();
            _transaction = null;
        }

        private SqlConnection CreateConnection(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}
