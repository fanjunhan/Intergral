using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Integral.Models;
using Microsoft.Extensions.Logging;
using Dapr.Client;
using Integral.Actors;
using Integral.WebApi.Encryption;
using Services.WebApi;
using System.Collections.Generic;
using Services.IntergralServer.DapperPlus;
using Services.IntergralServer;

namespace Integral.WebApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class IntegralController : Controller
    {

        private readonly ILogger<IntegralController> _logger;
        private readonly IIntegralConfigStore configStore;
        private readonly IDapperPlusWrite _dapperPlusWrite;
        private readonly IDapperPlusRead _dapperPlusRead;
        private const string spendSql = "INSERT INTO integral_cost (UserId,DataId,Title,AddTime,Score,AreaCode) VALUES (@UserId,@ItemId,@Title,@AddTime,@Score,@AreaCode);";
        private const string updateSql = "UPDATE integral_area set PayCount = PayCount+@Score where Integral_Userid=@UserId and AreaCode=@AreaCode and Integral_Count>=PayCount+@Score;";
        private const string integralSql = "select ClassId,UserId,DataId as ItemId,Title,Score,AddTime from integral_record_Table1 where UserId=@UserId and ClassId=@ClassId "
            + "UNION select ClassId,UserId,DataId as ItemId,Title,Score,AddTime from integral_record_Table2 where UserId=@UserId and ClassId=@ClassId"
            + " order by AddTime desc limit @Limit offset @Offset";
        private const string mySql = "select Integral_Count as Item1,PayCount as Item2,UnionId as Item3 from integral_area where AreaCode=@AreaCode and Integral_Userid=@UserId";
        private const string InsertConfigSql = "INSERT INTO integral_config (ClassId,`Limit`,AddTime,Score,AreaCode) VALUES (@ClassId,@Limit,@AddTime,@Score,@AreaCode) ON DUPLICATE KEY UPDATE `Limit` = @Limit,Score=@Score,AddTime=@AddTime,AreaCode=@AreaCode;";
        private const string billSql = "select Integral_Count as AllCount,PayCount,UnionId,Integral_Userid as UserId,UserName from integral_area where AreaCode=@AreaCode and AreaId=@AreaId and UnionId=@UnionId order by AllCount desc,PayCount desc limit @Limit offset @Offset";
        private const string updateAreaSql = "UPDATE integral_area set AreaId = @AreaId,UserName = @UserName where Integral_Userid=@UserId";
        private const string areaSql= "SELECT sum(Integral_Count) as AllCount,sum(PayCount) as PaysCount,AreaId from integral_area GROUP BY AreaId";
        private const string classSql = "SELECT Score,AreaId,Day,ClassId  from integral_day where  Day>=@Date and Day<=@Date2 and AreaId=@AreaId and ClassId=@ClassId order by AreaId,Day,ClassId";
        private const string monthSql = "SELECT Score,AreaId,Month,ClassId  from integral_month where  Month>=@Date and Month<=@Date2 and AreaId=@AreaId and ClassId=@ClassId order by AreaId,Month,ClassId";
        private const string interSql = "SELECT ELT(INTERVAL (Integral_Count, 0,10*@Count, 20*@Count, 30*@Count, 40*@Count,50*@Count,60*@Count,70*@Count,80*@Count,90*@Count,100*@Count),CONCAT('0-',10*@Count),CONCAT(10*@Count,'-',20*@Count),CONCAT(20*@Count,'-',30*@Count),CONCAT(30*@Count,'-',40*@Count),CONCAT(40*@Count,'-',50*@Count),CONCAT(50*@Count,'-',60*@Count),CONCAT(60*@Count,'-',70*@Count),CONCAT(70*@Count,'-',80*@Count),CONCAT(80*@Count,'-',90*@Count),CONCAT(90*@Count,'-',100*@Count),CONCAT(100*@Count,'+')) AS intName,count(Integral_Count) AS intCount  FROM integral_area GROUP BY intName order by INTERVAL (Integral_Count, 0,10*@Count, 20*@Count, 30*@Count, 40*@Count,50*@Count,60*@Count,70*@Count,80*@Count,90*@Count,100*@Count)";
        private const string topSql = "SELECT sum(Integral_Count),sum(Integral_Count)-sum(PayCount),max(Integral_Count),max(Integral_Count-PayCount) from integral_area";
        public IntegralController(ILoggerFactory logFactory, IIntegralConfigStore configStore, IDapperPlusWrite dapperPlusWrite, IDapperPlusRead dapperPlusRead)
        {
            this.configStore = configStore;
            _logger = logFactory.CreateLogger<IntegralController>();

            if (_dapperPlusWrite == null) _dapperPlusWrite = dapperPlusWrite;
            if (_dapperPlusRead == null) _dapperPlusRead = dapperPlusRead;
        }

        /// <summary>
        /// 获取用户积分列表
        /// </summary>
        /// <param name="areaCode">区域编码</param>
        /// <param name="userId">用户ID</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetIntegral(string areaCode, string userId, int pageSize = 10, int pageIndex = 1,string classId="")
        {            
            if (pageIndex < 1) pageIndex = 1;
            pageIndex--;
            if (string.IsNullOrEmpty(areaCode)||string.IsNullOrEmpty(userId))return Ok(false);
            string intSql = integralSql.Replace("Table1", $"{areaCode}{DateTime.Now.Year - 2020}_" + DateTime.Now.Month);
            intSql = intSql.Replace("Table2", DateTime.Now.Month == 1 ? $"{areaCode}{DateTime.Now.Year - 2021}_12" : $"{areaCode}{DateTime.Now.Year - 2020}_" + (DateTime.Now.Month - 1));
			if(string.IsNullOrEmpty(classId))intSql = intSql.Replace("ClassId=", "ClassId!=");
            var result = await _dapperPlusRead.QueryAsync<IntegralRecord>(intSql, new { UserId = userId, ClassId = classId,Limit = pageSize, Offset = pageIndex * pageSize });

            return Ok(result);
        }
        /// <summary>
        /// 我的总积分和花费积分
        /// </summary>
        /// <param name="areaCode">区域编码</param>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        [HttpGet("My")]
        public async Task<IActionResult> GetMyIntegral(string areaCode, string userId)
        {
            var result = await _dapperPlusRead.QueryFirstAsync<ValueTuple<int, int, string>>(mySql, new { AreaCode = areaCode, UserId = userId });
            return Ok(result);
        }


        [HttpGet("day/{date}/{date2}")]
        public async Task<IActionResult> GetMydayclass(string date, string date2, string classId, string areaId = "")
        {
            string daySql;
            if (date.Length == 10)
            {
                daySql = string.IsNullOrEmpty(classId) ? classSql.Replace("=@ClassId", " is not null") : classSql;
            }
            else{
                daySql = string.IsNullOrEmpty(classId) ? monthSql.Replace("=@ClassId", " is not null") : monthSql;
            }            
            var result = await _dapperPlusRead.QueryAsync<ValueTuple<int, string, string, string>>(areaId.Length == 0 ? daySql.Replace("AreaId=", "AreaId!=") : daySql, new { Date = date, Date2 = date2, ClassId = classId, AreaId = areaId });
    
            return Ok(result);
        }

        [HttpGet("toparea")]
        public async Task<IActionResult> GetToparea()
        {
            var result = await _dapperPlusRead.QueryAsync<ValueTuple<int, int, string>>(areaSql,"");
            return Ok(result);
        }

        [HttpGet("topmax")]
        public async Task<IActionResult> GetTopmax()
        {
            var result = await _dapperPlusRead.QueryAsync<ValueTuple<int, int, int, int>>(topSql, "");
            return Ok(result);
        }

        [HttpGet("inter")]
        public async Task<IActionResult> GetInter(uint count=1, string areaId = "")
        {
            var result = await _dapperPlusRead.QueryAsync<ValueTuple<string, int>>(areaId.Length >10 ? interSql.Replace("GROUP", "where AreaId=@AreaId GROUP") : interSql, new { Count = count, AreaId = areaId });
            return Ok(result);
        }

        /// <summary>
        /// 区域排行榜
        /// </summary>
        /// <param name="areaCode">区域编码</param>
        /// <param name="unionId">工会ID</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpGet("top/{areaId?}/{unionId?}")]
        public async Task<CommonPagedResponseDto<IList<TopUser>>> GetTop(string areaCode, string areaId = "", string unionId="", int pageSize = 10, int pageIndex = 1)
        {
            if(string.IsNullOrEmpty(areaCode)) areaCode = "510500";
            string countsql = $"select count(1) from integral_area where AreaCode=@AreaCode and AreaId=@AreaId and UnionId=@UnionId";
            var mysql = billSql;
            if (string.IsNullOrEmpty(areaId))
            {
                mysql = mysql.Replace("AreaId=@AreaId", "1=1");
                countsql= countsql.Replace("AreaId=@AreaId", "1=1");
            }
            if (string.IsNullOrEmpty(unionId))
            {
                mysql = mysql.Replace("UnionId=@UnionId", "1=1");
                countsql = countsql.Replace("UnionId=@UnionId", "1=1");
            }
            if (pageIndex < 1) pageIndex = 1;
            var result = await _dapperPlusRead.QueryAsync<TopUser>(mysql,
                new { AreaCode = areaCode, AreaId = areaId, UnionId = unionId, Limit = pageSize, Offset = (pageIndex-1) * pageSize });
            
            var count = await _dapperPlusRead.QueryFirstAsync<int>(countsql, new { areaCode, areaId, unionId });
            return CommonPagedResponseDto<IList<TopUser>>.BuildOK(result, pageIndex, pageSize, count);
        }

        /// <summary>
        /// 花费积分
        /// </summary>
        /// <param name="AppParameterDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Spend([FromBody] AppParameterDto dto)
        {
            var webApiInput = EncryptionHelper.BaseAseDecrypt<IntegralInput>(ClaptrapCodes.Key, dto.Value);
            if(webApiInput==null) return Ok(false);
            var record = new IntegralRecord
            {
                ClassId = webApiInput.ClassId,
                Score = webApiInput.Score ?? 0,
                AreaCode = webApiInput.AreaCode ?? "",
                ItemId = webApiInput.ItemId,
                Title = webApiInput.Title,
                UserId = webApiInput.UserId
            };
            if (record.Score == 0|| record.AreaCode=="") return Ok(false);
            int result = await _dapperPlusWrite.ExecuteAsync(updateSql, record);
            if (result == 0)
            {
                _logger.LogInformation(webApiInput.UserId + " 积分兑换不足");
                return Ok(false);
            }
            await _dapperPlusWrite.ExecuteAsync(spendSql, webApiInput);
            _logger.LogInformation(webApiInput.UserId + " 积分兑换花费分数：" + webApiInput.Score);
            return Ok(true);
        }


        /// <summary>
        /// 设置用户区域工会
        /// </summary>
        /// <param name="AppParameterDto"></param>
        /// <returns></returns>
        [HttpPost("setarea")]
        public async Task<IActionResult> SetUserArea([FromBody] AppParameterDto dto)
        {
            var areaInput = EncryptionHelper.BaseAseDecrypt<AreaInput>(ClaptrapCodes.Key, dto.Value);
            if (areaInput == null) return Ok(false);
            int result = await _dapperPlusWrite.ExecuteAsync(updateAreaSql, areaInput);
            return Ok(result>0);
        }


        [HttpPost("Saveconfig")]
        public async Task<IActionResult> SaveConfig([FromBody] AppParameterDto dto)
        {
            var config = EncryptionHelper.BaseAseDecrypt<IntegralConfig>(ClaptrapCodes.Key, dto.Value);
            if (config == null) return Ok(false);
            int result = 0;
            try
            {
                result = await _dapperPlusWrite.ExecuteAsync(InsertConfigSql, config);
                if (result > 0)
                {
                    configStore.Save(config);
                    _logger.LogInformation($"新增市州{config.AreaCode}积分{config.ClassId}配置成功！");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"保存{config.AreaCode}积分配置出错：" + ex.Message);
            }

            return Ok(result);
        }
    }

    
}
