using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scheduler
{
    public partial class Form3 : Form
    {
        //atribut
        private string file_path; //path dari file yang diakses
        private List<Microsoft.Msagl.GraphViewerGdi.GViewer> list_viewer; //menyimpan list of viewer punya class BFS
        private int total_step; //total step pada list
        private int step_now; //step sekarang (dari list)
        private int play_cond; //lagi di stop = 0, lagi di play = 1
        private int[,] matrix_count;
        private string[] courses;
        private int[,] matrix_result;
        private Dictionary<string, Color> color_dict;
        private int[] semester;

        //getter setter
        public string File_path { get => file_path; set => file_path = value; }

        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            //inisiasi
            step_now = 0;
            play_cond = 0;

            //memasukkan warna-warna
            addColorDict();

            //membuat matriks dari file_path
            Graph M2 = new Graph(file_path);
            List<int> li = M2.SearchInit();
            M2.DTsort(li);
            semester = (int[])M2.Semester.Clone();

            //men-clone list
            list_viewer = new List<Microsoft.Msagl.GraphViewerGdi.GViewer>(M2.List_viewer);

            //me-assign total_step dengan banyaknya viewer pada list
            total_step = list_viewer.Count();

            //meng-clone courses dari matrix
            courses = (string[])M2.Courses.Clone();

            //menampilkan graph dengan step_now = 0 (step ke-0)
            pictureBox1.Controls.Add(list_viewer.ElementAt(step_now));
            list_viewer.ElementAt(step_now).Dock = DockStyle.Fill;
            list_viewer.ElementAt(step_now).BackColor = Color.FromArgb(45, 45, 48);
            list_viewer.ElementAt(step_now).ForeColor = Color.FromArgb(45, 45, 48);
            pictureBox1.ResumeLayout();
            pictureBox1.Show();

            //untuk penggunaan play
            timer1.Interval = 750; //interval 750 ms biar gk lama-lama
            timer1.Tick += new EventHandler(playStep); //menambahkan playstep pada tick

            //untuk penampilan prev dan next button (di-enable atau tidak)
            prev.Enabled = step_now == 0 ? false : true;
            next.Enabled = step_now == total_step - 1 ? false : true;
            play.Enabled = next.Enabled;

            //inisiasi warna
            prev.BackColor = prev.Enabled ? color_dict["button_en"] : color_dict["button_nen"];
            next.BackColor = next.Enabled ? color_dict["button_en"] : color_dict["button_nen"];
            play.BackColor = play.Enabled ? color_dict["button_en"] : color_dict["button_nen"];

            //pewarnaan background
            this.BackColor = color_dict["background"];
        }

        private void Form3_FormClosing(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void next_Click(object sender, EventArgs e)
        {
            nextStep(sender, e);
        }

        private void prev_Click(object sender, EventArgs e)
        {
            prevStep(sender, e);
        }

        private void play_Click(object sender, EventArgs e)
        {
            //jika graph dalam keadaan stop
            if (play_cond == 0)
            {
                timer1.Start();
                play.Text = "Stop";
                play_cond = 1;
            }
            //jika graph dalam keadaan play
            else if (play_cond == 1)
            {
                timer1.Stop();
                play.Text = "Play";
                play_cond = 0;
            }
        }

        private void playStep(object sender, EventArgs e)
        {
            //playstep akan jalan selama timer nyala dan tiap interval 500ms
            nextStep(sender, e);

            //jika sudah sampai step terakhir
            if (step_now == total_step - 1)
            {
                //stop timer1
                timer1.Stop();

                //mengubah format button
                play_Click(sender, e);
            }
        }

        private void addColorDict()
        {
            //membaut kamus warna
            color_dict = new Dictionary<string, Color>();

            color_dict.Add("background", Color.FromArgb(45, 45, 48));
            color_dict.Add("button_en", Color.FromArgb(89, 201, 165));
            color_dict.Add("button_nen", Color.FromArgb(185, 227, 198));
        }

        private void nextStep(object sender, EventArgs e)
        {
            //menambahkan step
            step_now = step_now + 1;

            //menampilkan viewer pada index ke - step_now
            pictureBox1.Controls.Add(list_viewer.ElementAt(step_now));
            list_viewer.ElementAt(step_now).Dock = DockStyle.Fill;
            list_viewer.ElementAt(step_now).BringToFront();
            pictureBox1.ResumeLayout();
            pictureBox1.Show();

            checkButton(sender, e);


            // Menampilkan teks di teksboks 2 jika step selesai
            int max = semester.Max();
            if (step_now == total_step - 1)
            {
                for (int i = 0; i < max; i++)
                {
                    textBox1.Text += String.Format("== Semester {0} ==\r\n", i + 1);
                    for (int j = 0; j < step_now / 2; j++)
                    {
                        if (semester[j] == i + 1)
                        {
                            textBox1.Text += String.Format("- {0}\r\n", courses[j]);
                        }
                    }
                    textBox1.Text += String.Format("\r\n");
                }
            }
            else
            {
                textBox1.Clear();
            }
        }

        private void prevStep(object sender, EventArgs e)
        {
            textBox1.Clear();

            //mengurangi step
            step_now = step_now - 1;

            //menampilkan viewer pada index ke - step_now
            pictureBox1.Controls.Add(list_viewer.ElementAt(step_now));
            list_viewer.ElementAt(step_now).Dock = DockStyle.Fill;
            list_viewer.ElementAt(step_now).BringToFront();
            pictureBox1.ResumeLayout();
            pictureBox1.Show();

            checkButton(sender, e);
        }

        private void checkButton(object sender, EventArgs e)
        {
            //untuk penentuan format button next dan prev

            if (step_now == 0)
            {
                prev.Enabled = false;
            }
            if (step_now != total_step - 1)
            {
                next.Enabled = true;
                play.Enabled = true;
            }

            if (step_now == total_step - 1)
            {
                next.Enabled = false;
                play.Enabled = false;
            }
            if (step_now != 0)
            {
                prev.Enabled = true;
            }
        }

        private void play_EnabledChanged(object sender, EventArgs e)
        {
            play.BackColor = play.Enabled ? color_dict["button_en"] : color_dict["button_nen"];
        }

        private void next_EnabledChanged(object sender, EventArgs e)
        {
            next.BackColor = next.Enabled ? color_dict["button_en"] : color_dict["button_nen"];
        }

        private void prev_EnabledChanged(object sender, EventArgs e)
        {
            prev.BackColor = prev.Enabled ? color_dict["button_en"] : color_dict["button_nen"];
        }
    }
}
