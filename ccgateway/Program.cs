/*
    Copyright(c) 2018 Gluwa, Inc.

    This file is part of Creditcoin.

    Creditcoin is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Lesser General Public License for more details.
    
    You should have received a copy of the GNU Lesser General Public License
    along with Creditcoin. If not, see <https://www.gnu.org/licenses/>.
*/

using ccplugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text;

namespace ccgateway
{
    class Program
    {
        static void Main(string[] args)
        {
            bool verboseLogging = false;
            if(args.Length > 0)
            {
                if(args[0] == "-v")
                {
                    verboseLogging = true;
                    Console.WriteLine($"{getTimestamp()}: Argument '-v' found -- Verbose logging enabled.");
                }
            }

            IConfiguration config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true, false)
                    .AddJsonFile("appsettings.dev.json", true, true)
                .Build();

            string root = Directory.GetCurrentDirectory();
            string folder = TxBuilder.GetPluginsFolder(root);
            if (folder == null)
            {
                Console.WriteLine("Failed to locate plugin folder");
                return;
            }

            string ip = config["bindIP"];
            if (string.IsNullOrWhiteSpace(ip))
            {
                Console.WriteLine("bindIP is not set.. defaulting to 127.0.0.1 local connection only");
                ip = "127.0.0.1";
            }

            var loader = new Loader<ICCGatewayPlugin>();
            var msgs = new List<string>();

            loader.Load(folder, msgs);
            foreach (var msg in msgs)
            {
                Console.WriteLine($"{getTimestamp()}: {msg}");
            }

            using (var socket = new ResponseSocket())
            {
                socket.Bind($"tcp://{ip}:55555");

                while (true)
                {
                    string response;
                    string requestString = null;
                    try
                    {
                        requestString = socket.ReceiveFrameString();

                        string[] command = requestString.Split();

                        if (command.Length < 2)
                        {
                            response = "poor";
                            Console.WriteLine($"{getTimestamp()}: Not enough parameters ({requestString}).");
                        }
                        else
                        {
                            string action = command[0];
                            if (verboseLogging)
                                Console.WriteLine($"{getTimestamp()}: Processing action '{action}' ({requestString}).");
                            ICCGatewayPlugin plugin = loader.Get(action);

                            if (plugin == null)
                            {
                                if (verboseLogging)
                                    Console.WriteLine($"{getTimestamp()}: Plugin for {action} not found!");
                                response = "miss";
                            }
                            else
                            {
                                command = command.Skip(1).ToArray();
                                var pluginConfig = config.GetSection(action);
                                if (verboseLogging)
                                    Console.WriteLine($"{getTimestamp()}: Processing request '{requestString}'.");

                                string msg;
                                bool done = plugin.Run(pluginConfig, command, out msg);
                                if (done)
                                {
                                    if (verboseLogging)
                                        Console.WriteLine($"{getTimestamp()}: Request '{requestString}' succeeded.");
                                    Debug.Assert(msg == null);
                                    response = "good";
                                }
                                else
                                {
                                    Debug.Assert(msg != null);
                                    StringBuilder err = new StringBuilder();
                                    err.Append(requestString).Append(": ").Append(msg);
                                    Console.WriteLine($"{getTimestamp()}: {err.ToString()}");
                                    response = "fail";
                                }
                            }
                        }
                    }
                    catch (Exception x)
                    {
                        StringBuilder err = new StringBuilder();
                        if (requestString != null)
                        {
                            err.Append(requestString).Append(": ");
                        }
                        err.Append(x.Message);
                        Console.WriteLine($"{getTimestamp()}: {err.ToString()}");
                        response = "fail";
                    }
                    socket.SendFrame(response);
                }
            }
        }

        static string getTimestamp(){
            return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
        }
    }
}