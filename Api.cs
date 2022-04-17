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

using GameOverlay;
using MapAssist.Helpers;
using MapAssist.Types;
using System;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using static MapAssist.Types.Stats;


namespace MapAssist
{
    public class Api : IDisposable
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private GameDataReader _gameDataReader;
        private AreaData _areaData;
        private volatile GameData _gameData;
        private List<PointOfInterest> _pointsOfInterest;
        private string _currentArea = "";
        private int _currentMapHeight = 0;
        private int _currentMapWidth = 0;
        private bool _disposed;
        private static readonly object _lock = new object();

        // Server data
        private HttpListener listener;
        private string url = "http://localhost:1111/";

        public Api()
        {
            _gameDataReader = new GameDataReader();
            TimerService.EnableHighPrecisionTimers();
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
                Console.WriteLine("Request: {0}", req.Url.AbsoluteUri);

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                var jsonData = "{\"success\": \"false\"}";
                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/get_data"))
                {
                    if (_disposed) return;
                    try
                    {
                        lock (_lock)
                        {
                            // Check if forcemap is set
                            var forceMap = false;
                            if (req.QueryString.Count != 0 && req.QueryString.Get(0) == "true")
                            {
                                forceMap = true;
                                Console.WriteLine("Force Map update");
                            }
                            
                            (_gameData, _areaData, _pointsOfInterest, _) = _gameDataReader.Get();
                            if (_gameData != null && _areaData != null && _pointsOfInterest != null)
                            {
                                // Figure out if we should update collison grid or not
                                var current_area = _areaData.Area.ToString();
                                var mapH = 0;
                                var mapW = 0;
                                if (_areaData.CollisionGrid != null)
                                {
                                    mapH = _areaData.CollisionGrid.GetLength(0);
                                    if (mapH > 0)
                                    {
                                        mapW = _areaData.CollisionGrid[0].GetLength(0);
                                    }
                                }
                                var map_changed = current_area != _currentArea || mapH != _currentMapHeight || mapW != _currentMapWidth;
                                _currentArea = current_area;
                                _currentMapHeight = mapH;
                                _currentMapWidth = mapW;

                                // Create msg
                                var msg = new
                                {
                                    success = true,
                                    monsters = new List<dynamic>(),
                                    objects = new List<dynamic>(),
                                    points_of_interest = new List<dynamic>(),
                                    player_pos = _gameData.PlayerPosition,
                                    area_origin = _areaData.Origin,
                                    collision_grid = map_changed || forceMap ? _areaData.CollisionGrid : null,
                                    current_area = _areaData.Area.ToString(),
                                };

                                foreach (UnitMonster m in _gameData.Monsters)
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

                                foreach (PointOfInterest p in _pointsOfInterest)
                                {
                                    msg.points_of_interest.Add(new
                                    {
                                        position = p.Position,
                                        type = p.Type,
                                        label = p.Label
                                    });
                                }

                                foreach (UnitObject o in _gameData.Objects)
                                {
                                    msg.objects.Add(new
                                    {
                                        position = o.Position,
                                        id = o.UnitId,
                                        selectable = o.ObjectData.InteractType != 0x00,
                                        name = ((GameObject)o.TxtFileNo).ToString()
                                    });
                                }

                                jsonData = JsonConvert.SerializeObject(msg);
                            }
                            else
                            {
                                _currentArea = "";
                                _currentMapHeight = 0;
                                _currentMapWidth = 0;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex);
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

        public void Dispose()
        {
            // Close the listener
            listener.Close();
            lock (_lock)
            {
                if (!_disposed)
                {
                    _disposed = true; // This first to let GraphicsWindow.DrawGraphics know to return instantly
                    // if (_compositor != null) _compositor.Dispose(); // This last so it's disposed after GraphicsWindow stops using it
                }
            }
        }
    }
}
