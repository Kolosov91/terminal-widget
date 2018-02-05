using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Globalization;
using System.Windows;

namespace KTerminalLib.Blocks
{
    /// <summary>
    /// Шаблон блока-строка для KTerminal.
    /// Производит рендеринг содержимого.
    /// </summary>
    public class BlockKTerminalLine : BlockBase
    {
        /// <summary>
        /// Текстовая строка
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Цвет фона
        /// </summary>
        public Brush ColorBackground { get; set; }
        /// <summary>
        /// Цвет шрифта
        /// </summary>
        public Brush ColorFont { get; set; }
        /// <summary>
        /// размер шрифта
        /// </summary>
        public double FontSize { get; set; }
        /// <summary>
        /// Название шрифта
        /// </summary>
        public FontFamily FontFamily { get; set; }
        /// <summary>
        /// Конструктор
        /// </summary>
        public BlockKTerminalLine()
        {
        }

        /// <summary>
        /// Переопределённый Алгоритм рендеринга - обязательно
        /// </summary>
        /// <param name="dc"> контекст рисования </param>
        protected override void DataRender(DrawingContext dc)
        {
            List<string> токены = KTerminalStringParser.РазбитьСтрокуНаТокены(Text);
            currentBrush = ColorFont;
            текущаяТочкаРисования = this.RenderRect.Location;
            if (токены.Count == 0) { РИСОВАТЬ_СТРОКУ(" ", dc); return; }
            foreach (string токен in токены)
                switch (токен[0])
                {
                    case KTerminalStringParser.ЦВЕТ: currentBrush = KTerminalStringParser.ПолучитьЦвет(токен); break;
                    case KTerminalStringParser.ЖИРНОСТЬ: isBold = true; break;
                    case KTerminalStringParser.КУРСИВ: isItalic = true; break;
                    case KTerminalStringParser.КОНЕЦ_ФОРМАТ: currentBrush = ColorFont; isItalic = false; isBold = false; break;
                    default:
                        РИСОВАТЬ_СТРОКУ(токен, dc);
                        break;
                }
        }
        #region ПАРАМЕТРЫ РИСОВАНИЯ
        /// <summary>
        /// Признак курсива
        /// </summary>
            bool isItalic = false;
        /// <summary>
        /// признак выделения
        /// </summary>
            bool isBold = false;
        /// <summary>
        /// текущий цвет
        /// </summary>
            Brush currentBrush;
        /// <summary>
        /// текущая точка рисования
        /// </summary>
            Point текущаяТочкаРисования; 
        #endregion
        /// <summary>
            /// Отрисвоать строку
        /// </summary>
        /// <param name="токен">токен</param>
            /// /// <param name="dc"> контекст рисования </param>
        private void РИСОВАТЬ_СТРОКУ(string токен, DrawingContext dc)
        {
            //форматируем текст для рисования
            FormattedText txt = new FormattedText(токен, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily,
                    isItalic ? FontStyles.Italic : FontStyles.Normal,
                    isBold ? FontWeights.Bold : FontWeights.Normal,
                    FontStretches.Normal), FontSize, currentBrush);
            txt.TextAlignment = TextAlignment.Left;
            txt.MaxTextWidth = txt.Width;
            txt.MaxTextHeight = txt.Height;
            Rect текущийПрямоугольникРисования = new Rect(текущаяТочкаРисования, new Size(txt.MaxTextWidth, txt.MaxTextHeight));
            dc.DrawRectangle(ColorBackground, null, текущийПрямоугольникРисования);//рисуем фон
            dc.DrawText(txt, текущаяТочкаРисования);//рисуем текст
            //переход к след.
            текущаяТочкаРисования.X += текущийПрямоугольникРисования.Width;
            if (this.RenderRect.Height < текущийПрямоугольникРисования.Height) { this.RenderRect.Height = текущийПрямоугольникРисования.Height; }
            this.RenderRect.Width += текущийПрямоугольникРисования.Width;
        }
    }
}
