using System;
using System.Windows.Forms;

namespace drilling
{
    public partial class LengthInputForm : Form
    {
        public decimal TargetLength { get; private set; }

        public LengthInputForm()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtLength.Text, out decimal length) && length > 0)
            {
                TargetLength = length;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("لطفاً یک عدد معتبر برای متراژ وارد کنید.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}