using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KTerminalLib.Blocks;
using System.Globalization;

namespace KTerminalLib
{
    /// <summary>
    /// Визуальный WPF-компонент Терминал (со служебными управляющими символамии).
    /// Автор: Колосов В.В.
    /// 
    /// (WPF, DonNetFramework 4)
    /// 
    ///'\5 RRGGBB \5' - цвет шрифта (RRGGBB - шестнадцатеричное число)
    ///'\6' - жирность
    ///'\16' - курсив
    ///'\26' - конец форматирования
    ///'\r' - стирание текущей строки
    ///'\n' - переход на следующую строку и стирание
    ///'\27 DEC \27' - переход на строку N (c 0) и стирание, N - десятичное число (если N == -1, то переход на последний символ последней строки)
    ///'\28' - очистка экрана
    /// </summary>
    public partial class KTerminal : UserControl
    {
        #region КоличествоСтрокЭкранногоБуфера
            /// <summary>
            /// Свойство - Количество Строк Экранного Буфера
            /// </summary>
            public static readonly DependencyProperty КоличествоСтрокЭкранногоБуфераProperty;
            /// <summary>
            /// Допустимое Количество Строк Экранного Буфера
            /// </summary>
            public int КоличествоСтрокЭкранногоБуфера
            {
                get { return (int)GetValue(КоличествоСтрокЭкранногоБуфераProperty); }
                set { SetValue(КоличествоСтрокЭкранногоБуфераProperty, value); }
            }
            /// <summary>
            /// Проверка значения Количества Строк Экранного Буфера
            /// </summary>
            /// <param name="value">количество</param>
            /// <returns>признак валидации</returns>
            private static bool ValidateКоличествоСтрокЭкранногоБуфера(object value)
            {
                return ((int)value <= 0) ? false : true;
            }
            /// <summary>
             /// Обработка при изменении свойтва Количество Строк Экранного Буфера
            /// </summary>
            private static void КоличествоСтрокЭкранногоБуфераChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
            {
                KTerminal control = (KTerminal)depObj;
                control.ПроконтролироватьКоличествоСтрок();
                control.InvalidateVisual();
            }
            /// <summary>
            /// Корректировка значения Количество Строк Экранного Буфера
            /// </summary>
            private static object CoerceКоличествоСтрокЭкранногоБуфера(DependencyObject depObj, object value)
            {
                return ((int)value <= 0) ? 1 : value;
            }
        #endregion
        #region Цвет фона
            /// <summary>
            /// Свойство - Цвет фона
            /// </summary>
            public static readonly DependencyProperty ЦветФонаProperty;
            /// <summary>
            /// Цвет фона
            /// </summary>
            public Brush ЦветФона
            {
                get { return (Brush)GetValue(ЦветФонаProperty); }
                set { SetValue(ЦветФонаProperty, value); }
            }
            /// <summary>
            /// Реакция на изменение цвета фона
            /// </summary>
            private static void ЦветФонаChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
            {
                KTerminal control = (KTerminal)depObj;
                control.InvalidateVisual();
            }
        #endregion
        /// <summary>
        /// Статический конструктор
        /// </summary>
        static KTerminal()
        {
            КоличествоСтрокЭкранногоБуфераProperty = DependencyProperty.Register("КоличествоСтрокЭкранногоБуфера", typeof(int), typeof(KTerminal), 
                new PropertyMetadata(1000, new PropertyChangedCallback(КоличествоСтрокЭкранногоБуфераChanged), new CoerceValueCallback(CoerceКоличествоСтрокЭкранногоБуфера)), 
                new ValidateValueCallback(ValidateКоличествоСтрокЭкранногоБуфера));
            ЦветФонаProperty = DependencyProperty.Register("ЦветФона", typeof(Brush), typeof(KTerminal),
                new PropertyMetadata(SystemColors.WindowBrush, new PropertyChangedCallback(ЦветФонаChanged)));
        }
        /// <summary>
        /// Внутренний буфер, содержащий список строк
        /// </summary>
        private List<string> СписокСтрок { get; set; }
        /// <summary>
        /// Индекс первой видимой строки на экране
        /// </summary>
        private int ИндексПервойВидимойСтроки { get; set; }
        /// <summary>
        /// Количество видимых строк на экране
        /// </summary>
        private int КоличествоВидимыхСтрок { get; set; }
        /// <summary>
        /// Конструктор
        /// </summary>
        public KTerminal()
        {
            InitializeComponent();
            СписокСтрок = new List<string>(КоличествоСтрокЭкранногоБуфера);
            СписокСтрок.Add(string.Empty);
            СписокПрямоугольныхОбластейВидимыхСтрок = new List<Rect>();
            ИндексПервойВидимойСтроки = -1;
            КоличествоВидимыхСтрок = -1;
            this.Background = new SolidColorBrush(Color.FromArgb(0,0,0,0));//костыль
            ПозицияКурсора_Строка = 0;
            ПозицияКурсора_Столбец = 0;
        }
        /// <summary>
        /// Расчёт оптимального индекса видимой строки
        /// </summary>
        private void ПосчитатьОптимальнуюВидимуюСтроку()
        {
            //Текущая фактическая высота области рисования
            double currentHeight = this.RenderSize.Height - this.hScrollBar.RenderSize.Height;
            //форматируем текст для рисования
            FormattedText txt = new FormattedText("Пробный текст", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily,
                    FontStyles.Normal,
                    FontWeights.Normal,
                    FontStretches.Normal), this.FontSize, this.Foreground);
            int КоличествоВозможныхВидимыхЯчеек = (int)(currentHeight / txt.Height);
            if ((СписокСтрок.Count - КоличествоВозможныхВидимыхЯчеек) < 0)
            {
                this.vScrollBar.Value = 0;
            }
            else
            {
                this.vScrollBar.Value = СписокСтрок.Count - КоличествоВозможныхВидимыхЯчеек;
            }            
            ИндексПервойВидимойСтроки = (int)this.vScrollBar.Value;
            if (ПозицияКурсора_Строка < ИндексПервойВидимойСтроки)
            {
                this.vScrollBar.Value = ПозицияКурсора_Строка;
                ИндексПервойВидимойСтроки = ПозицияКурсора_Строка;
            }
        }
        /// <summary>
        /// Уонтроль количества строк (при необходимости удаление)
        /// </summary>
        private void ПроконтролироватьКоличествоСтрок()
        {
            while (СписокСтрок.Count > КоличествоСтрокЭкранногоБуфера)
            {
                СписокСтрок.RemoveAt(0);
                ПозицияКурсора_Строка--;
            }
        }
        /// <summary>
        /// Получить список строк 
        /// без служебных символов
        /// </summary>
        /// <returns>список строк</returns>
        public List<string> ПолучитьСписокСтрок()
        {
            List<string> списокСтрок = new List<string>();
            foreach (string строка in СписокСтрок)
                списокСтрок.Add(KTerminalStringParser.ПолучитьЧистуюСтроку(строка));
            return списокСтрок;
        }
        /// <summary>
        /// Получает список строк, видимых на экране 
        /// без служебных символов
        /// </summary>
        /// <returns></returns>
        public List<string> ПолучитьВидимыйСписокСтрок()
        {
            if ((ИндексПервойВидимойСтроки < 0) || (КоличествоВидимыхСтрок < 1))
                return null;
            List<string> списокВидимыхСтрок = new List<string>();
            for (int i = ИндексПервойВидимойСтроки; i < (КоличествоВидимыхСтрок + ИндексПервойВидимойСтроки); i++)
                списокВидимыхСтрок.Add(KTerminalStringParser.ПолучитьЧистуюСтроку(СписокСтрок[i]));
            return списокВидимыхСтрок;
        }
        /// <summary>
        /// Индекс активной строки
        /// </summary>
        private int ПозицияКурсора_Строка { get; set; }
        /// <summary>
        /// Индекс активного символа
        /// </summary>
        private int ПозицияКурсора_Столбец { get; set; } 
        /// <summary>
        /// Добавление массива символов
        /// в буфер терминала
        /// </summary>
        /// <param name="массив">массив символов</param>
        public void ДобавитьВСписокСтрок(char[] массив)
        {
            if ((ПозицияКурсора_Строка < 0) || (ПозицияКурсора_Строка >= СписокСтрок.Count)) return;
            List<string> списокТокенов = KTerminalStringParser.РазбитьСтрокуНаТокены(СписокСтрок[ПозицияКурсора_Строка] + new string(массив));
            СписокСтрок[ПозицияКурсора_Строка] = string.Empty;
            ПозицияКурсора_Столбец = 0;
            /*Разбор входной строки*/
            foreach (string токен in списокТокенов)
                switch (токен[0])
                {
                    case KTerminalStringParser.ОЧИСТИТЬ_ЭКРАН:
                        СписокСтрок.Clear();
                        СписокСтрок.Add(string.Empty);
                        ПозицияКурсора_Строка = 0;
                        ПозицияКурсора_Столбец = 0;
                        break;
                    case KTerminalStringParser.ВОЗВРАТ_КАРЕТКИ:
                        СписокСтрок[ПозицияКурсора_Строка] = string.Empty;
                        ПозицияКурсора_Столбец = 0;
                        break;
                    case KTerminalStringParser.ПЕРЕНОС_КАРЕТКИ:
                        ПозицияКурсора_Строка += 1;
                        if (ПозицияКурсора_Строка >= СписокСтрок.Count)
                            СписокСтрок.Add(string.Empty);
                        else
                            СписокСтрок[ПозицияКурсора_Строка] = string.Empty;                       
                        ПозицияКурсора_Столбец = 0;
                        break;
                    case KTerminalStringParser.ПЕРЕВОД_КУРСОРА:
                        if (KTerminalStringParser.IsToken(токен))
                        {
                            ПозицияКурсора_Строка = KTerminalStringParser.ПолучитьИндексСтроки(токен);
                            if (ПозицияКурсора_Строка >= СписокСтрок.Count)
                            { 
                                ПозицияКурсора_Строка = this.СписокСтрок.Count - 1;
                                continue; }
                            if (ПозицияКурсора_Строка < 0)
                            {
                                ПозицияКурсора_Строка = СписокСтрок.Count - 1;
                                int temp = (СписокСтрок[ПозицияКурсора_Строка].Length - 1);
                                ПозицияКурсора_Столбец = (temp < 0) ? 0 : temp;
                            }
                            else
                            {
                                ПозицияКурсора_Столбец = 0;
                                СписокСтрок[ПозицияКурсора_Строка] = string.Empty;
                            }
                        }
                        else
                            goto ДОБАВЛЕНИЕ_ТОКЕНА;
                        break;
                    default:ДОБАВЛЕНИЕ_ТОКЕНА:
                        if (СписокСтрок[ПозицияКурсора_Строка].Length >= ПозицияКурсора_Столбец)
                            if (токен.Length < (СписокСтрок[ПозицияКурсора_Строка].Length - ПозицияКурсора_Столбец))
                            {
                                СписокСтрок[ПозицияКурсора_Строка] = СписокСтрок[ПозицияКурсора_Строка].Remove(ПозицияКурсора_Столбец, токен.Length);
                            }
                            else
                            {
                                СписокСтрок[ПозицияКурсора_Строка] = СписокСтрок[ПозицияКурсора_Строка].Remove(ПозицияКурсора_Столбец, СписокСтрок[ПозицияКурсора_Строка].Length - ПозицияКурсора_Столбец);
                            }
                        СписокСтрок[ПозицияКурсора_Строка] = СписокСтрок[ПозицияКурсора_Строка].Insert(ПозицияКурсора_Столбец, токен);
                        ПозицияКурсора_Столбец += токен.Length;
                        break;
                }
            /*Конец Разбора входной строки*/
            ПроконтролироватьКоличествоСтрок();
            this.vScrollBar.Minimum = 0;
            this.vScrollBar.Maximum = this.СписокСтрок.Count - 2;
            ПосчитатьОптимальнуюВидимуюСтроку();
            this.InvalidateVisual();
            vScrollBar.IsEnabled = true;
        }
        /// <summary>
        /// Очищение буфера терминала
        /// </summary>
        public void ОчиститьСписокСтрок()
        {
            СписокСтрок.Clear();
            СписокСтрок.Add(string.Empty);
            ПозицияКурсора_Строка = 0;
            this.vScrollBar.Maximum = this.СписокСтрок.Count - 1; 
            this.vScrollBar.Value = this.vScrollBar.Maximum; 
            this.InvalidateVisual();
            vScrollBar.IsEnabled = false;
        }

        #region ОБРАБОТКА ИЗМЕНЕНИЯ СТАНДАРТНЫХ СВОЙТСВ
        /// <summary>
        /// реакция на изменение свойств зависимостей
        /// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            //изменение размера шрифта
            if (e.Property.Equals(KTerminal.FontSizeProperty))
            {
                ПосчитатьОптимальнуюВидимуюСтроку();
            }
        } 
        #endregion

        #region РЕНДЕРИНГ
        /// <summary>
        /// Ширина курсора
        /// </summary>
        protected const double ШиринаКурсора = 1;
        /// <summary>
        /// Список прямоугольных областей,
        /// видимых строк
        /// </summary>
        private List<Rect> СписокПрямоугольныхОбластейВидимыхСтрок { get; set; }
        /// <summary>
        /// Преобразование текстовой строки в
        /// специальный блок данных с параметрами форматирования
        /// </summary>
        /// <param name="строка">строка</param>
        /// <param name="index">индекс строки</param>
        /// <returns>специальный блок</returns>
        BlockBase ConvertToBlockTemplate(string строка, int index)
        {
            BlockKTerminalLine блокСтроки = new BlockKTerminalLine();
            блокСтроки.IsClipping = false;
            блокСтроки.ColorBackground = this.ЦветФона;
            блокСтроки.ColorFont = this.Foreground;
            блокСтроки.FontFamily = this.FontFamily;
            блокСтроки.FontSize = this.FontSize;
            блокСтроки.Text = строка;
            return блокСтроки;
        }
        /// <summary>
        /// Рендеринг компонента
        /// </summary>
        /// <param name="drawingContext">контекст рисования</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            #region Алгоритм подготовки к рисованию
            base.OnRender(drawingContext);
            //Текущая фактическая высота области рисования
            double currentHeight = this.RenderSize.Height - this.hScrollBar.RenderSize.Height;
            //Текущая фактическая ширина области рисования
            double currentWidth = this.RenderSize.Width - this.vScrollBar.RenderSize.Width;
            //Область рисования (вне этой области рисовать нельзя)
            Size clipSize = new Size(currentWidth, currentHeight);
            //ограничиваем
            drawingContext.PushClip(new RectangleGeometry(new Rect(new Point(0, 0), clipSize)));
            //очищаем фон
            drawingContext.DrawRectangle(this.ЦветФона, null, new Rect(new Point(0, 0), clipSize));
            //Проверка на кол-во строк
            if (this.СписокСтрок.Count <= 0) return;
            //текущий индекс рисуемой строки
            int индекс = this.ИндексПервойВидимойСтроки;
            //текущая точка рисования на канвасе компонента
            Point текущаяТочкаРендеринга = new Point(0, 0);
            //учёт горизонтального скроллинга (если ползунок передвинут) (в случае когда не помещается полностью элемент)
            текущаяТочкаРендеринга.X = -this.hScrollBar.Value;
            //обнуляем вертикальный скролбар (нечего отображать)
            this.hScrollBar.Maximum = 0;
            //очистка Списка областей (прямоугольников), в которых отрисованы видымые элементы списка SourceData
            this.СписокПрямоугольныхОбластейВидимыхСтрок.Clear(); 
            #endregion
            #region Цикл отрисовки видимых блоков (элементов) - пока они видны на экране (канвасе компонента)
            while (текущаяТочкаРендеринга.Y < currentHeight)
            {
                //Проверка индексов на допустимость
                if (индекс < 0) return;
                //Обработка ситуации превышения текущего индекса над общим кол-вом строк
                if (индекс >= this.СписокСтрок.Count) break;

                //преобразоваем элемент пользовательского типа в универсальный шаблон отображения
                BlockBase блокТекущейРисуемойСтроки = this.ConvertToBlockTemplate(this.СписокСтрок[индекс], индекс);

                //рендерим шаблон
                DrawingVisual визуальныйБуферТекущейРисуемойСтроки = блокТекущейРисуемойСтроки.Render(текущаяТочкаРендеринга);
                //рисуем его на канвасе компонента
                drawingContext.DrawDrawing(визуальныйБуферТекущейРисуемойСтроки.Drawing);
                //область рисования текущего шаблона
                Rect прямоугольнаяОбластьТекущейРисуемойСтроки = new Rect(текущаяТочкаРендеринга, блокТекущейРисуемойСтроки.RenderSize);
                //добавляем его в список (пригодится для реализации клика по элементу)
                this.СписокПрямоугольныхОбластейВидимыхСтрок.Add(прямоугольнаяОбластьТекущейРисуемойСтроки);

                //выбираем самую длинную ширину (самы длинный элемент) (для реализации горизонтального скроллинга)
                double deltaWidth = блокТекущейРисуемойСтроки.RenderSize.Width - currentWidth;
                if (deltaWidth > 0)
                    if (this.hScrollBar.Maximum <= deltaWidth) { this.hScrollBar.Maximum = deltaWidth; hScrollBar.IsEnabled = true; }

                //переходим вниз, на свободное место для рисования
                текущаяТочкаРендеринга.Y += блокТекущейРисуемойСтроки.RenderSize.Height;
                //переход к следующей строке
                индекс++;
            } 
            #endregion
            #region Выделение активной строки - отрисовка курсора
            if ((ПозицияКурсора_Строка >= ИндексПервойВидимойСтроки) && (ПозицияКурсора_Строка < (ИндексПервойВидимойСтроки + СписокПрямоугольныхОбластейВидимыхСтрок.Count)))
            {
                //отрисовка курсора - по факту это прямоугольник :)))
                drawingContext.DrawRectangle
                    (
                        this.Foreground/*цвет курсора*/, null,
                        new Rect
                            (
                                СписокПрямоугольныхОбластейВидимыхСтрок[ПозицияКурсора_Строка - ИндексПервойВидимойСтроки].TopRight/*Положение курсора*/,
                                new Size
                                    (
                                        ШиринаКурсора/*установка ширины курсора*/,
                                        СписокПрямоугольныхОбластейВидимыхСтрок[ПозицияКурсора_Строка - ИндексПервойВидимойСтроки].Height/*Установка высоты курсора*/
                                    )
                            )
                    );
            } 
            #endregion
        } 
        #endregion

        #region СКРОЛЛИНГ
        /// <summary>
        /// Горизонтальный скроллинг
        /// </summary>
        private void hScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            InvalidateVisual();
        }
        /// <summary>
        /// Вертикальный скроллинг
        /// </summary>
        private void vScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.ИндексПервойВидимойСтроки = (int)e.NewValue;
            this.InvalidateVisual();
        }
        /// <summary>
        /// Горизонтальный скроллинг колёсиком мыши
        /// </summary>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            int oldVal = (int)vScrollBar.Value;
            int DELTA = e.Delta;
            if (DELTA < 0)
            {
                if ((vScrollBar.Value + vScrollBar.LargeChange) >= vScrollBar.Maximum)
                {
                    vScrollBar.Value = vScrollBar.Maximum;
                    return;
                }
                vScrollBar.Value += vScrollBar.LargeChange;
            }
            else
            {
                if ((vScrollBar.Value - vScrollBar.LargeChange) <= vScrollBar.Minimum)
                {
                    vScrollBar.Value = vScrollBar.Minimum;
                    return;
                }
                vScrollBar.Value -= vScrollBar.LargeChange;
            }
        }
        /// <summary>
        /// Обработка изменений размеров экрана
        /// </summary>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            ПосчитатьОптимальнуюВидимуюСтроку();
            this.InvalidateVisual();
            vScrollBar.IsEnabled = true;
        }
        #endregion
    }
}
