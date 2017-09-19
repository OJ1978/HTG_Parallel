using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HTG_Parallel_PI
{
    public partial class frmParallel : Form
    {
        const int Steps = 2000;
        static ListBox lb = new ListBox();

        public frmParallel()
        {
            InitializeComponent();
        }

        static void Duration<T>(Func<T> Calc)
        {
            var swPi = Stopwatch.StartNew();

            var Result = Calc();

            lb.Items.Add("Duration: " + swPi.Elapsed + " - Value: " + Result);

        }

        static double ParallelLinq()
        {
            double dblIncrement = 1.0 / (double)Steps;

            return (from pe in ParallelEnumerable.Range(0, Steps)

                    let objX = (pe + 0.5) * dblIncrement

                    select 4.0 / (1.0 + objX * objX)).Sum() * dblIncrement;

        }

        static double ParallelForPi()
        {
            double dblSum = 0.0;

            double dblIncrement = 1.0 / (double)Steps;

            object objPI = new object();

            Parallel.For(0, Steps, () => 0.0, (i, LoopState, objLocal) =>
            {
                double dblX = (i + 0.5) * dblIncrement;

                return objLocal + 4.0 / (1.0 + dblX * dblX);

            }, objTemp => { lock (objPI) dblSum += objTemp; });

            return dblIncrement * dblSum;

        }

        static double ParallelForEachRangePartitioner()
        {
            double sum = 0.0;

            double increment = 1.0 / (double)Steps;

            object pi = new object();

            Parallel.ForEach(Partitioner.Create(0, Steps), () => 0.0, (tplRange, LoopState, objLocal) =>
            {
                for (int i = tplRange.Item1; i < tplRange.Item2; i++)

                {
                    double x = (i + 0.5) * increment;

                    objLocal += 4.0 / (1.0 + x * x);

                }

                return objLocal;

            }, local => { lock (pi) sum += local; });

            return increment * sum;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            lb.Width = panel1.Width - 10;
            lb.Height = panel1.Height - 10;

            lb.Left = 5;
            lb.Top = 5;

            lb.Visible = true;

            this.panel1.Controls.Add(lb);

        }

        private void btnCalc_Click(object sender, EventArgs e)
        {
            
                Duration(() => ParallelLinq());
                Duration(() => ParallelForPi());
                Duration(() => ParallelForEachRangePartitioner());

        }
    }
}
