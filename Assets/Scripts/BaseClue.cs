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
        [SerializeField]
        private NotebookEntrySO clueNote;
        public virtual void Activate()
        {
            if (!Activated)
            {
                Activated = true;
                Notebook.instance.addEntry(clueNote);
                OnClueActivated.Invoke();
            }
        }
    }
    public enum ClueStage
    {
        stage_1,
        stage_2,
        stage_3
    }
}
