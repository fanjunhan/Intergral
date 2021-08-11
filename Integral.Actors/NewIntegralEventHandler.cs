using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Integral.Models;
using Integral.Models.Events;
using Microsoft.Extensions.Logging;
using Newbe.Claptrap;

namespace Integral.Actors
{
    public class NewIntegralEventHandler
        : NormalEventHandler<IntegralState, IntegralEvent>
    {
        private readonly IIntegralConfigStore configStore;

        public NewIntegralEventHandler(IIntegralConfigStore configStore)
        {        
            this.configStore = configStore;
        }
        public override ValueTask HandleEvent(IntegralState stateData, IntegralEvent eventData, IEventContext eventContext)
        {            
            if (stateData.IntegralRecords == null || stateData.ToDay!= DateTime.Now.DayOfYear)
            {
                stateData.InitIntegralRecords(configStore.Get(stateData.ClassId));
            }
            if (stateData.IntegralConfig == null) 
                stateData.IntegralConfig = configStore.Get(stateData.ClassId);
            var records = stateData.IntegralRecords;
            string userId = eventData.UserId;
            if (stateData.IntegralConfig.Limit ==1)
            {
                var idArray = stateData.UserIdArray;
                if (idArray.Length < stateData.UserIndex + 10)
                    Array.Resize<string>(ref idArray, idArray.Length + 100);
                idArray[stateData.UserIndex++] = userId;
                stateData.UserIdArray = idArray;
            }
            else if (records.ContainsKey(userId))
            {
                if(records[userId].Count < stateData.IntegralConfig.Limit - 1)
                {
                    records[userId].Add(eventData.ItemId);
                }
                else
                {
                    var idArray = stateData.UserIdArray;
                    if (idArray.Length < stateData.UserIndex + 10)
                        Array.Resize<string>(ref idArray, idArray.Length + 100);
                    idArray[stateData.UserIndex++] = userId;
                    stateData.UserIdArray = idArray;
                    records.Remove(userId);
                }
            }
            else
            {
                records.Add(userId, new List<string> { eventData.ItemId });
            }
            
            stateData.IntegralRecords = records;
            
            return ValueTask.CompletedTask;
        }
    }
}
