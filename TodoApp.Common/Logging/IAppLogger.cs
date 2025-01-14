﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TodoApp.Common.Logging
{
    public interface IAppLogger<T>
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void LogError(Exception exception, string message, params object[] args);
    }
}
