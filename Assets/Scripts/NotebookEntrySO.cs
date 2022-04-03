using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new entry", menuName = "Create entry")]
public class NotebookEntrySO : ScriptableObject
{
    public ClueStage stage;
    public string title;
    [TextArea]
    public string shortDescription;
}
