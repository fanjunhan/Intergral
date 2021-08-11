using System;
using System.Threading.Tasks;
using Integral.Models;
using Integral.Models.Events;
using Newbe.Claptrap;
using Newbe.Claptrap.Dapr.Core;

namespace Integral.Actors
{
    [ClaptrapState(typeof(IntegralState), ClaptrapCodes.IntegralActor)]
    [ClaptrapEvent(typeof(IntegralEvent), ClaptrapCodes.NewIntegralEvent)]
    public interface IIntegralActor : IClaptrapActor
    {
        Task<int> GetTodayScoreAsync();
        Task<bool> TryIntegral(IntegralInput input);
        Task<IntegralState> GetStateAsync();
    }

    public record IntegralInput
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ClassId { get; set; }
        public string ItemId { get; set; }
        public string Title { get; set; }
        public int? Score { get; set; }
        public string? AreaCode { get; set; }
        public string UnionId { get; set; }
    }

    public record AreaInput
    {
        public string UserId { get; set; }
        public string AreaId { get; set; }
        public string UserName { get; set; }
    }

}
