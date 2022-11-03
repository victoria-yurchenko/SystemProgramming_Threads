using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Exam_SystemProgramming_Threads
{
    public delegate void HelperToCall(ProgressBar progressBar);

    public partial class Form1 : Form
    {
        private const int _COUNT_THREADS_SIMULTANEOUSLY = 3;
        private ICollection<ProgressBar> _progressBars = null;
        private ICollection<Label> _labels = null;
        private Random _random = null;
        private Semaphore _semaphore = null;
        private HelperToCall _helperToCall = null;
        private int _positionInList = 0;
        private bool _isRunningProcess = false;

        public Form1()
        {
            InitializeComponent();

            nudEnterField.Maximum = 10;
            btnStart.Enabled = false;
            FormBorderStyle = FormBorderStyle.Fixed3D;
            timer.Start();

            _random = new Random();
            _progressBars = new List<ProgressBar>();
            _labels = new List<Label>();
            _helperToCall = new HelperToCall(ChangeValue);
        }

        private void DeleteFullProgressBars()
        {
            foreach (var progressBar in _progressBars)
            {
                if (progressBar.Value == progressBar.Maximum)
                {
                    var label = _labels.Where
                    (l =>
                        l.Location.X == progressBar.Location.X + progressBar.Width &&
                        l.Location.Y == progressBar.Location.Y
                    ).FirstOrDefault();

                    _labels.Remove(label);
                    _progressBars.Remove(progressBar);
                    Controls.Remove(label);
                    Controls.Remove(progressBar);
                    _positionInList--;
                    break;
                }
            }
        }

        private ProgressBar GetNextProgressBar()
        {
            int i = 0;
            foreach (var progressBar in _progressBars)
            {
                if (i == _positionInList)
                {
                    _positionInList++;
                    return progressBar;
                }
                else
                {
                    i++;
                }
            }
            return null;
        }

        private void ChangeValue(ProgressBar progressBar)
        {
            if (progressBar.Value != progressBar.Maximum)
            {
                progressBar.Value++;
            }
        }

        private void ExecuteProgressBar()
        {
            _semaphore.WaitOne();

            var progressBar = GetNextProgressBar();

            for (int i = 0; i < progressBar?.Maximum; i++)
            {
                Thread.Sleep(1000);
                Invoke(_helperToCall, progressBar);
            }
            _semaphore.Release();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (_progressBars.Count < 10)
            {
                try
                {
                    int number = _random.Next(1, Convert.ToInt32(nudEnterField.Value));

                    ProgressBar progressBar = new ProgressBar();
                    progressBar.Location = new Point(e.X, e.Y);
                    progressBar.Width = 250;
                    progressBar.Height = 20;
                    progressBar.Maximum = number;
                    progressBar.Minimum = 0;

                    Label label = new Label();
                    label.Location = new Point(e.X + progressBar.Width, e.Y);
                    label.Text = number.ToString();
                    label.ForeColor = Color.YellowGreen;

                    this.Controls.Add(progressBar);
                    this.Controls.Add(label);
                    _progressBars.Add(progressBar);
                    _labels.Add(label);
                }
                catch
                {
                    MessageBox.Show("Cannot create progress bar without entered " +
                        "value in the numeric up and down field!");
                }
            }
            else
            {
                MessageBox.Show("The number of progress bars cannot be " +
                    "more than 10!");
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            _isRunningProcess = true;
            nudEnterField.Value = 0;

            if (_semaphore == null)
            {
                _semaphore = new Semaphore(_COUNT_THREADS_SIMULTANEOUSLY, _COUNT_THREADS_SIMULTANEOUSLY);
            }

            for (int i = 0; i < _progressBars.Count; i++)
            {
                var thread = new Thread(ExecuteProgressBar);
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void UnrunningProcess()
        {
            if (_progressBars.Count == 0)
            {
                _isRunningProcess = false;
            }
        }

        private void ValidateNumericUpDown()
        {
            if (!_isRunningProcess)
            {
                nudEnterField.Enabled = true;
            }
            else
            {
                nudEnterField.Enabled = false;
            }
        }

        private void ValidateButtonStart()
        {
            if (nudEnterField.Value > 0 &&
                _progressBars.Count > 0 &&
                !_isRunningProcess)
            {
                btnStart.Enabled = true;
            }
            else
            {
                btnStart.Enabled = false;
            }
        }

        private void ValidateControls()
        {
            UnrunningProcess();
            ValidateNumericUpDown();
            ValidateButtonStart();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ValidateControls();
            DeleteFullProgressBars();
            this.Refresh();
        }
    }
}
