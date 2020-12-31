using GopherClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GopherClient.Service
{
    public class Client
    {
        private Stack<GopherLine> stack;
        private Dictionary<GopherLine, string> cache;
        public GopherLine currentSite { get; set; }

        public string serverAdress { get; set; }
        private int port = 70;
        private TcpClient client;
        private byte[] data;

        private CancellationTokenSource tokenSource;
        private CancellationToken cancellationToken;
        private Task getDataTask;

        public Client()
        {
            stack = new Stack<GopherLine>();
            cache = new Dictionary<GopherLine, string>();
            tokenSource = new CancellationTokenSource();
            cancellationToken = tokenSource.Token;
        }

        private async Task<string> VisitAsync(GopherLine gopherLine)
        {
            StringBuilder sb = new StringBuilder();

            getDataTask = Task.Run(() => { 
                int length;
                data = Encoding.ASCII.GetBytes($"{gopherLine.Selector}\n\r");

                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                client = new TcpClient();
                try
                {
                    client.Connect(gopherLine.Host, int.Parse(gopherLine.Port));
                    
                    NetworkStream stream = client.GetStream();

                    stream.Write(data, 0, data.Length);

                    do
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(); ;
                        data = new byte[client.ReceiveBufferSize];
                        length = stream.Read(data, 0, data.Length);
                        string responseData = Encoding.ASCII.GetString(data, 0, length);
                        sb.Append(responseData);
                    } while (length > 0);
                }
                catch (SocketException)
                {
                    sb.Append("iServer timed out...\t \t \t ");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
                finally
                {
                    client.Close();
                }
            }, tokenSource.Token);

            await getDataTask;

            return sb.ToString();
        }

        public async Task<List<string>> GetTextContentAsync(GopherLine destination)
        {
            string rawContent;
            if (cache.ContainsKey(destination))
            {
                rawContent =  cache[destination];
            }
            else
            {
                rawContent = await VisitAsync(destination);
                cache.Add(destination, rawContent);
            }
            List<string> textContent = rawContent.Split('\n').ToList();
            stack.Push(currentSite);
            currentSite = destination;
            return textContent;
        }

        public async Task<string> GetMenuContentAsync(GopherLine destination)
        {
            string rawContent;
            if (cache.ContainsKey(destination))
            {
                rawContent = cache[destination];
                stack.Push(currentSite);
                currentSite = destination;

                return rawContent;
            }
            else
            {
                try
                {
                    rawContent = await VisitAsync(destination);
                    cache.Add(destination, rawContent);
                    stack.Push(currentSite);
                    currentSite = destination;

                    return rawContent;
                } catch (TaskCanceledException)
                {
                    throw new TaskCanceledException();
                }
            }

            
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

        public void CancelRequest()
        {
            if (getDataTask != null && getDataTask.Status == TaskStatus.Running)
            {
                tokenSource.Cancel();
            }
        }
    }
}
