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

namespace shooting
{
    internal class clsBullet_P
    {
        //private Bitmap imgbullet, imgknife;
        private string type;
        private int damage;
        private float x, y, dir, v;
        private Image img;
        private List<Enemy> enemies;
        private PointF center;
        public bool dead, first;



        public clsBullet_P(string startType, float startX, float startY, float startDir, Image startImg, List<Enemy> Enemies)
        {
            type = startType;
            img = startImg;
            x = startX;
            y = startY;
            dir = startDir;
            enemies = Enemies;

            //imgbullet = zombieyamasaki.Properties.Resources.nasu;
            //imgknife = zombieyamasaki.Properties.Resources.tama;

            switch (type)
            {
                case "knife":
                    //出来てない    
                    v = 10;
                    damage = 0;
                    break;

                case "handgun":
                    v = 10;
                    damage = 10;
                    break;

                case "shotgun":
                    v = 20;
                    damage = 8;
                    break;

                case "carbine":
                    v = 30;
                    damage = 14;
                    var rand = new Random();
                    //集弾率
                    dir += rand.Next(-2, 2);
                    break;
                case "sniper":
                    v = 40;
                    damage = 20;
                    break;
            }
        }

        public void Tick()
        {
            center = new PointF(x + img.Width / 2, y + img.Height / 2);

            first = true;
            for (int i = 1; i <= v; i++)
            {
                x += (float)(Math.Cos(Math.PI / 180 * dir));
                y += (float)(Math.Sin(Math.PI / 180 * dir));
                DamageCheck(center, img.Width / 2);
                CollisionCheck(center, img.Width / 2);
            }

        }

        public void draw(Graphics gr)
        {
            gr.ResetTransform();
            gr.TranslateTransform(-x + -img.Width / 2, -y + -img.Height / 2);
            gr.RotateTransform((float)dir, MatrixOrder.Append);
            gr.TranslateTransform(x + img.Width / 2, y + img.Height / 2, MatrixOrder.Append);
            if (!dead) gr.DrawImage(img, x, y);
        }
        private void DamageCheck(PointF c, int r)
        {
            foreach (Enemy enemy in enemies)
            {
                //敵に当たったら
                if (Math.Sqrt(Math.Pow(enemy.center.X - c.X, 2) + Math.Pow(enemy.center.Y - c.Y, 2)) <= (r + enemy.img.Width / 2))
                {
                    if (first)
                    {
                        enemy.health -= damage;
                        first = false;
                    }
                    if(type != "Sniper") dead = true;
                }
                else if(type == "Sniper")
                {
                    if (first) first = true;
                }
            }

        }
        private void CollisionCheck(PointF c, int r)
        {
            //ぶつかったら死ぬ
            foreach (Rectangle rect in Main.All_Rect[Main.now_stage])
            {
                if (((rect.X - r < c.X) && (rect.Right + r > c.X)) &&
                        ((rect.Y - r < c.Y) && (rect.Bottom + r > c.Y)))
                    dead = true;
            }
        }
    }
}
