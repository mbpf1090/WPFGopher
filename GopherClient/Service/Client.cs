﻿using GopherClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GopherClient.Service
{
    public class Client
    {

        private readonly Stack<GopherLine> stack;
        private readonly Dictionary<GopherLine, string> cache;
        private byte[] data;

        private Task<string> getDataTask;

        public GopherLine currentSite { get; set; }
        public string serverAdress { get; set; }


        public Client()
        {
            stack = new Stack<GopherLine>();
            cache = new Dictionary<GopherLine, string>();

        }

        public ImageSource GetImage(GopherLine line)
        {
            TcpClient client = new TcpClient();

            data = Encoding.UTF8.GetBytes($"{line.Selector}\n\r");

            try
            {
                client.Connect(line.Host, int.Parse(line.Port));
                NetworkStream stream = client.GetStream();

                stream.Write(data, 0, data.Length);

                // Copy to memory
                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                // Create BitmapImage
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = memoryStream;
                bitmap.EndInit();
                // Freeze for threading
                //bitmap.Freeze();

                return bitmap;
            }
            catch (Exception e)
            {
                // TODO: Handle error
                Debug.WriteLine(e.Message);
                throw new Exception("foo");
            }
            finally
            {
                client.Close();
            }
        }

        public string Visit(GopherLine gopherLine)
        {
            StringBuilder sb = new StringBuilder();
            int length;
            TcpClient client = new TcpClient();

            data = Encoding.UTF8.GetBytes($"{gopherLine.Selector}\r\n");

            try
            {
                client.Connect(gopherLine.Host, int.Parse(gopherLine.Port));
                NetworkStream stream = client.GetStream();

                stream.Write(data, 0, data.Length);

                do
                {
                    data = new byte[4096];
                    length = stream.Read(data, 0, data.Length);
                    
                    string responseData = Encoding.UTF8.GetString(data, 0, length);
                    sb.Append(responseData);
                } while (length > 0);
            }
            catch (SocketException)
            {
                sb.Append("iServer timed out...\t \t \t ");
            }
            catch (ArgumentOutOfRangeException)
            {
                return string.Empty;
            }
            finally
            {
                client.Close();
            }

            return sb.ToString();
        }

        public async Task<string> GetTextContentAsync(GopherLine destination)
        {
            string rawContent;

            if (cache.ContainsKey(destination))
            {
                rawContent =  cache[destination];
            }
            else
            {
                rawContent = await Task.Run(() => Visit(destination));
                cache.Add(destination, rawContent);
            }

            stack.Push(currentSite);
            currentSite = destination;

            return rawContent;
        }

        public async Task<string> GetMenuContentAsync(GopherLine destination, CancellationToken token)
        {
            string rawContent;

            if (cache.ContainsKey(destination))
            {
                rawContent = cache[destination];
            }
            else
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    getDataTask = Task.Run(() => Visit(destination));
                    rawContent = await getDataTask;
                    token.ThrowIfCancellationRequested();

                    cache.Add(destination, rawContent);
                }
                catch (OperationCanceledException)
                {
                    return "";
                }
            }

            stack.Push(currentSite);
            currentSite = destination;

            return rawContent;
        }

        public string GoBack()
        {
            GopherLine destination = stack.Pop();
            string rawContent = cache[destination];
            currentSite = destination;
            return rawContent;
        }

        public bool CanGoBack()
        {
            // First element of stack is always null
            return stack.Count > 1;
        }

    }
}
