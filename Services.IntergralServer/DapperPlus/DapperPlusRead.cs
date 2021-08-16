using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Services.IntergralServer.DapperPlus
{

    /// <summary>
    /// DapperPlus读接口
    /// </summary>
    public interface IDapperPlusRead
    {
        /// <summary>
        /// 连接
        /// </summary>
        public IDbConnection Connection
        {
            get;
        }
        /// <summary>
        /// Executes a single-row query, returning the data typed as type.
        /// </summary>
        /// <param name="type">The type to return.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer results in memory.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>A sequence of data of the supplied type; if a basic type(int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is created per row, and a direct column-name===member-name mapping is assumed(case insensitive).
        /// 异常:T:System.ArgumentNullException:type is null.
        /// </returns>
        Task<IList<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        Task<T> QueryFirstAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    }
    /// <summary>
    /// DapperPlus读实现类
    /// </summary>
    public class DapperPlusRead : IDapperPlusRead
    {
        protected IDbConnection _connection;
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="connection">连接</param>
        /// <param name="configuration">配置</param>
        public DapperPlusRead(IDbConnection connection, IConfiguration configuration)
        {
            var connectionStrings = configuration.GetSection("ConnectionStrings").Get<Dictionary<string, string>>();
            _connection = connection;
            _connection.ConnectionString = connectionStrings.Where(s => s.Key.ToLower().Contains("read")).FirstOrDefault().Value;
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
        /// Executes a single-row query, returning the data typed as type.
        /// </summary>
        /// <param name="type">The type to return.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer results in memory.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>A sequence of data of the supplied type; if a basic type(int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is created per row, and a direct column-name===member-name mapping is assumed(case insensitive).
        /// 异常:T:System.ArgumentNullException:type is null.
        /// </returns>
        public async Task<IList<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null,  int? commandTimeout = null, CommandType? commandType = null)
        {
            return (await _connection.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType)).ToList();
        }


        public async Task<T> QueryFirstAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await _connection.QueryFirstAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }
    }
}
