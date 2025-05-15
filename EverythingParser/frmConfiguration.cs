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
    public partial class frmConfiguration : Form
    {
        ParsingConfiguration ParsingConfiguration;

        public frmConfiguration(ParsingConfiguration parsingConfiguration)
        {
            InitializeComponent();
            ParsingConfiguration = parsingConfiguration;
        }

        private void txtCountIterations_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int Count = Convert.ToInt32(txtCountIterations.Text);
                ParsingConfiguration.IterationsCountToFindParentElement = Count;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
