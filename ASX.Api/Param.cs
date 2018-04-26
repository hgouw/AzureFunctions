using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASX.Api
{
    public class Param
    {
        public string Company { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }
    }
}