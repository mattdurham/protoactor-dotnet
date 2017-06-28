﻿// -----------------------------------------------------------------------
//   <copyright file="RemoteProcess.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using Google.Protobuf;

namespace Proto.Remote
{
    public class RemoteProcess : Process
    {
        private readonly PID _pid;

        public RemoteProcess(PID pid)
        {
            _pid = pid;
        }

        protected override void SendUserMessage(PID _, object message)
        {
            Send(message);
        }

        protected override void SendSystemMessage(PID _, object message)
        {
            Send(message);
        }

        private void Send(object msg)
        {
            if (msg is Watch w)
            {
                var rw = new RemoteWatch(w.Watcher, _pid);
                Remote.EndpointManagerPid.Tell(rw);
            }
            else if (msg is Unwatch uw)
            {
                var ruw = new RemoteUnwatch(uw.Watcher, _pid);
                Remote.EndpointManagerPid.Tell(ruw);
            }
            else
            {
                SendRemoteMessage(_pid, msg,Serialization.DefaultSerializerId);
            }
        }

        public static void SendRemoteMessage(PID pid, object msg,int serializerId)
        {
            var (message, sender, _) = Proto.MessageEnvelope.Unwrap(msg);

            if (message is IMessage protoMessage)
            {
                var env = new RemoteDeliver(protoMessage, pid, sender, serializerId);
                Remote.EndpointManagerPid.Tell(env);
            }
            else
            {
                throw new NotSupportedException("Non protobuf message");
            }
        }
    }
}