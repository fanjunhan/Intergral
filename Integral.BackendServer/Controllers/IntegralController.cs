using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapr;
using Dapper;
using Integral.Models;
using Microsoft.Extensions.Logging;
using Newbe.Claptrap.StorageProvider.MySql;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace Services.IntergralServer.Controllers
{
  
    [ApiController]
    public class IntegralController : Controller
    {
        private MySqlConnection db;
        private readonly IDbFactory _dbFactory;
        private readonly IIntegralConfigStore configStore;
        private readonly ILogger<IntegralController> _logger;
        const string addSql = "INSERT INTO integral_record_51 (ClassId,UserId,DataId,Title,Score,AddTime) VALUES (@ClassId,@UserId,@ItemId,@Title,@Score,@AddTime)";
        const string updateIntegralSql = "INSERT INTO integral_area (Integral_Userid,Integral_Count,MonthCount,AreaCode,UnionId,UserName) VALUES (@UserId,@Score,@Score,@AreaCode,@UnionId,@UserName) ON DUPLICATE KEY UPDATE Integral_Count = Integral_Count+@Score,MonthCount = MonthCount+@Score,UnionId=@UnionId,UserName=@UserName;";
        private static ConcurrentDictionary<string, BlockingCollection<IntegralRecord>> newsList = new ConcurrentDictionary<string, BlockingCollection<IntegralRecord>>();
        private string suffix = (DateTime.Now.Year - 2020) + "_" + DateTime.Now.Month;//每月一张表
        const int timeDiff = 500;
        public IntegralController(ILoggerFactory logFactory, IDbFactory dbFactory, IIntegralConfigStore configStore)
        {
            _logger = logFactory.CreateLogger<IntegralController>();
            this.configStore = configStore;
            _dbFactory = dbFactory;
        }

        [Topic(ClaptrapCodes.PubSub, ClaptrapCodes.IntegralCompleted)]
        [HttpPost(ClaptrapCodes.IntegralCompleted)]
        public async Task AppNewsAsync(IntegralRecord dto)
        {            
            var config = configStore.Get(dto.ClassId);
            var areaCode = dto.AreaCode = config.AreaCode;
            if (config.Limit >1|| dto.Score> config.Score) dto.Score = config.Score;
            if (!newsList.ContainsKey(areaCode))
            {
                newsList[areaCode] = new BlockingCollection<IntegralRecord>();
            }
            newsList[areaCode].Add(dto);
            if (dto.AddTime > newsList[areaCode].FirstOrDefault().AddTime.AddMilliseconds(timeDiff))
            {
                var dataList = new List<IntegralRecord>();
                while (dataList.Count < 500 && newsList[areaCode].Count > 1)
                {
                    dataList.Add(newsList[areaCode].Take());
                }
                await SaveIntegralRecord(dataList);
            }
        }

        private async Task SaveIntegralRecord(List<IntegralRecord> lists)
        {            
            if (lists.Count < 1) return;
            string tableName = lists[0].AreaCode + suffix;
            db= _dbFactory.GetConnection("claptrap");
            try
            {              
                  await db.ExecuteAsync(addSql.Replace("51", tableName), lists);
            }
            catch (Exception ex)
            {
                _logger.LogError("保存积分数据出错："+ex.Message);
                await createYearTable(tableName); 
                await db.ExecuteAsync(addSql.Replace("51", tableName), lists);
            }
            await db.ExecuteAsync(updateIntegralSql, lists);
            _logger.LogInformation($" 处理积分信息：{lists.Count}条");
            db.Close();
        }

        private  async Task createYearTable(string suffix)
        {          
            const string isSql = "SELECT count(1) FROM information_schema.TABLES WHERE table_name =@table_name;";
            _logger.LogInformation($"integral_record_{suffix}");
            int count = await db.QueryFirstAsync<int>(isSql, new { table_name = $"integral_record_{suffix}" });
            //不存在创建表
            if (count==0)
            {
                await db.ExecuteAsync($"create table integral_record_{suffix} like integral_record;", "");
            }
        }
    }
}
