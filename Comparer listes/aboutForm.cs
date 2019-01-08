/*
 * aboutForm.cs
 * Copyright (c) 2018 Gilbert Fernandes
 * 
 */

using System;
using System.Windows.Forms;

namespace Comparer_listes {
    public partial class aboutForm : Form {
        public aboutForm() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            Close();
        }
    }
}
