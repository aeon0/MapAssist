/**
 *   Copyright (C) 2021 okaygo
 *
 *   https://github.com/misterokaygo/MapAssist/
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 **/

using MapAssist.Helpers;
using MapAssist.Types;
using System;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MapAssist
{
    public class Api : IDisposable
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private GameDataReader _gameDataReader;
        private GameData _gameData;
        private Compositor _compositor;
        private static readonly object _lock = new object();

        // Server data
        private HttpListener listener;
        private string url = "http://localhost:1111/";
        private int requestCount = 0;

        public Api()
        {
            _gameDataReader = new GameDataReader();
            GameOverlay.TimerService.EnableHighPrecisionTimers();
        }

        public async Task HandleIncomingConnections()
        {
            var runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                var jsonData = "{\"success\": \"false\"}";
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/get_data"))
                {
                    if (disposed) return;
                    try
                    {
                        lock (_lock)
                        {
                            (_compositor, _gameData) = _gameDataReader.Get();
                            if (_compositor != null && _gameData != null)
                            {
                                var msg = new
                                {
                                    success = true,
                                    monsters = new List<dynamic>(),
                                    objects = new List<dynamic>(),
                                    items = new List<dynamic>(),
                                    points_of_interest = new List<dynamic>(),
                                    player_pos = _gameData.PlayerPosition,
                                    area_origin = _compositor._areaData.Origin,
                                    collision_grid = _compositor._areaData.CollisionGrid,
                                    current_area = _compositor._areaData.Area.ToString()
                                };

                                foreach (UnitAny m in _gameData.Monsters)
                                {
                                    if (m.UnitType == UnitType.Monster)
                                    {
                                        //using (var processContext = GameManager.GetProcessContext())
                                        //{
                                        //    var stats = processContext.Read<MapAssist.Structs.MonStats>(m.MonsterData.pMonStats);
                                        //}
                                        msg.monsters.Add(new
                                        {
                                            position = m.Position,
                                            immunities = m.Immunities,
                                            unit_type = m.UnitType.ToString(),
                                            type = m.MonsterData.MonsterType.ToString(),
                                            id = m.UnitId,
                                            name = ((Npc)m.TxtFileNo).ToString()
                                        });
                                    }
                                }

                                foreach (PointOfInterest p in _compositor._pointsOfInterest)
                                {
                                    msg.points_of_interest.Add(new
                                    {
                                        position = p.Position,
                                        type = p.Type,
                                        label = p.Label
                                    });
                                }

                                foreach (UnitAny o in _gameData.Objects)
                                {
                                    if (o.UnitType == UnitType.Object)
                                    {
                                        msg.objects.Add(new
                                        {
                                            position = o.Position,
                                            id = o.UnitId,
                                            selectable = o.ObjectData.InteractType != 0x00,
                                            name = ((GameObject)o.TxtFileNo).ToString()
                                        });
                                    }
                                }

                                foreach (UnitAny i in _gameData.Items)
                                {
                                    if (i.UnitType == UnitType.Item)
                                    {
                                        msg.items.Add(new
                                        {
                                            position = i.Position,
                                            id = i.UnitId,
                                            flags = i.ItemData.ItemFlags,
                                            quality = i.ItemData.ItemQuality,
                                            name = Items.ItemName(i.TxtFileNo)
                                        });
                                    }
                                }

                                jsonData = JsonConvert.SerializeObject(msg);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex);
                        GameManager.ResetPlayerUnit();
                    }
                }

                // Write the response info
                var buffer = Encoding.ASCII.GetBytes(jsonData);
                resp.ContentLength64 = buffer.Length;
                resp.ContentType = "application/json";
                resp.ContentEncoding = Encoding.UTF8;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                resp.Close();
            }
        }

        public void runServer()
        {
            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();
        }

        ~Api() => Dispose();

        private bool disposed = false;

        public void Dispose()
        {
            // Close the listener
            listener.Close();
            lock (_lock)
            {
                if (!disposed)
                {
                    disposed = true; // This first to let GraphicsWindow.DrawGraphics know to return instantly
                    if (_compositor != null) _compositor.Dispose(); // This last so it's disposed after GraphicsWindow stops using it
                }
            }
        }
    }
}
