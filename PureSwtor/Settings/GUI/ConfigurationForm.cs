using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Buddy.Common;
using Buddy.CommonBot;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using PureSWTor.Helpers;
using PureSWTor.Managers;

namespace PureSWTor.Settings.GUI
{
    public partial class ConfigurationForm : Form
    {
        private static TorPlayer Me { get { return BuddyTor.Me; } }

        public ConfigurationForm()
        {
            InitializeComponent();

            Text = "Pure SWTor";

            InitializePlainStyle();
            
        }

        private void InitializePlainStyle()
        {
            pgMain.LineColor = GetClassColor(); // Category
            pgMain.CategoryForeColor = GetClassForeColor();
            
            panel4.BackColor = GetClassColor();

            // hide the toolbar
            pgMain.ToolbarVisible = false;

            // Collapse All Grid Items
            pgMain.CollapseAllGridItems();
        }

        private Color GetClassForeColor()
        {
            switch (BuddyTor.Me.AdvancedClass)
            {
                case AdvancedClass.Assassin:
                case AdvancedClass.Commando:
                case AdvancedClass.Guardian:
                case AdvancedClass.Gunslinger:
                case AdvancedClass.Juggernaut:
                case AdvancedClass.Marauder:
                case AdvancedClass.Mercenary:
                case AdvancedClass.Operative:
                case AdvancedClass.Powertech:
                case AdvancedClass.Sage:
                case AdvancedClass.Scoundrel:
                case AdvancedClass.Sentinel:
                case AdvancedClass.Shadow:
                case AdvancedClass.Sniper:
                case AdvancedClass.Sorcerer:
                case AdvancedClass.Vanguard:
                    return Color.FromArgb(0, 0, 0);
                default:
                    return Color.FromArgb(255, 255, 255);
            }
        }

        private Color GetClassColor()
        {
            switch (BuddyTor.Me.AdvancedClass)
            {
                case AdvancedClass.Assassin:
                    return Color.FromArgb(196, 30, 59);
                case AdvancedClass.Commando:
                    return Color.FromArgb(255, 124, 10);
                case AdvancedClass.Guardian:
                    return Color.FromArgb(170, 211, 114);
                case AdvancedClass.Gunslinger:
                    return Color.FromArgb(104, 204, 239);
                case AdvancedClass.Juggernaut:
                    return Color.FromArgb(0, 132, 93);
                case AdvancedClass.Mercenary:
                    return Color.FromArgb(244, 140, 186);
                case AdvancedClass.Marauder:
                    return Color.FromArgb(255, 255, 255);
                case AdvancedClass.Operative:
                    return Color.FromArgb(255, 244, 104);
                case AdvancedClass.Powertech:
                    return Color.FromArgb(35, 89, 221);
                case AdvancedClass.Sage:
                    return Color.FromArgb(147, 130, 170);
                case AdvancedClass.Scoundrel:
                    return Color.FromArgb(199, 156, 87);
                case AdvancedClass.Sentinel:
                    return Color.FromArgb(199, 156, 87);
                case AdvancedClass.Shadow:
                    return Color.FromArgb(199, 156, 87);
                case AdvancedClass.Sniper:
                    return Color.FromArgb(199, 156, 87);
                case AdvancedClass.Sorcerer:
                    return Color.FromArgb(199, 156, 87);
                case AdvancedClass.Vanguard:
                    return Color.FromArgb(199, 156, 87);
                default:
                    return Color.FromArgb(47, 47, 47);
            }
        }

        private void SaveCloseButton_Click(object sender, EventArgs e)
        {

        }

        private void SaveCloseButton_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var party = Me.PartyMembers(true).ToList();
            comboBox1.Items.Clear();
            foreach (TorCharacter c in party)
            {
                comboBox1.Items.Add(c.Name);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HealingManager.TankName = comboBox1.Items[comboBox1.SelectedIndex].ToString();
            Logging.Write("Tank is " + HealingManager.TankName);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RandomGrind.EnableRandomGrind();
        }
    }
}
