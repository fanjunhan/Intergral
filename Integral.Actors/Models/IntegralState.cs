using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newbe.Claptrap;

namespace Integral.Models
{
    
    public record IntegralState : IStateData
    {
        public Dictionary<string, List<string>> IntegralRecords { get; set; }
        public int ToDay { get; set; }        
        public string ClassId { get; set; }
        public String[] UserIdArray { get; set; } = new string[1]; //已经获得最大积分的用户集合
        public IntegralConfig IntegralConfig { get; set; }
        public int UserIndex { get; set; } = 0;
        
        public int TotalScore
        {
            get { return IntegralConfig == null?0:UserIndex * IntegralConfig.Limit * IntegralConfig.Score
                    + (IntegralRecords == null?0:IntegralRecords.Values.Count * IntegralConfig.Score); }
        }
 
        public void InitIntegralRecords(IntegralConfig config)
        {
            IntegralConfig = config;
            UserIdArray = new String[1000];
            IntegralRecords = new Dictionary<string, List<string>>();
            if (ToDay != DateTime.Now.DayOfYear)
            {
                ToDay = DateTime.Now.DayOfYear;
                UserIndex = 0;
            }
        }
    }

 
    public record IntegralRecord
    {
        [Required]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ClassId { get; set; }
        [Required]
        public string ItemId { get; set; }
        public string Title { get; set; }
        [Range(1, 100)]
        public int Score { get; set; } = 1;
        public string AreaCode { get; set; }
        public DateTime AddTime { get; set; } = DateTime.Now;
        public string UnionId { get; set; }
    }

}
