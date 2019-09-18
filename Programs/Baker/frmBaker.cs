using OpenMetaverse.Imaging;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Baker
{
    public partial class frmBaker : Form
    {
        Bitmap AlphaMask;

        public frmBaker()
        {
            InitializeComponent();
        }

        private void frmBaker_Load(object sender, EventArgs e)
        {
            cboMask.SelectedIndex = 0;
            DisplayResource(cboMask.Text);
        }

        private void DisplayResource(string resource)
        {
            Stream stream = OpenMetaverse.Helpers.GetResourceStream(resource + ".tga");

            if (stream != null)
            {
                AlphaMask = LoadTGAClass.LoadTGA(stream);
                stream.Close();

                //ManagedImage managedImage = new ManagedImage(AlphaMask);

                // FIXME: Operate on ManagedImage instead of Bitmap
                pic1.Image = Oven.ModifyAlphaMask(AlphaMask, (byte)scrollWeight.Value, 0.0f);
            }
            else
            {
                MessageBox.Show("Failed to load embedded resource \"" + resource + "\"", "Baker",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void scrollWeight_Scroll(object sender, ScrollEventArgs e)
        {
            pic1.Image = Oven.ModifyAlphaMask(AlphaMask, (byte)scrollWeight.Value, 0.0f);
        }

        private void frmBaker_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void cmdLoadSkin_Click(object sender, EventArgs e)
        {

        }

        private void cboMask_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayResource(cboMask.Text);
        }
    }
}
