using System.Drawing;

namespace CognitiveServicesDemo.Extensions.CognitiveServices
{
    public class FaceRectangle
    {
        public int Left { get; set; }

        public int Top { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public Rectangle ToRectangle()
        {
            return new Rectangle(Left, Top, Width, Height);
        }
    }
}