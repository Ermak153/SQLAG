using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SQLAG
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
        public void PlotData(double depth_min, double depth_max, double amplitude)
        {
            // Очистить график перед построением нового
            chart1.Series.Clear();

            // Создать новую серию данных для графика
            Series series = new Series();
            series.ChartType = SeriesChartType.Line;

            // Добавить несколько точек на график, исходя из переданных данных
            for (double depth = depth_min; depth <= depth_max; depth += 0.1) // Примерно каждые 0.1
            {
                // Вычислить Глубина * Амплитуда
                double depthTimesAmplitude = amplitude * (depth - depth_min); // Используем глубину, а не среднюю глубину

                // Добавить точку на график
                series.Points.AddXY(depth, depthTimesAmplitude);
            }

            // Добавить серию данных на график
            chart1.Series.Add(series);
        }


    }
}
