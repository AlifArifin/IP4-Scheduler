using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Drawing = Microsoft.Msagl.Drawing;


namespace Scheduler
{
    public partial class Form1 : Form
    {
        private Dictionary<string, Color> color_dict; // Kamus warna

        public Form1()
        {
            InitializeComponent();
        }

        //button 1 = BFS, digunakan untuk menampilkan solusi dengan pendekatan BFS
        private void button1_Click(object sender, EventArgs e)
        {
            //menyimpan path dari file
            Form2 secondForm = new Form2();
            secondForm.File_path = fileBrowser.FileName;
            secondForm.Show();
        }

        //button 2 = Browse, digunakan untuk memilih file
        private void button2_Click(object sender, EventArgs e)
        {
            fileBrowser.Title = "Open File";
            fileBrowser.InitialDirectory = @"d:\";

            // Filter file untuk .txt
            fileBrowser.Filter = "Text File (*.txt) | *.txt";

            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;

            if (fileBrowser.ShowDialog() == DialogResult.OK)
            {
                //Menampilkan path dari file yang dipilih
                textBox1.Text = fileBrowser.FileName;

                //membuat button DFS dan BFS menjadi enable dan mengganti warnanya
                button1.Enabled = true;
                button1.BackColor = color_dict["button_en"];
                button3.Enabled = true;
                button3.BackColor = color_dict["button_en"];
            }
        }

        //button 3 = DFS, digunakan untuk menampilkan solusi dengan pendekatan DFS
        private void button3_Click(object sender, EventArgs e)
        {
            //menyimpan path dari file
            Form3 thirdForm = new Form3();
            thirdForm.File_path = fileBrowser.FileName;
            thirdForm.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            addColorDict();
            pictureBox1.Image = pictureBox1.InitialImage;
            pictureBox1.Show();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            this.BackColor = color_dict["background"];

            //Mengeset agar button BFS dan DFS dalam keadaan false (tidak bisa diklik)
            button1.Enabled = false;
            button3.Enabled = false;


            //Pewarnaan
            button1.BackColor = !button1.Enabled ? color_dict["button_nen"] : color_dict["button_en"];
            button3.BackColor = !button3.Enabled ? color_dict["button_nen"] : color_dict["button_en"];
            button2.BackColor = color_dict["button_en"];
            textBox1.BackColor = Color.White;
            textBox2.BackColor = color_dict["background"];
            
            textBox2.ForeColor = Color.White;
        }

        private void addColorDict()
        {
            //membuat kamus warna
            color_dict = new Dictionary<string, Color>();

            color_dict.Add("background", Color.FromArgb(45, 45, 48));
            color_dict.Add("button_en", Color.FromArgb(89, 201, 165)); 
            color_dict.Add("button_nen", Color.FromArgb(185, 227, 198));
        }
    }

    class Graph
    {
        //atribut
        private int[,] matrix; //Matriks ketetanggan dari graf
        private string[] courses; //Larik yang menyimpan informasi nama-nama mata kuliah pada graf
        private bool[] visit; //Array yang menyatakan apakah suatu simpul sudah ditelusuri atau belum 
        private int[] start; // Array yang menyimpan data simpul saat suatu simpul baru ditelusuri
        private int[] finish; // Array yang menyimpan data simpul saat suatu simpul selesai ditelusuri
        private int[] semester; // Array yang menyimpan data suatu simpul (matkul) berada di semester berapa

        //Atribut yang digunakan untuk visualisasi graf
        private Microsoft.Msagl.GraphViewerGdi.GViewer viewer;
        private Microsoft.Msagl.Drawing.Graph graph_show;

        private List<Microsoft.Msagl.GraphViewerGdi.GViewer> list_viewer; // list dari visualisasi untuk graf

        //getter dan setter
        public int[,] Matrix { get => matrix; set => matrix = value; }
        public string[] Courses { get => courses; set => courses = value; }
        public bool[] Visit { get => visit; set => visit = value; }
        public int[] Start { get => start; set => start = value; }
        public int[] Finish { get => finish; set => finish = value; }
        public int[] Semester { get => semester; set => semester = value; }
        public Microsoft.Msagl.GraphViewerGdi.GViewer Viewer { get => viewer; set => viewer = value; }
        public Drawing.Graph Graph_show { get => graph_show; set => graph_show = value; }
        public List<Microsoft.Msagl.GraphViewerGdi.GViewer> List_viewer { get => list_viewer; set => list_viewer = value; }
        

        //Konstruktor Graf Mata kuliah
        public Graph(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            int lineCount = lines.Length;   //mencatat jumlah baris pada file (jumlah baris = jumlah course)
            int i = 0, j;  //untuk pencatatan courses
            int indexKeyCourse = -1, indexCourse = -1;    //index dari course pada inisiasi awal (walaupun pasti ketimpa)

            matrix = new int[lineCount, lineCount];
            courses = new string[lineCount];

            //melakukan pendataan course sesuai dengan urutan line pada file
            foreach (string line in lines)
            {
                string[] temp = line.Split(',');

                //temp[0] merupakan courseKey (course yang dituju)
                courses[i] = temp[0].Trim().Replace(".", string.Empty);
                i++;
            }

            //pembuatan matriks ketetanggaan
            foreach (string line in lines)
            {
                string[] temp = line.Split(',');
                for (j = 0; j < temp.Length; j++)
                {
                    string courseName = temp[j].Trim().Replace(".", string.Empty);  //menghilangkan spasi di depan/belakang dan titik

                    //jika j == 0 (merupakan course yang dituju)
                    if (j == 0)
                    {
                        indexKeyCourse = Array.IndexOf(courses, courseName);
                    }
                    else //jika j > 0 (merupakan pre-requisite dari course yang dituju
                    {
                        indexCourse = Array.IndexOf(courses, courseName);

                        matrix[indexCourse, indexKeyCourse] = 1;
                    }
                }
            }
            //membuat graph-nya
            viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();

            //create a graph object 
            graph_show = new Microsoft.Msagl.Drawing.Graph("graph");

            for (i = 0; i < matrix.GetLength(0); i++)
            {
                for (j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] == 1)
                    {
                        graph_show.AddEdge(i.ToString(), j.ToString());
                    }
                }
            }


            //bind the graph to the viewer
            viewer.Graph = graph_show;
        }


        //Mencetak Graph dan legendanya
        public void printGraph()
        {
            int i, j;

            //Mencetak matriks ketetanggaan dengan
            //-> 0 artinya i tidak bertetangga dengan j
            //-> 1 artinya i bertetangga dengan j
            Console.WriteLine("Matriks Ketetanggaan");
            for (i = 0; i < matrix.GetLength(0); i++)
            {
                for (j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write(String.Format("{0} ", matrix[i, j]));
                }
                Console.WriteLine();
            }

            //Mencetak legenda dari matriks ketetanggaan
            Console.WriteLine();
            Console.WriteLine("Dengan indeks : ");
            for (i = 0; i < courses.Length; i++)
            {
                Console.WriteLine(String.Format("{0} = {1}", i, courses[i]));
            }
        }

        // Mengembalikan indeks course-course yang tidak memiliki prasyarat
        public List<int> SearchInit()
        {
            int size = courses.Length; // ukuran matriks
            List<int> L = new List<int>();
            int inc; // jumlah incoming edge

            //mencari node dengan jumlah incoming edge 0
            for (int i = 0; i < size; i++)
            {
                inc = CountInc(i);
                if (inc == 0)
                {
                    L.Add(i);
                }
            }
            return L;
        }

        // Menghitung Jumlah Incoming edge suatu vertex
        public int CountInc(int v)
        {
            int inc = 0;
            for (int i = 0; i < courses.Length; i++)
            {
                if (matrix[i, v] == 1)
                {
                    inc++;
                }
            }
            return inc;
        }

        //Melakukan Topology Sort dan DFS untuk menentukan urutan pengambilan mata kuliah
        public void DTsort(List<int> f)
        {
            int n = courses.Length;
            int ts = 0;
            
            //Inisiasi atribut untuk DFS
            visit = new bool[n];
            start = new int[n];
            finish = new int[n];
            semester = new int[n];

            foreach (int fi in f)
            {
                ts++;
                DFS(fi, ref ts);
            }

            int[] l = finish;
            int max;
            int maxIndex;
            int inc;

            //penentuan semester dari tiap mata kuliah
            for (int i = 0; i < n; i++)
            {
                max = l.Max();
                maxIndex = Array.IndexOf(l, max);
                inc = CountInc(maxIndex);
                Console.WriteLine(courses[maxIndex]);
                Console.WriteLine("Incoming {0}", inc);
                if (inc == 0)
                {
                    semester[maxIndex] = 1;
                }
                else
                {
                    semester[maxIndex] = 0;
                    for (int j = 0; j < n; j++)
                    {
                        if ((matrix[j, maxIndex] == 1) && (semester[j] >= semester[maxIndex]))
                        {
                            semester[maxIndex] = semester[j] + 1;
                        }
                    }
                }
                l[maxIndex] = 0;
            }
        }

        //Menelusuri Graph dengan Depth First Search dan memberikan timestamp ke setiap simpul yang baru ditelusuri dan selesai ditelusuri
        public void DFS(int f, ref int ts)
        {
            // Menandai simpul telah dikunjungi dan menandai waktu mulai simpul
            visit[f] = true;
            start[f] = ts; 

            //Algoritma DFS
            for (int i = 0; i < courses.Length; i++)
            {
                if (matrix[f, i] == 1)
                {
                    if (!visit[i])
                    {
                        ts++;
                        DFS(i, ref ts);
                    }
                }
            }
            ts++;

            // Menandai waktu selesai suatu simpul
            finish[f] = ts;

            //Membuatviewer keadaan graf sekarang dan menambahkannya ke list
            makeList_viewer();
        }

        // Menambahkan viewer untuk visualisasi graf ke list viewer
        private void makeList_viewer()
        {
            //create a viewer object 
            Microsoft.Msagl.GraphViewerGdi.GViewer viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            Microsoft.Msagl.GraphViewerGdi.GViewer viewer_temp = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            Drawing.Graph graph_show = new Drawing.Graph("graph");
            Drawing.Edge edge;
            Drawing.Node node;

            int num_node = matrix.GetLength(0);
            int count = 0; //iterasi untuk penggunaan start dan finish

            list_viewer = new List<Microsoft.Msagl.GraphViewerGdi.GViewer>();

            for (int i = 0; i <= num_node * 2; i++)
            {
                viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
                graph_show = new Drawing.Graph("graph");

                //Membuat node dan edge
                for (int k = 0; k < num_node; k++)
                {
                    graph_show.AddNode(courses[k]);
                }

                for (int m = 0; m < num_node; m++)
                {
                    for (int n = 0; n < num_node; n++)
                    {
                        if (matrix[m, n] == 1)
                        {
                            edge = graph_show.AddEdge(courses[m], courses[n]);
                        }
                    }
                    graph_show.FindNode(courses[m]).LabelText = String.Format("-/-\r\n{0}", courses[m]);
                }

                //Melakukan pewarnaan
                for (int j = 0; j <= i; j++)
                {
                    for (int k = 0; k < num_node; k++)
                    {
                        if (j == start[k] || j == finish[k])
                        {
                            node = graph_show.FindNode(courses[k]);

                            if (j == i)
                            {
                                node.Attr.FillColor = Drawing.Color.LightGreen;
                            }
                            else
                            {
                                node.Attr.FillColor = start[k] == j ? Drawing.Color.Gold : Drawing.Color.LightGray;
                            }

                            node.LabelText = start[k] == j ? String.Format("{0}/-\r\n{1}", j, courses[k]) : String.Format("{0}/{1}\r\n{2}", start[k], finish[k], courses[k]);
                        }
                    }
                }
                viewer.Graph = graph_show;
                list_viewer.Add(viewer);
            }
        }
    }


    class BFS
    {
        private int[,] matrix_count;
        private int[] Count;
        private int[,] result;
        private string[] courses;
        private List<Microsoft.Msagl.GraphViewerGdi.GViewer> list_viewer;
        private int num_node;
        private int[] result_2d;

        public int[,] Result { get => result; set => result = value; }
        public List<Microsoft.Msagl.GraphViewerGdi.GViewer> List_viewer { get => list_viewer; set => list_viewer = value; }
        public int[,] Matrix_count { get => matrix_count; set => matrix_count = value; }

        public void PrintMat(int[,] Matrix)
        {
            int N = Matrix.GetLength(0);
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Console.Write(Matrix[i, j]);
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }

        public void PrintResult()
        {
            Console.WriteLine("RESULT:");
            int N = num_node;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (result[i, j] != 0)
                    {
                        Console.Write(result[i, j]);
                        Console.Write(" ");
                    }
                }
                if (result[i, 0] != 0)
                {
                    Console.Write("-> ");
                }
            }
            Console.WriteLine();
        }

        public void PrintCount()
        {
            Console.Write("COUNT: ");
            int N = Count.Length;
            for (int i = 0; i < N; i++)
            {
                Console.Write(Count[i]);
                Console.Write("  ");
            }
            Console.WriteLine();
        }

        public bool IsInResult(int X)
        {
            bool found = false;
            int N = num_node;
            int i = 0;
            while ((i < N) && (!found))
            {
                int j = 0;
                while ((j < N) && (!found))
                {
                    if (result[i, j] == X)
                    {
                        found = true;
                    }
                    j++;
                }
                i++;
            }
            return found;
        }

        public void CountEdgeIn(int[,] Matrix)
        {
            int N = Matrix.GetLength(0);
            // Menghitung banyaknya derajat-masuk setiap simpul
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (Matrix[j, i] == 1)
                    {
                        Count[i]++;
                    }
                }
            }
        }

        public BFS(int[,] Matrix, string[] _courses)
        {
            courses = (string[])_courses.Clone();
            int N = Matrix.GetLength(0);
            Count = new int[N];
            result = new int[N, N];
            num_node = Matrix.GetLength(0);
            int[,] matrix_copy = (int[,])Matrix.Clone();
            matrix_count = new int[num_node + 1, num_node];

            // Inisialisasi nilai
            for (int i = 0; i < N; i++)
            {
                Count[i] = 0;
            }
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    result[i, j] = 0;
                }
            }

            // Menghitung banyaknya derajat-masuk setiap simpul
            CountEdgeIn(Matrix);

            // Mencari hasilnya
            int a = 0; //indeks result ke-a
            int c = 0; //counter
            int d = 0; //counter untuk matriks

            if (d == 0)
            {
                for (int idx = 0; idx < num_node; idx++)
                {
                    matrix_count[0, idx] = Count[idx];
                }
            }

            while ((a < N) && (c < N))
            {
                bool found = true;
                int i;
                int j = 0;
                while ((found) && (j < N))
                {
                    // Cari yg 0
                    i = 0;
                    found = false;
                    while ((i < N) && (!found))
                    {
                        if ((Count[i] == 0) && (!IsInResult((i + 1))))
                        {
                            found = true;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    if (found)
                    {
                        // ketemu
                        result[a, j] = i + 1;
                        j++;
                        c++; //counter
                    }
                }

                
                // hilangkan
                int k = 0;
                while ((k < N) && (result[a, k] != 0))
                {
                    for (int l = 0; l < N; l++)
                    {
                        if (Matrix[result[a, k] - 1, l] == 1)
                        {
                            Count[l] = Count[l] - 1;
                            Matrix[result[a, k] - 1, l] = 0;
                        }

                    }
                    d++;
                    for (int idx = 0; idx < num_node; idx++)
                    {
                        matrix_count[d, idx] = Count[idx];
                    }
                    k++;
                }
                a++;
            }

            result_2d = new int[num_node];
            for (int i = 0, k = 0; i < num_node; i++)
            {
                for (int j = 0; j < num_node; j++)
                {
                    if (result[i, j] != 0)
                    {
                        result_2d[k] = result[i, j] - 1;
                        k++;
                    }
                }
            }

            makeList_Viewer(matrix_copy);
        }

        public void makeList_Viewer(int[,] _matrix)
        {
            int i, j;
            Drawing.Edge[,] matrix_edge;

            //create a viewer object 
            Microsoft.Msagl.GraphViewerGdi.GViewer viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            Microsoft.Msagl.GraphViewerGdi.GViewer viewer_temp = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            Drawing.Graph graph = new Drawing.Graph("graph");
            Drawing.Edge edge;
            Drawing.Node node;

            list_viewer = new List<Microsoft.Msagl.GraphViewerGdi.GViewer>();

            matrix_edge = new Drawing.Edge[num_node, num_node];

            for (i = 0; i <num_node; i++)
            {
                graph.AddNode(courses[i]);
            }

            for (i = 0; i < num_node; i++)
            {
                for (j = 0; j < num_node; j++)
                {
                    if (_matrix[i, j] == 1)
                    {
                        edge = graph.AddEdge(courses[i], courses[j]);
                        matrix_edge[i, j] = edge;
                    }
                }
            }

            viewer.Graph = graph;

            list_viewer.Add(viewer);

            for (i = 0; i < num_node; i++)
            {
                viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
                graph = new Drawing.Graph("graph");
                matrix_edge = new Drawing.Edge[num_node, num_node];

                //Membuat node dan edge
                for (int k = 0; k < num_node; k++)
                {
                    graph.AddNode(courses[k]);
                }

                for (int m = 0; m < num_node; m++)
                {
                    for (int n = 0; n < num_node; n++)
                    {
                        if (_matrix[m, n] == 1)
                        {
                            edge = graph.AddEdge(courses[m], courses[n]);
                            matrix_edge[m, n] = edge;
                        }
                    }
                }

                //Melakukan pewarnaan
                for (j = 0; j <= i; j++)
                {
                    //Mewarnai node (jika sudah dilewati (merah) jika sekarang dipakai (hijau))
                    if (j == i)
                    {
                        node = graph.FindNode(courses[result_2d[j]]);
                        node.Attr.FillColor = Drawing.Color.LightGreen;
                    }
                    else
                    {
                        node = graph.FindNode(courses[result_2d[j]]);
                        node.Attr.FillColor = Drawing.Color.LightGray;
                    }

                    //pewarnaan edge
                    for (int k = 0; k < num_node; k++)
                    {
                        //mencari index dari result_2d[j] untuk pewarnaan graf (biar tau node-nya yang mana)
                        int temp = result_2d[j];

                        if (matrix_edge[temp, k] != null)
                        {
                            graph.RemoveEdge(matrix_edge[temp, k]);
                            edge = graph.AddEdge(courses[temp], courses[k]);
                            if (j == i)
                            {
                                edge.Attr.Color = Drawing.Color.LightGreen;
                            }
                            else
                            {
                                edge.Attr.Color = Drawing.Color.White;
                            }
                        }
                    }
                }
                viewer.Graph = graph;
                list_viewer.Add(viewer);
            }
        }
    }
}

