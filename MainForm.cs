using Feodosiya.Lib.Conf;
using POFileManagerService.Configuration;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;


namespace Configurator {
    public partial class MainForm : Form {

        private void LoadConfiguration(string confPath) {
            try {
                if (!File.Exists(confPath)) {
                    throw new Exception("Выбранный файл конфигурации приложения не существует");
                }
                InfoStatusLabel.Text = confPath;

                AppHelper.ConfHelper = new ConfHelper(confPath);
                AppHelper.Configuration = AppHelper.ConfHelper.LoadConfig<Global>();
                if (!AppHelper.ConfHelper.Success) {
                    throw new Exception(AppHelper.ConfHelper.LastError.ToString());
                }

                GlobalPage_LoadConf();
                DBPage_LoadConf();
                ExternalFtp_LoadConf();
                ExchangePage_LoadConf();
                TasksPage_LoadConf();

                AppHelper.IsSaved = false;
            }
            catch (Exception ex) {
                MessageBox.Show("Ошибка при загрузке конфигурации:\r\n" + ex.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public MainForm() {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1) {
                LoadConfiguration(args[1]);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e) {
            try {
                if (AppHelper.ConfHelper == null) {
                    if (MessageBox.Show("Файл конфигурации не открыт. Создать новый?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                        string newFile = Path.Combine(Feodosiya.Lib.IO.IOHelper.GetCurrentDir(System.Reflection.Assembly.GetExecutingAssembly()),
                            "POFileManagerService.conf");
                        if (File.Exists(newFile)) {
                            if (MessageBox.Show(string.Format("Файл '{0}' уже существует. Заменить?", newFile),
                                "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) {
                                return;
                            }
                        }
                        AppHelper.ConfHelper = new ConfHelper(newFile);
                        AppHelper.ConfHelper.SaveConfig(new Global(), Encoding.UTF8, true);
                        if (!AppHelper.ConfHelper.Success) {
                            throw new Exception(AppHelper.ConfHelper.LastError.ToString());
                        }
                        AppHelper.Configuration = AppHelper.ConfHelper.LoadConfig<Global>();
                    }
                    else {
                        System.Media.SystemSounds.Beep.Play();
                        return;
                    }
                }
                else {
                    if (!AppHelper.ConfHelper.Success) {
                        throw new Exception(AppHelper.ConfHelper.LastError.ToString());
                    }
                }

                if (MessageBox.Show("Сохранить изменения?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    GlobalPage_UpdateConf();
                    DBPage_UpdateConf();
                    ExternalFtp_UpdateConf();
                    ExchangePage_UpdateConf();
                    AppHelper.ConfHelper.SaveConfig(AppHelper.Configuration, Encoding.UTF8, true);
                    if (!AppHelper.ConfHelper.Success) {
                        MessageBox.Show(AppHelper.ConfHelper.LastError.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else {
                        AppHelper.IsSaved = true;
                        MessageBox.Show("Файл конфигурации сохранен!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenButton_Click(object sender, EventArgs e) {
            if (MainOpenFileDialog.ShowDialog() == DialogResult.OK) {
                LoadConfiguration(MainOpenFileDialog.FileName);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (AppHelper.IsSaved) {
                return;
            }
            if (MessageBox.Show("Выйти без сохранения?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) {
                e.Cancel = true;
            }
        }

        private void DebuggingBox_CheckedChanged(object sender, EventArgs e) {
            DebuggingLevelLabel.Visible = DebuggingBox.Checked;
            DebuggingLevelBox.Visible = DebuggingBox.Checked;
        }
    }
}
