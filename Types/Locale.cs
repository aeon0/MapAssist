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

using System.Collections.Generic;

namespace MapAssist.Types
{
    public enum Locale : uint
    {
        enUS,
        esMX,
        ptBR,
        frFR,
        deDE,
        esES,
        itIT,
        ruRU,
        plPL,
        koKR,
        jaJP,
        zhCN,
    };

    public static class LocaleExtensions
    {
        private readonly static Dictionary<Locale, string> _localeNames = new Dictionary<Locale, string>()
        {
            [Locale.enUS] = "English (United States)",
            [Locale.esMX] = "Spanish (Mexico)",
            [Locale.ptBR] = "Portuguese (Brazil)",
            [Locale.frFR] = "French (France)",
            [Locale.deDE] = "German (Germany)",
            [Locale.esES] = "Spanish (Spain)",
            [Locale.itIT] = "Italian (Italy)",
            [Locale.ruRU] = "Russian (Russia)",
            [Locale.plPL] = "Polish (Poland)",
            [Locale.koKR] = "Korean (South Korea)",
            [Locale.jaJP] = "Japanese (Japan)",
            [Locale.zhCN] = "Chinese (China)",
        };

        public static string Name(this Locale locale)
        {
            return _localeNames.TryGetValue(locale, out var localeName) ? localeName : locale.ToString();
        }

        public static bool IsValid(this Locale locale)
        {
            var intLocale = (uint)locale;
            return intLocale >= 0 && intLocale <= 11;
        }
    }
}
