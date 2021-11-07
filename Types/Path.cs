﻿/**
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

using System;
using MapAssist.Helpers;
using MapAssist.Interfaces;

namespace MapAssist.Types
{
    public class Path : IUpdatable<Path>
    {
        private readonly IntPtr _pPath = IntPtr.Zero;
        private Structs.Path _path;

        public Path(IntPtr pPath)
        {
            _pPath = pPath;
            Update();
        }

        public Path Update()
        {
            using (var processContext = GameManager.GetProcessContext())
            {
                _path = processContext.Read<Structs.Path>(_pPath);
            }
            return this;
        }

        public ushort DynamicX { get => _path.DynamicX; }
        public ushort DynamicY { get => _path.DynamicY; }
        public ushort StaticX { get => _path.StaticX; }
        public ushort StaticY { get => _path.StaticY; }
        public Room Room { get => new Room(_path.pRoom); }
}
}
