using System.Windows.Input;

namespace MicMuter.Code
{
    static internal class KeyNamer
    {
        public static string GetKeyDisplayName(Key key)
        {
            switch (key)
            {
                case Key.Oem8:
                    return "Tilder";
                default:
                    return key.ToString();
            }
        }
    }
}
