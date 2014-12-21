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

using CoAP.Net;
using CoAP.Server;

namespace CoAP.Stack
{
    /// <summary>
    /// Builds up the stack of CoAP layers
    /// that process the CoAP protocol.
    /// </summary>
    public class CoapStack : LayerStack
    {
        public CoapStack(ICoapConfig config)
        {
            AddLast("Observe", new ObserveLayer(config));
            AddLast("Blockwise", new BlockwiseLayer(config));
            AddLast("Token", new TokenLayer(config));
            AddLast("Reliability", new ReliabilityLayer(config));
        }
    }
}
