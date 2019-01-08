/*
 * mainForm.cs
 * Copyright (c) 2018 Gilbert Fernandes
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Comparer_listes {

    public partial class mainForm : Form {

        public BackgroundWorker worker = null;

        public mainForm() {
            InitializeComponent();
            Application.ThreadException                += new ThreadExceptionEventHandler(exceptionThreadHook);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(unhandledExceptionHook);
        }

        static void exceptionThreadHook(object sender, ThreadExceptionEventArgs e) {
            // Not handled for now
        }

        static void unhandledExceptionHook(object sender, UnhandledExceptionEventArgs e) {
            // Not handled for now
        }

        private void purgeButton_Click(object sender, EventArgs e) {
            textBox1.Clear();
            textBox2.Clear();
            textBox1.Focus();
            toolStripProgressBar1.Value = 0;
            toolStripStatusLabel1.Text = "";
        }

        private void compareButton_Click(object sender, EventArgs e) {
            
            if(textBox1.TextLength == 0 || textBox2.TextLength == 0) {
                toolStripStatusLabel1.Text = "Liste A et/ou B vide !";
                return;
            }

            toolStripStatusLabel1.Text = string.Empty;

            worker = new BackgroundWorker() {
                WorkerReportsProgress      = true,
                WorkerSupportsCancellation = true
            };

            worker.DoWork             += new DoWorkEventHandler(workerDoWork);
            worker.ProgressChanged    += new ProgressChangedEventHandler(workerProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerCompleted);

            toolStripProgressBar1.Value = 0;
            compareButton.Enabled = false;
            cancelButton.Enabled  = true;

            worker.RunWorkerAsync();
        }

        private void workerDoWork(object sender, DoWorkEventArgs e) {
            Cursor.Current = Cursors.WaitCursor;
            BackgroundWorker worker = sender as BackgroundWorker;

            string[] itemsFromA = splitList(textBox1.Text);
            string[] itemsFromB = splitList(textBox2.Text);

            long doneCount  = 0;
            long totalCount = itemsFromA.LongLength + itemsFromB.LongLength;

            List<string> onlyInA     = new List<string>();
            List<string> onlyInB     = new List<string>();
            List<string> inBothLists = new List<string>();

            parseList(itemsFromA, itemsFromB, ref inBothLists, ref onlyInA, ref e, ref doneCount, totalCount);
            parseList(itemsFromB, itemsFromA, ref inBothLists, ref onlyInB, ref e, ref doneCount, totalCount);

            toolStripStatusLabel1.Text = onlyInA.Count     + " uniquement dans A, "
                                       + onlyInB.Count     + " uniquement dans B et "
                                       + inBothLists.Count + " communs";

            writeResultToTempFile(formatAllLists(onlyInA, onlyInB, inBothLists));
        }

        private void writeResultToTempFile(string fileContents) {
            string fileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";
            StreamWriter file = new StreamWriter(fileName);
            file.WriteLine(fileContents);
            file.Close();
            System.Diagnostics.Process.Start(fileName);
        }

        private string formatAllLists(List<string> onlyInA, List<string> onlyInB, List<string> inBothLists) {
            StringBuilder sb = new StringBuilder();
            formatList(ref sb, onlyInA, "Uniquement dans liste A");
            formatList(ref sb, onlyInB, "Uniquement dans liste B");
            formatList(ref sb, inBothLists, "Dans liste A et B");
            return sb.ToString();
        }

        private string[] splitList(string listContent) {
            return listContent.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        void workerProgressChanged(object sender, ProgressChangedEventArgs e) {
            toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        private void workerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            Cursor.Current = Cursors.Default;
            toolStripProgressBar1.Value = 0;
            compareButton.Enabled = true;
            cancelButton.Enabled = false;

            if (e.Cancelled) {
                toolStripStatusLabel1.Text = "Opération annulée";
            }

            worker = null;
        }

        private void parseList(string[] sourceList, string[] targetList, ref List<string> bothList, ref List<string> singleList, ref DoWorkEventArgs e, ref long doneCount, long totalCount) {

            foreach(string s in sourceList) {
                if(worker.CancellationPending) {
                    e.Cancel = true;
                    break;
                }
                else {
                    ////worker.ReportProgress((int)((double)++doneCount / (double)totalCount * 100.0));
                    if (sourceList.Contains(s) && targetList.Contains(s) && !bothList.Contains(s)) {
                        bothList.Add(s);
                    }
                    else if(!targetList.Contains(s)) {
                        singleList.Add(s);
                    }
                }
            }
        }

        private void formatList(ref StringBuilder sb, List<string> list, string comment) {
            sb.AppendLine("----------------------------------------------------------------------");
            sb.AppendLine();
            sb.AppendLine(comment);
            sb.AppendLine();
            if(list.Count > 0) {
                foreach (string s in list) {
                    sb.AppendLine(s);
                }
            }
            else {
                sb.AppendLine("Aucun");
            }
            sb.AppendLine();
        }

        private void aboutButton_Click(object sender, EventArgs e) {
            aboutForm f = new aboutForm();
            f.ShowDialog();
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            if(worker.IsBusy && !worker.CancellationPending) {
                worker.CancelAsync();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            int howMany = textBox1.Text.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Count();
            groupBox1.Text = howMany == 0 ? "Liste A" : "Liste A (" + howMany + " éléments)";
        }

        private void textBox2_TextChanged(object sender, EventArgs e) {
            int howMany = textBox2.Text.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Count();
            groupBox2.Text = howMany == 0 ? "Liste B" : "Liste B (" + howMany + " éléments)";
        }
    }

}
