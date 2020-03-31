﻿using System;
using Assembler.Core.Enums;

namespace Assembler.Core.Entities
{
    public abstract class BaseMessageInAssembly
    {
        public Guid Guid { get; }
        public ReleaseReason ReleaseReason { get; set; }
        public bool MiddleReceived { get; set; }
        public DateTime AssemblingStartTime { get; set; }
        public DateTime LastFrameReceived { get; set; }

        protected BaseMessageInAssembly(DateTime assemblingStartTime, DateTime lastFrameReceived)
        {
            Guid = Guid.NewGuid();
            ReleaseReason = ReleaseReason.Unreleased;
            MiddleReceived = false;
            AssemblingStartTime = assemblingStartTime;
            LastFrameReceived = lastFrameReceived;
        }
    }
}