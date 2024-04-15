using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Windows;
using System.Xml;
using System.IO;

namespace WayPoint
{
    public partial class Main : Form
    {
        private Rectangle mouse,goal;
        private Point Start, End;
        private const int interval = 30;
        private const int Wall = 100;
        public int now_stage = 0;
        //全ステージを格納するリスト
        public List<List<Point>> All_Points = new List<List<Point>>();
        //全ステージ表を格納するリスト
        public List<int[,]> All_Table = new List<int[,]>();
        //全障害物を格納するリスト
        public List<Rectangle[]> All_Rect = new List<Rectangle[]>();
        public List<Rectangle[]> All_Rect_ex = new List<Rectangle[]>();

        double shortest_my;
        double shortest_goal;
        private List<Point> shortest_route;
        private int my_p, end_p;
        private double shortest_s, shortest_e;

        public Main()
        {
            InitializeComponent();
            //使用するサイズを入力
            this.ClientSize = new Size(1300,1000);
            //自分自身のフォームを最大化
            //this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
        }
        private void Output()
        {
            try
            {
                //All_rectを出力(上書き)
                using (StreamWriter sw = new StreamWriter(@"S:\jg2プログラミング技術\R4\山\Text\All_Rect.txt", false, Encoding.UTF8))
                {
                    //ステージ回繰り返す
                    for(int a = 0;a < All_Rect.Count; a++)
                    {
                        //1ステージに入っている障害物の数だけ繰り返す
                        for(int b = 0;b < All_Rect[a].Length; b++)
                        {
                            //Rectangleを出力
                            sw.Write($"{All_Rect[a][b].X},{All_Rect[a][b].Y},{All_Rect[a][b].Width},{All_Rect[a][b].Height}");
                            //最後なら
                            if ((b == All_Rect[a].Length-1)&&(a != All_Rect.Count-1)) sw.WriteLine(",");
                            else sw.Write(",");
                        }
                    }
                    Console.WriteLine("All_Rect Output Completed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                //All_rect_exを出力(上書き)
                using (StreamWriter sw = new StreamWriter(@"S:\jg2プログラミング技術\R4\山\Text\All_Rect_ex.txt", false, Encoding.UTF8))
                {
                    //ステージ回繰り返す
                    for (int a = 0; a < All_Rect_ex.Count; a++)
                    {
                        //1ステージに入っている障害物の数だけ繰り返す
                        for (int b = 0; b < All_Rect_ex[a].Length; b++)
                        {
                            //Rectangle_exを出力
                            sw.Write($"{All_Rect_ex[a][b].X},{All_Rect_ex[a][b].Y},{All_Rect_ex[a][b].Width},{All_Rect_ex[a][b].Height}");
                            //最後なら
                            if ((b == All_Rect_ex[a].Length-1)&&(a != All_Rect_ex.Count-1)) sw.WriteLine(",");
                            else sw.Write(",");
                        }
                    }
                    Console.WriteLine("All_Rect_ex Output Completed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                //All_Pointsを出力(上書き)
                using (StreamWriter sw = new StreamWriter(@"S:\jg2プログラミング技術\R4\山\Text\All_Points.txt", false, Encoding.UTF8))
                {
                    //ステージ回繰り返す
                    for (int a = 0; a < All_Points.Count; a++)
                    {
                        //1ステージに入っているポイントの数だけ繰り返す
                       for (int b = 0; b < All_Points[a].Count; b++)
                        {
                            //Pointを出力
                            sw.Write($"{All_Points[a][b].X},{All_Points[a][b].Y}");
                            //最後なら
                            if ((b == All_Points[a].Count-1)&&(a != All_Points.Count-1)) sw.WriteLine(",");
                            else sw.Write(",");
                        }
                    }
                    Console.WriteLine("All_Points Output Completed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                //All_Tablesを出力(上書き)
                using (StreamWriter sw = new StreamWriter(@"S:\jg2プログラミング技術\R4\山\Text\All_Table.txt", false, Encoding.UTF8))
                {
                    //ステージ回繰り返す
                    for (int a = 0; a < All_Table.Count; a++)
                    {
                        for(int y = 0;y < All_Points[a].Count; y++)
                        {
                            for (int x = 0; x < All_Points[a].Count; x++)
                            {
                                //intを出力
                                sw.Write($"{All_Table[a][x,y]}");
                                //最後なら
                                if (((x == All_Points[a].Count-1)&&(y == All_Points[a].Count-1))&&(a != All_Table.Count-1)) sw.WriteLine(",");
                                else sw.Write(",");
                            }
                        }
                    }
                    Console.WriteLine($"All_Table Output Completed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            AddObject();
            MakeTable();
            SearchPoint();
            Start = new Point(60, 50);
            mouse = new Rectangle(Start.X - 10, Start.Y - 10, 20, 20);
            End = new Point(300, 150);
            goal = new Rectangle(End.X - 10, End.Y - 10, 20, 20);
            Route();
            Output();

            GameTimer.Interval = 1000/60;
            GameTimer.Start();
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            //障害物を塗りつぶす
            foreach (Rectangle rect in All_Rect[now_stage])
                g.FillRectangle(Brushes.Blue, rect);
            //範囲線の表示
            foreach (Rectangle rect in All_Rect_ex[now_stage])
                g.DrawRectangle(Pens.Red, rect);
            //点を視覚化
            foreach (Point point in All_Points[now_stage])
                g.DrawLine(Pens.Black, point.X, point.Y, point.X+2, point.Y+2);
            //マウスを視覚化
            g.FillEllipse(Brushes.Green, mouse);
            g.FillEllipse(Brushes.Red, goal);
            //線をつなげる
            for (int i = 0; i < shortest_route.Count-1; i++)
                g.DrawLine(Pens.BlueViolet, shortest_route[i], shortest_route[i+1]);
            
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            //画面座標でマウスポインタの位置を取得する
            System.Drawing.Point sp = System.Windows.Forms.Cursor.Position;
            //画面座標をクライアント座標に変換する
            System.Drawing.Point cp = this.PointToClient(sp);
            Start = cp;
            mouse = new Rectangle(Start.X - 10, Start.Y - 10, 20, 20);
            Route();
            this.Invalidate();
        }
        private void Route()
        {
            shortest_my = 100000000;
            shortest_goal = 100000000;
            for (int i = 0; i < All_Points[now_stage].Count; i++)
            {
                //より短いのが見つかったら
                shortest_s = Distance(Start, All_Points[now_stage][i]);
                if (shortest_s < shortest_my)
                {
                    shortest_my = shortest_s;
                    my_p = i;
                }
                //より短いのが見つかったら
                shortest_e = Distance(End, All_Points[now_stage][i]);
                if (shortest_e < shortest_goal)
                {
                    shortest_goal = shortest_e;
                    end_p = i;
                }
            }
            shortest_route = new List<Point>();
            //最後になるまで繰り返す
            while (my_p != end_p)
            {
                shortest_route.Add(All_Points[now_stage][my_p]);
                my_p = All_Table[now_stage][my_p, end_p];
            }
            shortest_route.Add(All_Points[now_stage][end_p]);
        }
        private double Distance(Point S,Point E)
        {
            return Math.Sqrt(Math.Pow((S.X - E.X), 2) + Math.Pow((S.Y - E.Y), 2));
        }
        private void SearchPoint()
        {
            //作成したテーブルを格納するリスト
            List<int[,]> tables;
            List<int> get_member,direct_member,index1,index2;
            int[,] table,table_sub;
            int count;
            int nullcheck;
            int loop = 0;
            int num;
            double[] sort_point_to_goal,sort_point_to_my;
            //ステージ数の数だけ繰り返す
            for (int i = 0; i < All_Rect.Count; i++)
            {
                loop = 0;
                Console.WriteLine("1周目開始");
                //1周目用テーブル作成
                table = new int[All_Points[i].Count, All_Points[i].Count];
                //テーブルの長さ(1次元)を格納
                num = table.GetLength(1);
                //全てを-2で初期化
                for (int x = 0;x < num; x++)
                {
                    for (int y = 0; y < num; y++) table[x, y] = -2;
                }
                count = 0;
                nullcheck = 0;
                //1周目（直接いけるルートを走査する）
                for (int x = 0;x < num; x++)
                {
                    nullcheck = 0;
                    for(int y = 0;y < num; y++)
                    {
                        //まだ何も入っていないなら続行
                        if (table[x,y] == -2)
                        {
                            //出発と到着が同じでないなら続行
                            if(x != y)
                            {
                                //始点→終点の線分と4つの辺の線分が交差しているか
                                //ベクトル計算?しらないねぇ
                                //https://www.nekonecode.com/math-lab/pages/collision2/line-and-rect/参照
                                if (CheckCollisionRect(i, x, y))
                                {
                                    count++;
                                    nullcheck++;
                                }
                                //全てをくぐり抜けたら
                                else
                                {
                                    table[x, y] = y;
                                    //A→Bが可能なら、B→Aも確定する
                                    table[y, x] = x;
                                }
                            }
                            else
                            {
                                //-1で自分自身であることを識別させる
                                table[x, y] = -1;
                            }
                        }
                    }
                    //縦列が全て埋まらなかった(どこからもアクセスできない)点があったときは表の作成を中断する
                    if(nullcheck == (num - 1))
                    {
                        Console.WriteLine($"Could'nt create stage{i} table due to inaccessibility of {x}");
                        break;
                    }
                }
                if (nullcheck == (num - 1)) break;
                //1周目で全て埋まっていたら
                if (count == 0)
                {
                    All_Table.Add(table);
                    Console.WriteLine($"Successfully created stage{i} table");
                }
                else
                {
                    //2周目～(全て埋まるまで)※当然、絶対にアクセスできない場所(囲まれているなど)はあってはならない
                    //1周目の表をコピー
                    tables = new List<int[,]>();
                    tables.Add(table);
                    
                    while (true)
                    {
                        count = 0;
                        //新しい周用の配列を作成
                        table_sub = new int[num, num];
                        //前の周の情報を継承
                        Array.Copy(tables[loop], table_sub, num*num);
                        Console.WriteLine($"{loop+2}周目開始");
                        for (int x = 0;x < num; x++)
                        {
                            for(int y = 0;y < num; y++)
                            {
                                //まだ何も入っていないなら続行
                                if (table_sub[x, y] == -2)
                                {
                                    get_member = new List<int>();
                                    //行きたい先(y)に言ったことがある場所を探す
                                    for(int x2 = 0;x2 < num; x2++)
                                    {
                                        //-1と-2以外の横要素を取得
                                        if (((tables[loop][x2, y] != -1) && (tables[loop][x2, y] != -2))&&(!get_member.Contains(x2))) get_member.Add(x2);
                                    }
                                    //自分から直接いけるものを抽出する
                                    direct_member = new List<int>();
                                    for (int a = 0; a < get_member.Count; a++)
                                    {
                                       for (int Y = 0; Y < num; Y++)
                                           //1周目の表から一つずつ取り出す
                                           if (table[x, Y] == get_member[a]) direct_member.Add(get_member[a]);
                                    }
                                    //取得できなかったら次の周へ
                                    if (direct_member.Count == 0)
                                    {
                                        count++;
                                    }
                                    //1つで確定したら表に追加
                                    else if(direct_member.Count == 1)
                                    {
                                        table_sub[x, y] = direct_member[0];
                                        //2周目なら、反対の場所にも当てはまる
                                        if (loop == 0) table_sub[y, x] = direct_member[0];
                                    }
                                    //距離が近い順に並び替える
                                    else
                                    {
                                        sort_point_to_goal = new double[direct_member.Count];
                                        index1 = new List<int>();
                                        //候補ポイントと到着点の距離を比較
                                        for(int b = 0;b < direct_member.Count; b++)
                                        {
                                            sort_point_to_goal[b] = Math.Sqrt((Math.Pow(All_Points[i][y].X - All_Points[i][direct_member[b]].X, 2) + Math.Pow(All_Points[i][y].Y - All_Points[i][direct_member[b]].Y, 2)));
                                        }
                                        for (int b = 0; b < direct_member.Count; b++)
                                        {
                                            //最短距離なら添え字を追加
                                            if (sort_point_to_goal.Min() == sort_point_to_goal[b]) index1.Add(b);
                                        }
                                        //最短が一つなら
                                        if(index1.Count == 1)
                                        {
                                            table_sub[x, y] = direct_member[index1[0]];
                                            //2周目なら、反対の場所にも当てはまる
                                            if (loop == 0) table_sub[y, x] = direct_member[index1[0]];
                                        }
                                        //候補ー到着点間で複数最短があったなら
                                        else
                                        {
                                            sort_point_to_my = new double[index1.Count];
                                            index2 = new List<int>();
                                            //候補ポイントと自分の距離を比較
                                            for (int b = 0; b < index1.Count; b++)
                                            {
                                                sort_point_to_my[b] = Math.Sqrt((Math.Pow(All_Points[i][x].X - All_Points[i][direct_member[index1[b]]].X, 2) + Math.Pow(All_Points[i][x].Y - All_Points[i][direct_member[index1[b]]].Y, 2)));
                                            }
                                            for (int b = 0; b < index1.Count; b++)
                                            {
                                                //最短距離なら添え字を追加
                                                if (sort_point_to_my.Min() == sort_point_to_my[b]) index2.Add(b);
                                            }
                                            //最短が出たなら
                                            if (index2.Count == 1)
                                            {
                                                table_sub[x, y] = direct_member[index2[0]];
                                                //2周目なら、反対の場所にも当てはまる
                                                if (loop == 0) table_sub[y, x] = direct_member[index2[0]];
                                            }
                                            else
                                            {
                                                //ランダムに決める
                                                Random r = new Random();
                                                table_sub[x, y] = direct_member[index2[r.Next(0,index2.Count)]];
                                                //2周目なら、反対の場所にも当てはまる
                                                if (loop == 0) table_sub[y, x] = direct_member[index2[r.Next(0, index2.Count)]];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        tables.Add(table_sub);
                        loop++;
                        //全て埋まったら
                        if (count == 0)
                        {
                            All_Table.Add(table_sub);
                            Console.WriteLine($"Successfully created stage{i} table");
                            break;
                        }
                    }
                }
            }
        }
        private bool CheckCollisionRect(int i, int x, int y)
        {
            Rectangle[] rect = All_Rect_ex[i];
            Point Start,End;
            List<Point> points;
            Start = All_Points[i][x];
            End = All_Points[i][y];

            //対象ステージの障害物を走査する
            for (int b = 0;b < rect.Length; b++)
            {
                //壁には絶対当たらない
                if (b > 3)
                {
                    points = new List<Point>();
                    //左上
                    points.Add(new Point(rect[b].X, rect[b].Y));
                    //右上
                    points.Add(new Point(rect[b].X + rect[b].Width, rect[b].Y));
                    //右下
                    points.Add(new Point(rect[b].X + rect[b].Width, rect[b].Y + rect[b].Height));
                    //左下
                    points.Add(new Point(rect[b].X, rect[b].Y + rect[b].Height));
                    //左上
                    points.Add(new Point(rect[b].X, rect[b].Y));

                    if (coli(Start, End, points[0], points[1])) return true;
                    else if (coli(Start, End, points[1], points[2])) return true;
                    else if (coli(Start, End, points[2], points[3])) return true;
                    else if (coli(Start, End, points[3], points[0])) return true;
                }
            }
            //全てに当たらなかったなら
            return false;
        }
        private bool coli(Point a,Point b,Point c,Point d)
        {
            double s, t;
            s = (a.X - b.X) * (c.Y - a.Y) - (a.Y - b.Y) * (c.X - a.X);
            t = (a.X - b.X) * (d.Y - a.Y) - (a.Y - b.Y) * (d.X - a.X);
            if (s * t > 0) return false;

            s = (c.X - d.X) * (a.Y - c.Y) - (c.Y - d.Y) * (a.X - c.X);
            t = (c.X - d.X) * (b.Y - c.Y) - (c.Y - d.Y) * (b.X - c.X);
            if (s * t > 0) return false;
            return true;
        }
        private void MakeTable()
        {
            //縦横それぞれにいくつ点を打つか
            int interval_x = (int)(ClientRectangle.Width / interval) + 1 ;
            int interval_y = (int)(ClientRectangle.Height / interval) + 1 ;
            //ステージ数の数だけ繰り返す
            for (int i = 0;i < All_Rect.Count; i++)
            {
                List<Point> point = new List<Point>();
                Point p;
                int count = 0;

                for(int a = 0; a < interval_y; a++)
                {
                    for(int b = 0;b < interval_x; b++)
                    {
                        //Pointに座標を代入
                        p = new Point(b * interval, a * interval);

                        //障害物の数だけ繰り返す
                        //もし障害物にめり込んでいなかったらカウントを増やす（1つでもめり込めばアウト→breakで抜ける）
                            count = 0;
                            foreach (Rectangle rect in All_Rect_ex[i])
                                if ((((rect.X <= p.X)&&(rect.Y <= p.Y))&&((rect.Right >= p.X)&&(rect.Bottom >= p.Y)))) break; else count++;
                            //もしすべての障害物をくぐり抜けたら点を作成する
                            if (count == All_Rect_ex[i].Length)  point.Add(p);
                    }
                }
                All_Points.Add(point);
                Console.WriteLine($"stage{i} has {All_Points[i].Count} points");
            }
        }
        private void AddObject()
        {
            //ステージ1の障害物(矩形と円どちらも作成しておくこと)
            Rectangle[] rect1 =
            {
                new Rectangle(0,0,ClientRectangle.Width,Wall),
                new Rectangle(0,0,Wall,ClientRectangle.Height),
                new Rectangle(0,ClientRectangle.Height - Wall,ClientRectangle.Width,Wall),
                new Rectangle(ClientRectangle.Width - Wall,0,Wall,ClientRectangle.Height),
                
                new Rectangle(100,100,1200,400),               
            };
            Rectangle[] rect2 =
            {
                new Rectangle(0,0,ClientRectangle.Width,Wall),
                new Rectangle(0,0,Wall,ClientRectangle.Height),
                new Rectangle(0,ClientRectangle.Height - Wall,ClientRectangle.Width,Wall),
                new Rectangle(ClientRectangle.Width - Wall,0,Wall,ClientRectangle.Height),
                
                new Rectangle(500,300,300,300),
            };
            Rectangle[] rect3 =
            {
                new Rectangle(0,0,ClientRectangle.Width,Wall),
                new Rectangle(0,0,Wall,ClientRectangle.Height),
                new Rectangle(0,ClientRectangle.Height - Wall,ClientRectangle.Width,Wall),
                new Rectangle(ClientRectangle.Width - Wall,0,Wall,ClientRectangle.Height),
                
                new Rectangle(300,300,300,400),
                new Rectangle(600,500,600,200),
            };
            Rectangle[] rect4 =
            {
                new Rectangle(0,0,ClientRectangle.Width,Wall),
                new Rectangle(0,0,Wall,ClientRectangle.Height),
                new Rectangle(0,ClientRectangle.Height - Wall,ClientRectangle.Width,Wall),
                new Rectangle(ClientRectangle.Width - Wall,0,Wall,ClientRectangle.Height),
                
                new Rectangle(300,300,200,200),
                new Rectangle(800,500,200,200),
            };
            Rectangle[] rect5 =
            {
                new Rectangle(0,0,ClientRectangle.Width,Wall),
                new Rectangle(0,0,Wall,ClientRectangle.Height),
                new Rectangle(0,ClientRectangle.Height - Wall,ClientRectangle.Width,Wall),
                new Rectangle(ClientRectangle.Width - Wall,0,Wall,ClientRectangle.Height),
                
                new Rectangle(300,300,200,600),
                new Rectangle(700,100,200,600),
            };
            Rectangle[] rect6 =
            {
                new Rectangle(0,0,ClientRectangle.Width,Wall),
                new Rectangle(0,0,Wall,ClientRectangle.Height),
                new Rectangle(0,ClientRectangle.Height - Wall,ClientRectangle.Width,Wall),
                new Rectangle(ClientRectangle.Width - Wall,0,Wall,ClientRectangle.Height),
                
                new Rectangle(100,100,100,300),
                new Rectangle(200,100,100,200),
                new Rectangle(300,100,100,100),
                
                new Rectangle(900,100,100,100),
                new Rectangle(1000,100,100,200),
                new Rectangle(1100,100,100,300),

                new Rectangle(100,600,100,300),
                new Rectangle(200,700,100,200),
                new Rectangle(300,800,100,100),
            };
            Rectangle[] rect7 =
            {
                new Rectangle(0,0,ClientRectangle.Width,Wall),
                new Rectangle(0,0,Wall,ClientRectangle.Height),
                new Rectangle(0,ClientRectangle.Height - Wall,ClientRectangle.Width,Wall),
                new Rectangle(ClientRectangle.Width - Wall,0,Wall,ClientRectangle.Height),
                
            };
            //作成したらここに追加
            All_Rect.Add(rect1);
            All_Rect.Add(rect2);
            All_Rect.Add(rect3);
            All_Rect.Add(rect4);
            All_Rect.Add(rect5);
            All_Rect.Add(rect6);
            All_Rect.Add(rect7);

            //Allrect内のすべてを拡張する(expansion)敵の半径を指定
            Rect_ex(50, rect1);
            Rect_ex(50, rect2);
            Rect_ex(50, rect3);
            Rect_ex(50, rect4);
            Rect_ex(50, rect5);
            Rect_ex(50, rect6);
            Rect_ex(50, rect7);
        }

        private void Main_MouseClick(object sender, MouseEventArgs e)
        {
            End = Start;
            goal = new Rectangle(End.X - 10, End.Y - 10, 20, 20);
        }

        private void Rect_ex(int Enemy_r, Rectangle[] rect)
        {
            Rectangle[] rect_ex = new Rectangle[rect.Length];
            for(int i = 0;i < rect.Length; i++)
            {
                rect_ex[i] = new Rectangle(rect[i].X - Enemy_r, rect[i].Y - Enemy_r, rect[i].Width + Enemy_r * 2, rect[i].Height + Enemy_r * 2);
            }
            All_Rect_ex.Add(rect_ex);
        }
    }
}
