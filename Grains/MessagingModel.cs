using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grains
{
    public class MessagingModel
    {
        public Guid Id { get; set; }
        public string MessageContent { get; set; } = null!;
    }
}
