using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace KTerminalLib.Blocks
{
    /// <summary>
    /// Базовый шаблон отображения
    /// </summary>
    public class BlockBase
    {
        /// <summary>
        /// Область рисования
        /// </summary>
        protected Rect RenderRect;
        /// <summary>
        /// Буфер рисования
        /// </summary>
        protected DrawingVisual RenderBuffer;
        /// <summary>
        /// Размеры области рисования
        /// </summary>
        public Size RenderSize { get { return this.RenderRect.Size; } set { this.RenderRect.Size = value; } }
        /// <summary>
        /// Признак разрешения ограничения отрисовки
        /// </summary>
        public bool IsClipping { get; set; }
        /// <summary>
        /// Конструктор
        /// </summary>
        public BlockBase() { RenderRect = new Rect(); RenderSize = new Size(); RenderBuffer = new DrawingVisual(); IsClipping = true; }
        /// <summary>
        /// Базовый алгоритм рендеринга
        /// </summary>
        /// <param name="renderLocation"> точка отрисовки </param>
        /// <returns> буфер рисования </returns>
        public DrawingVisual Render(Point renderLocation)
        {
            using (DrawingContext dc = RenderBuffer.RenderOpen())
            {
                RenderRect.Location = renderLocation;
                if (IsClipping)
                    dc.PushClip(new RectangleGeometry(RenderRect));
                //вызываем функцию в которой описан алгоритм риcования
                DataRender(dc);
                dc.Close();
            }
            return RenderBuffer;
        }
        /// <summary>
        /// указатель на функцию с алгоритмом рисования
        /// </summary>
        protected virtual void DataRender(DrawingContext dc) 
        {
            string message = string.Format("Ошибка!\nНеобходимо переопределить функцию\n'void DataRender(DrawingContext dc)'\nв наследнике класса 'BlockBase'!"); 
            //форматируем текст для рисования
            FormattedText txt = new FormattedText(message, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface(SystemFonts.CaptionFontFamily, FontStyles.Normal, FontWeights.Normal,
                    FontStretches.Normal), SystemFonts.CaptionFontSize, Brushes.Red);
            txt.TextAlignment = TextAlignment.Left;
            txt.MaxTextWidth = txt.Width;
            txt.MaxTextHeight = txt.Height;
            Rect текущийПрямоугольникРисования = new Rect(new Point(0,0), new Size(txt.MaxTextWidth, txt.MaxTextHeight));
            dc.DrawRectangle(SystemColors.ControlBrush, null, текущийПрямоугольникРисования);//рисуем фон
            dc.DrawText(txt, new Point(0, 0));//рисуем текст
        }
    }
}
