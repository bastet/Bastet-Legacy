/*
 * Copyright (c) 2011-2014, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;
using System.Runtime.Serialization;

namespace CoAP.Proxy
{
    [Serializable]
    public class TranslationException : Exception
    {
        public TranslationException() { }

        public TranslationException(String message) : base(message) { }

        public TranslationException(String message, Exception inner) : base(message, inner) { }

        protected TranslationException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }
}
