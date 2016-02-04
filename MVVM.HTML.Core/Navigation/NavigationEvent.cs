﻿using System;

namespace MVVM.HTML.Core
{
    public class NavigationEvent : EventArgs
    {
        public object NewViewModel { get; private set; }

        public object OldViewModel { get; private set; }

        public NavigationEvent(object iNewViewModel, object iOldViewModel)
        {
            NewViewModel = iNewViewModel;
            OldViewModel = iOldViewModel;
        }
    }
}
