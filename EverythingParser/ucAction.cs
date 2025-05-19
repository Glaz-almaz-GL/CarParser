using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EverythingParser
{
    public partial class ucAction : UserControl
    {
        public ucAction(string attribute)
        {
            InitializeComponent();
            lbAttribute.Text = attribute;
        }
    }
}
