using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EverythingParser
{
    public class ParsingConfiguration
    {
        public int IterationsCountToFindParentElement { get; set; }


        public ParsingConfiguration(int iterationsCountToFindParentElement)
        {
            IterationsCountToFindParentElement = iterationsCountToFindParentElement;
        }
    }
}
