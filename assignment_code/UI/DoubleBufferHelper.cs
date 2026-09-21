using System;
using System.Reflection;
using System.Windows.Forms;

namespace assignment_code.UI
{
    public static class DoubleBufferHelper
    {
        private static readonly PropertyInfo _doubleBufferedProperty;

        static DoubleBufferHelper()
        {
            try
            {
                _doubleBufferedProperty = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            catch { }
        }

        /// <summary>
        /// Enables optimized double buffering on any control, eliminating screen tearing and white repaint flashes.
        /// </summary>
        public static void EnableDoubleBuffering(Control control)
        {
            if (control == null) return;

            try
            {
                _doubleBufferedProperty?.SetValue(control, true, null);
            }
            catch { }

            // CRITICAL: NEVER set UserPaint on TextBoxBase, ComboBox, or native EDIT controls!
            // Setting UserPaint on a TextBox disables native Windows message handling and hides all text.
            if (control is TextBoxBase || control is ComboBox || control is ListBox)
            {
                return;
            }

            try
            {
                MethodInfo setStyleMethod = typeof(Control).GetMethod("SetStyle", BindingFlags.NonPublic | BindingFlags.Instance);
                if (setStyleMethod != null)
                {
                    setStyleMethod.Invoke(control, new object[] {
                        ControlStyles.OptimizedDoubleBuffer |
                        ControlStyles.AllPaintingInWmPaint,
                        true
                    });
                }
            }
            catch { }
        }

        /// <summary>
        /// Recursively enables double buffering on a control and all its children.
        /// </summary>
        public static void EnableDoubleBufferingTree(Control parent)
        {
            if (parent == null) return;

            EnableDoubleBuffering(parent);

            foreach (Control child in parent.Controls)
            {
                EnableDoubleBufferingTree(child);
            }
        }
    }
}
