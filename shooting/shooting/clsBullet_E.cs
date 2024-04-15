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
    internal class clsBullet_E
    {
        //private Bitmap imgbullet, imgknife;
        private int damage;
        private float x, y, dir, v;
        private Image img;
        private Player player;
        private PointF center;
        public bool dead, first;



        public clsBullet_E(float startX, float startY, float startDir, Image startImg, Player p)
        {
            img = startImg;
            x = startX;
            y = startY;
            dir = startDir;
            player = p;
            v = 10;
            damage = 10;

            //imgbullet = zombieyamasaki.Properties.Resources.nasu;
            //imgknife = zombieyamasaki.Properties.Resources.tama;
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
            //プレイヤーに当たったら
            if (Math.Sqrt(Math.Pow(player.center.X - c.X, 2) + Math.Pow(player.center.Y - c.Y, 2)) <= (r + player.img.Width / 2))
            {
                if (first)
                {
                    player.health -= damage;
                    first = false;
                }
                dead = true;
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
