using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Assets.Scripts
{
    public class BaseClue : MonoBehaviour
    {
        public UnityEvent OnClueActivated;
        public bool Activated;
        public bool SetActiveOnStageStart;
        public virtual void Activate()
        {
            if (!Activated)
            {
                Activated = true;
                OnClueActivated.Invoke();
            }
        }
    }
}
