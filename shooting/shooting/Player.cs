using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using static System.Windows.Forms.LinkLabel;
using static shooting.Main;
using System.Runtime.Remoting.Services;
using System.Threading;

namespace shooting
{
    internal class Player
    {
        //リスト
        private List<clsBullet_P> Bullets_P = new List<clsBullet_P>();
        private List<Image[]> Shot_Ani = new List<Image[]>();
        private List<Image[]> Reload_Ani = new List<Image[]>();
        private List<Image[]> Change_Ani = new List<Image[]>();
        //インスタンス作成
        private Move move = new Move();
        private clsBullet_P shot;
        //座標
        public PointF my, center;
        //速度
        public int speed;
        //回転角度
        public float angle;
        //死んだか
        public bool dead;
        //画像
        public Image img,m_b,s_b,death;
        //体力
        public int health, max_health, before_health;
        //弾
        public int hg_b, hg_mag, ar_b, ar_mag, sg_b, sg_mag, sr_b, sr_mag;
        //武器の記録
        public String before_weapon = "knife";
        //アニメーション中か?
        public bool during_animation = false;
        //timer
        public int time = 0;
        //武器の情報
        private const int mag_hg_b = 18;
        private const int mag_ar_b = 30;
        private const int mag_sg_b = 7;
        private const int mag_sr_b = 10;
        private const int max_hg_b = 50;
        private const int max_ar_b = 100;
        private const int max_sg_b = 30;
        private const int max_sr_b = 20;

        public Player(float x, float y, int s, float a, int h)
        {
            my = new PointF(x, y);
            speed = s;
            angle = a;
            health = h;
            max_health = h;
            dead = false;

            hg_b = mag_hg_b;
            ar_b = mag_ar_b;
            sg_b = mag_sg_b;
            sr_b = mag_sr_b;

            hg_mag = mag_hg_b;
            ar_mag = mag_ar_b;
            sg_mag = mag_sg_b;
            sr_mag = mag_sr_b;

            m_b = Properties.Resources.m_b;
            s_b = Properties.Resources.s_b;
            death = Division(107, 107, 145, 290, 1f, Properties.Resources.dokuro);
            img = Division(100,100,1,0,1f, Properties.Resources.main);

        }
        Bitmap Division(int width, int height, int x, int y, float scale,Image g)
        {//画像切り出し
            Rectangle sourceRectange = new Rectangle(new Point(x, y), new Size(width, height));
            Bitmap bitmap1 = new Bitmap((int)(width * scale), (int)(height * scale));
            Graphics graphics = Graphics.FromImage(bitmap1);
            graphics.DrawImage(g, new Rectangle(0, 0, (int)(width * scale), (int)(height * scale)), sourceRectange, GraphicsUnit.Pixel);
            graphics.Dispose();
            return bitmap1;
        }
        public void Draw(Graphics g)
        {
            foreach (clsBullet_P bullet_P in Bullets_P)
            {
                bullet_P.draw(g);
            }

            if (!dead)
            {
                g.ResetTransform();
                g.TranslateTransform(-(my.X + img.Width / 2), -(my.Y + img.Width / 2));
                g.RotateTransform(angle, MatrixOrder.Append);
                g.TranslateTransform(my.X + img.Width / 2, my.Y + img.Width / 2, MatrixOrder.Append);
                if (before_health == health) g.DrawImage(img, my.X, my.Y);
                before_health = health;
            }
            else
            {
                g.DrawImage(death, my.X, my.Y);
                fadeout = true;
            }

        }
        public void Tick(Point mouse, List<Enemy> enemies)
        {
            if (health <= 0) dead = true;
            center = new PointF(my.X + img.Width / 2, my.Y + img.Height / 2);
            Item();
            Enter();
            Rotate(mouse);
            move.Pmove(speed, center, ref my, All_Rect[now_stage], img.Width / 2, Key.up, Key.down, Key.left, Key.right);
            if ((!during_animation) && (!fadeout) && (!fadein)) Shot(enemies);
        }
        private void Shot(List<Enemy> Enemies)
        {
            if (Key.wait <= 0)
            {
                if (Key.click == true)
                {
                    switch (Key.weapon)
                    {
                        case "knife":
                            Key.wait += 1;
                            Key.click = false;
                            break;

                        case "handgun":
                            Key.wait += 2;
                            Bullets_P.Add(shot = new clsBullet_P(Key.weapon, my.X, my.Y, angle - 90, s_b, Enemies));
                            Key.click = false;
                            break;

                        case "shotgun":
                            Key.wait += 20;
                            Bullets_P.Add(shot = new clsBullet_P(Key.weapon, my.X, my.Y, angle + 5 - 90, s_b, Enemies));
                            Bullets_P.Add(shot = new clsBullet_P(Key.weapon, my.X, my.Y, angle - 90, s_b, Enemies));
                            Bullets_P.Add(shot = new clsBullet_P(Key.weapon, my.X, my.Y, angle - 5 - 90, s_b, Enemies));
                            Key.click = false;
                            break;

                        case "carbine":
                            Key.wait += 8;
                            Bullets_P.Add(shot = new clsBullet_P(Key.weapon, my.X, my.Y, angle - 90, m_b, Enemies));
                            break;

                        case "sniper":
                            Key.wait += 30;
                            Bullets_P.Add(shot = new clsBullet_P(Key.weapon, my.X, my.Y, angle - 90, m_b, Enemies));
                            Key.click = false;
                            break;
                    }
                }
            }
            else Key.wait--;

            for (int i = Bullets_P.Count - 1; i >= 0; i--)
            {
                Bullets_P[i].Tick();
                //死んでいたら消す
                if (Bullets_P[i].dead) Bullets_P.RemoveAt(i);
            }
        }
        private void Rotate(Point m)
        {
            //旋回角度を計算
            //偏角を計算     Math.Atan2(y, x) * 180 / Math.PI
            angle = -(float)(Math.Atan2(center.X - m.X, center.Y - m.Y) * 180 / Math.PI);
        }
        private void Enter()
        {
            if (!fadeout)
            {
                for (int i = 0; i < Door_In.Count; i++)
                {
                    //今のステージにあるものなら
                    if ((Door_In[i].stage == now_stage) && (stage_enemy == 0))
                    {
                        //ぶつかったら
                        if (((Door_In[i].rect.X - img.Width / 2 < center.X) && (Door_In[i].rect.Right + img.Width / 2 > center.X)) &&
                           ((Door_In[i].rect.Y - img.Width / 2 < center.Y) && (Door_In[i].rect.Bottom + img.Width / 2 > center.Y)))
                        {
                            fadeout = true;
                            Bullets_P = new List<clsBullet_P>();
                        }
                    }
                }
            }
        }
        private void Item()
        {
            //All_bを後ろから走査
            for (int i = All_Bullet.Count - 1; i >= 0; i--)
            {
                //今のステージにあるものなら
                if (All_Bullet[i].stage == now_stage)
                {
                    //ぶつかったら
                    if (ItemCollision(All_Bullet[i].x, All_Bullet[i].y, center.X, center.Y, img.Width / 2, All_Bullet[i].img.Width))
                    {
                        switch (All_Bullet[i].type)
                        {
                            case "hg_b":
                                if (hg_b + All_Bullet[i].amount > max_hg_b) hg_b = max_hg_b;
                                else hg_b += All_Bullet[i].amount;
                                break;
                            case "ar_b":
                                if (ar_b + All_Bullet[i].amount > max_ar_b) ar_b = max_ar_b;
                                else ar_b += All_Bullet[i].amount;
                                break;
                            case "sg_b":
                                if (sg_b + All_Bullet[i].amount > max_sg_b) sg_b = max_sg_b;
                                else sg_b += All_Bullet[i].amount;
                                break;
                            case "sr_b":
                                if (sr_b + All_Bullet[i].amount > max_sr_b) sr_b = max_sr_b;
                                else sr_b += All_Bullet[i].amount;
                                break;
                        }
                        All_Bullet.RemoveAt(i);
                    }
                }
            }
            //All_hを後ろから走査
            for (int i = All_heal.Count - 1; i >= 0; i--)
            {
                //今のステージにあるものなら
                if (All_heal[i].stage == now_stage)
                {
                    //ぶつかったら
                    if (ItemCollision(All_heal[i].x, All_heal[i].y, center.X, center.Y, img.Width / 2, All_heal[i].img.Width))
                    {
                        if (All_heal[i].amount + health > max_health) health = max_health;
                        else health += All_heal[i].amount;
                        All_heal.RemoveAt(i);
                    }
                }
            }
            //All_wを後ろから走査
            for (int i = All_weapon.Count - 1; i >= 0; i--)
            {
                //今のステージにあるものなら
                if (All_weapon[i].stage == now_stage)
                {
                    //ぶつかったら
                    if (ItemCollision(All_weapon[i].x, All_weapon[i].y, center.X, center.Y, img.Width / 2, All_weapon[i].img.Width))
                    {
                        switch (All_weapon[i].type)
                        {
                            case "hg":
                                Key.hg = true;
                                break;
                            case "ar":
                                Key.ar = true;
                                break;
                            case "sg":
                                Key.sg = true;
                                break;
                            case "sr":
                                Key.sr = true;
                                break;
                        }
                        All_weapon.RemoveAt(i);
                    }
                }
            }
        }
        private bool ItemCollision(int obj_x, int obj_y, float x, float y, int radius, int width)
        {
            //大雑把に判定
            if (((obj_x - radius < x) && (obj_x + width + radius > x)) &&
                    ((obj_y - radius < y) && (obj_y + width + radius > y))) return true;
            return false;
        }
    }
}
