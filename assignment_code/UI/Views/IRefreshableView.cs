using System;

namespace assignment_code.UI.Views
{
    /// <summary>
    /// Contract for all modular views to support instant, real-time UI & data refresh
    /// </summary>
    public interface IRefreshableView
    {
        void RefreshView();
    }
}
