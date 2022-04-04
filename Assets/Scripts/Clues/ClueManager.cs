using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Assets.Scripts
{
    public class ClueManager : MonoBehaviour
    {
        [SerializeField]
        private ClueStage[] stages;
        // Start is called before the first frame update
        void Start()
        {
            //Initialize first stage
            if (stages.Length > 0)
                stages[0].initialize();
            //Chain stages together
            for (int i = 0; i < stages.Length - 1; i++)
            {
                stages[0].OnStageEnd.AddListener(stages[i + 1].initialize);
            }
        }
    }
    [System.Serializable]
    public class ClueStage
    {
        public UnityEvent OnStageStart;
        public UnityEvent OnStageEnd;
        [SerializeField]
        private BaseClue[] clues;
        private int progress;
        private bool complete = false;
        public void initialize()
        {
            OnStageStart.Invoke();
            foreach(BaseClue clue in clues)
            {
                clue.gameObject.SetActive(clue.SetActiveOnStageStart || clue.gameObject.activeSelf);
                clue.OnClueActivated.AddListener(() =>
                {
                    progress++;
                    evaluateProgress();
                });
            }
        }
        private void evaluateProgress()
        {
            if (progress >= clues.Length && !complete)
            {
                complete = true;
                OnStageEnd.Invoke();
            }
        }
    }
}
