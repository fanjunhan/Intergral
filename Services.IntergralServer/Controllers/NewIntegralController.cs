using Dapr.Actors.Client;
using Dapr.Client;
using Integral.Actors;
using Integral.Models;
using Integral.WebApi.Encryption;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newbe.Claptrap;
using Newbe.Claptrap.Dapr;
using Services.WebApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Integral.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewIntegralController : ControllerBase
    {
        private readonly IActorProxyFactory _actorProxyFactory;
        private readonly ILogger<NewIntegralController> _logger;
        public NewIntegralController(ILoggerFactory logFactory, IActorProxyFactory actorProxyFactory)
        {
            _actorProxyFactory = actorProxyFactory;
            _logger = logFactory.CreateLogger<NewIntegralController>();
        }

        [HttpGet("{classId}")]
        public async Task<IActionResult> GetState(string classId)
        {
            var id = new ClaptrapIdentity(classId, ClaptrapCodes.IntegralActor);
            var integralActor = _actorProxyFactory.GetClaptrap<IIntegralActor>(id);
            var state = await integralActor.GetStateAsync();
            return Ok(new { state = state });
        }

        [HttpPost]
        public async Task<IActionResult> TryIntegral([FromBody] AppParameterDto dto, [FromServices] DaprClient daprClient)
        {
            var webApiInput = EncryptionHelper.BaseAseDecrypt<IntegralInput>(ClaptrapCodes.Key, dto.Value);
            var record = new IntegralRecord
            {
                ClassId = webApiInput.ClassId,
                ItemId = webApiInput.ItemId,
                Title = webApiInput.Title,
                UnionId = webApiInput.UnionId,
                UserName = webApiInput.UserName,
                UserId = webApiInput.UserId
            };
            var id = new ClaptrapIdentity(webApiInput.ClassId,
                ClaptrapCodes.IntegralActor);
            var integralActor = _actorProxyFactory.GetClaptrap<IIntegralActor>(id);
            var result = await integralActor.TryIntegral(webApiInput);
            if (result)
            {
                if(webApiInput.Score!=null&& webApiInput.Score.Value>1) record.Score = webApiInput.Score.Value;
                await daprClient.PublishEventAsync(ClaptrapCodes.PubSub,
                    ClaptrapCodes.IntegralCompleted, record);
            }

            return Ok(result);
        }

        [HttpGet("test/{classId}/{count?}")]
        public async Task<IActionResult> TestIntegral(string classId, [FromServices] DaprClient daprClient,int count= 1000)
        {            
            TestInputData(classId, count);
            var startTime = DateTime.Now;
            var id = new ClaptrapIdentity(classId, ClaptrapCodes.IntegralActor);
            var integralActor = _actorProxyFactory.GetClaptrap<IIntegralActor>(id);
            foreach (var list in dataList.GetConsumingEnumerable())
            {
                var integralResult = await integralActor.TryIntegral(list);
                if (integralResult)
                {
                    await daprClient.PublishEventAsync<IntegralRecord>(ClaptrapCodes.PubSub,
                        ClaptrapCodes.IntegralCompleted, new IntegralRecord
                        {
                            ClassId = list.ClassId,
                            ItemId = list.ItemId,
                            Title = list.Title,
                            UnionId = list.UnionId,
                            UserName = list.UserName,
                            UserId = list.UserId
                        });
                }
            };
            var state = await integralActor.GetStateAsync();
            var result = new
            {
                excuteTime = DateTime.Now.Subtract(startTime).TotalMilliseconds,
                score = state.TotalScore
            };

            return Ok(result);
        }
        BlockingCollection<IntegralInput> dataList = new BlockingCollection<IntegralInput>();
        string classid = "";
        private void TestInputData(string classId, int count)
        {
            classid = classId;
            _logger.LogInformation("开始：********");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                Parallel.For(0, count, AddTestInput);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            stopwatch.Stop();
            Task.Delay(count);
            dataList.CompleteAdding();
            _logger.LogInformation("结束：***{0}毫秒***", stopwatch.ElapsedMilliseconds);
        }

        private void AddTestInput(int i)
        {
            var userid = Guid.NewGuid().ToString();
            dataList.Add(new IntegralInput
            {
                ClassId = classid,
                ItemId = "fedde3b2e7c04753973cfa986dfd9cca",
                Title = "三台县召开群团工作暨党风廉政建设汇报会",
                UserName="测试者",
                UserId = userid
            });
            dataList.Add(new IntegralInput
            {
                ClassId = classid,
                ItemId = "a6ea00cbb7ea4f188f04fa42a9999a9c",
                Title = "什邡市总工会召开心灵驿站建设工作协调会",
                UserName = "测试者",
                UserId = userid
            });
            dataList.Add(new IntegralInput
            {
                ClassId = classid,
                ItemId = "8cb56ec9296945a3a91ec7aa32039acf",
                Title = "自贡市总工会将开展系列阅读主题活动庆祝建党100周年",
                UserName = "测试者",
                UserId = userid
            });
        }
    }
}
