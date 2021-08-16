using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Services.IntergralServer.DapperPlus
{
    /// <summary>
    /// 写Dapper接口
    /// </summary>
    public interface IDapperPlusWrite
    {
        /// <summary>
        /// 连接
        /// </summary>
        public IDbConnection Connection
        {
            get;
        }
        /// <summary>
        /// Execute parameterized SQL.
        /// </summary>
        /// <param name="sql">The SQL to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The number of rows affected.</returns>
        Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

    }

    /// <summary>
    /// 写Dapper类
    /// </summary>
    public class DapperPlusWrite : IDapperPlusWrite
    {
        private readonly IDbConnection _connection;
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="connection">连接</param>
        /// <param name="configuration">配置</param>
        public DapperPlusWrite(IDbConnection connection, IConfiguration configuration)
        {
            var connectionStrings = configuration.GetSection("ConnectionStrings").Get<Dictionary<string, string>>();
            _connection = connection;
            _connection.ConnectionString = connectionStrings.Where(s => s.Key.ToLower().Contains("write")).FirstOrDefault().Value;
        }
        /// <summary>
        /// 连接
        /// </summary>
        public IDbConnection Connection
        {
            get
            {
                return _connection;
            }
        }
        /// <summary>
        /// Execute parameterized SQL.
        /// </summary>
        /// <param name="sql">The SQL to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The number of rows affected.</returns>
        public async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await _connection.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
        }
   
    }
}
