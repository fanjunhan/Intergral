using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newbe.Claptrap;

namespace Integral.Models.Events
{
    public record IntegralEvent : IEventData
    {
        public string UserId { get; set; }
        public string ItemId { get; set; }
    }
}
