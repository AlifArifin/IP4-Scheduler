using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/// <summary>
/// INI BUAT BFS
/// </summary>
namespace Scheduler
{
    public partial class Form2 : Form
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

        //getter setter
        public string File_path { get => file_path; set => file_path = value; }

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //inisiasi
            step_now = 0;
            play_cond = 0;

            //memasukkan warna-warna
            addColorDict();

            //membuat matriks dari file_path
            Graph M2 = new Graph(file_path);

            //membuat BFS dengan input matriks ketetanggaan dan array yang menyimpan nama matkul
            BFS BFS2 = new BFS(M2.Matrix, M2.Courses);

            //men-clone list
            list_viewer = new List<Microsoft.Msagl.GraphViewerGdi.GViewer>(BFS2.List_viewer);

            //me-assign total_step dengan banyaknya viewer pada list
            total_step = list_viewer.Count();

            //meng-clone matrix_count
            matrix_count = (int[,])BFS2.Matrix_count.Clone();

            //meng-clone courses dari matrix
            courses = (string[])M2.Courses.Clone();

            //meng-clone matrix result
            matrix_result = (int[,])BFS2.Result.Clone();
            
            //menampilkan graph dengan step_now = 0 (step ke-0)
            pictureBox1.Controls.Add(list_viewer.ElementAt(step_now));
            list_viewer.ElementAt(step_now).Dock = DockStyle.Fill;
            list_viewer.ElementAt(step_now).BackColor = Color.FromArgb(45, 45, 48);
            list_viewer.ElementAt(step_now).ForeColor = Color.FromArgb(45, 45, 48);
            pictureBox1.ResumeLayout();
            pictureBox1.Show();
            showCount(sender, e);

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

        private void Form2_FormClosing(object sender, EventArgs e)
        {
            //agar tidak terjadi disposed
            Application.Exit();
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
            showCount(sender, e);

            checkButton(sender, e);

            // Menampilkan teks di teksboks 2 jika step selesai
            if (step_now == total_step - 1)
            {
                for (int i = 0; i < step_now; i++)
                {
                    string temp = "";

                    if (matrix_result[i, 0] != 0)
                    {
                        textBox2.Text += String.Format("== Semester {0} ==\r\n", i + 1);
                    }

                    for (int j = 0; j < step_now; j++)
                    {
                        if (matrix_result[i,j] != 0)
                        {
                            textBox2.Text += String.Format("- {0}\r\n", courses[matrix_result[i, j] - 1]);
                        }
                    }
                    textBox2.Text += String.Format("\r\n");
                }
            }
            else
            {
                textBox2.Clear();
            }
        }

        private void prevStep(object sender, EventArgs e)
        {
            //mengurangi step
            step_now = step_now - 1;

            //menampilkan viewer pada index ke - step_now
            pictureBox1.Controls.Add(list_viewer.ElementAt(step_now));
            list_viewer.ElementAt(step_now).Dock = DockStyle.Fill;
            list_viewer.ElementAt(step_now).BringToFront();
            pictureBox1.ResumeLayout();
            pictureBox1.Show();
            showCount(sender, e);

            checkButton(sender, e);
            textBox2.Clear();
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

        private void showCount(object sender, EventArgs e)
        {
            textBox1.Clear();
            for (int i = 0; i < total_step - 1; i++)
            {
                textBox1.Text += String.Format("{0} = {1}\r\n", courses[i], matrix_count[step_now, i]);
            }
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

        private void addColorDict()
        {
            //membaut kamus warna
            color_dict = new Dictionary<string, Color>();

            color_dict.Add("background", Color.FromArgb(45, 45, 48));
            color_dict.Add("button_en", Color.FromArgb(89, 201, 165));
            color_dict.Add("button_nen", Color.FromArgb(185, 227, 198));
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
