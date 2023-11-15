
using UnityEngine.UI;

namespace AD.Utility
{
    public static class ImageExtension 
    {
        public static Image SetColor_R(this Image self, float value)
        {
            self.color= self.color.Result_R(value);
            return self;
        }

        public static Image SetColor_G(this Image self, float value)
        {
            self.color = self.color.Result_G(value);
            return self;
        }

        public static Image SetColor_B(this Image self, float value)
        {
            self.color = self.color.Result_B(value);
            return self;
        }

        public static Image SetColor_A(this Image self, float value)
        {
            self.color = self.color.Result_A(value);
            return self;
        }
    }
}
