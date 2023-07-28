using System;

namespace Fall_Friends.States
{
    public abstract class BaseState 
    {
        public abstract Type Tick();
        public virtual void FrameUpdate() {}
        public virtual void OnEnter() {}
        public virtual void OnExit() {}

    }
}


