using DocumentFormat.OpenXml.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EverythingParser
{
    public class ParsingElementConfiguration
    {
        public string ElementXPath { get; private set; }
        public List<AttributeActions> AttributeActions { get; private set; }

        public ParsingElementConfiguration(string elementXPath, List<AttributeActions> attributeActions)
        {
            ElementXPath = elementXPath;
            AttributeActions = attributeActions;
        }

        public void StartParsing()
        {
            foreach (var attributeAction in AttributeActions)
            {
                attributeAction.StartAction();
            }
        }
    }
}
