using MySql.Data.MySqlClient;
using Newbe.Claptrap.StorageProvider.MySql;
using System;
using System.Collections;
using Dapper;

namespace Integral.Models
{
    public interface IIntegralConfigStore
    {
        IntegralConfig Get(string classId);
        void Save(IntegralConfig config);
    }
    public class IntegralConfigStore : IIntegralConfigStore
    {
        private MySqlConnection db;
        private IDbFactory _dbFactory;
        private static Hashtable database = new Hashtable();
        public IntegralConfigStore(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public IntegralConfigStore() {}
        public IntegralConfig Get(string classId)
        {
            if (database.ContainsKey(classId))
                return database[classId] as IntegralConfig;
            db = _dbFactory.GetConnection("claptrap");
            var config = db.QueryFirst<IntegralConfig>(ClaptrapCodes.ClassIdSql, new { ClassId = classId });
            Save(config);
            db.Close();
            return config;
        }

        public void Save(IntegralConfig config)
        {
            if (database.ContainsKey(config.ClassId))
                database[config.ClassId] = config;
            else
                database.Add(config.ClassId, config);
        }
    }

    public class IntegralConfig
    {
        public string ClassId { get; set; }
        public int Score { get; set; } = 1;
        public int Limit { get; set; }
        public string AreaCode { get; set; } = "510500";
        public DateTime AddTime { get; set; } = DateTime.Now;
    }
}
