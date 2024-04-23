using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQLAG
{
    public partial class Form1 : Form
    {
        private SqlConnection connection;
        private string selectedValue;
        public Form1()
        {
            InitializeComponent();
            SaveBtn.Enabled = false;
            DeleteBtn.Enabled = false;
            AddNewBtn.Enabled = false;
            EditBtn.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            dataGridView1.CellBeginEdit += dataGridView1_CellBeginEdit; ;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(new string[] { "Заказ", "Измерение", "Оператор", "Прибор", "Скважина" });
            comboBox1.SelectedItem = comboBox1.Items[0];
            selectedValue = "Заказ";
        }
        private string ShowInputBox(string prompt)
        {
            Form promptForm = new Form();
            promptForm.Width = 300;
            promptForm.Height = 150;
            promptForm.Text = prompt;

            TextBox textBox = new TextBox();
            textBox.Left = 50;
            textBox.Top = 20;
            textBox.Width = 200;

            Button confirmButton = new Button();
            confirmButton.Text = "OK";
            confirmButton.Left = 100;
            confirmButton.Top = 50;
            confirmButton.DialogResult = DialogResult.OK;

            confirmButton.Click += (sender, e) => { promptForm.Close(); };

            promptForm.Controls.Add(textBox);
            promptForm.Controls.Add(confirmButton);

            return promptForm.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectionString = ShowInputBox("Введите строку подключения:");
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                MessageBox.Show("Соединение открыто");
                selectedValue = comboBox1.SelectedItem.ToString();
                UpdateDataGridView();
                connection.Close();
                button1.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при подключении к базе данных: " + ex.Message);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                selectedValue = comboBox1.SelectedItem.ToString();
                UpdateDataGridView();
            }
        }
        private void UpdateDataGridView()
        {
            try
            {
                string query = "SELECT * FROM [" + selectedValue + "]";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command; // Установка команды для адаптера
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
                // Активируем кнопки после загрузки данных
                SaveBtn.Enabled = false;
                button2.Enabled = true;
                button3.Enabled = true;
                AddNewBtn.Enabled = true;
                DeleteBtn.Enabled = true;
                EditBtn.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка при выполнении запроса: " + ex.Message);
            }
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // Проверяем, что это столбец с ID и что у нас есть данные для заполнения ComboBox
            if (dataGridView1.Columns[e.ColumnIndex].Name == "ID")
            {
                DataGridViewComboBoxCell comboBoxCell = new DataGridViewComboBoxCell();

                // Загрузка значений ComboBox из соответствующей таблицы (например, таблицы "Заказ")
                SqlCommand loadComboBoxValuesCommand = new SqlCommand("SELECT ID FROM " + selectedValue, connection);
                SqlDataReader reader = loadComboBoxValuesCommand.ExecuteReader();
                while (reader.Read())
                {
                    comboBoxCell.Items.Add(reader["ID"]);
                }
                reader.Close();

                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex] = comboBoxCell;
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                SqlDataAdapter sAdapter = new SqlDataAdapter("SELECT * FROM [" + selectedValue + "]", connection);
                SqlCommandBuilder sBuilder = new SqlCommandBuilder(sAdapter);
                DataTable sTable = (DataTable)dataGridView1.DataSource;
                sAdapter.Update(sTable);
                dataGridView1.ReadOnly = true;
                SaveBtn.Enabled = false;
                AddNewBtn.Enabled = true;
                DeleteBtn.Enabled = true;
                EditBtn.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при сохранении данных: " + ex.Message);
            }
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0) // Проверяем, есть ли выбранная строка
                {
                    if (MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        int rowIndex = dataGridView1.SelectedRows[0].Index;
                        dataGridView1.Rows.RemoveAt(rowIndex);
                        // Обновляем данные в базе данных после удаления строки
                        SaveBtn_Click(sender, e);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите строку для удаления.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при удалении записи: " + ex.Message);
            }
        }

        private void AddNewBtn_Click(object sender, EventArgs e)
        {
            dataGridView1.ReadOnly = false;
            SaveBtn.Enabled = true;
            AddNewBtn.Enabled = false;
            DeleteBtn.Enabled = false;
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            EditBtn.Enabled = false;
            SaveBtn.Enabled = true;
            dataGridView1.ReadOnly = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                double depth_min = Convert.ToDouble(selectedRow.Cells["Глубина_мин"].Value);
                double depth_max = Convert.ToDouble(selectedRow.Cells["Глубина_макс"].Value);
                double amplitude = Convert.ToDouble(selectedRow.Cells["Амплитуда"].Value);

                Form2 form2 = new Form2();
                this.Hide();
                form2.FormClosed += SecondForm_FormClosed;
                form2.Owner = this; // Установка родительской формы
                form2.PlotData(depth_min, depth_max, amplitude); // Передача значений непосредственно в метод
                form2.Show();
            }
            else
            {
                MessageBox.Show("Выберите строку для отображения графика.");
            }
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                double depth_min = Convert.ToDouble(selectedRow.Cells["Глубина_мин"].Value);
                double depth_max = Convert.ToDouble(selectedRow.Cells["Глубина_макс"].Value);
                double amplitude = Convert.ToDouble(selectedRow.Cells["Амплитуда"].Value);

                Form2 form2 = new Form2();
                form2.Owner = this; // Установка родительской формы
                form2.PlotData(depth_min, depth_max, amplitude); // Передача значений непосредственно в метод
            }
        }
        private void SecondForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Show(); // Показываем первую форму после закрытия второй формы
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dataGridView1.ReadOnly = false;
            RandomizeSelectedCell();
            SaveBtn.Enabled = true;
        }

        private void RandomizeSelectedCell()
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                DataGridViewCell selectedCell = dataGridView1.SelectedCells[0];

                // Определение типа данных в выбранной ячейке
                Type dataType = dataGridView1.Columns[selectedCell.ColumnIndex].ValueType;

                // Генерация случайного значения в зависимости от типа данных
                object randomValue = GenerateRandomValue(dataType);

                // Установка случайного значения в выбранную ячейку
                selectedCell.Value = randomValue;

            }
            else
            {
                MessageBox.Show("Выберите ячейку для заполнения случайными данными.");
            }
        }


        private object GenerateRandomValue(Type dataType)
        {
            Random rand = new Random();

            // Генерация случайного значения в зависимости от типа данных
            if (dataType == typeof(int))
            {
                return rand.Next();
            }
            else if (dataType == typeof(double))
            {
                return rand.NextDouble() * 100; // Пример случайного значения для типа double
            }
            else if (dataType == typeof(string))
            {
                return Path.GetRandomFileName().Replace(".", "").Substring(0, 8); // Пример случайной строки
            }
            else if (dataType == typeof(DateTime))
            {
                return DateTime.Now.AddDays(rand.Next(365)); // Пример случайной даты
            }
            // Добавьте другие типы данных при необходимости

            return null; // Возвращаем null в случае, если тип данных неизвестен
        }
    }

}
