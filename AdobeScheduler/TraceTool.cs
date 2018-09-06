/*
Copyright (c) 2007-2009 Dmitry Stroganov (DmitryStroganov.info)
Redistributions of any form must retain the above copyright notice.
 
Use of any commands included in this SDK is at your own risk. 
Dmitry Stroganov cannot be held liable for any damage through the use of these commands.
*/

#define TRACE

using System;
using System.Diagnostics;
using System.Text;
using System.Reflection;

namespace AdobeConnectSDK
{
    [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.LinkDemand, Unrestricted = true)]
    static class TraceTool
    {
        public static void TraceMessage(string msg)
        {
            string ClassName = null;
            string MethodName = null;

            StackTrace stackTrace = new StackTrace();
            if (stackTrace.FrameCount > 1)
            {
                try
                {
                    StackFrame stackFrame = stackTrace.GetFrame(1);
                    MethodBase methodBase = stackFrame.GetMethod();
                    MethodName = methodBase.Name;
                    ClassName = methodBase.ReflectedType.Name;
                }
                finally { }
            }

            if (string.IsNullOrEmpty(ClassName))
                ClassName = "_UnknownClass";

            if (string.IsNullOrEmpty(MethodName))
                MethodName = "_UnknownMethod";

            Trace.WriteLine(string.Format("[{0}] at '{1}.{2}': {3}", DateTime.Now.ToString("g"), ClassName, MethodName, msg));
            Trace.Flush();
        }

        public static void TraceException(Exception ex)
        {
            string ClassName = null;
            string MethodName = null;
            string _srcLoc = null;

            StackTrace stackTrace = new StackTrace();
            if (stackTrace.FrameCount > 1)
            {
                try
                {
                    StackFrame stackFrame = stackTrace.GetFrame(1);
                    string _fName = stackFrame.GetFileName();
                    int _lNumber = stackFrame.GetFileLineNumber();
                    int _cNumber = stackFrame.GetFileColumnNumber();
                    _srcLoc = (!string.IsNullOrEmpty(_fName)) ? string.Format("'{0}', position [{1},{2}]", _fName, _lNumber, _cNumber) : null;
                    MethodBase methodBase = stackFrame.GetMethod();
                    MethodName = methodBase.Name;
                    ClassName = methodBase.ReflectedType.Name;
                }
                finally { }
            }

            if (string.IsNullOrEmpty(ClassName))
                ClassName = "_UnknownClass";

            if (string.IsNullOrEmpty(MethodName))
                MethodName = "_UnknownMethod";

            StringBuilder exTxt = new StringBuilder();
            //exTxt.AppendLine("--------------");
            exTxt.AppendFormat("\n[{0}] at '{1}.{2}'", DateTime.Now.ToString("g"), ClassName, MethodName);
            if (!string.IsNullOrEmpty(_srcLoc))
                exTxt.AppendLine(_srcLoc);

            exTxt.Append(GetExceptionInfo(ex));
            exTxt.AppendLine("[-----]");

            Trace.WriteLine(exTxt.ToString());
            Trace.Flush();
        }


        public static string GetExceptionInfo(Exception ex)
        {
            StringBuilder _exceptionStack = new StringBuilder();

            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                _exceptionStack.AppendFormat("{0}\n{1}\n", ex.Message, ex.StackTrace);
            }
            else
            {
                _exceptionStack.AppendFormat("{0}\n", ex.Message);
            }

            if (ex.InnerException != null)
                RetriveStackRecursively(ex.InnerException);
            _exceptionStack.Append(m_StackTrace.ToString());

            return _exceptionStack.ToString();
        }


        static StringBuilder m_StackTrace = new StringBuilder();
        static short _rlim = 4, _cnt = 0;
        private static void RetriveStackRecursively(Exception innerEx)
        {
            if (!string.IsNullOrEmpty(innerEx.StackTrace))
            {
                m_StackTrace.AppendFormat("{0}\n", innerEx.StackTrace);
            }
            if (_cnt > _rlim) return;
            _cnt++;
            if (innerEx.InnerException != null) RetriveStackRecursively(innerEx.InnerException);
        }
    }
}
