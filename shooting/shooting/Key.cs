using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace shooting
{
    internal class Key
    {
        public static bool up, down, left, right,grid, click, reload;
        public static int wait;
        public static String weapon = "knife";
        public static bool hg, ar, sg, sr;

        public void Push(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    left = true;
                    break;
                case Keys.D:
                    right = true;
                    break;
                case Keys.W:
                    up = true;
                    break;
                case Keys.S:
                    down = true;
                    break;
                case Keys.Q:
                    wait = 0;
                    weapon = "knife";
                    break;
                case Keys.G:
                    grid = true;
                    break;
                case Keys.D1:
                    if (hg)
                    {
                        wait = 0;
                        weapon = "handgun";
                    }
                    break;
                case Keys.D2:
                    if (sg)
                    {
                        wait = 0;
                        weapon = "shotgun";
                    }
                    break;
                case Keys.D3:
                    if (ar)
                    {
                        wait = 0;
                        weapon = "carbine";
                    }
                    break;
                case Keys.D4:
                    if (sr)
                    {
                        wait = 0;
                        weapon = "sniper";
                    }
                    break;
                case Keys.R:
                    if ((weapon != "knife")&&(reload == false)) reload = true;
                    break;
            }
        }
        public void Release(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    left = false;
                    break;
                case Keys.D:
                    right = false;
                    break;
                case Keys.W:
                    up = false;
                    break;
                case Keys.S:
                    down = false;
                    break;
                case Keys.G:
                    grid = false;
                    break;
            }
        }
        public void Mouse_down(MouseEventArgs e)
        {
            click = true;
        }
        public void Mouse_up(MouseEventArgs e)
        {
            if (wait >= 0) click = false;
        }
    }
}
