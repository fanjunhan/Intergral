using Integral.Models;
using Newbe.Claptrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integral.Actors
{
    public class IntegralActorInitialStateDataFactory : IInitialStateDataFactory
    {
  //      public static IntegralState[] States { get; set; }

        static IntegralActorInitialStateDataFactory()
        {
        /*    IntegralState.ClassScore.Add("lznews1", 1);
            IntegralState.ClassLimit.Add("lznews1", 5);
            IntegralState.ClassScore.Add("lzwxy", 5);
            IntegralState.ClassLimit.Add("lzwxy", 2);
            IntegralState.ClassScore.Add("lsnews1", 2);
            IntegralState.ClassLimit.Add("lsnews1", 3);*/
        }
        public Task<IStateData> Create(IClaptrapIdentity identity)
        {
            var state = new IntegralState() with { ClassId = identity.Id, ToDay = DateTime.Now.DayOfYear-1 };
            return Task.FromResult((IStateData)state);
        }
    }
}
