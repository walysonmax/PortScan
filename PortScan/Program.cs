using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PortScan
{
    class Program
    {

        static List<int> portas = new List<int> { 20, 21, 80, 135, 82, 110, 119, 220, 445, 765, 767, 873, 902, 992, 993, 994, 995, 1270, 1311, 1248, 1234, 1433, 1434, 1716, 2030, 2082, 2710, 3306, 3333, 4000, 6969, 7777 };


        static bool stop = false;

        //static int startPort;

        //static int endPort;

        static List<int> openPorts = new List<int>();

        static object consoleLock = new object();

        static int waitingForResponses;

        static int maxQueriesAtOneTime = 100;


        static void Main(string[] args)
        {
        begin:

            Console.WriteLine("Digite o ip:");
            string ip = Console.ReadLine();

            IPAddress ipAddress;


            //Convertendo o ip digitado em Ip Address
            if (!IPAddress.TryParse(ip, out ipAddress))
                goto begin;

            //startP:

            //Console.WriteLine("Digite a porta inicial:");
            //string sp = Console.ReadLine();

            //if (!int.TryParse(sp, out startPort))
            //    goto startP;

            //endP:

            //Console.WriteLine("Digite a porta final:");
            //string ep = Console.ReadLine();

            //if (!int.TryParse(ep, out endPort))
            //    goto endP;

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("Pressione alguma tecla para parar o scanner...");

            Console.WriteLine("");
            Console.WriteLine("");



            //Pool de thread
            ThreadPool.QueueUserWorkItem(StartScan, ipAddress);

            Console.ReadKey();

            stop = true;

            Console.WriteLine("Pressione alguma tecla para sair...");
            Console.ReadKey();
        }

        static void StartScan(object o)
        {

            IPAddress ipAddress = o as IPAddress;



            foreach (var porta in portas)
            {



                lock (consoleLock)
                {
                    int top = Console.CursorTop;

                    Console.CursorTop = 7;
                    Console.WriteLine("Scanning porta: {0}    ", porta);


                    Thread.Sleep(2000);

                    Console.CursorTop = top;
                }

                while (waitingForResponses >= maxQueriesAtOneTime)
                    Thread.Sleep(0);

                if (stop)
                    break;

                try
                {
                    Socket s = new Socket(AddressFamily.InterNetwork,
                                          SocketType.Stream, ProtocolType.Tcp);

                    s.BeginConnect(new IPEndPoint(ipAddress, porta), EndConnect, s);

                    Interlocked.Increment(ref waitingForResponses);
                }
                catch (Exception)
                {

                }
            }
        }

        static void EndConnect(IAsyncResult ar)
        {
            try
            {
                DecrementResponses();

                Socket s = ar.AsyncState as Socket;

                s.EndConnect(ar);

                if (s.Connected)
                {
                    int openPort = Convert.ToInt32(s.RemoteEndPoint.ToString().Split(':')[1]);

                    openPorts.Add(openPort);

                    lock (consoleLock)
                    {
                        Console.WriteLine("Conectado na porta TCP: {0}", openPort);
                    }

                    s.Disconnect(true);
                }
            }
            catch (Exception)
            {

            }
        }
        static void IncrementResponses()
        {
            Interlocked.Increment(ref waitingForResponses);

            PrintWaitingForResponses();
        }

        static void DecrementResponses()
        {
            Interlocked.Decrement(ref waitingForResponses);

            PrintWaitingForResponses();
        }

        static void PrintWaitingForResponses()
        {
            lock (consoleLock)
            {
                int top = Console.CursorTop;

                Console.CursorTop = 8;
                Console.WriteLine("Aguardando respostas de {0} sockets ", waitingForResponses);

                Console.CursorTop = top;
            }
        }

    }
}