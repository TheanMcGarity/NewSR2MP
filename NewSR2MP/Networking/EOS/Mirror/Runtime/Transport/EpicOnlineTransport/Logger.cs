using Epic.OnlineServices.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EpicTransport {
    public static class Logger {

        public static void EpicDebugLog(LogMessage message) {
            switch (message.Level) {
                case LogLevel.Info:
                    SRMP.Log($"Epic Manager: Category - {message.Category} Message - {message.Message}");
                    break;
                case LogLevel.Error:
                    SRMP.Error($"Epic Manager: Category - {message.Category} Message - {message.Message}");
                    break;
                case LogLevel.Warning:
                    SRMP.Warn($"Epic Manager: Category - {message.Category} Message - {message.Message}");
                    break;
                case LogLevel.Fatal:
                    SRMP.Error(new Exception($"[FATAL ERROR!!!]\n[FATAL ERROR!!!] Epic Manager: Category - {message.Category} Message - {message.Message}\n[FATAL ERROR!!!]").ToString());
                    break;
                default:
                    SRMP.Log($"Epic Manager: Unknown log processing. Category - {message.Category} Message - {message.Message}");
                    break;
            }
        }
    }
}