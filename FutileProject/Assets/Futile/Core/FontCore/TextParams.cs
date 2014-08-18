namespace Futile.Core.FontCore
{
    public class TextParams
    {
        public float ScaledKerningOffset = 0;
        public float ScaledLineHeightOffset = 0;
        private float kerningOffset = 0;
        private float lineHeightOffset = 0;

        public float KerningOffset
        {
            get { return kerningOffset; }
            set
            {
                kerningOffset = value;
                ScaledKerningOffset = value;
            }
        }

        public float LineHeightOffset
        {
            get { return lineHeightOffset; }
            set
            {
                lineHeightOffset = value;
                ScaledLineHeightOffset = value;
            }
        }
    }
}