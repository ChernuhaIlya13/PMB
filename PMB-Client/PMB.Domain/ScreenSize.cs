namespace PMB.Cef.Core.FakeConfig
{
    public class ScreenSize
    {
        public ScreenSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public int Height { get; private set; }

        public int Width { get; private set; }

        public int WindowOuterHeight//Окно Наружная Высота
        {
            get
            {
                return this.Height - 40;
            }
        }

        public int WindowOuterWidth//Окно Наружная Ширина
        {
            get
            {
                return this.Width;
            }
        }

        public int ScreenAvailHeight//Высота экрана в наличии
        {
            get
            {
                return this.Height - 40;
            }
        }

        public int WindowScreenAvailWidth//Ширина оконного экрана
        {
            get
            {
                return this.Width;
            }
        }

        public int WindowScreenHeight//Высота экрана окна
        {
            get
            {
                return this.Height;
            }
        }

        public int WindowScreenWidth//Ширина экрана окна
        {
            get
            {
                return this.Width;
            }
        }
    }
}