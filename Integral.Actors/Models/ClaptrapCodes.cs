using System.Collections;

namespace Integral.Models
{
    public static class ClaptrapCodes
    {   
        public const string IntegralActor = "integral_claptrap_newbe";
        public const string NewIntegralEvent = "newIntegral" + IntegralActor;
        public const string Suffix = "_";
        public const string PubSub = "pubsub";
        public const string Key = "Sczgh";
        public const string IntegralCompleted = "OnIntegral_Completed";
        public const string StoreName = "statestore";
        public const string InitSql = "select * from integral_config where AreaCode like @AreaCode";
        public const string ClassIdSql = "select * from integral_config where ClassId = @ClassId";

    }
}