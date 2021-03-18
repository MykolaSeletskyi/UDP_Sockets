using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace _02_Chat_Server
{
    class Program
    {
        // порт для прослуховування
        private const int port = 8080;
        // список учасників чату
        //private static List<IPEndPoint> members = new List<IPEndPoint>();
        private static Dictionary<IPEndPoint, string> members = new Dictionary<IPEndPoint, string>();
        static void Main(string[] args)
        {
            // створення об'єкту UdpClient та встановлюємо порт для прослуховування
            UdpClient server = new UdpClient(port);
            // створюємо об'єкт для збреження адреси віддаленого хоста
            IPEndPoint groupEP = null;

            try
            {
                while (true)
                {
                    Console.WriteLine("\tWaiting for a message...");
                    byte[] bytes = server.Receive(ref groupEP);

                    // конвертуємо масив байтів в рядок
                    string msg = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                    bool isSuccesful;
                    if (msg.Substring(0,2) == "#1")
                    {
                        try
                        {
                            members.Values.Single(i => i == msg.Substring(2));
                            string str = $"NotUnique#";
                            byte[] strByteArr = Encoding.UTF8.GetBytes(str.ToString());
                            server.Send(strByteArr, strByteArr.Length, groupEP);
                            continue;
                        }
                        catch (InvalidOperationException) { }

                        isSuccesful = AddMember(groupEP, msg.Substring(2));
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Request to connect from {groupEP} at {DateTime.Now.ToShortTimeString()}\n");
                        Console.WriteLine($"Name : {msg.Substring(2)}");
                        if (isSuccesful)
                        {
                            Console.WriteLine($"Operation completed succesful!\n");

                            Console.ForegroundColor = ConsoleColor.Cyan;
                            string str = $"@{msg.Substring(2)} connected";
                            byte[] strByteArr= Encoding.UTF8.GetBytes(str);
                            foreach (var m in members)
                            {
                                try
                                {
                                    server.Send(strByteArr, strByteArr.Length, m.Key);
                                }
                                catch (Exception ex)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Error with {m}: {ex.Message}\n");
                                }
                            }
                        }
                    }
                    else if (msg.Substring(0, 2) == "#2")
                    {
                        string name = members[groupEP];
                        isSuccesful = RemoveMember(groupEP);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Request to leave from {groupEP} at {DateTime.Now.ToShortTimeString()}\n");
                        if (isSuccesful)
                        {
                            Console.WriteLine($"Operation completed succesful!\n");
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            string str = $"@{name} disconnected";
                            byte[] strByteArr = Encoding.UTF8.GetBytes(str);
                            foreach (var m in members)
                            {
                                try
                                {
                                    server.Send(strByteArr, strByteArr.Length, m.Key);
                                }
                                catch (Exception ex)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Error with {m}: {ex.Message}\n");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"Message from {groupEP} at {DateTime.Now.ToShortTimeString()}: {msg}\n");
                        string str = $"{members[groupEP]}#{msg.Substring(2)}";
                        byte[] strByteArr = Encoding.UTF8.GetBytes(str.ToString());
                        foreach (var m in members)
                        {
                            try
                            {
                                server.Send(strByteArr, strByteArr.Length, m.Key);
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Error with {m}: {ex.Message}\n");
                            }
                        }
                    }
                    Console.ResetColor();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                // закриття з'єднання
                server.Close();
            }
        }

        static bool AddMember(IPEndPoint endPoint,string name)
        {
            var member = members.FirstOrDefault(m => m.ToString() == endPoint.ToString());
            if (member.Value == null)
            {
                if (members.Values.Where(i=>i==name).Count()==0)
                {
                    members.Add(endPoint, name);
                    return true;
                }
            }
            return false;
        }
        static bool RemoveMember(IPEndPoint endPoint)
        {
            return members.Remove(endPoint);
        }
    }
}
