using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace shooting
{
    internal class Move
    {
        public void Pmove(int speed,PointF center,ref PointF my,Rectangle[] Rect,int radius, bool up,bool down,bool left,bool right)
        {
            //斜め移動用の値（約0.7）
            float s = speed / (float)Math.Sqrt(2);

            if (up && left)
            {        //左上
                if (!Collision(Rect, center.X - s, center.Y, radius)) my.X -= s;
                if (!Collision(Rect, center.X, center.Y - s, radius)) my.Y -= s;
            }
            else if (up && right)
            {      //右上
                if (!Collision(Rect, center.X + s, center.Y, radius)) my.X += s;
                if (!Collision(Rect, center.X, center.Y - s, radius)) my.Y -= s;
            }
            else if (down && left)
            {     //左下
                if (!Collision(Rect, center.X - s, center.Y, radius)) my.X -= s;
                if (!Collision(Rect, center.X, center.Y + s, radius)) my.Y += s;
            }
            else if (down && right)
            {        //右下
                if (!Collision(Rect, center.X + s, center.Y, radius)) my.X += s;
                if (!Collision(Rect, center.X, center.Y + s, radius)) my.Y += s;
            }
            //通常移動
            else if (up && !down)       //上
            {
                if (!Collision(Rect, center.X, center.Y - speed, radius)) my.Y -= speed;
            }
            else if (down && !up)   //下
            {
                if (!Collision(Rect, center.X, center.Y + speed, radius)) my.Y += speed;
            }
            else if (left && !right)    //左
            {
                if (!Collision(Rect, center.X - speed, center.Y, radius)) my.X -= speed;
            }
            else if (right && !left)    //右
            {
                if (!Collision(Rect, center.X + speed, center.Y, radius)) my.X += speed;
            }
        }
        private bool Collision(Rectangle[] rect,float x,float y,int radius)
        {
            foreach(Rectangle r in rect)
            {
                //大雑把に判定
                if (((r.X - radius < x)&&(r.Right + radius > x))&&
                        ((r.Y - radius < y)&&(r.Bottom + radius > y))) return true;
            }
            return false;
        }
        public void Emove(int speed,PointF target,PointF center, ref PointF my)
        {
            //プレイヤーまでの距離を求める
            float d = (float)Math.Sqrt((target.X - center.X) * (target.X - center.X) + (target.Y - center.Y) * (target.Y - center.Y));

                if(d > 50)
                {
                    my.X += (target.X - center.X) / d * speed;
                    my.Y += (target.Y - center.Y) / d * speed;
                }
                else
                {//バックステップ
                    //my.X -= (target.X - center.X) / d * speed * 10;
                    //my.Y -= (target.Y - center.Y) / d * speed * 10;
                }
        }
    }
}
