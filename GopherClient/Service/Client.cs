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
        private byte[] data;

        private CancellationTokenSource tokenSource;
        private CancellationToken cancellationToken;
        private Task<string> getDataTask;

        public Client()
        {
            stack = new Stack<GopherLine>();
            cache = new Dictionary<GopherLine, string>();
            tokenSource = new CancellationTokenSource();
            cancellationToken = tokenSource.Token;

        }

        public string Visit(GopherLine gopherLine, CancellationToken token)
        {

            StringBuilder sb = new StringBuilder();
            tokenSource = new CancellationTokenSource();
            cancellationToken = tokenSource.Token;
            int length;
            TcpClient client = new TcpClient();

            data = Encoding.UTF8.GetBytes($"{gopherLine.Selector}\n\r");

            try
            {
                token.ThrowIfCancellationRequested();
                client.Connect(gopherLine.Host, int.Parse(gopherLine.Port));
                NetworkStream stream = client.GetStream();

                stream.Write(data, 0, data.Length);

                do
                {
                    token.ThrowIfCancellationRequested();
                    data = new byte[4096];
                    length = stream.Read(data, 0, data.Length);
                    token.ThrowIfCancellationRequested();
                    string responseData = Encoding.UTF8.GetString(data, 0, length);
                    sb.Append(responseData);
                } while (length > 0);
            }
            catch (SocketException)
            {
                sb.Append("iServer timed out...\t \t \t ");
            }
            catch (OperationCanceledException)
            {
                return string.Empty;
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

        /*
        private async Task<string> VisitAsync(GopherLine gopherLine)
        {
            StringBuilder sb = new StringBuilder();

            if (getDataTask?.Status == TaskStatus.Running)
            {
                tokenSource.Cancel();
            }

            tokenSource = new CancellationTokenSource();
            // Timeout after 5 sec
            //tokenSource.CancelAfter(5000);
            cancellationToken = tokenSource.Token;

            

            getDataTask = Task.Run(() => { 
                int length;
                TcpClient client = new TcpClient();

                data = Encoding.UTF8.GetBytes($"{gopherLine.Selector}\n\r");

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
                finally
                {
                    client.Close();
                }
            }, cancellationToken);



            try
            {
                await getDataTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cancelled");
                throw new OperationCanceledException();
            }


            return sb.ToString();
        }
        */
        public async Task<string> GetTextContentAsync(GopherLine destination)
        {
            string rawContent;
            if (cache.ContainsKey(destination))
            {
                rawContent =  cache[destination];
            }
            else
            {
                rawContent = Visit(destination, cancellationToken);
                cache.Add(destination, rawContent);
            }

            stack.Push(currentSite);
            currentSite = destination;

            return rawContent;
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

            try
            {
                if (getDataTask?.Status == TaskStatus.Running)
                {
                    tokenSource.Cancel();
                }

                getDataTask = Task.Run(() => Visit(destination, cancellationToken));
                rawContent = await getDataTask;
                if (getDataTask.IsCanceled)
                    throw new OperationCanceledException();
                cache.Add(destination, rawContent);
                stack.Push(currentSite);
                currentSite = destination;

                return rawContent;
            } 
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException();
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
            if (getDataTask?.Status == TaskStatus.Running)
            {
                tokenSource.Cancel();
            }
        }
    }
}
