using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace KTerminalLib
{
    /// <summary>
    /// Парсер для KTerminal
    /// </summary>
    public static class KTerminalStringParser
    {
        #region ПАРАМЕТРЫ ПАРСЕРА
        /// <summary>
        /// char-код символа = 5
        /// </summary>
        public const char ЦВЕТ = (char)5;
        /// <summary>
        /// char-код символа = 6
        /// </summary>
        public const char ЖИРНОСТЬ = (char)6;
        /// <summary>
        /// char-код символа = 16
        /// </summary>
        public const char КУРСИВ = (char)16;
        /// <summary>
        /// char-код символа = 26
        /// </summary>
        public const char КОНЕЦ_ФОРМАТ = (char)26;
        /// <summary>
        /// char-код символа 'r'
        /// стирает последнюю строку
        /// </summary>
        public const char ВОЗВРАТ_КАРЕТКИ = '\r';//стирает последнюю строку
        /// <summary>
        /// char-код символа 'n'
        /// переходит к новой строке и стирание
        /// </summary>
        public const char ПЕРЕНОС_КАРЕТКИ = '\n';//переходит к новой строке
        /// <summary>
        /// Перевод курсора на строчку N и её стирание
        /// </summary>
        public const char ПЕРЕВОД_КУРСОРА = (char)27;
        /// <summary>
        /// Очистка экранного буфера
        /// </summary>
        public const char ОЧИСТИТЬ_ЭКРАН = (char)28;
        #endregion
        #region ФУНКЦИИ ПАРСЕРА
        /// <summary>
        /// Получить строку без служебных символов
        /// </summary>
        /// <param name="строка">Строка</param>
        /// <returns>Строка без служебных символов</returns>
        public static string ПолучитьЧистуюСтроку(string строка)
        {
            string результат = строка;
            //Удаление цвета
            int индексПервСимв = -1;
            while (true)
            {
                if (индексПервСимв == -1) { индексПервСимв = результат.IndexOf(ЦВЕТ.ToString()); if (индексПервСимв == -1) break; результат = результат.Remove(индексПервСимв, 1); }
                else { if (результат.IndexOf(ЦВЕТ.ToString()) == -1) break; результат = результат.Remove(индексПервСимв, результат.IndexOf(ЦВЕТ.ToString()) - индексПервСимв); индексПервСимв = -1; }
            }
            //Удаление остальных служебных символов
            string[] temp = результат.Split(new char[] { ЖИРНОСТЬ, КУРСИВ, КОНЕЦ_ФОРМАТ });
            результат = string.Empty;
            for (int i = 0; i < temp.Length; i++)
                результат += temp[i];
            return результат;
        }
        /// <summary>
        /// Разбивает строку на токены (по служебным символам)
        /// </summary>
        /// <param name="строка">строка</param>
        /// <returns>список текстовых токенов</returns>
        public static List<string> РазбитьСтрокуНаТокены(string строка)
        {
            List<string> списокТокенов = new List<string>();
            int индексПервСимв = -1;
            bool IsSpecialText = false;
            for (int индексСимв = 0; индексСимв < строка.Count(); индексСимв++)
                switch (строка[индексСимв])
                {
                    case ЦВЕТ:
                    case ПЕРЕВОД_КУРСОРА:
                        if ((индексПервСимв != -1) && (!IsSpecialText))
                        {
                            списокТокенов.Add(строка.Substring(индексПервСимв, индексСимв - индексПервСимв));
                        }
                        if (!IsSpecialText) { IsSpecialText = true; индексПервСимв = индексСимв; continue; }
                        else { IsSpecialText = false; if (индексПервСимв != -1) списокТокенов.Add(строка.Substring(индексПервСимв, индексСимв - индексПервСимв + 1)); индексПервСимв = -1; }
                        break;
                    case ЖИРНОСТЬ:
                    case КУРСИВ:
                    case КОНЕЦ_ФОРМАТ:
                    case ОЧИСТИТЬ_ЭКРАН:
                    case ВОЗВРАТ_КАРЕТКИ:
                    case ПЕРЕНОС_КАРЕТКИ:
                        if (индексПервСимв != -1)
                        {
                            списокТокенов.Add(строка.Substring(индексПервСимв, индексСимв - индексПервСимв));
                        }
                        индексПервСимв = индексСимв;
                        списокТокенов.Add(строка.Substring(индексПервСимв,1));
                        индексПервСимв = -1;
                        break;
                    default:
                        if (IsSpecialText) continue;
                        if (индексПервСимв == -1) индексПервСимв = индексСимв;
                        break;
                }
            if (индексПервСимв != -1)
            {
                списокТокенов.Add(строка.Substring(индексПервСимв, строка.Count() - индексПервСимв));
            }
            return списокТокенов;
        }
        /// <summary>
        /// Проверка токена строковый или управляющий
        /// </summary>
        /// <param name="токен">токен</param>
        /// <returns>признак Управляющего окена</returns>
        public static bool IsToken(string токен)
        {
            switch (токен[0])
            {
                case ЦВЕТ:
                case ПЕРЕВОД_КУРСОРА:
                    if (токен[0].Equals(токен[токен.Length - 1]) && (токен.Length > 1))
                        return true;
                    else
                        return false;
                case ЖИРНОСТЬ:
                case КУРСИВ:
                case КОНЕЦ_ФОРМАТ:
                case ОЧИСТИТЬ_ЭКРАН:
                case ВОЗВРАТ_КАРЕТКИ:
                case ПЕРЕНОС_КАРЕТКИ:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Получает значение цвета из токена ЦВЕТ '5' RRGGBB '5'
        /// </summary>
        /// <param name="токен">строковый токен</param>
        /// <returns>Цвет</returns>
        public static Brush ПолучитьЦвет(string токен)
        {
            if (токен[0].Equals(ЦВЕТ) && (токен[токен.Length - 1].Equals(ЦВЕТ)))
            {
                if ((токен.Length - 2) < 0) return Brushes.Black;
                string значениеЦвета = токен.Substring(1, токен.Length - 2).Trim();
                if (значениеЦвета.Length != 6) return Brushes.Black;
                byte a,b,c;
                if (!byte.TryParse(значениеЦвета.Substring(0,2),System.Globalization.NumberStyles.AllowHexSpecifier, null, out a)) return Brushes.Black;
                if (!byte.TryParse(значениеЦвета.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier, null, out b)) return Brushes.Black;
                if (!byte.TryParse(значениеЦвета.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier, null, out c)) return Brushes.Black;
                Brush цвет = new SolidColorBrush(Color.FromRgb(a,b,c));
                return цвет;
            }
            else
                return Brushes.Black;
        }
        /// <summary>
        /// Получить значение индекса строки из токена '27' DEC '27'
        /// </summary>
        /// <param name="токен">строковый токен</param>
        /// <returns>Индекс</returns>
        public static int ПолучитьИндексСтроки(string токен)
        {
            if (токен[0].Equals(ПЕРЕВОД_КУРСОРА) && (токен[токен.Length - 1].Equals(ПЕРЕВОД_КУРСОРА)))
            {
                if ((токен.Length - 2) < 0) return 0;
                string значениеИндексаСтроки = токен.Substring(1, токен.Length - 2).Trim();
                int индексСтроки = 0;
                if (int.TryParse(значениеИндексаСтроки, out индексСтроки)) return индексСтроки;
                return 0;
            }
            else
                return 0;
        }
        #endregion
    }
}
