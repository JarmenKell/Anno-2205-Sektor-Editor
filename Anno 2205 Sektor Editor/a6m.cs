using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using Microsoft.Win32;

namespace Anno_2205_Sektor_Editor
{
    struct map
    {
        public string descriptor;
        public short size;
        public int length;
        public string type;
        public object data;

        public map(string descriptor, short size, int length, string type, object data)
        {
            this.descriptor = descriptor;
            this.size = size;
            this.length = length;
            this.type = type;
            this.data = data;
        }
    }

    class a6m
    {
        public List<byte[]> blocks = new List<byte[]>();
        public List<map> maps = new List<map>();

        public a6m (byte[] data)
        {
            int pos = 0;
            while (data[pos] != 130 || data[pos + 1] != 192 || data[pos + 2] != 128 || data[pos + 3] != 4) pos++;
            pos += 4;
            next_block(pos, 0, data);

            byte[,,] heightmap = new byte[2048, 2048, 2];
            for (int i = 2047; i >= 0; i--)
            {
                for (int j = 0; j < 2048; j++)
                {
                    heightmap[j, i, 0] = data[pos++];
                    heightmap[j, i, 1] = data[pos++];
                }
                pos += 2;
            }
            maps.Add(new map("Heightmap", 2048, 2049 * 4096, "Grün 1 Byte, Blau 1 Byte", heightmap));

            int block_beginn = pos;
            while (data[pos] != 128 || data[pos + 1] != 128 || data[pos + 2] != 128 || data[pos + 3] != 64) pos++;
            pos += 4;
            next_block(pos, block_beginn, data);

            byte[,] sectionmap = new byte[1024, 1024];
            for (int i = 1023; i >= 0; i--)
            {
                for (int j = 0; j < 1024; j++)
                {
                    sectionmap[j, i] = data[pos++];
                }
            }
            maps.Add(new map("Sectionmap", 1024, 1024 * 1024, "Graustufen 1 Byte", sectionmap));

            block_beginn = pos;
            while (data[pos] != 128 || data[pos + 1] != 128 || data[pos + 2] != 128 || data[pos + 3] != 64) pos++;
            pos += 4;
            next_block(pos, block_beginn, data);

            sectionmap = new byte[1024, 1024];
            for (int i = 1023; i >= 0; i--)
            {
                for (int j = 0; j < 1024; j++)
                {
                    sectionmap[j, i] = data[pos++];
                }
            }
            maps.Add(new map("Sectionmap", 1024, 1024 * 1024, "Graustufen 1 Byte", sectionmap));

            block_beginn = pos;
            while (data[pos] != 128 || data[pos + 1] != 128 || data[pos + 2] != 128 || data[pos + 3] != 8) pos++;
            pos += 4;
            next_block(pos, block_beginn, data);

            byte[] temp = new byte[128 * 1024];
            Buffer.BlockCopy(data, pos, temp, 0, 128 * 1024);
            BitArray bitmap = new BitArray(temp);
            pos += 128 * 1024;
            maps.Add(new map("Bitmap", 1024, 128 * 1024, "Schwarz Weiss", bitmap));

            block_beginn = pos;
            while (data[pos] != 128 || data[pos + 1] != 128 || data[pos + 2] != 128 || data[pos + 3] != 1) pos++;
            pos += 4;
            next_block(pos, block_beginn, data);

            byte[,] minimap = new byte[128, 128];
            for (int i = 127; i >= 0; i--)
            {
                for (int j = 0; j < 128; j++)
                {
                    minimap[j, i] = data[pos++];
                }
            }
            maps.Add(new map("Minimap", 128, 128 * 128, "Graustufen 1 Byte", minimap));

            block_beginn = pos;
            while (data[pos] != 128 || data[pos + 1] != 128 || data[pos + 2] != 128 || data[pos + 3] != 8) pos++;
            pos += 4;
            next_block(pos, block_beginn, data);

            temp = new byte[128 * 1024];
            Buffer.BlockCopy(data, pos, temp, 0, 128 * 1024);
            bitmap = new BitArray(temp);
            pos += 128 * 1024;
            maps.Add(new map("Bitmap", 1024, 128 * 1024, "Schwarz Weiss", bitmap));

            next_block(data.Length, pos, data);
        }

        private void next_block(int pos, int block_beginn, byte[] data)
        {
            byte[] temp = new byte[pos - block_beginn];
            Buffer.BlockCopy(data, block_beginn, temp, 0, pos - block_beginn);
            blocks.Add(temp);
        }

        public Byte[] getBytes()
        {
            MemoryStream data = new MemoryStream(1024 * 1024 * 32);
            int h;
            for (h = 0; h < maps.Count; h++)
            {
                data.Write(blocks[h], 0, blocks[h].Length);
                byte[] map = new byte[0];
                int pos = 0;
                switch (maps[h].descriptor)
                {
                    case "Heightmap":
                        byte[,,] Heightdata = (byte[,,])maps[h].data;
                        map = new byte[maps[h].length];
                        for (int i = 2047; i >= 0; i--)
                        {
                            for (int j = 0; j < 2048; j++)
                            {
                                map[pos++] = Heightdata[j, i, 0];
                                map[pos++] = Heightdata[j, i, 1];
                            }
                            map[pos++] = 0;
                            map[pos++] = 0;
                        }
                        break;

                    case "Minimap":
                    case "Sectionmap":
                        byte[,] Sectiondata = (byte[,])maps[h].data;
                        map = new byte[maps[h].length];
                        for (int i = maps[h].size - 1; i >= 0; i--)
                        {
                            for (int j = 0; j < maps[h].size; j++)
                            {
                                map[pos++] = Sectiondata[j, i];
                            }
                        }
                        break;

                    case "Bitmap":
                        BitArray Bitdata = (BitArray)maps[h].data;
                        map = new byte[maps[h].length];
                        Bitdata.CopyTo(map, 0);
                        break;
                }
                data.Write(map, 0, map.Length);
            }
            data.Write(blocks[h], 0, blocks[h].Length);
            data.Capacity = (int)data.Length;
            return data.GetBuffer();
        }

        public Bitmap create_map(int index)
        {
            map map = maps[index];
            Bitmap bitmap = new Bitmap(map.size, map.size);

            switch (map.descriptor)
            {
                case "Heightmap":
                    byte[,,] Heightdata = (byte[,,])map.data;
                    for (int i = 0; i < 2048; i++)
                    {
                        for (int j = 0; j < 2048; j++)
                        {
                            bitmap.SetPixel(i, j, Color.FromArgb(0, Heightdata[i, j, 0], Heightdata[i, j, 1]));
                        }
                    }
                    break;

                case "Minimap":
                case "Sectionmap":
                    byte[,] Sectiondata = (byte[,])map.data;
                    for (int i = 0; i < map.size; i++)
                    {
                        for (int j = 0; j < map.size; j++)
                        {
                            bitmap.SetPixel(i, j, Color.FromArgb(Sectiondata[i, j], Sectiondata[i, j], Sectiondata[i, j]));
                        }
                    }
                    break;

                case "Bitmap":
                    BitArray Bitdata = (BitArray)map.data;
                    int k = 0;
                    for (int i = 1023; i >= 0; i--)
                    {
                        for (int j = 0; j < 1024; j++)
                        {
                            bitmap.SetPixel(j, i, Bitdata.Get(k++) ? Color.White : Color.Black);
                        }
                    }
                    break;
            }
            return bitmap;
        }

        public void read_map(int index, Bitmap bitmap)
        {
            map map = maps[index];

            switch (map.descriptor)
            {
                case "Heightmap":
                    byte[,,] Heightdata = (byte[,,])map.data;
                    for (int i = 0; i < 2048; i++)
                    {
                        for (int j = 0; j < 2048; j++)
                        {
                            Heightdata[i, j, 0] = bitmap.GetPixel(i, j).G;
                            Heightdata[i, j, 1] = bitmap.GetPixel(i, j).B;
                        }
                    }
                    break;

                case "Minimap":
                case "Sectionmap":
                    byte[,] Sectiondata = (byte[,])map.data;
                    for (int i = 0; i < map.size; i++)
                    {
                        for (int j = 0; j < map.size; j++)
                        {
                            Sectiondata[i, j] = bitmap.GetPixel(i, j).R;
                        }
                    }
                    break;

                case "Bitmap":
                    BitArray Bitdata = (BitArray)map.data;
                    int k = 0;
                    for (int i = 1023; i >= 0; i--)
                    {
                        for (int j = 0; j < 1024; j++)
                        {
                            Bitdata.Set(k, bitmap.GetPixel(j, i) == Color.White);
                        }
                    }
                    break;
            }
        }
    }
}
