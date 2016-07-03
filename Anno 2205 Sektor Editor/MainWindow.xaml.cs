using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Drawing;
using System.IO;

namespace Anno_2205_Sektor_Editor
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        a6m gamedata;

        public MainWindow()
        {
            InitializeComponent();
            openfile_Click(null, null);
        }

        private void openfile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Sektor |*gamedata*.data";

            if (openFileDialog.ShowDialog() == true)
            {
                FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open);
                byte[] data = new byte[(int)fs.Length];
                fs.Read(data, 0, (int)fs.Length);
                fs.Close();

                string text = openFileDialog.FileName;
                while (text.Length > 42) text = text.Substring(1);
                filename.Text = text;

                gamedata = new a6m(data);

                bytes_list.Items.Clear();
                gamedata.blocks.ForEach(block =>
                {
                    bytes_list.Items.Add((new TextBlock()).Text = block.Length + " Bytes");
                });

                map_list.Items.Clear();
                gamedata.maps.ForEach(map =>
                {
                    map_list.Items.Add((new TextBlock()).Text = map.descriptor + " (" + map.length / 1024 + "kB)");
                });
            }
        }
        private void savefile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Sektor |*gamedata*.data";

            if (saveFileDialog.ShowDialog() == true)
            {
                FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create);
                byte[] data = gamedata.getBytes();
                fs.Write(data, 0, data.Length);
                fs.Close();
                MessageBox.Show("gespeichert");
            }
        }

        private void save_map_Click(object sender, RoutedEventArgs e)
        {
            if (gamedata == null || map_list.SelectedIndex == -1) return;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Als png speichern|*.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                Window save = new Window();
                save.Width = 218;
                save.Height = 32;
                save.Title = "Bitte warten";
                save.Show();
                gamedata.create_map(map_list.SelectedIndex).Save(saveFileDialog.FileName);
                save.Close();
            }
        }

        private void save_allmaps_Click(object sender, RoutedEventArgs e)
        {
            if(gamedata == null) return;
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Window save = new Window();
                save.Width = 218;
                save.Height = 32;
                save.Title = "Bitte warten";
                save.Show();
                for (int i = 0; i < gamedata.maps.Count; i++)
                {
                    gamedata.create_map(i).Save(folderDialog.SelectedPath + "\\" + i + "_" + gamedata.maps[i].descriptor + ".png");
                }
                save.Close();
            }
        }

        private void open_map_Click(object sender, RoutedEventArgs e)
        {
            int index = map_list.SelectedIndex;
            if (gamedata == null || index == -1) return;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Aus png einlesen|*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                Bitmap bitmap = new Bitmap(openFileDialog.FileName);
                map map = gamedata.maps[index];
                if (bitmap.Size.Width != map.size || bitmap.Size.Height != map.size)
                {
                    MessageBox.Show("Die " + map.descriptor + " muss eine größe von " + map.size + "x" + map.size + " pixel haben");
                }
                else
                {
                    Window save = new Window();
                    save.Width = 218;
                    save.Height = 32;
                    save.Title = "Bitte warten";
                    save.Show();
                    gamedata.read_map(index, bitmap);
                    save.Close();
                    MessageBox.Show("Die " + map.descriptor + " wurde eingelesen");
                }
            }
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            if (gamedata == null || bytes_list.SelectedIndex == -1) return;
            SaveFileDialog saveFile = new SaveFileDialog();
            if (saveFile.ShowDialog() == true)
            {
                FileStream file = File.Create(saveFile.FileName);
                byte[] block = gamedata.blocks[bytes_list.SelectedIndex];
                file.Write(block, 0, block.Length);
                file.Close();
            }
        }

        //private void button_Click(object sender, RoutedEventArgs e)
        //{
        //    SaveFileDialog saveFile = new SaveFileDialog();
        //    if (saveFile.ShowDialog() == true)
        //    {
        //        int length = 2048 * 4096 + 128;
        //        byte[] dds = new byte[length];

        //        dds[0] = 0x44;
        //        dds[1] = 0x44;
        //        dds[2] = 0x53;
        //        dds[3] = 0x20;
        //        dds[4] = 0x7C;
        //        dds[8] = 0x07;
        //        dds[9] = 0x30;
        //        dds[10] = 0x08;
        //        dds[13] = 0x10;
        //        dds[17] = 0x10;
        //        dds[21] = 0x20;
        //        dds[28] = 0x01;
        //        dds[76] = 0x20;
        //        dds[80] = 0x04;
        //        dds[84] = 0x44;
        //        dds[85] = 0x58;
        //        dds[86] = 0x54;
        //        dds[87] = 0x31;
        //        dds[109] = 0x10;
        //        dds[111] = 0x04;

        //        for (int i = 128; i < length;)
        //        {
        //            dds[i++] = 0xF0;
        //            dds[i++] = 0x87;
        //            dds[i++] = 0xEF;
        //            dds[i++] = 0x7F;
        //            dds[i++] = 0xFF;
        //            dds[i++] = 0xFF;
        //            dds[i++] = 0xFF;
        //            dds[i++] = 0xFF;
        //        }

        //        FileStream file = File.Create(saveFile.FileName);
        //        file.Write(dds, 0, length);
        //        file.Close();
        //    }
        //}
    }
}
