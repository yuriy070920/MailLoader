﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using Core.SpecialClasses;
using NpgsqlTypes;
using Order1.Forms;

namespace Order1
{
    public partial class Import : Form
    {
        /// <summary>
        /// Variables that resposible for input fields of user
        /// valid - valid state. Get only 'valid'/'invalid'
        /// pathFile - path to file, that contains e-mails
        /// selectedNews - news name, that user want to connect to e-mails
        /// selectedDate - date, that will be assigned to e-mail
        /// </summary>
        private bool valid;
        private string pathFile;
        private string selectedNews;
        private string selectedDate;
        private DateTime SelectedDate;


        private bool isFileSelected = false;
        private bool isNewsSelected = false;
        private bool isDateSelected = false;

        private string connectionString = Program.connectionString;

        NpgsqlConnection conn;


        public Import()
        {
            InitializeComponent();
        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void СправкаToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void RadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            valid = true;
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox1.Enabled = false;
            checkBox2.Enabled = false;
            comboBox1.Enabled = true;
            dateTimePicker1.Enabled = true;
            ActivateCheckBox1();
            ActivateCheckBox2();
        }

        private void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            valid = false;
            checkBox1.Enabled = true;
            checkBox2.Enabled = true;
        }

        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            isDateSelected = true;
            SelectedDate = dateTimePicker1.Value;
        }

        private void DateTimePicker1_OnCloseUp(object sender, EventArgs e)
        {
            isDateSelected = true;
            SelectedDate = dateTimePicker1.Value;
        }


        private void Import_Load(object sender, EventArgs e)
        {
            conn = new NpgsqlConnection(connectionString);
            try
            {
                conn.Open();
            }
            catch(TimeoutException)
            {
                MessageBox.Show("Время ожидания подключения к серверу истекло\n" +
                                    "Проверьте соединение с интернетом и повторите запуск приложения!",
                                    "Время ожидания истекло",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                return;
            }
            var reader = new NpgsqlCommand("SELECT news_name FROM news", conn).ExecuteReader();
            int counter = 0;
            while(reader.Read())
            {
                comboBox1.Items.Add(reader[counter].ToString());
            }
            conn.Close();
            isNewsSelected = false;
            radioButton1.Checked = true;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            ActivateFileDialog();
        }

        private void ВыйтиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ActivateFileDialog()
        {
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.Title = "Выберите файл для импорта";
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
            openFileDialog1.DefaultExt = "txt";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePathText.Text = openFileDialog1.FileName;
                pathFile = openFileDialog1.FileName;
                isFileSelected = true;
            }
        }

        private void ВыбратьФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivateFileDialog();
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text != null || comboBox1.Text != "")
            {
                isNewsSelected = true;
                selectedNews = comboBox1.Text;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (valid && isFileSelected && isNewsSelected && isDateSelected ||
                (!valid && isFileSelected && isNewsSelected && isDateSelected))
            {
                LoadingProcessForm processForm = new LoadingProcessForm();
                processForm.Show();
                Task.Factory.StartNew(() =>
                {
                    List<string> mails = FileWorker.ReadFromFile(pathFile);
                    SqlCommander commander = new SqlCommander(processForm);
                    int amountCoincidences = commander.AddMails(new NpgsqlConnection(connectionString), mails, valid, SelectedDate, selectedNews);
                    conn.Close();
                    commander.DeleteAll();
                    MessageBox.Show("Импортировано " + mails.Count + " строк\n\n" + amountCoincidences.ToString() + " совпадений найдено.");
                });
            }

            else if (!valid && isFileSelected && (!isNewsSelected && !isDateSelected))
            {
                LoadingProcessForm processForm = new LoadingProcessForm();
                processForm.Show();
                Task.Factory.StartNew(() =>
                {
                    NpgsqlConnection conn = new NpgsqlConnection(connectionString);
                    List<string> mails = FileWorker.ReadFromFile(pathFile);
                    SqlCommander commander = new SqlCommander(processForm);
                    int amountCoincidences = commander.AddMails(conn, mails, valid);
                    conn.Close();
                    commander.DeleteAll();
                    MessageBox.Show("Импортировано " + mails.Count + " строк\n\n" + amountCoincidences.ToString() + " совпадений найдено.");
                });
            }
            else
            {
                MessageBox.Show("НЕ ВСЕ ПОЛЯ ЗАПОЛНЕНЫ!\nПОЖАЛУЙСТА, ПРОВЕРЬТЕ ПРАВИЛЬНОСТЬ ВВЕДЕННЫХ ДАННЫХ",
                                "Ошибка при вводе данных",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            ActivateCheckBox1();
        }

        private void ActivateCheckBox1()
        {
            if(!valid)
            {
                if (checkBox1.Checked)
                {
                    isDateSelected = false;
                    dateTimePicker1.Enabled = false;
                    if (!checkBox2.Checked)
                    {
                        checkBox2.Checked = true;
                        ActivateCheckBox2();
                    }
                }
                else
                {
                    dateTimePicker1.Enabled = true;
                    if (checkBox2.Checked)
                    {
                        checkBox2.Checked = false;
                        ActivateCheckBox2();
                    }
                }
            }
            else
            {
                checkBox1.Checked = false;
            }
        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            ActivateCheckBox2();
        }

        private void ActivateCheckBox2()
        {
            if(!valid)
            {
                if (checkBox2.Checked)
                {
                    comboBox1.Enabled = false;
                    isNewsSelected = false;
                    if (!checkBox1.Checked)
                    {
                        checkBox1.Checked = true;
                        ActivateCheckBox1();
                    }
                }
                else
                {
                    comboBox1.Enabled = true;
                    if (checkBox1.Checked)
                    {
                        checkBox1.Checked = false;
                        ActivateCheckBox1();
                    }
                }
            }
            else
            {
                checkBox2.Checked = false;
            }
        }

        private void ВыйтиToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void СвязьСРазработчикомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Communication().Show();
        }
    }
}
