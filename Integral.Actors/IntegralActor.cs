using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapr.Actors.Runtime;
using Integral.Models;
using Integral.Models.Events;
using Microsoft.Extensions.Logging;
using Newbe.Claptrap;
using Newbe.Claptrap.Dapr;
using Newbe.Claptrap.StorageProvider.Relational.StateStore;

namespace Integral.Actors
{
    [Actor(TypeName = ClaptrapCodes.IntegralActor)]
    [ClaptrapStateInitialFactoryHandler(typeof(IntegralActorInitialStateDataFactory))]
    [ClaptrapEventHandler(typeof(NewIntegralEventHandler), ClaptrapCodes.NewIntegralEvent)]
    public class IntegralActor : ClaptrapBoxActor<IntegralState>, IIntegralActor
    {

        public IntegralActor(
            ActorHost actorHost,
            IClaptrapActorCommonService claptrapActorCommonService
            ) : base(actorHost, claptrapActorCommonService)
        {
        }
        public Task<IntegralState> GetStateAsync()
        {
            return Task.FromResult(StateData);
        }

        public Task<int> GetTodayScoreAsync()
        {
            return Task.FromResult(StateData.TotalScore);
        }

        public async Task<bool> TryIntegral(IntegralInput input)
        {
            //用户已经达到今日最大积分
            if (GetIntegralStatus(input.UserId, input.ItemId)) return false;
            var dataEvent = this.CreateEvent(new IntegralEvent
            {
                ItemId = input.ItemId,
                UserId = input.UserId
            });
            await Claptrap.HandleEventAsync(dataEvent);
            return true;
        }
        //用户和内容是否已经获得积分
        private bool GetIntegralStatus(string userId,string itemId)
        {
            if (StateData.ToDay != DateTime.Now.DayOfYear) return false;
			if (StateData.UserIdArray.Contains(userId)) return true;
            var records = StateData.IntegralRecords;
            return (records!=null && records.ContainsKey(userId) && records[userId].Contains(itemId));
        }
        protected override Task OnDeactivateAsync()
        {
            // Provides Opporunity to perform optional cleanup.
            Console.WriteLine($"Deactivating actor id: {this.Id}");
            return Task.CompletedTask;
        }
 
    }
}
