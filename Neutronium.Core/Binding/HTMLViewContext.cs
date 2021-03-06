﻿using System;
using System.Threading.Tasks;
using Neutronium.Core.Binding.Listeners;
using Neutronium.Core.Exceptions;
using Neutronium.Core.JavascriptFramework;
using Neutronium.Core.WebBrowserEngine.JavascriptObject;
using Neutronium.Core.WebBrowserEngine.Window;

namespace Neutronium.Core.Binding
{
    public class HtmlViewContext : IDisposable
    {
        public IWebView WebView => _IWebBrowserWindow.MainFrame;
        public IUiDispatcher UiDispatcher { get; }
        public IJavascriptSessionInjector JavascriptSessionInjector { get; private set; }
        public IJavascriptViewModelUpdater ViewModelUpdater { get; private set; }
        public bool JavascriptFrameworkIsMappingObject => _JavascriptFrameworkManager.IsMappingObject;
        public IWebSessionLogger Logger { get; }

        private IJavascriptObject _Listener;
        private IJavascriptViewModelManager _VmManager;
        private readonly IJavascriptFrameworkManager _JavascriptFrameworkManager;
        private readonly IWebBrowserWindow _IWebBrowserWindow;

        public HtmlViewContext(IWebBrowserWindow webBrowserWindow, IUiDispatcher uiDispatcher, IJavascriptFrameworkManager javascriptFrameworkManager,
            IWebSessionLogger logger)
        {
            _IWebBrowserWindow = webBrowserWindow;
            Logger = logger;
            UiDispatcher = uiDispatcher;
            _JavascriptFrameworkManager = javascriptFrameworkManager;
        }

        public void CheckUiContext()
        {
            if (UiDispatcher.IsInContext())
                return;

            throw ExceptionHelper.Get("MVVM ViewModel should be updated from UI thread. Use await pattern and Dispatcher to do so.");
        }

        internal void InitOnJsContext(IJavascriptChangesListener javascriptChangesListener, bool debugMode)
        {
            var builder = new BinderBuilder(WebView, javascriptChangesListener);
            _Listener = builder.BuildListener();
            _VmManager = _JavascriptFrameworkManager.CreateManager(WebView, _Listener, Logger, debugMode);
            ViewModelUpdater = _VmManager.ViewModelUpdater;
            JavascriptSessionInjector = _VmManager.Injector;
        }

        public Task RunOnUiContextAsync(Action act) 
        {
            return UiDispatcher.RunAsync(act);
        }

        public Task RunOnJavascriptContextAsync(Action act)
        {
            return WebView.RunAsync(act);
        }

        public Task<T> EvaluateOnUiContextAsync<T>(Func<T> act)
        {
            return UiDispatcher.EvaluateAsync(act);
        }

        public void Dispose() 
        {
            _VmManager.Dispose();
            _Listener.Dispose();
            Logger.Debug("HTMLViewContext Disposed");
        }
    }
}
