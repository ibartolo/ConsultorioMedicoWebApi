using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ConsultorioMedicoWebApi.DAL
{
    /// <summary>
    /// Base abstracta de acceso a datos. Usa System.Data.SqlClient y Stored Procedures por defecto.
    /// Mantiene una sola SqlConnection y una transacción "ambiente" opcional (evita MSDTC).
    /// </summary>
    public abstract class BaseDbWrapper
    {
        protected abstract string SQLConnectionString { get; }
        protected abstract TimeSpan SQLCommandTimeOut { get; }

        private SqlConnection _connection;
        private SqlTransaction _transaction;

        protected SqlConnection Connection
        {
            get
            {
                if (_connection == null)
                    _connection = new SqlConnection(SQLConnectionString);
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();
                return _connection;
            }
        }

        protected object ExecuteScalar(string cmdText, CommandType cmdType = CommandType.StoredProcedure, IEnumerable<SqlParameter> pars = null)
        {
            using (var cmd = new SqlCommand(cmdText, Connection, _transaction))
            {
                cmd.CommandType = cmdType;
                cmd.CommandTimeout = (int)SQLCommandTimeOut.TotalSeconds;
                if (pars != null) foreach (var p in pars) cmd.Parameters.Add(p);
                return cmd.ExecuteScalar();
            }
        }

        protected int ExecuteNonQuery(string cmdText, CommandType cmdType = CommandType.StoredProcedure, IEnumerable<SqlParameter> pars = null)
        {
            using (var cmd = new SqlCommand(cmdText, Connection, _transaction))
            {
                cmd.CommandType = cmdType;
                cmd.CommandTimeout = (int)SQLCommandTimeOut.TotalSeconds;
                if (pars != null) foreach (var p in pars) cmd.Parameters.Add(p);
                return cmd.ExecuteNonQuery();
            }
        }

        protected T GetObject<T>(string cmdText, Func<IDataReader, T> mapper, CommandType cmdType = CommandType.StoredProcedure, IEnumerable<SqlParameter> pars = null)
        {
            using (var cmd = new SqlCommand(cmdText, Connection, _transaction))
            {
                cmd.CommandType = cmdType;
                cmd.CommandTimeout = (int)SQLCommandTimeOut.TotalSeconds;
                if (pars != null) foreach (var p in pars) cmd.Parameters.Add(p);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) return mapper(reader);
                    return default(T);
                }
            }
        }

        protected IEnumerable<T> GetObjects<T>(string cmdText, Func<IDataReader, T> mapper, CommandType cmdType = CommandType.StoredProcedure, IEnumerable<SqlParameter> pars = null)
        {
            using (var cmd = new SqlCommand(cmdText, Connection, _transaction))
            {
                cmd.CommandType = cmdType;
                cmd.CommandTimeout = (int)SQLCommandTimeOut.TotalSeconds;
                if (pars != null) foreach (var p in pars) cmd.Parameters.Add(p);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) yield return mapper(reader);
                }
            }
        }

        public void BeginTransaction()
        {
            if (_transaction == null)
                _transaction = Connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
        }
    }
}
