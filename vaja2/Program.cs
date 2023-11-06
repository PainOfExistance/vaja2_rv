using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace vaja1
{
    internal class Program
    {
        public static int indexIncr = 76;
        public static bool IsBitSet(int value, int position)
        {
            if (position < 0 || position >= 32)
            {
                throw new ArgumentOutOfRangeException("Position is out of the range of bits in an int.");
            }
            return (value & (1 << position)) != 0;
        }

        public static void SetBit(ref int num, int position)
        {
            num |= (1 << position);
        }

        static List<int> Predict(Bitmap img, int height, int width)
        {
            List<int> E = new List<int>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        E.Add(img.GetPixel(x, y).R);
                    }
                    else if (y == 0)
                    {
                        E.Add(img.GetPixel(x - 1, 0).R - img.GetPixel(x, 0).R);
                    }
                    else if (x == 0)
                    {
                        E.Add(img.GetPixel(0, y - 1).R - img.GetPixel(0, y).R);
                    }
                    else
                    {
                        int max = Math.Max(img.GetPixel(x - 1, y).R, img.GetPixel(x, y - 1).R);
                        int min = Math.Min(img.GetPixel(x - 1, y).R, img.GetPixel(x, y - 1).R);
                        int currentPixel = img.GetPixel(x, y).R;
                        if (img.GetPixel(x - 1, y - 1).R >= max)
                        {
                            E.Add(min - currentPixel);
                        }
                        else if (img.GetPixel(x - 1, y - 1).R <= min)
                        {
                            E.Add(max - currentPixel);
                        }
                        else
                        {
                            E.Add((img.GetPixel(x - 1, y).R + img.GetPixel(x, y - 1).R - img.GetPixel(x - 1, y - 1).R) - currentPixel);
                        }
                    }
                }
            }

            return E;
        }

        static Bitmap InversePredict(List<int> E, int height, int width)
        {
            Bitmap img = new Bitmap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int keke = E[x * height + y];
                    if (x == 0 && y == 0)
                    {
                        Color color = Color.FromArgb(E[x * height + y], E[x * height + y], E[x * height + y]);
                        img.SetPixel(x, y, color);
                    }
                    else if (y == 0)
                    {
                        int x1 = img.GetPixel(x - 1, 0).R - E[x * height + y];
                        Color color = Color.FromArgb(x1, x1, x1);
                        img.SetPixel(x, 0, color);
                    }
                    else if (x == 0)
                    {
                        int y1 = img.GetPixel(0, y - 1).R - E[x * height + y];
                        Color color = Color.FromArgb(y1, y1, y1);
                        img.SetPixel(0, y, color);
                    }
                    else
                    {
                        int max = Math.Max(img.GetPixel(x - 1, y).R, img.GetPixel(x, y - 1).R);
                        int min = Math.Min(img.GetPixel(x - 1, y).R, img.GetPixel(x, y - 1).R);

                        if (img.GetPixel(x - 1, y - 1).R >= max)
                        {
                            Color color = Color.FromArgb(min - E[x * height + y], min - E[x * height + y], min - E[x * height + y]);
                            img.SetPixel(x, y, color);
                        }
                        else if (img.GetPixel(x - 1, y - 1).R <= min)
                        {
                            Color color = Color.FromArgb(max - E[x * height + y], max - E[x * height + y], max - E[x * height + y]);
                            img.SetPixel(x, y, color);
                        }
                        else
                        {
                            int tmp = (img.GetPixel(x - 1, y).R + img.GetPixel(x, y - 1).R - img.GetPixel(x - 1, y - 1).R);
                            Color color = Color.FromArgb(tmp - E[x * height + y], tmp - E[x * height + y], tmp - E[x * height + y]);
                            img.SetPixel(x, y, color);
                        }
                    }
                }
            }

            return img;
        }

        static void IC(ref BitArray B, List<int> C, int l, int h, int index)
        {
            if (h - l > 1)
            {
                if (C[h] != C[l])
                {
                    int m = (int)Math.Floor((l + h) / 2.0);
                    int g = (int)Math.Ceiling(Math.Log(C[h] - C[l] + 1, 2));
                    int delta = C[m] - C[l];
                    B.Length += g;


                    int j = g - 1;
                    for (int i = 0; i < g; i++)
                    {
                        B[index + i] = IsBitSet(delta, j);
                        j--;
                    }

                    if (l < m)
                    {
                        IC(ref B, C, l, m, B.Length);
                    }
                    if (m < h)
                    {
                        IC(ref B, C, m, h, B.Length);
                    }
                }
            }
        }

        static void DeIC(BitArray B, ref List<int> C, int l, int h, int index)
        {
            if (h - l > 1)
            {
                if (C[h] != C[l])
                {
                    int m = (int)Math.Floor((l + h) / 2.0);
                    int g = (int)Math.Ceiling(Math.Log(C[h] - C[l] + 1, 2));
                    int j = g - 1;
                    int buffer = 0;

                    for (int i = 0; i < g; i++)
                    {
                        if (B[indexIncr + i])
                        {
                            SetBit(ref buffer, j);
                        }
                        j--;
                    }

                    C[m] = C[l] + buffer;
                    indexIncr += g;

                    if (l < m)
                    {
                        DeIC(B, ref C, l, m, index + g);
                    }
                    if (m < h)
                    {
                        DeIC(B, ref C, m, h, index + g);
                    }
                }
                else if (C[h] == C[l])
                {
                    for (int i = l; i <= h; i++)
                    {
                        C[i] = C[l];
                    }
                }
            }
        }

        static BitArray SetHeader(UInt16 height, byte c0, int cl, int n)
        {
            BitArray B = new BitArray(76);
            int i = 0;

            for (int j = 11; j >= 0; j--)
            {
                B[i] = IsBitSet(height, j);
                i++;
            }

            for (int j = 7; j >= 0; j--)
            {
                B[i] = IsBitSet(c0, j);
                i++;
            }

            for (int j = 31; j >= 0; j--)
            {
                B[i] = IsBitSet(cl, j);
                i++;
            }

            for (int j = 23; j >= 0; j--)
            {
                B[i] = IsBitSet(n, j);
                i++;
            }

            return B;
        }

        static void GetHeader(BitArray B, ref int height, ref int width, ref int c0, ref int cl, ref int n)
        {
            int i = 0;
            for (int j = 11; j >= 0; j--)
            {
                if (B[i]) SetBit(ref height, j);
                i++;
            }

            for (int j = 7; j >= 0; j--)
            {
                if (B[i]) SetBit(ref c0, j);
                i++;
            }

            for (int j = 31; j >= 0; j--)
            {
                if (B[i]) SetBit(ref cl, j);
                i++;
            }

            for (int j = 23; j >= 0; j--)
            {
                if (B[i]) SetBit(ref n, j);
                i++;
            }
            width = n / height;
        }

        static void Compress(Bitmap bmp, int height, int width)
        {
            List<int> E = Predict(bmp, height, width);
            int n = width * height;
            List<int> N = new List<int> { E[0] };

            foreach (int e in E.Skip(1))
            {
                if (e >= 0) N.Add(2 * e);
                else N.Add(2 * Math.Abs(e) - 1);
            }

            List<int> C = new List<int> { N[0] };
            for (int i = 1; i < n; i++)
            {
                C.Add(C[i - 1] + N[i]);
            }

            BitArray B = SetHeader((UInt16)height, (byte)C[0], C.Last(), n);
            IC(ref B, C, 0, n - 1, 76);

            byte[] bytes = new byte[(B.Length - 1) / 8 + 1];
            B.CopyTo(bytes, 0);

            for (int i = 0; i < bytes.Length; i++)
            {
                byte originalByte = bytes[i];
                byte reversedByte = 0;

                for (int f = 0; f < 8; f++)
                {
                    if ((originalByte & (1 << f)) != 0)
                    {
                        reversedByte |= (byte)(1 << (7 - f));
                    }
                }

                bytes[i] = reversedByte;
            }

            File.WriteAllBytes("out.bin", bytes);
        }

        public static void Decompress(string path)
        {
            byte[] fileBytes = File.ReadAllBytes(path);

            BitArray B = new BitArray(fileBytes);

            for (int i = 0; i < B.Length; i += 8)
            {
                int j = 0;
                for (int z = 7; z >= 0; z--)
                {
                    bool tmp = B[i + z];
                    B[i + z] = B[i + j];
                    B[i + j] = tmp;
                    j++;
                    if (j == 4) break;
                }
            }

            int height = 0;
            int width = 0;
            int c0 = 0;
            int cl = 0;
            int n = 0;

            GetHeader(B, ref height, ref width, ref c0, ref cl, ref n);
            List<int> C = new List<int>
            {
                c0
            };

            for (int i = 1; i < n - 1; i++)
            {
                C.Add(0);
            }
            C.Add(cl);
            DeIC(B, ref C, 0, n - 1, 76);

            List<int> N = new List<int> { C[0] };
            for (int i = 1; i < n; i++)
            {
                N.Add(C[i] - C[i - 1]);
            }

            List<int> E = new List<int> { N[0] };
            foreach (int nn in N.Skip(1))
            {
                if (nn % 2 == 0) E.Add(nn / 2);
                else E.Add(-(nn + 1) / 2);
            }

            Bitmap bmp = InversePredict(E, height, width);

            bmp.Save("output.png");
            bmp.Dispose();
        }

        static void Main(string[] args)
        {
            string path = "Pens.bmp";
            Bitmap bmp = new(path);
            int width = bmp.Width;
            int height = bmp.Height;
            Stopwatch comp = new Stopwatch();
            Stopwatch decomp = new Stopwatch();
            comp.Start();
            Compress(bmp, height, width);
            comp.Stop();
            decomp.Start();
            Decompress("out.bin");
            decomp.Stop();
            bmp.Dispose();

            FileInfo precomp = new FileInfo(path);
            double precompKB = precomp.Length / 1024.0;

            FileInfo postcomp = new FileInfo("out.bin");
            double postcompKB = postcomp.Length / 1024.0;

            string neke = path + " " + precompKB + "KB " + postcompKB + "KB " + precompKB / postcompKB + "orig./stis. " + comp.ElapsedMilliseconds + "ms " + decomp.ElapsedMilliseconds + "ms";
            Console.WriteLine(neke);
            File.AppendAllText("out.txt", neke + "\n");
        }
    }
}