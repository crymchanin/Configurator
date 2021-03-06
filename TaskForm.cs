using Feodosiya.Lib.IO;
using POFileManagerService.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;


namespace Configurator {
    public partial class TaskForm : Form {

        private Task _task = new Task();

        private T GetValue<T>(T unsafeVal) {
            T def = (typeof(string) == typeof(T))
                ? (T)Activator.CreateInstance(typeof(T), new char[] { })
                : (T)Activator.CreateInstance(typeof(T));
            return (unsafeVal == null) ? def : unsafeVal;
        }

        public Task Task {
            get {
                return _task;
            }
            set {
                _task.AllowDuplicate = value.AllowDuplicate;
                _task.DayInterval = value.DayInterval;
                _task.ExternalLib = new string(GetValue(value.ExternalLib).ToCharArray());
                _task.ExternalLibParams = GetValue(value.ExternalLibParams).ToDictionary(k => k.Key, v => v.Value);
                _task.MoveFile = value.MoveFile;
                _task.Name = new string(GetValue(value.Name).ToCharArray());
                _task.Recursive = value.Recursive;
                _task.Regex = new string(GetValue(value.Regex).ToCharArray());
                _task.Source = new string(GetValue(value.Source).ToCharArray());
            }
        }

        public TaskForm() {
            InitializeComponent();
        }

        private void UpdateExternalLibParams() {
            ExternalLibParamsView.Items.Clear();

            ListViewItem[] items = new ListViewItem[_task.ExternalLibParams.Count];
            int index = 0;
            foreach (KeyValuePair<string, object> par in _task.ExternalLibParams) {
                ListViewItem item = new ListViewItem(par.Key);
                item.SubItems.Add(par.Value.ToString());
                item.Tag = par;

                items[index] = item;
                index++;
            }

            ExternalLibParamsView.Items.AddRange(items);
        }

        public TaskForm(Task task) : this() {
            Task = task;

            NameBox.Text = _task.Name;
            PathBox.Text = _task.Source;
            RegexBox.Text = _task.Regex;
            IntervalBox.Value = _task.DayInterval;
            AllowDuplicateBox.Checked = _task.AllowDuplicate;
            MoveFileBox.Checked = _task.MoveFile;
            RecursiveBox.Checked = _task.Recursive;
            bool flag = (!string.IsNullOrEmpty(_task.ExternalLib));
            UseExternalLibBox.Checked = flag;
            if (flag) {
                ExternalLibBox.Text = _task.ExternalLib;
                UpdateExternalLibParams();
            }
            NameBox.SelectionStart = NameBox.Text.Length;
            NameBox.SelectionLength = 0;
        }

        private void AddExternalLibParamsButton_Click(object sender, EventArgs e) {
            try {
                ExternalLibParamsForm form = new ExternalLibParamsForm();
                if (form.ShowDialog() == DialogResult.OK) {
                    _task.ExternalLibParams = new Dictionary<string, object>();
                    _task.ExternalLibParams.Add(form.Param.Key, form.Param.Value);
                    UpdateExternalLibParams();
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "??????", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditExternalLibParamsButton_Click(object sender, EventArgs e) {
            try {
                if (ExternalLibParamsView.SelectedItems.Count < 1) {
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }
                KeyValuePair<string, object> itemData = (KeyValuePair<string, object>)ExternalLibParamsView.SelectedItems[0].Tag;

                ExternalLibParamsForm form = new ExternalLibParamsForm(itemData);
                if (form.ShowDialog() == DialogResult.OK) {
                    _task.ExternalLibParams.Remove(itemData.Key);
                    _task.ExternalLibParams.Add(form.Param.Key, form.Param.Value);
                    UpdateExternalLibParams();
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "??????", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveExternalLibParamsButton_Click(object sender, EventArgs e) {
            try {
                if (ExternalLibParamsView.SelectedItems.Count < 1) {
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }

                KeyValuePair<string, object> itemData = (KeyValuePair<string, object>)ExternalLibParamsView.SelectedItems[0].Tag;
                if (MessageBox.Show($"??????? ???????? '{itemData.Key}'?", "????????", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    _task.ExternalLibParams.Remove(itemData.Key);
                    UpdateExternalLibParams();
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "??????", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UseExternalLibBox_CheckedChanged(object sender, EventArgs e) {
            bool flag = UseExternalLibBox.Checked;

            ExternalLibBox.Enabled = flag;
            ExternalLibParamsView.Enabled = flag;
            AddExternalLibParamsButton.Enabled = flag;
            EditExternalLibParamsButton.Enabled = flag;
            RemoveExternalLibParamsButton.Enabled = flag;
        }

        private void OkButton_Click(object sender, EventArgs e) {
            try {
                IOHelper.IsPathDirectory(PathBox.Text);
            }
            catch {
                MessageBox.Show("????????? ???? ?? ??????????", "??????", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _task.Name = NameBox.Text;
            _task.Source = PathBox.Text;
            _task.Regex = RegexBox.Text;
            _task.DayInterval = (int)IntervalBox.Value;
            _task.AllowDuplicate = AllowDuplicateBox.Checked;
            _task.MoveFile = MoveFileBox.Checked;
            _task.Recursive = RecursiveBox.Checked;
            if (UseExternalLibBox.Checked) {
                _task.ExternalLib = ExternalLibBox.Text;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BrowseButton_Click(object sender, EventArgs e) {
            BrowseMenuStrip.Show(BrowseButton, new Point(0, BrowseButton.Height),
                ToolStripDropDownDirection.BelowRight);
        }

        private void FolderMenuItem_Click(object sender, EventArgs e) {
            if (MainFolderDialog.ShowDialog() == DialogResult.OK) {
                PathBox.Text = MainFolderDialog.SelectedPath;
            }
        }

        private void FileMenuItem_Click(object sender, EventArgs e) {
            if (MainFileDialog.ShowDialog() == DialogResult.OK) {
                PathBox.Text = MainFileDialog.FileName;
            }
        }

        private bool IsInvalidFileNameChar(char c) {
            return (c < 64U) ? (1UL << c & 0xD4008404FFFFFFFFUL) != 0 : c == '\\' || c == '|';
        }

        private void NameBox_TextChanged(object sender, EventArgs e) {
            string text = NameBox.Text;
            NameBox.Text = new string(text.Where(c => !IsInvalidFileNameChar(c) && c != ' ').ToArray());
            if (text != NameBox.Text) {
                NameBox.SelectionStart = NameBox.Text.Length;
                NameBox.SelectionLength = 0;
                System.Media.SystemSounds.Beep.Play();
            }
        }
    }
}