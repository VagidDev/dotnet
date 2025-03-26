using Color = Cairo.Color;

namespace Backend
{
    public class BackendService
    {
        public Color getRed()
        {
            return new Color(1, 0, 0);
        }

        public Color getGreen()
        {
            return new Color(0, 1, 0);
        }

        public Color getBlue()
        {
            return new Color(0, 0, 1);
        }

        public Color getPurple()
        {
            return new Color(1, 0, 1);
        }

        public Color getYellow()
        {
            return new Color(1, 1, 0);
        }

        public Color getWhite()
        {
            return new Color(1, 1, 1);
        }

        public Color getBlack()
        {
            return new Color(0, 0, 0);
        }

    }
}