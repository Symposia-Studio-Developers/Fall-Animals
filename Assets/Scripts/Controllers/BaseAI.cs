using System;
using System.Collections.Generic;
using UnityEngine;
using Fall_Friends.States;
using Fall_Friends.Manager;
using System.Collections;

namespace Fall_Friends.Controllers {
    public abstract class BaseAI : MonoBehaviour
    {
        protected Dictionary<Type, BaseState> availableStates;

        protected BaseState currentState;
        public String CurrentState => currentState?.GetType().Name;

        [SerializeField] protected bool doTick = true; // pause updates or not
        

        public bool Grounded {get; protected set;}
        public bool Falling {get; protected set;}

        protected virtual void Start() {}

        protected virtual void Update() 
        {
            if (currentState == null) 
                Debug.LogError($"State machine not initialized for {this.name}");
            
            if (!doTick) return;

            var stateType = currentState.Tick();
            SwitchState(stateType);
        }

        protected virtual void FixedUpdate() 
        {
            if (GameManager.Instance == null) 
                Debug.LogError("GameManager instance is null");
            
            if (currentState == null) 
                Debug.LogError($"State machine not initialized for {this.name}");
            if (!doTick) return;

            currentState.FrameUpdate();
        }

        public void SwitchState(Type newStateType) 
        {
            if (newStateType != null && currentState.GetType() != newStateType) {
                availableStates.TryGetValue(newStateType, out var newState);
                if (newState == null) 
                {
					Debug.LogError($"{this.name} has no available state for {newStateType.Name}");
				} 
                else 
                {
                    Debug.Log($"{this.name} Entering a new state {newStateType.Name} from previous state {currentState.GetType().Name} ");
					currentState.OnExit();
					currentState = newState;
					currentState.OnEnter();
				}
                
            }
        }

    }
}

