using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hops
{
    public partial class ThumbnailViewer : Form
    {
        public ThumbnailViewer()
        {
            InitializeComponent();
        }
        protected override bool CanRaiseEvents
        {
            get
            {
                return false;
            }
        }
        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }
    }
}
