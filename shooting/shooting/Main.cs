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
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using System.Drawing.Drawing2D;

namespace shooting
{
    public partial class Main : Form
    {
        //全ステージを格納するリスト
        public static List<List<Point>> All_Points = new List<List<Point>>();
        //全ステージ表を格納するリスト
        public static List<int[,]> All_Table = new List<int[,]>();
        //全障害物を格納するリスト
        public static List<Rectangle[]> All_Rect = new List<Rectangle[]>();
        public static List<Rectangle[]> All_Rect_ex = new List<Rectangle[]>();
        //敵のリスト
        List<Enemy> Enemies = new List<Enemy>();
        //弾薬配置リスト
        public static List<Bullet> All_Bullet = new List<Bullet>();
        //回復配置リスト
        public static List<Heal_item> All_heal = new List<Heal_item>();
        //武器配置リスト
        public static List<Weapon> All_weapon = new List<Weapon>();
        //敵配置リスト
        public static List<Enemy_data> All_Enemy = new List<Enemy_data>();
        //開始ドア位置のリスト
        public static List<Door_data> Door_In = new List<Door_data>();
        //終わりドアのリスト
        public static List<Door_data> Door_Out = new List<Door_data>();
        //インスタンス
        private Enemy enemy;
        private Key key = new Key();
        private Player player;
        //ドアの大きさ
        private const int door_width = 40;
        private const int door_height = 50;
        //ステージ
        public static int now_stage = 0;
        public static int Max_stage;
        public static bool clear;
        private int stage_copy = -1;
        //タイマー
        DateTime startDT = DateTime.Now;
        //サイズ調整用
        private const float scale = 1f;
        //ペン
        Pen pen1 = new Pen(Color.Black, 10);
        Pen pen2 = new Pen(Color.White, 10);
        //描画用ポイント
        Point[] p1,p2,p3,p4;
        //カーソル
        Cursor HG, AR, SG, SR, KN;
        //カーソル用ポイント
        Point sp, cp;
        //保存用
        private int now_score;
        private int high_score;
        private String high_string;
        public struct Bullet
        {
            public int stage;
            public int x;
            public int y;
            public String type;
            public int amount;
            public Image img;
        }
        public struct Heal_item
        {
            public int stage;
            public int x;
            public int y;
            public int amount;
            public Image img;
        }
        public struct Weapon
        {
            public int stage;
            public int x;
            public int y;
            public String type;
            public Image img;
        }
        public struct Enemy_data
        {
            public int stage;
            public int x;
            public int y;
            public int health;
            public int speed;
            public float angle;
            public String type;
        }
        public struct Door_data
        {
            public int stage;
            public Rectangle rect;
        }
        //これはッペンです！！(ブラシ)
        public SolidBrush[] hokusai = new SolidBrush[256];
        //フェード
        public static bool fadeout,fadein;
        private int time = 0;
        private bool dead;

        //ステージにいる敵数
        public static int stage_enemy, stage_All_enemy;
        //画像
        private Image charaBmp,
            Pistol_icon, Assault_icon, Shotgun_icon, Knife_icon, Sniper_icon, Health_icon, 
            Pistol_b_icon, Assault_b_icon, Shotgun_b_icon, Sniper_b_icon,
            Knife_B_icon, Pistol_B_icon, Assault_B_icon, Shotgun_B_icon, Sniper_B_icon;

        private void death_b_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void Main_MouseUp(object sender, MouseEventArgs e)
        {
            key.Mouse_up(e);
        }
        private void Main_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) key.Mouse_down(e);
        }
        private void TimeTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan timeSpan = DateTime.Now - startDT;
            TimerLabel.Text = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 10);
        }
        public Main()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {//初期化
            Setup();
            Input();
            CharaAdd();
            StageMake();
            UI_Arrangement();
            Max_stage = All_Points.Count - 1;
            GameTimer.Start();
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {//描画
            Graphics g = e.Graphics;

            g.ResetTransform();
            //障害物を塗りつぶす
            foreach (Rectangle rect in All_Rect[now_stage])
                g.FillRectangle(Brushes.Blue, rect);
            //範囲線の表示
            //foreach (Rectangle rect in All_Rect_ex[now_stage])
            //    g.DrawRectangle(Pens.Red, rect);
            //点を視覚化
            if(Key.grid)foreach (Point point in All_Points[now_stage])
                g.DrawLine(Pens.Black, point.X, point.Y, point.X+5, point.Y+5);

            DrawItem(g);
            player.Draw(g);
            foreach (Enemy enemies in Enemies) enemies.Draw(g);
            
            DrawUI(g);
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {//押された
            key.Push(e);
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {//離れた
            key.Release(e);
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {//Tick
            //画面座標でマウスポインタの位置を取得する
            sp = Cursor.Position;
            //画面座標をクライアント座標に変換する
            cp = this.PointToClient(sp);

            //ステージ変更を検知
            if (stage_copy != now_stage) StageChange();
            //キャラクター動作
            if((!fadeout)&&(!fadein)) player.Tick(cp, Enemies);
            if ((!fadeout)&&(!fadein))
            {
                for(int i = Enemies.Count - 1;i >= 0; i--)
                {
                    Enemies[i].Tick(player.center,player);
                    if (Enemies[i].dead)
                    {
                        Enemies.RemoveAt(i);
                        stage_enemy--;
                    }
                }
            }

            CursorChange();
            Fade();
                
            this.Invalidate();
        }
        private void Fade()
        {
            if (fadeout)
            {
                time += 8;
                if (time > 255)
                {
                    time = 255;
                    if (!player.dead)
                    {
                        fadein = true;
                        fadeout = false;
                        now_stage++;
                    }
                    else
                    {
                        dead = true;
                        this.Cursor = Cursors.Default;
                    } 
                }
            }
            else if (fadein)
            {
                time -= 8;
                if (time < 0)
                {
                    time = 0;
                    fadein = false;
                    fadeout = false;
                }
            }

        }
        private void CharaAdd()
        {//読み込み
            charaBmp = Properties.Resources.Weapon_img;
            Pistol_icon = Division(72, 72, 4, 4, 0.5f);
            Assault_icon = Division(72, 72, 77, 4, 0.5f);
            Shotgun_icon = Division(72, 72, 151, 4, 0.5f);
            Sniper_icon = Division(72, 72, 224, 4, 0.5f);
            Knife_icon = Division(72, 72, 297, 4, 0.5f);
            Pistol_b_icon = Division(72, 72, 4, 80, 0.5f);
            Assault_b_icon = Division(72, 72, 77, 80, 0.5f);
            Shotgun_b_icon = Division(72, 72, 151, 80, 0.5f);
            Sniper_b_icon = Division(72, 72, 224, 80, 0.5f);
            Health_icon = Division(72, 72, 297, 80, 0.5f);
            Knife_B_icon = Division(160,40,830,30, 1);
            Pistol_B_icon = Division(145,95,385,10, 1);
            Assault_B_icon = Division(200,80,610,10, 1);
            Shotgun_B_icon = Division(190,50,385,130, 1);
            Sniper_B_icon = Division(200,50,600,120, 1);


            //x/y/speed/angle/health/List
            player = new Player(500, 800, 5, 0,100);
        }
        private void Input()
        {
            String texts,one_text = null;
            char[] element;
            Rectangle[] rect;
            List<int> Rect_element,Point_element,Int_element;
            List<Point> point;
            int[,] table;

            try
            {
                //All_Rectを開く
                using (StreamReader sr = new StreamReader(@"S:\jg2プログラミング技術\R4\山\Text\All_Rect.txt"))
                {
                    //行があるか？(無かったら-1)
                    while (0 <= sr.Peek())
                    {
                        texts = null;
                        one_text = null;
                        element = null;
                        Rect_element = new List<int>();

                        //1行取得
                        texts = sr.ReadLine();
                        //textsを文字列に変換
                        element = texts.ToCharArray();
                        //1行を1文字ずつ見る
                        foreach(char txt in element)
                        {
                            //txtが文字でないなら
                            if(txt != ',')
                            {
                                //one_textに追加していく
                                one_text += txt;
                            }
                            //','に当たったら
                            else
                            {
                                //それを1つの数字として格納
                                Rect_element.Add(int.Parse(one_text));
                                one_text = null;
                            }
                        }
                        //rectangle構造体配列を作る
                        rect = new Rectangle[Rect_element.Count / 4];
                        for (int i = 0; i < rect.Length; i++)
                        {
                            rect[i] = new Rectangle(Rect_element[4*i+0], Rect_element[4*i+1], Rect_element[4*i+2], Rect_element[4*i+3]);
                        }
                        All_Rect.Add(rect);
                    }
                }
                Console.WriteLine("All_Rect Input Completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                //All_Rect_exを開く
                using (StreamReader sr = new StreamReader(@"S:\jg2プログラミング技術\R4\山\Text\All_Rect_ex.txt"))
                {
                    //行があるか？(無かったら-1)
                    while (0 <= sr.Peek())
                    {
                        texts = null;
                        one_text = null;
                        element = null;
                        Rect_element = new List<int>();

                        //1行取得
                        texts = sr.ReadLine();
                        //textsを文字列に変換
                        element = texts.ToCharArray();
                        //1行を1文字ずつ見る
                        foreach (char txt in element)
                        {
                            //txtが文字でないなら
                            if (txt != ',')
                            {
                                //one_textに追加していく
                                one_text += txt;
                            }
                            //','に当たったら
                            else
                            {
                                //それを1つの数字として格納
                                Rect_element.Add(int.Parse(one_text));
                                one_text = null;
                            }
                        }
                        //rectangle構造体配列を作る
                        rect = new Rectangle[Rect_element.Count / 4];
                        for (int i = 0; i < rect.Length; i++)
                        {
                            rect[i] = new Rectangle(Rect_element[4*i+0], Rect_element[4*i+1], Rect_element[4*i+2], Rect_element[4*i+3]);
                        }
                        All_Rect_ex.Add(rect);
                    }
                }
                Console.WriteLine("All_Rect_ex Input Completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                //All_Pointsを開く
                using (StreamReader sr = new StreamReader(@"S:\jg2プログラミング技術\R4\山\Text\All_Points.txt"))
                {
                    //行があるか？(無かったら-1)
                    while (0 <= sr.Peek())
                    {
                        texts = null;
                        one_text = null;
                        element = null;
                        Point_element = new List<int>();

                        //1行取得
                        texts = sr.ReadLine();
                        //textsを文字列に変換
                        element = texts.ToCharArray();
                        //1行を1文字ずつ見る
                        foreach (char txt in element)
                        {
                            //txtが文字でないなら
                            if (txt != ',')
                            {
                                //one_textに追加していく
                                one_text += txt;
                            }
                            //','に当たったら
                            else
                            {
                                //それを1つの数字として格納
                                Point_element.Add(int.Parse(one_text));
                                one_text = null;
                            }
                        }
                        //Point構造体を作る
                        point = new List<Point>(Point_element.Count / 2);
                        for (int i = 0; i < Point_element.Count / 2; i++)
                        {
                            point.Add(new Point(Point_element[2*i+0], Point_element[2*i+1]));
                        }
                        All_Points.Add(point);
                    }
                }
                Console.WriteLine("All_Points Input Completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                //All_Tableを開く
                using (StreamReader sr = new StreamReader(@"S:\jg2プログラミング技術\R4\山\Text\All_Table.txt"))
                {
                    //行があるか？(無かったら-1)
                    while (0 <= sr.Peek())
                    {
                        texts = null;
                        one_text = null;
                        element = null;
                        Int_element = new List<int>();

                        //1行取得
                        texts = sr.ReadLine();
                        //textsを文字列に変換
                        element = texts.ToCharArray();
                        //1行を1文字ずつ見る
                        foreach (char txt in element)
                        {
                            //txtが文字でないなら
                            if (txt != ',')
                            {
                                //one_textに追加していく
                                one_text += txt;
                            }
                            //','に当たったら
                            else
                            {
                                //それを1つの数字として格納
                                Int_element.Add(int.Parse(one_text));
                                one_text = null;
                            }
                        }
                        //表を作る
                        table = new int[(int)Math.Sqrt(Int_element.Count), (int)Math.Sqrt(Int_element.Count)];
                        for(int y = 0;y < table.GetLength(1); y++)
                        {
                            for(int x = 0;x < table.GetLength(1); x++)
                            {
                                table[x, y] = Int_element[table.GetLength(1)*y+x];
                            }
                        }
                        All_Table.Add(table);
                    }
                }
                Console.WriteLine("All_Table Input Completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private Bitmap Division(int width, int height, int x, int y,float scale)
        {//画像切り出し
            Rectangle sourceRectange = new Rectangle(new Point(x, y), new Size(width, height));
            Bitmap bitmap1 = new Bitmap((int)(width * scale), (int)(height * scale));
            Graphics graphics = Graphics.FromImage(bitmap1);
            graphics.DrawImage(charaBmp, new Rectangle(0, 0, (int)(width * scale), (int)(height * scale)), sourceRectange, GraphicsUnit.Pixel);
            graphics.Dispose();
            return bitmap1;
        }
        private void DrawUI(Graphics g)
        {
            g.ResetTransform();
            g.FillRectangle(Brushes.DarkSlateGray,ClientRectangle.X,ClientRectangle.Bottom-300,ClientRectangle.Right,ClientRectangle.Bottom);
            g.DrawRectangle(pen1, ClientRectangle.X, ClientRectangle.Bottom-300, ClientRectangle.Right, ClientRectangle.Bottom);
            g.DrawLine(pen1, (ClientRectangle.Width / 5)*4, ClientRectangle.Bottom-300, (ClientRectangle.Width / 5)*4, ClientRectangle.Bottom);
            g.FillRectangle(Brushes.DarkRed, (ClientRectangle.Width / 2) - (TimerLabel.Size.Width / 2) + (int)(60*scale), ClientRectangle.Bottom - 230, 
                ((ClientRectangle.Width / 2) + (TimerLabel.Size.Width / 2) - (int)(30*scale)) - ((ClientRectangle.Width / 2) - (TimerLabel.Size.Width / 2) + (int)(60*scale)), 50);
            g.FillRectangle(Brushes.LightGreen, (ClientRectangle.Width / 2) - (TimerLabel.Size.Width / 2) + (int)(60*scale), ClientRectangle.Bottom - 230,
                (int)((((ClientRectangle.Width / 2) + (TimerLabel.Size.Width / 2) - (int)(30*scale)) - ((ClientRectangle.Width / 2) - (TimerLabel.Size.Width / 2) + (int)(60*scale))) * ((float)player.health / (float)player.max_health)), 50);
            g.DrawPolygon(pen1, p1);
            g.DrawPolygon(pen1, p2);
            g.DrawPolygon(pen1, p3);
            g.DrawPolygon(pen1, p4);
            g.DrawImage(Knife_icon, ClientRectangle.X + (int)(10*scale), ClientRectangle.Bottom - (int)(100*scale));
            if (Key.hg)
            {
                g.DrawImage(Pistol_icon, ClientRectangle.X + (int)(30*scale) + 20, ClientRectangle.Bottom - (int)(100*scale));
                D1.Text = "1";
            }
            if (Key.sg)
            {
                g.DrawImage(Shotgun_icon, ClientRectangle.X +(int)(30 * scale) + 60, ClientRectangle.Bottom - (int)(100 * scale));
                D2.Text = "2";
            }            
            if (Key.ar)
            {
                g.DrawImage(Assault_icon, ClientRectangle.X + (int)(30*scale) + 100, ClientRectangle.Bottom - (int)(100*scale));
                D3.Text = "3";
            }
            if (Key.sr)
            {
                g.DrawImage(Sniper_icon, ClientRectangle.X +(int)(30 * scale) + 140, ClientRectangle.Bottom - (int)(100 * scale));
                D4.Text = "4";
            }
            switch (Key.weapon)
            {
                case "knife":
                    g.DrawImage(Knife_B_icon,ClientRectangle.X + 30,ClientRectangle.Bottom - 250);
                    amonum.Text = "- / -";
                    break;
                case "handgun":
                    g.DrawImage(Pistol_B_icon, ClientRectangle.X + 30, ClientRectangle.Bottom - 270);
                    amonum.Text = $"{player.hg_mag}/{player.hg_b}";
                    break;
                case "shotgun":
                    g.DrawImage(Shotgun_B_icon, ClientRectangle.X + 10, ClientRectangle.Bottom - 250);
                    amonum.Text = $"{player.sg_mag}/{player.sg_b}";
                    break;
                case "carbine":
                    g.DrawImage(Assault_B_icon, ClientRectangle.X + 10, ClientRectangle.Bottom - 270);
                    amonum.Text = $"{player.ar_mag}/{player.ar_b}";
                    break;
                case "sniper":
                    g.DrawImage(Sniper_B_icon, ClientRectangle.X + 10, ClientRectangle.Bottom - 250);
                    amonum.Text = $"{player.sr_mag}/{player.sr_b}";
                    break;
            }

            HPnum.Text = player.health.ToString();
            Enum.Text = $"{stage_enemy} / {stage_All_enemy}";
            if ((fadeout)||(fadein)) g.FillRectangle(hokusai[time],0,0,ClientRectangle.Right,ClientRectangle.Bottom - 300);

            if (dead)
            {
                death.Visible = true;
                death_b.Visible = true;
            }
            if (clear)
            {
                clear_l.Visible = true;
                praise.Visible = true;
                time1.Visible = true;
                time2.Visible = true;
                death_b.Visible = true;
            }
        }
        private void DrawItem(Graphics g)
        {
            g.ResetTransform();
            foreach (Bullet bullet_item in All_Bullet) if(bullet_item.stage == now_stage)g.DrawImage(bullet_item.img, bullet_item.x, bullet_item.y);
            foreach (Heal_item health_item in All_heal) if (health_item.stage == now_stage) g.DrawImage(health_item.img, health_item.x, health_item.y);
            foreach (Weapon weapon_item in All_weapon) if (weapon_item.stage == now_stage) g.DrawImage(weapon_item.img, weapon_item.x, weapon_item.y);
            for(int i = 0;i < Door_In.Count;i++)
            {
                if (Door_In[i].stage == now_stage)
                {
                    //全員倒したら
                    if (stage_enemy == 0) g.FillRectangle(Brushes.Green, Door_In[i].rect);
                    else g.FillRectangle(Brushes.Red, Door_In[i].rect);
                }
            }
            foreach (Door_data door in Door_Out) if (door.stage == now_stage) g.FillRectangle(Brushes.Red, door.rect);
        }
        private void UI_Arrangement()
        {
            TimerLabel.Location = new Point((ClientRectangle.Width / 2 - TimerLabel.Size.Width / 2) + 100, ClientRectangle.Bottom - TimerLabel.Size.Height + 10);
            Emark.Location = new Point((ClientRectangle.Width / 5)*4 + 30, ClientRectangle.Bottom - 280);
            Enum.Location = new Point((ClientRectangle.Width / 5)*4 + 50, ClientRectangle.Bottom - 220);
            Smark.Location = new Point((ClientRectangle.Width / 5)*4 + 20, ClientRectangle.Bottom - 150);
            Snum.Location = new Point((ClientRectangle.Width / 5)*4 + 40, ClientRectangle.Bottom - 80);
            HPmark.Location = new Point((ClientRectangle.Width / 2) - (TimerLabel.Size.Width / 2) - 100, ClientRectangle.Bottom - 240);
            HPnum.Location = new Point((ClientRectangle.Width / 2) + (TimerLabel.Size.Width / 2), ClientRectangle.Bottom - 240);
            Highmark.Location = new Point(ClientRectangle.Width / 2 - Highmark.Width, ClientRectangle.Bottom - (TimerLabel.Height + Highscore.Height) + 10);
            Highscore.Location = new Point(ClientRectangle.Width / 2, ClientRectangle.Bottom - (TimerLabel.Height + Highscore.Height) + 10);
            try
            {
                //All_Rectを開く
                using (StreamReader sr = new StreamReader(@"S:\jg2プログラミング技術\R4\山\Text\Highscore.txt")) {
                    high_score = int.Parse(sr.ReadLine());
                    high_string = sr.ReadLine();
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Highscore.Text = high_string;

            Q.Location = new Point(ClientRectangle.X  , ClientRectangle.Bottom - 50);
            D1.Location = new Point(ClientRectangle.X + 40, ClientRectangle.Bottom - 50);
            D1.Text = "";
            D2.Location = new Point(ClientRectangle.X + 80, ClientRectangle.Bottom - 50);
            D2.Text = "";
            D3.Location = new Point(ClientRectangle.X + 120, ClientRectangle.Bottom - 50);
            D3.Text = "";
            D4.Location = new Point(ClientRectangle.X + 160, ClientRectangle.Bottom - 50);
            D4.Text = "";
            amonum.Location = new Point(ClientRectangle.X + 180, ClientRectangle.Bottom - 200);
            death.Visible = false;
            death_b.Visible = false;
            clear_l.Visible = false;
            praise.Visible = false;
            time1.Visible = false;
            time2.Visible = false;


            //武器表示のところ
            p1 = new Point[4];
            p1[0] = new Point(ClientRectangle.X, ClientRectangle.Bottom-300);
            p1[1] = new Point(ClientRectangle.Width / 5 + 50, ClientRectangle.Bottom-300);
            p1[2] = new Point(ClientRectangle.Width / 5 - ClientRectangle.Width / 10 + 50, ClientRectangle.Bottom-150);
            p1[3] = new Point(ClientRectangle.X, ClientRectangle.Bottom-150);
            //タイマー表示のところ
            p2 = new Point[4];
            p2[0] = new Point((ClientRectangle.Width / 2) - (TimerLabel.Size.Width / 2), ClientRectangle.Bottom - 80);
            p2[1] = new Point((ClientRectangle.Width / 2) + (TimerLabel.Size.Width / 2), ClientRectangle.Bottom - 80);
            p2[2] = new Point(((ClientRectangle.Width / 2) + (TimerLabel.Size.Width / 2)) + (int)(70*scale), ClientRectangle.Bottom);
            p2[3] = new Point(((ClientRectangle.Width / 2) - (TimerLabel.Size.Width / 2)) - (int)(70*scale), ClientRectangle.Bottom);
            //体力左
            p3 = new Point[4];
            p3[0] = new Point((ClientRectangle.Width / 2) - (TimerLabel.Size.Width / 2) + (int)(30*scale), ClientRectangle.Bottom - 230);
            p3[1] = new Point((ClientRectangle.Width / 2) - (TimerLabel.Size.Width / 2) + (int)(60*scale), ClientRectangle.Bottom - 250);
            p3[2] = new Point((ClientRectangle.Width / 2) - (TimerLabel.Size.Width / 2) + (int)(60*scale), ClientRectangle.Bottom - 160);
            p3[3] = new Point((ClientRectangle.Width / 2) - (TimerLabel.Size.Width / 2) + (int)(30*scale), ClientRectangle.Bottom - 180);
            //体力右
            p4 = new Point[4];
            p4[0] = new Point((ClientRectangle.Width / 2) + (TimerLabel.Size.Width / 2) - (int)(30*scale), ClientRectangle.Bottom - 250);
            p4[1] = new Point((ClientRectangle.Width / 2) + (TimerLabel.Size.Width / 2), ClientRectangle.Bottom - 230);
            p4[2] = new Point((ClientRectangle.Width / 2) + (TimerLabel.Size.Width / 2), ClientRectangle.Bottom - 180);
            p4[3] = new Point((ClientRectangle.Width / 2) + (TimerLabel.Size.Width / 2) - (int)(30*scale), ClientRectangle.Bottom - 160);

        }
        private void Setup()
        {
            this.ClientSize = new Size((int)(1300*scale), (int)(1300*scale));
            //自分自身のフォームを最大化
            //this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
            BackColor = Color.White;
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            HG = new Cursor(asm.GetManifestResourceStream(asm.GetName().Name + ".HG.cur"));
            AR = new Cursor(asm.GetManifestResourceStream(asm.GetName().Name + ".AR.cur"));
            SG = new Cursor(asm.GetManifestResourceStream(asm.GetName().Name + ".SG.cur"));
            SR = new Cursor(asm.GetManifestResourceStream(asm.GetName().Name + ".SR.cur"));
            KN = new Cursor(asm.GetManifestResourceStream(asm.GetName().Name + ".KN.cur"));
            for (int i = 0; i < 256; i++) hokusai[i] = new SolidBrush(Color.FromArgb(i, 0, 0, 0));
            GameTimer.Interval = 1000 / 60;
            TimeTimer.Interval = 1000 / 60;
        }
        private void CursorChange()
        {
            switch (Key.weapon)
            {
                case "knife":
                    this.Cursor = KN;
                    break;
                case "handgun":
                    this.Cursor = HG;
                    break;
                case "shotgun":
                    this.Cursor = SG;
                    break;
                case "carbine":
                    this.Cursor = AR;
                    break;
                case "sniper":
                    this.Cursor = SR;
                    break;
            }
        }
        private void StageChange()
        {
            if (now_stage == Max_stage) clear = true;
            foreach(Enemy_data e_data in All_Enemy)
            {
                if (e_data.stage == now_stage)
                {
                    Enemies.Add(enemy = new Enemy(e_data.x, e_data.y, e_data.speed, e_data.angle, e_data.health, e_data.type, All_Points[now_stage], All_Table[now_stage]));
                    stage_enemy++;
                    stage_All_enemy++;
                }
            }
            stage_copy = now_stage;
            if(now_stage == 1)
            {
                startDT = DateTime.Now;             
                TimerLabel.Location = new Point((ClientRectangle.Width / 2 - TimerLabel.Size.Width / 2)+ 60, ClientRectangle.Bottom - TimerLabel.Size.Height + 10);
                TimeTimer.Start();
            }
            if (clear)
            {
                
                TimeTimer.Stop();

                String my_s;
                my_s = TimerLabel.Text;
                now_score = 0;
                //hours
                now_score += int.Parse(my_s.Substring(0, 2)) * 3600000;
                //minutes
                now_score += int.Parse(my_s.Substring(3, 2)) * 60000;
                //seconds
                now_score += int.Parse(my_s.Substring(6, 2)) * 1000;
                //ミリ秒
                now_score += int.Parse(my_s.Substring(9, 2));
                
                //ハイスコア更新なら
                if (now_score < high_score)
                {
                    time1.Text = $"新記録 : {TimerLabel.Text}";
                    time2.Text = $"前ハイスコア : {high_string}";
                    //上書き
                    high_score = now_score;
                    high_string = TimerLabel.Text;
                }
                else
                {
                    time1.Text = $"ハイスコア : {high_string}";
                    time2.Text = $"あなたのスコア : {TimerLabel.Text}";
                }

                try
                {
                    //High_scoreを出力(上書き)
                    using (StreamWriter sw = new StreamWriter(@"S:\jg2プログラミング技術\R4\山\Text\Highscore.txt", false, Encoding.UTF8))
                    {
                        sw.WriteLine(high_score);
                        sw.Write(high_string);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            Snum.Text = $"{now_stage} / {Max_stage}";
            foreach (Door_data door in Door_Out) if (door.stage == now_stage)
                {
                    player.my.X = door.rect.X + door.rect.Width / 2;
                    player.my.Y = door.rect.Y + door.rect.Height / 2;
                }
        }
        private void StageMake()
        {
            //stage,x,y,amotype,amount,img
            MakeBullet(0,100,100,"hg_b",10,Pistol_b_icon);
            MakeBullet(2, 300, 100, "sr_b", 10, Sniper_b_icon);
            //stage,x,y,amount
            MakeHealth_item(0,80,80,60);
            //stage,x,y,weapontype,img
            MakeWeapon(0,400,300,"ar",Assault_icon);
            MakeWeapon(0, 500, 300, "sg", Shotgun_icon);
            MakeWeapon(0, 550, 300, "hg", Pistol_icon);
            MakeWeapon(0, 700, 300, "sr", Sniper_icon);

            //stage,x,y,health,speed,angle,type
            MakeEnemy(0,500,500,50,3,0,"gun");

            //stage,x,y,type(in->playerが入っていける)
            MakeDoor(0,350,300,"in");
            MakeDoor(1, 250, 600, "out");
            MakeDoor(1, 500, 300, "in");
            MakeDoor(2, 100, 500, "out");
        }
        private void MakeBullet(int s,int X,int Y,String t,int a,Image i)
        {
            Bullet b1 = new Bullet()
            {
                stage = s,
                x = X,
                y = Y,
                type = t,
                amount = a,
                img = i,
            };
            All_Bullet.Add(b1);
        }
        private void MakeHealth_item(int s, int X, int Y, int a)
        {
            Heal_item h1 = new Heal_item()
            {
                stage = s,
                x = X,
                y = Y,
                amount = a,
                img = Health_icon
            };
            All_heal.Add(h1);
        }
        private void MakeWeapon(int s, int X, int Y, String t, Image i)
        {
            Weapon w1 = new Weapon()
            {
                stage = s,
                x = X,
                y = Y,
                type = t,
                img = i,
            };
            All_weapon.Add(w1);
        }
        private void MakeEnemy(int s, int X, int Y, int h,int sp, float a, String t)
        {
            Enemy_data e1 = new Enemy_data()
            {
                stage = s,
                x = X,
                y = Y,
                health = h,
                angle = a,
                speed = sp,
                type = t
            };
            All_Enemy.Add(e1);
        }
        private void MakeDoor(int s,int X,int Y,String t)
        {
            Door_data d1 = new Door_data()
            {
                stage = s,
                rect = new Rectangle(X, Y, door_width, door_height),
            };
            if (t == "in") Door_In.Add(d1);
            else Door_Out.Add(d1);
        }
    }
}
